using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace BlazingOrchard;

public sealed class BlazorAdminThemeOptions
{
    public string AdminPath { get; set; } = "/admin";
    public string BlazorThemeTag { get; set; } = "blazor";
    public string BlazorAdminThemeId { get; set; } = "BlazingOrchard.Admin";
    public string AdminThemeSourceWebRoot { get; set; } = "modules/BlazingOrchard/Themes/BlazingOrchard.Admin/wwwroot";
    public string AdminThemeBuildWebRoot { get; set; } = "modules/BlazingOrchard/Themes/BlazingOrchard.Admin/bin/BlazingOrchard.Admin/Debug/net10.0/wwwroot";
}

public sealed class BlazorAdminThemeMiddleware
{
    private static readonly PathString FrameworkPath = new("/_framework");
    private static readonly PathString ContentPath = new("/_content");
    private static readonly PathString BlazingAdminThemePreviewPath = new("/BlazingOrchard.Admin/Theme.png");

    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    private readonly IOptions<BlazorAdminThemeOptions> _options;
    private readonly FileExtensionContentTypeProvider _contentTypes = new();
    private readonly ILogger<BlazorAdminThemeMiddleware> _logger;

    public BlazorAdminThemeMiddleware(
        RequestDelegate next,
        IHostEnvironment environment,
        IOptions<BlazorAdminThemeOptions> options,
        ILogger<BlazorAdminThemeMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = context.Request.Path;
        var options = _options.Value;
        var adminPath = new PathString(options.AdminPath);

        var isAdminRoute = requestPath.StartsWithSegments(adminPath, out var adminRemainder);
        var isBlazorAssetRoute = IsBlazorAssetRoute(requestPath);
        var isBlazingAdminThemePreviewRoute = requestPath.Equals(BlazingAdminThemePreviewPath);

        if (!isAdminRoute && !isBlazorAssetRoute && !isBlazingAdminThemePreviewRoute)
        {
            await _next(context);
            return;
        }

        var webRoots = ResolveAdminThemeWebRoots(options).ToArray();
        if (isBlazingAdminThemePreviewRoute)
        {
            foreach (var webRoot in webRoots)
            {
                if (await TryServeFileAsync(context, webRoot, "Theme.png"))
                {
                    return;
                }
            }

            await _next(context);
            return;
        }

        if (!await IsBlazorAdminThemeAsync(context, options, requestPath))
        {
            await _next(context);
            return;
        }

        if (webRoots.Length == 0)
        {
            _logger.LogWarning("Blazor admin theme web roots were not found. Letting Orchard handle {Path}.", requestPath);
            await _next(context);
            return;
        }

        var relativePath = isAdminRoute
            ? GetAdminRelativePath(adminRemainder)
            : requestPath.Value?.TrimStart('/') ?? string.Empty;

        if (string.IsNullOrWhiteSpace(relativePath) || !Path.HasExtension(relativePath))
        {
            relativePath = "index.html";
        }

        foreach (var webRoot in webRoots)
        {
            if (await TryServeFileAsync(context, webRoot, relativePath))
            {
                return;
            }
        }

        // Deep links under /admin should still load the Blazor shell.
        if (isAdminRoute)
        {
            foreach (var webRoot in webRoots)
            {
                if (await TryServeFileAsync(context, webRoot, "index.html"))
                {
                    return;
                }
            }
        }

        await _next(context);
    }

    private static bool IsBlazorAssetRoute(PathString requestPath)
    {
        if (requestPath.StartsWithSegments(FrameworkPath) || requestPath.StartsWithSegments(ContentPath))
        {
            return true;
        }

        var value = requestPath.Value;
        return value is "/blazing.css" or "/blazing.theme.js" or "/favicon.ico";
    }

    private static string GetAdminRelativePath(PathString adminRemainder)
    {
        var value = adminRemainder.Value?.TrimStart('/') ?? string.Empty;
        return string.IsNullOrEmpty(value) ? "index.html" : value;
    }

