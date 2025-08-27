using Contracts; using MassTransit;
var b=WebApplication.CreateBuilder(args);
b.Services.AddMassTransit(x=>{ x.AddConsumer<ShelfLocationsConsumer>(); x.UsingRabbitMq((ctx,cfg)=>{ cfg.Host(b.Configuration["Rabbit:Host"] ?? b.Configuration["Rabbit__Host"] ?? "rabbitmq","/",h=>{}); cfg.ConfigureEndpoints(ctx); });});
var app=b.Build(); app.Run();
class ShelfLocationsConsumer:IConsumer<ShelfLocationsAssigned>{
  public async Task Consume(ConsumeContext<ShelfLocationsAssigned> ctx){
    var skus=ctx.Message.Locations.Select(l=>l.SkuId).Distinct().ToList();
    int maxPerRack=Math.Max(1, ctx.Message.Params.MaxSkusPerRack);
    int rackCount=(int)Math.Ceiling(skus.Count/(double)maxPerRack);
    var racks=new List<Rack>();
    for(int i=0;i<rackCount;i++){ var slice=skus.Skip(i*maxPerRack).Take(maxPerRack).ToList(); racks.Add(new Rack($"R{i+1}", slice)); }
    await ctx.Publish(new RackLayoutCalculated(ctx.Message.RunId, racks, ctx.Message.Params));
  }
}
