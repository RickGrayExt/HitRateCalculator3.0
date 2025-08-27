using Contracts; using MassTransit;
var b=WebApplication.CreateBuilder(args);
b.Services.AddMassTransit(x=>{ x.AddConsumer<StationsAllocatedConsumer>(); x.UsingRabbitMq((ctx,cfg)=>{ cfg.Host(b.Configuration["Rabbit:Host"] ?? b.Configuration["Rabbit__Host"] ?? "rabbitmq","/",h=>{}); cfg.ConfigureEndpoints(ctx); });});
var app=b.Build(); app.Run();
class StationsAllocatedConsumer:IConsumer<StationsAllocated>{
  public async Task Consume(ConsumeContext<StationsAllocated> ctx){
    int totalBatches=ctx.Message.Assignments.Sum(a=>a.BatchIds.Count);
    int totalItems=totalBatches*Math.Max(1, ctx.Message.Params.OrdersPerBatch);
    int rackPresentations=totalBatches;
    double hitRate=rackPresentations==0?0.0:(double)totalItems/rackPresentations;
    var byRack=new Dictionary<string,double>{{"ALL", hitRate}};
    await ctx.Publish(new HitRateCalculated(ctx.Message.RunId, new HitRateResult(ctx.Message.Params.Mode, hitRate, totalItems, rackPresentations, byRack)));
  }
}
