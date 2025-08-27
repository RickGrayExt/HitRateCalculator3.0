using Yarp.ReverseProxy;
var b = WebApplication.CreateBuilder(args);
b.Services.AddReverseProxy().LoadFromConfig(b.Configuration.GetSection("ReverseProxy"));
var app = b.Build();
app.MapReverseProxy();
app.Run();