    private async Task<bool> IsBlazorAdminThemeAsync(HttpContext context, BlazorAdminThemeOptions options, PathString requestPath)
    {
        var adminThemeService = context.RequestServices.GetService<IAdminThemeService>();
        if (adminThemeService is not null)
        {
            return await IsBlazorAdminThemeAsync(adminThemeService, options, context, requestPath);
        }

        var shellHost = context.RequestServices.GetService<IShellHost>();
        if (shellHost is null)
        {
            _logger.LogDebug("Blazor admin route check for {Path}: no shell host is available.", requestPath);
            return false;
        }

        await shellHost.InitializeAsync();

        if (!shellHost.TryGetSettings("Default", out var shellSettings))
        {
            _logger.LogDebug("Blazor admin route check for {Path}: Default shell settings are not available.", requestPath);
            return false;
        }

        var isBlazorAdminTheme = false;
        await (await shellHost.GetScopeAsync(shellSettings)).UsingServiceScopeAsync(async scope =>
        {
            var scopedAdminThemeService = scope.ServiceProvider.GetRequiredService<IAdminThemeService>();
            isBlazorAdminTheme = await IsBlazorAdminThemeAsync(scopedAdminThemeService, options, context, requestPath);
        });

        return isBlazorAdminTheme;
    }

    private async Task<bool> IsBlazorAdminThemeAsync(IAdminThemeService adminThemeService, BlazorAdminThemeOptions options, HttpContext context, PathString requestPath)
    {
        var adminThemeName = await adminThemeService.GetAdminThemeNameAsync();
        var adminTheme = await adminThemeService.GetAdminThemeAsync();
        var hasBlazorTag = HasBlazorTag(adminTheme, options.BlazorThemeTag);
        var isBlazorAdminTheme = string.Equals(adminThemeName, options.BlazorAdminThemeId, StringComparison.OrdinalIgnoreCase) || hasBlazorTag;

        _logger.LogDebug(
            "Blazor admin route check for {Path}: selected admin theme name '{AdminThemeName}', resolved extension '{ExtensionId}', has '{Tag}' tag: {HasBlazorTag}, serving Blazor: {ServeBlazor}.",
            requestPath,
            adminThemeName,
            adminTheme?.Id,
            options.BlazorThemeTag,
            hasBlazorTag,
            isBlazorAdminTheme);

        return isBlazorAdminTheme;
    }

    private static bool HasBlazorTag(IExtensionInfo? extension, string tag)
    {
        return extension?.Manifest?.Tags?.Any(candidate => string.Equals(candidate, tag, StringComparison.OrdinalIgnoreCase)) == true;
    }

    private IEnumerable<string> ResolveAdminThemeWebRoots(BlazorAdminThemeOptions options)
    {
        var candidates = new[]
        {
            Path.Combine(_environment.ContentRootPath, options.AdminThemeBuildWebRoot),
            Path.Combine(_environment.ContentRootPath, options.AdminThemeSourceWebRoot),
            Path.Combine(AppContext.BaseDirectory, options.AdminThemeBuildWebRoot),
            Path.Combine(AppContext.BaseDirectory, options.AdminThemeSourceWebRoot),
        };

        return candidates.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<bool> TryServeFileAsync(HttpContext context, string webRoot, string relativePath)
    {
        if (relativePath.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        var provider = new PhysicalFileProvider(webRoot);
        var file = provider.GetFileInfo(relativePath.Replace('\\', '/'));
        if (!file.Exists || file.IsDirectory)
        {
            return false;
        }

        if (!_contentTypes.TryGetContentType(file.Name, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        context.Response.ContentType = contentType;
        context.Response.ContentLength = file.Length;

        if (string.Equals(file.Name, "index.html", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        }

        await using var stream = file.CreateReadStream();
        await stream.CopyToAsync(context.Response.Body, context.RequestAborted);
        return true;
    }
}
