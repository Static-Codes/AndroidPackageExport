namespace AndroidPackageExport.Core.Common;

using System.Text.RegularExpressions;

public static partial class RegexPatterns 
{
    [GeneratedRegex(@"Successfully\spaired\sto\s(?<ip>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}):(?<port>[0-9]{1,5})\s+\[guid=(?<device_id>[^\]]+)")]
    public static partial Regex PairingRegex();

    [GeneratedRegex(@"([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})")] 
    public static partial Regex ValidateAddressIPV4();
}