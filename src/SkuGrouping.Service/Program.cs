using Contracts; using MassTransit;
var b=WebApplication.CreateBuilder(args);
b.Services.AddMassTransit(x=>{ x.AddConsumer<SalesPatternsConsumer>(); x.UsingRabbitMq((ctx,cfg)=>{ cfg.Host(b.Configuration["Rabbit:Host"] ?? b.Configuration["Rabbit__Host"] ?? "rabbitmq","/",h=>{}); cfg.ConfigureEndpoints(ctx); });});
var app=b.Build(); app.Run();
class SalesPatternsConsumer:IConsumer<SalesPatternsIdentified>{
  public async Task Consume(ConsumeContext<SalesPatternsIdentified> ctx){
    var sorted=ctx.Message.Demand.OrderByDescending(d=>d.Velocity).ToList();
    int n=sorted.Count; List<SkuGroup> groups=new();
    if(n>0){ int high=Math.Max(1,n/3); int med=Math.Max(1,(n*2)/3);
      groups.Add(new SkuGroup("HIGH", sorted.Take(high).Select(x=>x.SkuId).ToList()));
      groups.Add(new SkuGroup("MED", sorted.Skip(high).Take(med-high).Select(x=>x.SkuId).ToList()));
      groups.Add(new SkuGroup("LOW", sorted.Skip(med).Select(x=>x.SkuId).ToList()));
    }
    await ctx.Publish(new SkuGroupsCreated(ctx.Message.RunId, groups, ctx.Message.Params));
  }
}
