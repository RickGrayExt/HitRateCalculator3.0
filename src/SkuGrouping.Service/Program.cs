using MassTransit;
using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Register MassTransit with the SalesPatternsConsumer
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

// Health endpoint
app.MapGet("/", () => "SkuGrouping Service running");

app.Run();

public class SalesPatternsConsumer : IConsumer<SalesPatternsIdentified>
{
    public async Task Consume(ConsumeContext<SalesPatternsIdentified> context)
    {
        var message = context.Message;

        // Guard: ensure message has SkuDemands
        if (message.Demand == null || !message.TotalUnits.Any())
        {
            Console.WriteLine($"[SkuGrouping] No SKU demands found for Run {message.RunId}");
            return;
        }

        // Group SKU demands by SKU Id
        var groups = message.Demand
            .GroupBy(sku => sku.SkuId)
            .Select(g => new SkuGroup(g.Key, g.Sum(x => x.Demand))) // assuming SkuGroup ctor exists
            .ToList();

        Console.WriteLine($"[SkuGrouping] Created {groups.Count} groups for Run {message.RunId}");

        // Publish the grouped result (using constructor signature from Contracts)
        await context.Publish(new SkuGroupsCreated(
            message.RunId,
            groups,
            message.Demand
        ));
    }
}
