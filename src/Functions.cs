namespace AndroidPackageExport;

using System.Diagnostics;
using Core.Mappings;
using Core.Types;
using Core.Types.ADB.Wireless;
using Core.Types.Packaging;
using static Core.Common.InputValidation;
using static Core.Common.RegexPatterns;
using static Core.Helpers.InputHelper;
using static Core.Helpers.PSIHelper;
using static Core.Types.ADB.Connection;
using static Global.Constants;
using static Global.Logging;
using System.Text.RegularExpressions;

public class Functions 
{   

    /// <summary> Checks for the existence of the ADB binary at /usr/bin/adb </summary>
    public static bool ADBInstalled() 
    {
        if (!OperatingSystem.IsLinux()) {
            return false;
        }

        return File.Exists(ADBPath);
    }


    private static void AddMemberToConnectionOutput(ref object? existingObject, string memberName, string value) 
    {
        if (existingObject == null) {
            existingObject = new ConnectionOutput();
        }

        else if (existingObject is not ConnectionOutput) {
            throw new ArgumentException($"Expected an instance of {nameof(ConnectionOutput)}", nameof(existingObject));
        }

        var propertyInfo = 
            typeof(ConnectionOutput).GetProperty(memberName) ?? 
            throw new ArgumentNullException(memberName, $"{memberName} is not a valid property of {nameof(ConnectionOutput)}.");

        // Setting the new value for the member of the existing object.
        propertyInfo.SetValue(existingObject, value);
        
        ShowCaptureGroupInfo(memberName);
    }
    
    
    /// <summary> 
    ///     Performs a device connection check by executing "/usr/bin/adb devices -l". <br/>
    /// 
    ///     The output from this command is parsed with a call to DoDeviceConnectionRegex(). <br/> 
    ///
    ///     The result of the call to DoDeviceConnectionRegex() will be of the type ConnectionStatus.
    /// </summary>
    public static async Task<ConnectionStatus> CheckForDeviceConnection() 
    {
        var psi = GetDeviceCheckPSI();

        var processResult = await RunProcessAsync(psi);

        if (processResult.exception != null) {
            throw processResult.exception;
        }

        # if DEBUG
            foreach (var line in processResult.output) { WriteDebugMessage(line); }
            foreach (var line in processResult.error) { WriteDebugMessage(line); }
        #endif

        return DoDeviceConnectionRegex(processResult);
    }


    /// <summary> 
    ///     Requires the device IP, debug service port, and the parsed capture groups from DoDeviceConnectionRegex().
    ///     These values will be used for the initialization of a new object of the type ConnectionOutput.
    /// </summary>
    private static ConnectionOutput CreateConnectionOutput(string deviceIP, string debugPort, IEnumerable<Group> captureGroups) {
        ConnectionOutput output = new(
            DeviceIP: deviceIP,
            DebugPort: debugPort
        );

        var groupIndexesProcessed = 4;
        var remainingGroupsCount = captureGroups.Count() - groupIndexesProcessed;

        var remainingGroups = captureGroups.TakeLast(remainingGroupsCount);
        

        foreach (var remainingGroup in remainingGroups) 
        {
            if (!remainingGroup.Success) {
                WriteInformation($"Skipping capture group: {remainingGroup.Name}");
                continue;
            }

            var tempOutput = (object)output!;
            AddMemberToConnectionOutput(ref tempOutput, remainingGroup.Name, remainingGroup.Captures[0].Value.Replace("_", " "));
            output = (ConnectionOutput)tempOutput!;
            continue;
        }

        return output;
    }


    /// <summary>
    ///     Performs a match check using a regex pattern that will handle both WIFI and USB connections through ADB. <br/>
    /// 
    ///     Will exit if more than one device is connected. <br/>
    ///     
    ///     Returns a ConnectionStatus object, regardless of connection status (unless the condition above is met first).
    /// </summary> 
    private static ConnectionStatus DoDeviceConnectionRegex(ProcessResult connectionResult) 
    {   
        if (connectionResult.output.Count == 0)
        {
            WriteInformation(ConnectionSection);
            WriteErrorMessage(
                message: $"No connected devices were detected.",
                exit: true,
                exitCode: 1
            );
        }

        var connectionGroups = connectionResult.output
            .Where(line => line != "List of devices attached")
            .SelectMany(line => ConnectionRegex().Matches(line))
            .Where(match => match.Success)
            .Select(match => match.Groups);

        
        if (!connectionGroups.Any()) 
        {
            return new ConnectionStatus(
                Connected: false, 
                Method: null,
                Output: null,
                Result: connectionResult
            );
        }

        var captureGroups = connectionGroups.SelectMany(group => group.Values);

        // Capture Groups Legend/Key (Pulled from RegexPatterns.ConnectionRegex())
        // Index 0 -> Full line associated with the match. 
        // Index 1 -> "DeviceID"
        // Index 2 -> "DeviceIP"
        // Index 3 -> "Port"
        // Index 4 -> "DeviceName"
        // Index 5 -> "Codename"
        // Index 6 -> "TransportID"

        var usbConnection = captureGroups.ElementAt(1).Success;
        
        var wifiConnection = captureGroups.ElementAt(2).Success && 
                             captureGroups.ElementAt(3).Success;

        if (usbConnection) 
        {
            var deviceID = captureGroups.ElementAt(1).Captures[0].Value;
            return new (
                Connected: true, 
                Method: ConnectionMethod.USB,
                Output: new ConnectionOutput(deviceID), // Only the 14 digit alphanumeric ID is used for USB Pairing.
                Result: connectionResult,
                Identifier: deviceID
            );
        }

        if (wifiConnection) 
        {   
            var deviceIP = captureGroups.ElementAt(2).Captures[0].Value;
            var debugPort = captureGroups.ElementAt(3).Captures[0].Value;
            
            var output = CreateConnectionOutput(deviceIP, debugPort, captureGroups);
            var deviceAddress = $"{deviceIP}:{debugPort}";

            return new (
                Connected: true, 
                Method: ConnectionMethod.WIFI,
                Output: output,
                Result: connectionResult,
                Identifier: deviceAddress
            );
        }

        return new ConnectionStatus(
            Connected: false, 
            Method: null,
            Output: null,
            Result: connectionResult
        );
    }

    

