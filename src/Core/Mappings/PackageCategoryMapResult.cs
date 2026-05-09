namespace AndroidPackageExport.Core.Mappings;

using AndroidPackageExport.Core.Types.Packaging;

public struct PackageCategoryMapResult
{
    public PackageCategory Result { get; set; }
    public PackageCategoryMapResult(string packageName) 
    {
        Result = true switch {
            true when packageName.StartsWith("app") => PackageCategory.Application,
            true when packageName.StartsWith("com.") => PackageCategory.Commercial,
            true when packageName.StartsWith("dev.") => PackageCategory.Developer,
            true when packageName.StartsWith("org.") => PackageCategory.Organization,
            true when packageName.StartsWith("android.") => PackageCategory.System,
            _ => PackageCategory.Other
        };
    }


    public PackageCategoryMapResult() {
        throw new Exception($"No value for parameter 'category'");
    }
}