using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Yarp.ReverseProxy;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Register YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();
app.Run();

