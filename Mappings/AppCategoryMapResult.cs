using AndroidAppLister.Types;

namespace AndroidAppLister.Mapping;

public struct AppCategoryMapping
{
    public AppCategory Result { get; set; }
    public AppCategoryMapping(string packageName) 
    {
        Result = true switch {
            true when packageName.StartsWith("app") => AppCategory.Application,
            true when packageName.StartsWith("com.") => AppCategory.Commercial,
            true when packageName.StartsWith("dev.") => AppCategory.Developer,
            true when packageName.StartsWith("org.") => AppCategory.Organization,
            true when packageName.StartsWith("android.") => AppCategory.System,
            _ => AppCategory.Other
        };
    }


    public AppCategoryMapping() {
        throw new Exception($"No value for parameter 'category'");
    }
}