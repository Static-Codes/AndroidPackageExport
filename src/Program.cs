using System.Text.Json;
using AndroidPackageExport.Core.Helpers;
using AndroidPackageExport.Core.Mappings;
using AndroidPackageExport.Core.Types;
using AndroidPackageExport.Core.Types.ADB;
using AndroidPackageExport.Core.Types.Packaging;
using static AndroidPackageExport.Functions;
using static AndroidPackageExport.Global.Logging;


var connectionStatus = await CheckForDeviceConnection();

Console.WriteLine($"Connected: {connectionStatus.Connected}");

// if (connectionStatus.Result?.output != null) {
//     foreach (var line in connectionStatus.Result.output) { WriteDebugMessage(line); }
// }

var device = new Device();
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
