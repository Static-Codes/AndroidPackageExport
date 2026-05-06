using System.Diagnostics;
using static AndroidPackageExport.Constants;
using static AndroidPackageExport.ExceptionEvents;
using static AndroidPackageExport.PSIHelper;

namespace AndroidPackageExport;
public class Functions 
{

    public static bool ADBInstalled() 
    {
        if (!OperatingSystem.IsLinux()) {
            return false;
        }

        return File.Exists(ADBPath);
    }

    public static bool GioInstalled() 
    {
        if (!OperatingSystem.IsLinux()) {
            return false;
        }

        return File.Exists(GioPath);
    }

    

    public static async Task<(List<string> output, List<string> error, uint exitCode, Exception? exception)> RunProcess(
        ProcessStartInfo psi, 
        DataReceivedEventHandler? outputHandler = null, 
        DataReceivedEventHandler? errorHandler = null
    )
    {
        List<string> output = [];
        List<string> error = [];
        
        using Process process = new() { StartInfo = psi };

        var outputDone = new TaskCompletionSource<bool>();
        var errorDone = new TaskCompletionSource<bool>();

        process.OutputDataReceived += (sender, e) => 
        {
            if (e.Data == null) 
            {
                outputDone.TrySetResult(true);
            }
            else 
            {
                lock (output) {
                    output.Add(e.Data.Trim());
                }
                outputHandler?.Invoke(sender, e);
            }
        };

        process.ErrorDataReceived += (sender, e) => 
        {
            if (e.Data == null) 
            {
                errorDone.TrySetResult(true);
            }
            else 
            {
                lock (error) {
                    error.Add(e.Data.Trim());
                }
                errorHandler?.Invoke(sender, e);
            }
        };

        Exception? exception = null;
        uint exitCode = 0;

        try 
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            // Allowing all handlers to finish processing before continuing.
            await Task.WhenAll(outputDone.Task, errorDone.Task);

            exitCode = (uint)process.ExitCode;
        }

        catch (Exception ex) {
            exception = ex;
            exitCode = 1;
            
            // Ensure tasks complete if start fails
            outputDone.TrySetResult(false);
            errorDone.TrySetResult(false);
        }

        return (output, error, exitCode, exception);
    }

    public static async Task<bool> DoADBDaemonCheck() 
    {
        if (!GioInstalled()) {
            throw new Exception($"Please ensure gio is installed at: {GioPath}");
        }

        var psi = GetADBDaemonCheckPSI();
        var (output, error, exitCode, exception) = await RunProcess(psi);

        if (exception != null) {
            throw exception;
        }
        
        return exitCode == 0;
    }

    private static async Task<string[]> GetAddressesOfConnectedDevices() 
    {
        var psi = GetDeviceCheckPSI();

        var (output, error, exitCode, exception) = await RunProcess(psi);

        if (exception != null) {
            throw exception;
        }

        var addressesFound = output.Where(line => line.Contains("default_location"));

        if (exitCode != 0 || !addressesFound.Any()) 
        {
            ThrowDeviceNotConnectedException();
        }

        // Each device was identified twice in each test.
        return [.. 
            output
            .ElementAt(0)
            .Split(' ')
            .Select(i => i.Replace("default_location=", ""))
            .Distinct()
        ];
    }

    /// <summary> 
    ///     Returns a Tuple(bool, string[]) <br/>
    /// 
    ///     Item1: deviceFound: <br/>
    ///         Returns a bool which represents the current device status. <br/>
    ///         true: A device is connected <br/>
    ///         false: A device is not connected <br/>
    /// 
    ///     Item2: deviceAddresses: <br/>
    ///         Returns a string array containing the MTP address(es) of the connected device(s).
    /// </summary>
    public static async Task<(bool connectionMade, string[] deviceAddresses)> CheckForDeviceConnection() 
    {
        var devicesFound = await GetAddressesOfConnectedDevices();

        if (devicesFound.Length == 0) {
            Console.WriteLine("No device is connected, devicesFound has a zero length.");
            return (false, []);
        }
        
        return (true, devicesFound);
    }

    
    public static async Task<List<string>> ListPackagesUsingADB() 
    {
        if (!ADBInstalled()) {
            Console.WriteLine($"Please ensure adb is installed at: {ADBPath}");
            Environment.Exit(1);
        }

        if (!GioInstalled()) {
            Console.WriteLine($"Please ensure gio is installed at: {GioPath}");
            Environment.Exit(1);
        }


        var (connectionMade, deviceAddresses) = await CheckForDeviceConnection();

        if (!connectionMade) {
            ThrowDeviceNotConnectedException();
        }

        if (deviceAddresses.Length > 1) {
            ThrowMultipleDevicesConnectedException(deviceAddresses);
        }

        var (output, error, _, exception) = await RunProcess(psi: GetPackageListPSI());

        if (exception != null) {
            throw exception;
        }

        return output
            .Where(line => line.Contains("package:"))
            .Select(line => line.Replace("package:", "").Trim())
            .ToList();
    }

}