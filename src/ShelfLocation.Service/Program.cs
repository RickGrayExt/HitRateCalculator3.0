using Contracts; using MassTransit;
var b=WebApplication.CreateBuilder(args);
b.Services.AddMassTransit(x=>{ x.AddConsumer<SkuGroupsConsumer>(); x.UsingRabbitMq((ctx,cfg)=>{ cfg.Host(b.Configuration["Rabbit:Host"] ?? b.Configuration["Rabbit__Host"] ?? "rabbitmq","/",h=>{}); cfg.ConfigureEndpoints(ctx); });});
var app=b.Build(); app.Run();
class SkuGroupsConsumer:IConsumer<SkuGroupsCreated>{
  public async Task Consume(ConsumeContext<SkuGroupsCreated> ctx){
    var locations=new List<ShelfLocation>(); int idx=0; int rackIdx=1;
    foreach(var g in ctx.Message.Groups){ foreach(var sku in g.SkuIds){ idx++; locations.Add(new ShelfLocation(sku,$"R{rackIdx}",$"S{idx%50+1}",idx)); } rackIdx++; }
    await ctx.Publish(new ShelfLocationsAssigned(ctx.Message.RunId, locations, ctx.Message.Params));
  }
}
