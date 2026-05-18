using AndroidPackageExport.Core.Extensions;

namespace AndroidPackageExport.Core.Types.Packaging;

public class Package(string Name, PackageCategory Category, string? FriendlyName = null)
{
    public string? FriendlyName { get; set; } = FriendlyName;
    
    public string Name = Name;
    public PackageCategory Category = Category;

    
    public string? GetFriendlyName() 
    {
        if (Name == "android") {
            return "System Image (android)";
        }

        // Skipping PackageCategory "Settings"
        if (Category == PackageCategory.System) {
            return $"System Package ({Name})";
        }

        string? friendlyName = IconPatterns.Where(pattern => pattern.Name == Name)
                                           .FirstOrDefault()?
                                           .FriendlyName;

        if (friendlyName != null) {
            return friendlyName;
        }

        // Since some of the package names in IconPatterns, contain "com.android" this conditional is separated.
        // This ensures that if the package is present in IconPatterns, the known FriendlyName of that Package is used.
        if (Name.StartsWithAny(["com.android.", "com.google.android", "com.google.euiccpixel", ])) {
            return $"System Package ({Name})";
        }

        return null;


    }

    private readonly Package[] IconPatterns = [
        new("com.android.contacts", PackageCategory.Commercial, FriendlyName: "Contacts"),
        new("com.android.gallery3d", PackageCategory.Commercial, FriendlyName: "Gallery"),
        new("com.android.messaging", PackageCategory.Commercial, FriendlyName: "Messaging"),
        new("com.android.phone", PackageCategory.Commercial, FriendlyName: "Phone"),
        new("com.android.settings", PackageCategory.Commercial, FriendlyName: "Settings"),
    ];

    private readonly string[] NonIconPatterns = [
        "app.*.carrierconfig2",
        "app.*.AppCompatConfig",
        "app.*.config",
        "app.*.networklocation",
        "com.*.imsservice",
        "app.seamlessupdate.client",
        "android",
    ];
}