    /// <summary>
    ///     Performs a check using "adb devices -l" to ensure a single device is present.
    ///     If no device is found, or more than one device is connected at the same time, an exception is thrown.
    ///     Returns a tuple with an updated device object, alongside a ProcessResult object detailing the retrieval result.
    /// </summary>  
    private static async Task<(Device device, ProcessResult packageRetrievalResult)> GetPackagesOverUSB(Device device) 
    {
        var connectionStatus = await CheckForDeviceConnection();

        if (!connectionStatus.Connected) {
            WriteErrorMessage(
                message: $"No connected devices were detected.\n\n{ConnectionSection}",
                exit: true,
                exitCode: 1
            );
        }

        device.ID = connectionStatus.Identifier; 
        var processResult = await RunProcessAsync(psi: GetPackageListPSI());

        if (processResult.exception != null) {
            throw processResult.exception;
        }

        return (device, processResult);
    }


    /// <summary>
    ///     Performs pairing and connection operations with a call to PairAndConnectDeviceOverWifi(device). <br/>
    ///     Once a connection is made, "pm list packages" is executed on the connected device over the adb shell. <br/>
    ///     The device is object is updated with the pairing and connection operations. <br/>
    ///     This updated object is returned in a tuple alongside a ProcessResult object detailing the retrieval result.
    /// </summary>
    private static async Task<(Device device, ProcessResult packageRetrievalResult)> GetPackagesOverWIFI(Device device) 
    {
        // Performing the device pairing (if needed) and connection.
        device = await PairAndConnectDeviceOverWifi(device);

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
    }


    /// <summary>
    ///     Returns a Tuple(Device, ProcessResult) <br/>
    ///  
    ///     When using WIFI: The device object passed as a parameter is updated. <br/> 
    ///     When using USB: The device object returned is unmodified. <br/>
    /// 
    ///     Both methods will return a ProcessResult if successful, and exit out if an error is present. <br/>
    /// 
    ///     Additionally, this method will return an updated device object.
    /// </summary>
    public static async Task<(Device device, ProcessResult packageRetrievalResult)> RunPackageRetrieval(Device device) 
    {
        return device.ConnectionStatus.Method switch {
            ConnectionMethod.USB => await GetPackagesOverUSB(device),
            ConnectionMethod.WIFI => await GetPackagesOverWIFI(device),
            _ => throw new InvalidOperationException("Invalid connection method selected, please try again.")
        };
    }
    



