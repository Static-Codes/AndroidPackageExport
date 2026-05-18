using AndroidPackageExport.Core.Types;
using AndroidPackageExport.Core.Types.ADB;
using System.Text.Json;
using static AndroidPackageExport.Core.Helpers.FileHelper;
using static AndroidPackageExport.Core.Helpers.InputHelper;
using static AndroidPackageExport.Core.Types.ADB.Connection.ConnectionMethod;
using static AndroidPackageExport.Functions;
using static AndroidPackageExport.Global.Constants;
using static AndroidPackageExport.Global.Logging;

if (!ADBInstalled()) {
    Console.WriteLine($"Please ensure ADB is installed at: {ADBPath}");
    Environment.Exit(1);
}

CreateAppDataSubDirectory();

var connectionStatus = await CheckForDeviceConnection();

string deviceName = connectionStatus switch
{
    { Connected: false} => "Unknown", // Device is unknown at this point

    { Connected: true, Method: USB } => $"Android Device (USB) @ {connectionStatus.Identifier}",

    { Connected: true, Output: { DeviceName: not null, DeviceID: not null } } => 
        $"{connectionStatus.Output.DeviceName} @ {connectionStatus.Identifier}",

    _ => $"Android Device (WiFi) @ {connectionStatus.Identifier}"   
};


Device? device;
var whitelist = new Whitelist();


if (!connectionStatus.Connected) {
    device = new Device(deviceName);
}


else 
{
    device = new Device(
        Name: deviceName,
        ConnectionStatus: connectionStatus,
        ID: connectionStatus.Identifier
    );
}


if (whitelist.IsWhitelistedDevice(device)) {
    return;
}

string message = connectionStatus.Method switch {
    USB => $"Would you like to use the device with the identifier: {connectionStatus.Identifier}?",
    WIFI => $"Would you like use the {connectionStatus.Output?.DeviceName ?? "device"} at {connectionStatus.Identifier}",
    _ => throw new ArgumentException("Invalid method passed to connectionStatus.Method")
};


var confirmationSelection = AskForSelection(message, options: ["Yes", "No", "I don't know"]);

UserExitStatusCheck(confirmationSelection);

var deviceConfirmed = confirmationSelection == "Yes";
var userIsUnsure = confirmationSelection == "I don't know";

if (!deviceConfirmed && !userIsUnsure) { 
    Environment.Exit(1); 
}

if (userIsUnsure) {
    // Add a call to GetDeviceNameFromADB()
    // Redo the prompt with 
    // AskForSelection($"Do you wish to authorize the {deviceName} at {deviceAddress}?", ["Yes", "No"])
}

// if (connectionStatus.Result?.output != null) {
//     foreach (var line in connectionStatus.Result.output) { WriteDebugMessage(line); }
// }


ProcessResult? packageRetrievalResult;

// Add logic to save a config using the Device object.
(device, packageRetrievalResult) = await RunPackageRetrieval(device);

var packageCategoryInfo = ParsePackageProcessResult(packageRetrievalResult);

var finalFilePath = Path.Combine(AppContext.BaseDirectory, "packages.json");

try 
{
    var objectBytes = JsonSerializer.SerializeToUtf8Bytes(packageCategoryInfo);

    var stream = new FileStream(finalFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

    await stream.WriteAsync(objectBytes);
}

catch (Exception ex) {
    WriteErrorMessage("Unable to complete the operation.");
    throw new Exception(ex.Message);
}


WriteSuccessMessage($"Package list written to: {finalFilePath}");
