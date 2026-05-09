using AndroidPackageExport.Core.Types.Packaging;

namespace AndroidPackageExport.Core.Types;

public class Package(string Name, PackageCategory Category)
{
    public string Name = Name;
    public PackageCategory Category = Category;
}