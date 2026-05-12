namespace AndroidPackageExport.Core.Types;

using AndroidPackageExport.Core.Types.ADB;
using AndroidPackageExport.Core.Types.ADB.Wireless;
using AndroidPackageExport.Core.Types.Versioning;
using static AndroidPackageExport.Core.Helpers.InputHelper;

public class Device(string Name = "Unknown", AndroidOSVersion AndroidOSVersion = AndroidOSVersion.UNKNOWN, string? ID = null) 
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

    public ConnectionStatus ConnectionStatus { get; set; } = new(
        Connected: false, 
        Method: AskForConnectionMethod(), 
        Result: null
    );

    /// <summary>
    ///     Holds the Device IP, Pairing Port, and Pairing Code. (If WIFI pairing is used.)
    /// </summary>
    public PairingInfo? WirelessPairingInfo { get; set; } = null;


    /// <summary>
    ///    The identifier associated with the Android device connected via USB. 
    /// </summary>
    public string? ID { get; set; } = ID ?? "Unknown";
    
}