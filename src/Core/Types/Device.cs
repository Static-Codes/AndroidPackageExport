namespace AndroidPackageExport.Core.Types;

using AndroidPackageExport.Core.Types.ADB;
using AndroidPackageExport.Core.Types.ADB.Wireless;
using AndroidPackageExport.Core.Types.Versioning;
using static AndroidPackageExport.Core.Helpers.InputHelper;

public class Device(string Name = "Unknown", AndroidOSVersion AndroidOSVersion = AndroidOSVersion.UNKNOWN) 
{
    public string Name { get; set; } = Name;

    /// <summary> 
    ///     The version of the Android OS running on the specified device. 
    /// </summary>
    public AndroidOSVersion AndroidOSVersion { get; set; } = AndroidOSVersion;
    

    /// <summary> 
    ///     The version of the Android API that was bundled with the version of Android running on the specified device. 
    /// </summary>
    public int AndroidAPILevel { get; set; } = (int)AndroidOSVersion;

    public ConnectionMethod ConnectionMethod { get; set; } = AskForConnectionMethod();
    /// <summary>
    ///     Holds the Device IP, Pairing Port, and Pairing Code. (If WIFI pairing is used.)
    /// </summary>
    public PairingInfo? WirelessPairingInfo { get; set; } = null;


    /// <summary>
    ///     
    /// </summary>
    
}