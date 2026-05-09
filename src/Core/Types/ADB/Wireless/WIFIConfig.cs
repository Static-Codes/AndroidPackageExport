namespace AndroidPackageExport.Core.Types.ADB.Wireless;

public class WIFIConfig(string DeviceIP, PairingInfo PairingInfo, string ConnectionPort)
{
    public string DeviceIP { get; set; } = DeviceIP;
    public PairingInfo PairingInfo { get; set; } = PairingInfo;
    public string ConnectionPort { get; set; } = ConnectionPort;
}