    /// <summary>
    ///     Pairs a device over WiFi (if it is not already paired). <br/>
    ///     
    ///     If this pairing fails, execution haults immediately. <br/>
    /// 
    ///     Once the device is paired, an attempt is made to connect to the device over the Debug Service Port.
    /// 
    ///     Both pairing and connection operations will update values from the associated device object.
    /// </summary>
    private static async Task<Device> PairAndConnectDeviceOverWifi(Device device) 
    {
        #region Pairing Operations

        PairingInfo pairingInfo;
        var pairingNeeded = string.IsNullOrEmpty(device.ID);

        // If pairing is required the user is prompted for the device IP.
        // If pairing is not required, the device IP is pulled from the device Identifier (IP:Port)
        string deviceIP = pairingNeeded ? AskForInput("Device IP: ") : device.ID!.Split(":").ElementAt(0); 

        if (device.ID == null || !device.ID.Contains(':')) 
        {
            deviceIP = AskForInput("Device IP: "); 
            pairingInfo = PromptForPairingInfo(deviceIP);
        
            // Validating the provided input.
            DoPortValidation(ref pairingInfo);
            DoCodeValidation(ref pairingInfo);
            
            // Assigning the validated input (assuming execution didnt hault due to invalid input).
            device.WirelessPairingInfo = pairingInfo;

            var pairingResult = await RunProcessAsync
            (
                psi: GetDevicePairingPSI(
                    device.WirelessPairingInfo.IP, 
                    device.WirelessPairingInfo.Port, 
                    device.WirelessPairingInfo.Code
                )
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
        }

        #endregion



        # region Connection Operations

        string debugPort;
        
        // If a pre-existing device connection is present, that information is used to make the bridge connection.
        if (device.ConnectionStatus.Connected && device.ID != null) 
        {
            var addressParts = device.ID.Split(':');
            deviceIP = addressParts.ElementAt(0);
            debugPort = addressParts.ElementAt(1);
        }

        
        // Otherwise the user is prompted for a debug service port to make the connection via ADB.
        else {
            debugPort = PromptForConnectionInfo(deviceIP).port;
        }

        var deviceName = device.ConnectionStatus.Output?.DeviceName ?? "device";
        WriteInformation(
            $"Attempting to connect the current system to the {deviceName} at {deviceIP}:{debugPort}"
        );

        var connectionProcess = await RunProcessAsync(
            psi: GetDeviceConnectionPSI(deviceIP, debugPort)
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

        var connectionStatus = await CheckForDeviceConnection();

        if (!connectionStatus.Connected) 
        {
            WriteWarningMessage("Unable to connect the current system to the device at the specified address.");
            WriteErrorMessage(
                message: "ADB couldnt detect any connected devices, please clear the pairing and try again.",
                exit: true,
                exitCode: 1
            );
        }

        device.ID = connectionStatus.Identifier;
        
        device.ConnectionStatus = connectionStatus;


        WriteSuccessMessage($"Connected to a {connectionStatus.Output?.DeviceName ?? "device"} at address: {deviceIP}:{debugPort}");
        WriteWarningMessage(
            "If you didn't receive a notification that wireless debugging was connected, please clear the pairing and try again."
        );

        WriteSuccessMessage("Waiting two seconds for any remaining ADB requests to process.");
        await Task.Delay(2000);

        return device;

        # endregion
    }
    


    /// <summary>
    ///     Parses the ProcessResult object returned by RunPackageRetrieval(). <br/>
    ///     Returns a Dictionary with a key of the type PackageCategory and a value of the type string list.
    /// </summary>
    public static Dictionary<PackageCategory, List<string>> ParsePackageProcessResult(ProcessResult packageRetrievalResult) 
    {
        string[] packageNames = [.. packageRetrievalResult.output
            .Where(line => line.Contains("package:"))
            .Select(line => line.Replace("package:", "").Trim())];

        WriteSuccessMessage($"Located {packageNames.Length} packages.");

        var packageCategoryInfo = new Dictionary<PackageCategory, List<string>>();

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

            WriteSuccessMessage($"Parsed {packageCategoryInfo.Values.Sum(category => category.Count)} packages.");
        }

        catch (Exception ex) {
            WriteWarningMessage("Unable to parse the processResult object containing your device's package data.");
            WriteErrorMessage(ex.Message, exit: true, exitCode: 1);
        }

        return packageCategoryInfo;

    }


    /// <summary>
    ///     Provided a device IP, the user will be prompted for a debug service port. <br/>
    ///     A Tuple is returned (string deviceIP, string port).
    /// </summary>
    public static (string deviceIP, string port) PromptForConnectionInfo(string deviceIP) 
    {
        WriteInformation("The port to be used for connection will differ from the pairing port.");
        WriteInformation("Please refresh the 'Wireless debugging' tab and enter the updated values below.");

        return (
            deviceIP,
            AskForInput("Debug Service Port: ")
        );
    }


    /// <summary>
    ///     Provided a device IP, the user will be prompted for a pairing port and pairing code. <br/>
    ///     A PairingInfo object is returned using deviceIP, and the entered pairing port and pairing code.
    /// </summary>
    public static PairingInfo PromptForPairingInfo(string deviceIP) 
    {
        Console.WriteLine($"To locate your pairing code, please go to:\n{WIFISetting}\n");

        return new(
            deviceIP,
            AskForInput("Pairing Service Port: "),
            AskForInput("Pairing Code: ")
        );
    }


    /// <summary> 
    ///     Provided a device IP and pairing port, the user will be prompted for a pairing code. <br/>
    ///     A PairingInfo object is returned using deviceIP, pairingPort, and the entered pairing code.
    /// </summary>
    public static PairingInfo PromptForPairingCodeOnly(string deviceIP, string pairingPort) 
    {
        Console.WriteLine($"To locate your pairing code, please go to:\n{WIFISetting}\n");

        return new(
            deviceIP,
            pairingPort,
            AskForInput("Pairing Code: ")
        );
    }


    /// <summary>
    ///     Given a ProcessStartInfo object, a process is spawned and executed. <br/>
    ///     Optionally, if an inputArg is passed, this argument is written out to the process' StandardInput. <br/>
    ///     Finally, Both outputHandler and errorHandler are both optional arguments. <br/>
    ///     Passing either of these may be passed to override the default behavior of StandardError and StandardOutput.
    /// </summary>
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
    

    private static void ShowCaptureGroupInfo(string name) => WriteInformation(
        whiteText: "Processed capture group: ",
        coloredText: name,
        tagNameColor: "orange",
        reverse: true
    );

}