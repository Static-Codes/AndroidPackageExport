namespace AndroidPackageExport;

using System.Diagnostics;

public class PSIHelper 
{

    public static ProcessStartInfo GetADBDaemonCheckPSI() 
    {
        return new() {
            FileName = "/bin/bash",
            Arguments = "-c \"ps aux | grep adb -L | grep -v grep\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }
    
    public static ProcessStartInfo GetDeviceCheckPSI() 
    {
        return new() {
            FileName = "/bin/bash",
            Arguments = "-c \"gio mount -li | grep default_location=mtp:// | xargs\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    public static ProcessStartInfo GetPackageListPSI() 
    {
        return new() {
            FileName = "adb",
            Arguments = "-d shell pm list packages",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

}