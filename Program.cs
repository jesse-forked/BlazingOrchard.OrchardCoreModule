using Grove.Workflows.Extensions;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logger) =>
{
    logger.ReadFrom.Configuration(context.Configuration);
});

builder.Services
    .AddCors(options =>
    {
        options.AddDefaultPolicy(policy => policy
            .WithOrigins("http://localhost:5011", "http://127.0.0.1:5011")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    });

builder.Services
    .AddQuartz()
    .AddQuartzHostedService()
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseCors();
app.UseOrchardCore();

await app.RunAsync();
