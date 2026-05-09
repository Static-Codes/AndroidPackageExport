namespace AndroidPackageExport.Core.Helpers;

using System.Diagnostics;
using static Global.Constants;

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
        var psi = new ProcessStartInfo() {
            FileName = ADBPath,
            Arguments = "devices",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Allowing ADB to access the RSA keys stored in $HOME/.adb (likely not needed, but useful for debugging.)
        psi.EnvironmentVariables["HOME"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // Allowing ADB to access other Environment Variables (required on some configurations)
        psi.EnvironmentVariables["PATH"] = Environment.GetEnvironmentVariable("PATH");

        return psi;
    }

    public static ProcessStartInfo GetDeviceConnectionPSI(string ipAddress, string port) 
    {
        return new() {
            FileName = "/bin/bash",
            Arguments = $"-c \"adb connect {ipAddress}:{port}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    public static ProcessStartInfo GetDevicePairingPSI(string ipAddress, string port, string code) 
    {
        return new() {
            FileName = "/bin/bash",
            Arguments = $"-c \"echo {code} | adb pair {ipAddress}:{port}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    public static ProcessStartInfo GetPackageListPSI(bool isUSB = true) 
    {
        return new()
        {
            FileName = "adb",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = isUSB switch
            {
                true => "-d shell pm list packages",
                false => "shell pm list packages"
            }
        };
    }

}