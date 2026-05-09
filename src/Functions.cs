using System.Diagnostics;
using AndroidPackageExport.Core.Mappings;
using AndroidPackageExport.Core.Types;
using AndroidPackageExport.Core.Types.ADB;
using AndroidPackageExport.Core.Types.ADB.Wireless;
using AndroidPackageExport.Core.Types.Packaging;
using static AndroidPackageExport.Core.Common.InputValidation;
using static AndroidPackageExport.Core.Helpers.InputHelper;
using static AndroidPackageExport.Core.Helpers.PSIHelper;
using static AndroidPackageExport.Global.Constants;
using static AndroidPackageExport.Global.Logging;

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

    private static async Task<(bool connected, List<string> output, List<string> error)> CheckForDeviceConnection(string? ip = null, string? port = null) 
    {
        string? address = null;

        if (ip != null && port != null) {
            address = $"{ip}:{port}";
        }

        var psi = GetDeviceCheckPSI();

        var processResult = await RunProcessAsync(psi);

        if (processResult.exception != null) {
            throw processResult.exception;
        }

        # if DEBUG
            foreach (var line in processResult.output) { WriteDebugMessage(line); }
            foreach (var line in processResult.error) { WriteDebugMessage(line); }
        #endif

        var desiredSubstring = string.IsNullOrEmpty(address) switch {
            true => "      device",
            false => $"{address}      device"
        };

        var devicesFound = processResult.output.Select(line => line.Contains(desiredSubstring));

        if (processResult.exitCode != 0 || !devicesFound.Any()) 
        {
            WriteInformation(ConnectionSection);
            WriteErrorMessage(
                message: $"No connected devices were detected.",
                exit: true,
                exitCode: 1
            );
        }

        return (true, processResult.output, processResult.error);
    }
    

    private static async Task<(Device device, ProcessResult packageRetrievalResult)> GetPackagesOverUSB(Device device) 
    {
        if (!ADBInstalled()) {
            Console.WriteLine($"Please ensure ADB is installed at: {ADBPath}");
            Environment.Exit(1);
        }

        var (connectionMade, _, _) = await CheckForDeviceConnection();

        if (!connectionMade) {
            WriteErrorMessage(
                message: $"No connected devices were detected.\n\n{ConnectionSection}",
                exit: true,
                exitCode: 1
            );
        }

        var processResult = await RunProcessAsync(psi: GetPackageListPSI());

        if (processResult.exception != null) {
            throw processResult.exception;
        }

        return (device, processResult);
    }

    private static async Task<(Device device, ProcessResult packageRetrievalResult)> GetPackagesOverWIFI(Device device) 
    {
        if (!ADBInstalled()) {
            WriteInformation($"ADB is expected to be installed at: {ADBPath}");
            WriteErrorMessage("ADB was not located, it is required to continue.", exit: true, exitCode: 1);
        }

        // TODO: Add LAN scanning? (this may be risky due to CVEs found in Android 10-14)
        var deviceIP = AskForInput("Device IP: ");

        # region Pairing Operations

        var pairingInfo = PromptForPairingInfo(deviceIP);

        // Validating the provided input.
        DoPortValidation(ref pairingInfo);
        DoCodeValidation(ref pairingInfo);
        
        // Assigning the validated input to the Device object.
        device.WirelessPairingInfo = pairingInfo;

        var pairingResult = await RunProcessAsync(
            psi: GetDevicePairingPSI(pairingInfo.IP, pairingInfo.Port, pairingInfo.Code)
        );

        if (pairingResult.exitCode != 0) 
        {
            WriteWarningMessage("Unable to pair the current system to the device at the specified address.");
            WriteErrorMessage(
                message: $"The process returned a non-zero status code of {pairingResult.exitCode}.",
                exit: true,
                exitCode: 1
            );
        }

        // Adding a 2 second delay
        WriteSuccessMessage("Pairing successful, waiting two seconds for on-device processing.");
        await Task.Delay(2000);

        #endregion


        # region Connection Operations

        var (_, port) = PromptForConnectionInfo(deviceIP);

        WriteInformation($"Attempting to connect the current system to device using address: {deviceIP}:{port}");

        var connectionProcess = await RunProcessAsync(
            psi: GetDeviceConnectionPSI(deviceIP, port)
        );

        if (connectionProcess.exitCode != 0) {
            WriteErrorMessage("Unable to connect the current system to the device at the specified address.");
            WriteErrorMessage(
                message: $"The process returned a non-zero status code of {connectionProcess.exitCode}.",
                exit: true,
                exitCode: 1
            );
        }

        WriteInformation("Waiting five seconds for any remaining ADB requests to process.");
        await Task.Delay(5000);

        var (connectionMade, _, _) = await CheckForDeviceConnection(deviceIP, port);

        if (!connectionMade) 
        {
            WriteWarningMessage("Unable to connect the current system to the device at the specified address.");
            WriteErrorMessage(
                message: "ADB couldnt detect any connected devices, please clear the pairing and try again.",
                exit: true,
                exitCode: 1
            );
        }



        WriteSuccessMessage($"Connected to a device at {deviceIP}:{port}");
        WriteWarningMessage(
            "If you didn't receive a notification that wireless debugging was connected, please clear the pairing and try again."
        );

        WriteSuccessMessage("Waiting two seconds for any remaining ADB requests to process.");
        await Task.Delay(2000);


        # endregion


        # region Package Retrieval Operations

        WriteInformation("Starting retrieval operations, please wait...");
        await Task.Delay(1000);

        var packageRetrievalResult = await RunProcessAsync(
            psi: GetPackageListPSI(isUSB: false)
        );


        if (packageRetrievalResult.exception != null) {
            throw packageRetrievalResult.exception;
        }

        // Package retrieval operations end here
        return (device, packageRetrievalResult);
        
        # endregion
    }
    
    /// <summary>
    ///     Returns a Tuple(Device, ProcessResult) <br/>
    ///  
    ///     When using WIFI: The device object passed as a parameter is updated. <br/> 
    ///     When using USB: The device object returned is unmodified. <br/>
    /// 
    ///     Both methods will return a ProcessResult if successful, and exit out if an error is present.
    /// </summary>
    public static async Task<(Device device, ProcessResult packageRetrievalResult)> RunPackageRetrieval(Device device) 
    {
        return device.ConnectionMethod switch {
            ConnectionMethod.USB => await GetPackagesOverUSB(device),
            ConnectionMethod.WIFI => await GetPackagesOverWIFI(device),
            _ => throw new InvalidOperationException("Invalid connection method selected, please try again.")
        };
    }

    public static Dictionary<PackageCategory, List<string>> ParsePackageProcessResult(ProcessResult packageRetrievalResult) 
    {
        string[] packageNames = [.. packageRetrievalResult.output
            .Where(line => line.Contains("package:"))
            .Select(line => line.Replace("package:", "").Trim())];

        WriteSuccessMessage($"Located {packageNames.Length} packages.");

        var packageCategoryInfo = new Dictionary<PackageCategory, List<string>>();
        // var packages = new List<Package>();

        var categories = typeof(PackageCategory).GetEnumNames().Select(n => Enum.Parse<PackageCategory>(n));

        if (!categories.Any()) 
        {
            WriteErrorMessage(
                message: "Unable to deserialize the internal Package Categories, required to for this utility to operate.",
                exit: true,
                exitCode: 1
            );
        }

        foreach (var category in categories){
            packageCategoryInfo.Add(category, []);
        }

        var finalFilePath = Path.Combine(Environment.CurrentDirectory, "packages.json");

        try 
        {
            for (int i = 0; i < packageNames.Length; i++) {
                var packageName = packageNames[i];
                var packageCategory = new PackageCategoryMapResult(packageName).Result;
                packageCategoryInfo[packageCategory].Add(packageName);
            }

            WriteInformation($"Parsed {packageCategoryInfo.Count} packages.");
        }

        catch (Exception ex) {
            WriteWarningMessage("Unable to parse the processResult object containing your device's package data.");
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }

        return packageCategoryInfo;

    }

    public static (string deviceIP, string port) PromptForConnectionInfo(string deviceIP) 
    {
        WriteInformation("The port to be used for connection will differ from the pairing port.");
        WriteInformation("Please refresh the 'Wireless debugging' tab and enter the updated values below.");

        return (
            deviceIP,
            AskForInput("Debug Service Port: ")
        );
    }

    public static PairingInfo PromptForPairingInfo(string deviceIP) 
    {
        Console.WriteLine($"To locate your pairing code, please go to:\n{WIFISetting}\n");

        return new(
            deviceIP,
            AskForInput("Pairing Service Port: "),
            AskForInput("Pairing Code: ")
        );
    }

    public static async Task<ProcessResult> RunProcessAsync(
        ProcessStartInfo psi, 
        string? inputArg = null,
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

            if (inputArg != null) 
            {
                using StreamWriter writer = process.StandardInput;

                // Determining whether the calling process' STDInput Stream supports writing in the current context.
                if (!writer.BaseStream.CanWrite) 
                {
                    WriteErrorMessage(
                        message: "Unable to enter the input provided, please try again over USB.", 
                        exitCode: 1, 
                        exit: true
                    );
                }
                await writer.WriteAsync(inputArg);

                await process.WaitForExitAsync();

                // Allowing all handlers to finish processing before continuing.
                await Task.WhenAll(outputDone.Task, errorDone.Task);

                exitCode = (uint)process.ExitCode;
            }

            else 
            {
                await process.WaitForExitAsync();
                exitCode = (uint)process.ExitCode;
            }
        }

        catch (Exception ex) {
            exception = ex;
            exitCode = 1;
            
            // Ensuring the outstanding tasks complete if the process.Start() fails.
            outputDone.TrySetResult(false);
            errorDone.TrySetResult(false);
        }

        return new ProcessResult(output, error, exitCode, exception);
    }

    public static async Task<ProcessResult> RunProcessSimpleAsync(
        ProcessStartInfo psi 
    )
    {
        List<string> output = [];
        List<string> error = [];
        
        using Process process = new() { StartInfo = psi };

        process.OutputDataReceived += (sender, e) => 
        {
            if (e.Data != null) 
            {
                lock (output) {
                    output.Add(e.Data.Trim());
                }
            }
        };

        process.ErrorDataReceived += (sender, e) => 
        {
            if (e.Data != null) 
            {
                lock (error) {
                    error.Add(e.Data.Trim());
                }
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
            exitCode = (uint)process.ExitCode;
        }

        catch (Exception ex) {
            exception = ex;
            exitCode = 1;
        }

        return new ProcessResult(output, error, exitCode, exception);
    }

    public static bool TryGetDeviceConnection() {
        return true;
    }

}