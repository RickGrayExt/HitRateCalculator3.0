using Contracts; using MassTransit;
var b=WebApplication.CreateBuilder(args);
b.Services.AddMassTransit(x=>{ x.AddConsumer<RackLayoutConsumer>(); x.UsingRabbitMq((ctx,cfg)=>{ cfg.Host(b.Configuration["Rabbit:Host"] ?? b.Configuration["Rabbit__Host"] ?? "rabbitmq","/",h=>{}); cfg.ConfigureEndpoints(ctx); });});
var app=b.Build(); app.Run();
class RackLayoutConsumer:IConsumer<RackLayoutCalculated>{
  public async Task Consume(ConsumeContext<RackLayoutCalculated> ctx){
    var p=ctx.Message.Params; List<Batch> batches=new(); int totalOrders=200; int per=Math.Max(1,p.OrdersPerBatch); int bi=0;
    for(int i=0;i<totalOrders;i+=per){ var lines=new List<OrderLine>(); int cnt=Math.Min(per,totalOrders-i);
      for(int j=0;j<cnt;j++){ var sku=ctx.Message.Racks[(j)%ctx.Message.Racks.Count].SkuIds[0]; lines.Add(new OrderLine($"O{i+j+1}", sku, 1)); }
      batches.Add(new Batch($"B{++bi}", null, p.Mode, lines));
    }
    await ctx.Publish(new BatchesCreated(ctx.Message.RunId, batches, p.Mode, p));
  }
}
