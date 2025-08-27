using MassTransit;
using Contracts;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Register MassTransit and the consumer
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SalesPatternsConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h => { });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Simple health check
app.MapGet("/", () => "SkuGrouping Service running");

app.Run();

public class SalesPatternsConsumer : IConsumer<SalesPatternsIdentified>
{
    public async Task Consume(ConsumeContext<SalesPatternsIdentified> context)
    {
        var patterns = context.Message;

        // Group SKU demands by SKU Id and calculate totals
        var groups = patterns.SkuDemands
            .GroupBy(sku => sku.SkuId)
            .Select(g => new { SkuId = g.Key, TotalDemand = g.Sum(x => x.Demand) })
            .ToList();

        Console.WriteLine($"[SkuGrouping] Created {groups.Count} groups for Run {patterns.RunId}");

        await context.Publish(new SkuGroupsCreated
        {
            RunId = patterns.RunId,
            Groups = groups.Select(g => g.SkuId).ToList()
        });
    }
}
