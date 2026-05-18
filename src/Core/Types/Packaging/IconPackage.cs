namespace AndroidPackageExport.Core.Types.Packaging;


/// <summary> 
///     Similarly to AuthorizedDevice, IconPackage serves as a semantical differentiator. <br/>
///     The presence of an IconPackage indicates that the connected device's launcher displays an icon for this Package. <br/>
///     The underlying Package object can be accessed through IconPackage.Package
/// </summary> 
public class IconPackage(Package package)
{
    public Package Package = package;
}