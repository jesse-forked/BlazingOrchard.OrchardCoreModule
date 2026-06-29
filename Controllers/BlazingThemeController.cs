using Microsoft.AspNetCore.Mvc;
using OrchardCore.Settings;
using System.Text.Json;

namespace BlazingOrchard.Controllers;

[ApiController]
[IgnoreAntiforgeryToken]
[Route("api/blazing/theme")]
public sealed class BlazingThemeController(ISiteService siteService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var site = await siteService.GetSiteSettingsAsync();
        var settings = site.Properties["BlazingTheme"]?.Deserialize<BlazingThemeSettings>(JsonSerializerOptions.Web)
            ?? BlazingThemeSettings.Default;

        return Ok(settings);
    }
}

public sealed record BlazingThemeSettings(string RadzenTheme, Dictionary<string, string> Tokens)
{
    public static BlazingThemeSettings Default { get; } = new(
        "material-base",
        new Dictionary<string, string>
        {
            ["primary"] = "#2f6f4e",
            ["secondary"] = "#6d5d3f",
            ["surface"] = "#ffffff",
            ["background"] = "#f7f8f6",
            ["radius"] = "6px",
        });
}
