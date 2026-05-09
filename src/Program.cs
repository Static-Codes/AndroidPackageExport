
using System.Text.Json;
using AndroidPackageExport.Core.Helpers;
using AndroidPackageExport.Core.Mappings;
using AndroidPackageExport.Core.Types;
using AndroidPackageExport.Core.Types.ADB;
using AndroidPackageExport.Core.Types.Packaging;
using static AndroidPackageExport.Functions;


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
    Console.WriteLine("Unable to complete the operation.");
    throw new Exception(ex.Message);
}


Console.WriteLine($"Package list written to: {finalFilePath}");
