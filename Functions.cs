using System.Diagnostics;
using System.Threading.Tasks;

namespace AndroidAppLister;

public class Functions 
{

    public static bool ADBInstalled() 
    {
        if (!OperatingSystem.IsLinux()) {
            return false;
        }

        return File.Exists("/usr/bin/adb");
    }

    public static async Task<List<string>> ListPackagesUsingADB() {
        if (!ADBInstalled()) {
            Console.WriteLine("Please ensure adb is installed at /usr/bin/adb");
            Environment.Exit(1);
        }

        var psi = new ProcessStartInfo() {
            FileName = "adb",
            Arguments = "-d shell pm list packages",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        List<string> output = [];

        using var process = new Process();
        process.StartInfo = psi;

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data)) {
                string packageName = e.Data.Replace("package:", "").Trim();
                if (!string.IsNullOrEmpty(packageName)) {
                    lock (output) {
                        output.Add(packageName);
                    }
                }
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data)) {
                Console.WriteLine($"Error: {e.Data}");
            }
        };

        try 
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to execute adb: {ex.Message}");
            throw;
        }

        return output;
    }

}