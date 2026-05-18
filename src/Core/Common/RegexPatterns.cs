namespace AndroidPackageExport.Core.Common;

using System.Text.RegularExpressions;

public static partial class RegexPatterns 
{
    [GeneratedRegex(@"Successfully\spaired\sto\s(?<ip>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}):(?<port>[0-9]{1,5})\s+\[guid=(?<device_id>[^\]]+)")]
    public static partial Regex PairingRegex();

    [GeneratedRegex(@"([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})")] 
    public static partial Regex ValidateAddressIPV4();

    [GeneratedRegex(@"(?<DeviceID>[0-9A-Z]{14})\s{1,}device|(?<IP>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}):(?<Port>[0-9]{1,5})\s{1,5}device\s{1,}product:\w{1,}\s{1,}model:(?<DeviceName>\w{1,})\s{1,}device:(?<Codename>\w{1,})\s{1,}transport_id:(?<TransportID>\w{1,})")]
    public static partial Regex ConnectionRegex();
}