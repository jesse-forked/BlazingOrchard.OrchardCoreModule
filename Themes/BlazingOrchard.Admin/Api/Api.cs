using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BlazingOrchard.Admin.Api;

public interface IApi
{
    IBlazingArea Blazing { get; }
}

public interface IBlazingArea
{
    IRestApi Rest { get; }
}

public interface IRestApi
{
    IAuthApi Auth { get; }
    IAppApi App { get; }
    ISiteApi Site { get; }
    INavigationApi Navigation { get; }
    IContentApi Content { get; }
    IFeaturesApi Features { get; }
    IRolesApi Roles { get; }
    IThemeApi Theme { get; }
}

public interface IAppApi
{
    Task<AppManifest?> GetManifestAsync();
}

public interface ISiteApi
{
    Task<SiteSettings> GetAsync();
    Task<SiteSettings?> UpdateAsync(SiteSettingsUpdate update);
}

public interface INavigationApi
{
    Task<NavigationMenu> GetAdminMenuAsync();
    Task<NavigationMenu> GetMenuAsync(string menuName);
}

public interface IContentApi
{
    IContentTypesApi Types { get; }
    IContentItemsApi Items { get; }
}

public interface IContentTypesApi
{
    Task<ContentType[]> ListAsync();
    Task<ContentType?> GetAsync(string contentType);
}

public interface IContentItemsApi
{
    Task<ContentItem?> GetByHandleAsync(string handle);
}

public interface IFeaturesApi
{
    Task<Feature[]> ListAsync();
}

public interface IRolesApi
{
    Task<Role[]> ListAsync();
}

public interface IAuthApi
{
    Task<AuthUser> MeAsync();
    Task<AuthUser?> LoginAsync(LoginModel model);
    Task<AuthUser> LogoutAsync();
}

public interface IThemeApi
{
    Task<BlazingThemeSettings> GetAsync();
}

public sealed class Api(HttpClient http) : IApi
{
    public IBlazingArea Blazing { get; } = new BlazingArea(http);
}

public sealed class BlazingArea(HttpClient http) : IBlazingArea
{
    public IRestApi Rest { get; } = new RestApi(http);
}

public sealed class RestApi(HttpClient http) : IRestApi
{
    public IAuthApi Auth { get; } = new AuthApi(http);
    public IAppApi App { get; } = new AppApi(http);
    public ISiteApi Site { get; } = new SiteApi(http);
    public INavigationApi Navigation { get; } = new NavigationApi(http);
    public IContentApi Content { get; } = new ContentApi(http);
    public IFeaturesApi Features { get; } = new FeaturesApi(http);
    public IRolesApi Roles { get; } = new RolesApi(http);
    public IThemeApi Theme { get; } = new ThemeApi(http);
}

public sealed class AuthApi(HttpClient http) : IAuthApi
{
    public async Task<AuthUser> MeAsync()
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, "api/blazing/auth/me")));
        if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength == 0)
        {
            return AuthUser.Anonymous;
        }

        return await response.Content.ReadFromJsonAsync<AuthUser>() ?? AuthUser.Anonymous;
    }

    public async Task<AuthUser?> LoginAsync(LoginModel model)
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Post, "api/blazing/auth/login")
        {
            Content = JsonContent.Create(model),
        }));

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthUser>()
            : null;
    }

    public async Task<AuthUser> LogoutAsync()
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Post, "api/blazing/auth/logout")));
        return await response.Content.ReadFromJsonAsync<AuthUser>() ?? AuthUser.Anonymous;
    }

    private static HttpRequestMessage WithCredentials(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }
}

public sealed class AppApi(HttpClient http) : IAppApi
{
    public async Task<AppManifest?> GetManifestAsync()
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, "api/blazing/app/manifest")));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AppManifest>()
            : null;
    }

    private static HttpRequestMessage WithCredentials(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }
}

public sealed class SiteApi(HttpClient http) : ISiteApi
{
    public async Task<SiteSettings> GetAsync()
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, "api/blazing/site")));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<SiteSettings>() ?? SiteSettings.Default
            : SiteSettings.Default;
    }

    public async Task<SiteSettings?> UpdateAsync(SiteSettingsUpdate update)
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Put, "api/blazing/site")
        {
            Content = JsonContent.Create(update),
        }));

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<SiteSettings>()
            : null;
    }

    private static HttpRequestMessage WithCredentials(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }
}

public sealed class NavigationApi(HttpClient http) : INavigationApi
{
    public async Task<NavigationMenu> GetAdminMenuAsync()
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, "api/blazing/navigation/admin")));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<NavigationMenu>() ?? NavigationMenu.Empty("admin")
            : NavigationMenu.Empty("admin");
    }

    public async Task<NavigationMenu> GetMenuAsync(string menuName)
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, $"api/blazing/navigation/menus/{Uri.EscapeDataString(menuName)}")));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<NavigationMenu>() ?? NavigationMenu.Empty(menuName)
            : NavigationMenu.Empty(menuName);
    }

    private static HttpRequestMessage WithCredentials(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }
}

