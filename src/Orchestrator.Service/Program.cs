using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ResultConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["Rabbit:Host"] ?? "rabbitmq", "/", h => { });
        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();
app.UseCors();

app.MapPost("/runs", async (IPublishEndpoint bus, StartRequest req) =>
{
    var runId = Guid.NewGuid();
    var p = req.Params ?? new RunParams(true, 50, 100, 200, true, 4, 5, 20);
    var ds = string.IsNullOrWhiteSpace(req.DatasetPath) ? "/data/DataSetClean.csv" : req.DatasetPath!;
    var mode = string.IsNullOrWhiteSpace(req.Mode) ? "PTO" : req.Mode!;
    await bus.Publish(new StartRunCommand(runId, ds, mode, p));
    return Results.Accepted($"/runs/{runId}", new { runId, status="Started"});
});

app.MapGet("/", () => "Orchestrator OK");
app.Run();

public record StartRequest(string? DatasetPath, string? Mode, RunParams? Params);

class ResultConsumer : IConsumer<HitRateCalculated>
{
    public Task Consume(ConsumeContext<HitRateCalculated> ctx)
    {
        Console.WriteLine($"Run {ctx.Message.RunId} HitRate={ctx.Message.Result.HitRate:P2} Mode={ctx.Message.Result.Mode}");
        return Task.CompletedTask;
    }
}
