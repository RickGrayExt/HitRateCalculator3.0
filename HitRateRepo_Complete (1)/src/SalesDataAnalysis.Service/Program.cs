using Contracts;
using MassTransit;
using CsvHelper;
using System.Globalization;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMassTransit(x=>{
  x.AddConsumer<StartRunConsumer>();
  x.UsingRabbitMq((ctx,cfg)=>{ cfg.Host(builder.Configuration["Rabbit:Host"] ?? builder.Configuration["Rabbit__Host"] ?? "rabbitmq","/",h=>{}); cfg.ConfigureEndpoints(ctx); });
});
var app = builder.Build(); app.Run();
class StartRunConsumer : IConsumer<StartRunCommand>{
  public async Task Consume(ConsumeContext<StartRunCommand> ctx){
    var map = new Dictionary<string,(int units,int orders)>();
    using var http=new HttpClient();
    using var stream=await http.GetStreamAsync(ctx.Message.DatasetPath);
    using var reader=new StreamReader(stream);
    using var csv=new CsvReader(reader,CultureInfo.InvariantCulture);
    var recs=csv.GetRecords<dynamic>();
    foreach(var r in recs){ string sku=(string)r.SkuId; int qty=int.Parse((string)r.Qty); if(!map.ContainsKey(sku)) map[sku]=(0,0); var cur=map[sku]; map[sku]=(cur.units+qty, cur.orders+1); }
    var demand=map.Select(kv=> new SkuDemand(kv.Key, kv.Value.units, kv.Value.orders, kv.Value.orders==0?0:(double)kv.Value.units/kv.Value.orders, false)).ToList();
    await ctx.Publish(new SalesPatternsIdentified(ctx.Message.RunId,demand,ctx.Message.Params));
  }
}
