using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Contracts;

var builder = WebApplication.CreateBuilder(args);

// MassTransit setup
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SalesPatternsConsumer>();
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapGet("/", () => "SkuGrouping Service running...");

app.Run();

// Consumer implementation
public class SalesPatternsConsumer : IConsumer<SalesPatternsIdentified>
{
    public async Task Consume(ConsumeContext<SalesPatternsIdentified> context)
    {
        var salesPatterns = context.Message;

        // Group SKUs by Category, aggregate by TotalUnits
        var groups = salesPatterns.Demand
            .GroupBy(d => d.Category)
            .Select(g => new SkuGroup(g.Key, g.Sum(x => x.TotalUnits)))
            .ToList();

        Console.WriteLine($"[SkuGrouping] RunId {salesPatterns.RunId}: Created {groups.Count} groups.");

        // Publish the result
        await context.Publish(new SkuGroupsCreated(
            salesPatterns.RunId,
            groups,
            salesPatterns.Demand
        ));
    }
}
