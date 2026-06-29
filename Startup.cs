using Elsa.Mediator.Options;
using Grove.Workflows.Extensions;
using Grove.Workflows.Middleware;
using Grove.Workflows.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace BlazingOrchard;

[Feature("Blazing")]
public sealed class Startup : OrchardCore.Modules.StartupBase
{
    private const string BlazingWebCors = "BlazingWeb";

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(BlazingWebCors, policy => policy
                .WithOrigins("http://localhost:5011", "http://127.0.0.1:5011")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        services.Configure<MediatorOptions>(options => options.JobWorkerCount = 1);
        services.Configure<ElsaStudioBlazorOptions>(options => options.RenderMode = RenderMode.WebAssembly);
        services.ConfigureWebAssemblyStaticFiles();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.RewriteElsaStudioWebAssemblyAssets();
        app.UseCors(BlazingWebCors);
    }
}
