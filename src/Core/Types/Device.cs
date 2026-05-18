namespace AndroidPackageExport.Core.Types;

using ADB.Wireless;
using Versioning;

using static Helpers.InputHelper;
using static ADB.Connection;

public class Device(string Name = "Unknown", AndroidOSVersion AndroidOSVersion = AndroidOSVersion.UNKNOWN, ConnectionStatus? ConnectionStatus = null, PairingInfo? WirelessPairingInfo = null, string? ID = null, string? Codename = null) 
{
    /// <summary> 
    ///     The name of the connected device, as is reported by the Android Debug Bridge 
    /// </summary>
    public string Name { get; set; } = Name;

    /// <summary> 
    ///     The version of the Android OS running on the specified device. 
    /// </summary>
    public AndroidOSVersion AndroidOSVersion { get; set; } = AndroidOSVersion;
    

    /// <summary> 
    ///     The version of the Android API that was bundled with the version of Android running on the specified device. 
    /// </summary>
    public int AndroidAPILevel { get; set; } = (int)AndroidOSVersion;

    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus ?? new(
        Connected: false, 
        Method: AskForConnectionMethod(),
        Output: null, 
        Result: null
    );

    /// <summary> 
    ///     Holds the Device IP, Pairing Port, and Pairing Code. (If WIFI pairing is used.) 
    /// </summary>
    public PairingInfo? WirelessPairingInfo { get; set; } = WirelessPairingInfo ?? null;


    /// <summary> 
    ///     The identifier associated with the Android device. <br/> 
    ///     When using USB Pairing, this identifier is a 14 digit alphanumeric string. <br/> 
    ///     When using WIFI Pairing, this identifier is an IP:Port combination. <br/> 
    /// </summary>
    public string? ID { get; set; } = ID ?? "Unknown";

    public string Codename { get; set; } = Codename ?? "Unknown";
    
}

/// <summary> 
///     AuthorizedDevice serves as a semantic differentiator from the standard Device object. <br/>
///     This struct contains no additional fields, taking only a device object as a parameter. <br/>
///     When a device is "Authorized" in the codebase, the only action that is made is an object cast. <br/>
///     Whitelist.AuthorizeDevice(ref device) returns this casted object. To explicitly show the device is "Authorized". <br/>
/// </summary>
public struct AuthorizedDevice(Device device)
{
    public Device Device { get; set; } = device;
}