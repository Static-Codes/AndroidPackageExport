using System.Diagnostics.CodeAnalysis;

namespace AndroidPackageExport.Core.Types.ADB.Wireless;
public class PairingInfo(string IP, string Port, string Code)
{ 
    public string IP { get; set; } = IP;
    public string Port { get; set; } = Port;
    public string Code { get; set; } = Code;
}