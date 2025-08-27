using Contracts; using MassTransit;
var b=WebApplication.CreateBuilder(args);
b.Services.AddMassTransit(x=>{ x.AddConsumer<BatchesCreatedConsumer>(); x.UsingRabbitMq((ctx,cfg)=>{ cfg.Host(b.Configuration["Rabbit:Host"] ?? b.Configuration["Rabbit__Host"] ?? "rabbitmq","/",h=>{}); cfg.ConfigureEndpoints(ctx); });});
var app=b.Build(); app.Run();
class BatchesCreatedConsumer:IConsumer<BatchesCreated>{
  public async Task Consume(ConsumeContext<BatchesCreated> ctx){
    var p=ctx.Message.Params; int stations=Math.Max(1, Math.Min(p.MaxStationsOpen, p.MaxStations));
    var assigns=new List<StationAssignment>(); for(int s=0;s<stations;s++) assigns.Add(new StationAssignment($"S{s+1}", new List<string>()));
    int idx=0; foreach(var b in ctx.Message.Batches){ assigns[idx%stations].BatchIds.Add(b.BatchId); idx++; }
    await ctx.Publish(new StationsAllocated(ctx.Message.RunId, assigns, p));
  }
}
