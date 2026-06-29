using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Blazing Server",
    Author = "Blazing",
    Website = "https://blazing.local",
    Version = "1.0.0",
    Description = "Provides Blazing tenant APIs and server-side Orchard integrations.",
    Category = "Blazing"
)]

[assembly: Feature(
    Id = "Blazing",
    Name = "Blazing Server",
    Description = "Provides Blazing tenant APIs for authentication, theme settings, and app configuration.",
    Category = "Blazing",
    Dependencies = ["OrchardCore.Settings", "OrchardCore.Users"],
    IsAlwaysEnabled = true
)]
