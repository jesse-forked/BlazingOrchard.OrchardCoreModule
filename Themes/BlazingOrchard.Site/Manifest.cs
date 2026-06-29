using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Id = "BlazingOrchard.Site",
    Name = "Blazing Orchard Site",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "A regular Orchard Core site theme with content type, content menu, and site menu templates.",
    Tags = new[] { "Blog", "Bootstrap", "Liquid" }
)]