public sealed class ContentApi(HttpClient http) : IContentApi
{
    public IContentTypesApi Types { get; } = new ContentTypesApi(http);
    public IContentItemsApi Items { get; } = new ContentItemsApi(http);
}

public sealed class ContentTypesApi(HttpClient http) : IContentTypesApi
{
    public async Task<ContentType[]> ListAsync()
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, "api/blazing/content-types")));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ContentType[]>() ?? []
            : [];
    }

    public async Task<ContentType?> GetAsync(string contentType)
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, $"api/blazing/content-types/{Uri.EscapeDataString(contentType)}")));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ContentType>()
            : null;
    }

    private static HttpRequestMessage WithCredentials(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }
}

public sealed class ContentItemsApi(HttpClient http) : IContentItemsApi
{
    public async Task<ContentItem?> GetByHandleAsync(string handle)
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, $"api/blazing/content-items/by-handle/{Uri.EscapeDataString(handle)}")));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ContentItem>()
            : null;
    }

    private static HttpRequestMessage WithCredentials(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }
}

public sealed class FeaturesApi(HttpClient http) : IFeaturesApi
{
    public async Task<Feature[]> ListAsync()
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, "api/blazing/features")));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<Feature[]>() ?? []
            : [];
    }

    private static HttpRequestMessage WithCredentials(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }
}

public sealed class RolesApi(HttpClient http) : IRolesApi
{
    public async Task<Role[]> ListAsync()
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, "api/blazing/roles")));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<Role[]>() ?? []
            : [];
    }

    private static HttpRequestMessage WithCredentials(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }
}

public sealed class ThemeApi(HttpClient http) : IThemeApi
{
    public async Task<BlazingThemeSettings> GetAsync()
    {
        using var response = await http.SendAsync(WithCredentials(new(HttpMethod.Get, "api/blazing/theme")));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<BlazingThemeSettings>() ?? BlazingThemeSettings.Default
            : BlazingThemeSettings.Default;
    }

    private static HttpRequestMessage WithCredentials(HttpRequestMessage request)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return request;
    }
}

public sealed record LoginModel(string UserName, string Password, bool RememberMe);

public sealed record AuthUser(bool IsAuthenticated, string? UserName, string[] Roles)
{
    public static AuthUser Anonymous { get; } = new(false, null, []);
}

public sealed record AppManifest(
    Tenant Tenant,
    SiteSettings Site,
    AdminDescriptor Admin,
    int FeatureSerialNumber,
    string FeatureHash,
    Feature[] Features,
    NavigationMenu AdminMenu);

public sealed record Tenant(
    string Name,
    string TenantId,
    string State,
    string? RequestUrlHost,
    string[] RequestUrlHosts,
    string? RequestUrlPrefix);

public sealed record AdminDescriptor(string BasePath);

public sealed record SiteSettings(
    string SiteName,
    string PageTitleFormat,
    string BaseUrl,
    string TimeZoneId,
    string Calendar,
    int PageSize,
    int MaxPageSize,
    int MaxPagedCount)
{
    public static SiteSettings Default { get; } = new(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        10,
        100,
        0);
}

public sealed record SiteSettingsUpdate(
    string SiteName,
    string PageTitleFormat,
    string BaseUrl,
    string TimeZoneId,
    string Calendar,
    int PageSize,
    int MaxPageSize,
    int MaxPagedCount);

public sealed record NavigationMenu(string Name, NavigationItem[] Items)
{
    public static NavigationMenu Empty(string name) => new(name, []);
}

public sealed record NavigationItem(
    string Text,
    string? Id,
    string? Href,
    string? Url,
    string? Target,
    string? Position,
    string? Icon,
    string[] Classes,
    NavigationItem[] Items);

public sealed record ContentType(
    string Name,
    string DisplayName,
    JsonObject Settings,
    ContentTypePart[] Parts);

public sealed record ContentTypePart(
    string Name,
    JsonObject Settings,
    ContentPart Part);

public sealed record ContentPart(
    string Name,
    JsonObject Settings,
    ContentPartField[] Fields);

public sealed record ContentPartField(
    string Name,
    JsonObject Settings,
    ContentField Field);

public sealed record ContentField(string Name);

public sealed record ContentItem(
    string ContentItemId,
    string ContentItemVersionId,
    string ContentType,
    string DisplayText,
    bool Published,
    bool Latest,
    DateTime? CreatedUtc,
    DateTime? ModifiedUtc,
    DateTime? PublishedUtc,
    string Owner,
    string Author,
    JsonElement Content);

public sealed record Feature(
    string Id,
    string Name,
    string Category,
    string Description,
    string ExtensionId,
    string[] Dependencies,
    bool AlwaysEnabled);

public sealed record Role(string Name, string Description, bool IsAdmin, bool IsSystem);

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
