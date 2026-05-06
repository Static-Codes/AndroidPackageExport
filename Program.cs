using System.Text.Json;
using AndroidPackageExport.Mappings;
using AndroidPackageExport.Types;
using static AndroidPackageExport.Functions;



var packages = await ListPackagesUsingADB();
Console.WriteLine($"Located {packages.Count} packages.");


var packageCategoryInfo = new Dictionary<AppCategory, List<string>>();

var categories = typeof(AppCategory).GetEnumNames().Select(n => Enum.Parse<AppCategory>(n));

if (!categories.Any()) {
    Console.WriteLine("Unable to deserialize the internal App Categories, that are required to for this utility to operate.");
    Environment.Exit(1);
}

foreach (var category in categories){
    packageCategoryInfo.Add(category, []);
}

try 
{
    for (int i = 0; i < packages.Count; i++) {
        var packageCategory = new AppCategoryMapping(packages[i]).Result;
        packageCategoryInfo[packageCategory].Add(packages[i]);
    }

    var objectBytes = JsonSerializer.SerializeToUtf8Bytes(packageCategoryInfo);

    var stream = new FileStream("packages.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);

    await stream.WriteAsync(objectBytes);
}

catch (Exception ex) {
    Console.WriteLine("Unable to complete the operation.");
    throw new Exception(ex.Message);
}
