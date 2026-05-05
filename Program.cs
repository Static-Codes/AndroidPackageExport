using System.Text.Json;
using AndroidAppLister.Mapping;
using AndroidAppLister.Types;
using static AndroidAppLister.Functions;

var packages = await ListPackagesUsingADB();

// var packageClasses = new Dictionary<AppCategory, List<string>>() {
//     { AppCategory.Application, []},
//     { AppCategory.Common, []},
//     { AppCategory.Developer, []},
//     { AppCategory.System, []},
// }; 

var packageCategoryInfo = new Dictionary<AppCategory, List<string>>();
var categories = 
    typeof(AppCategory)
    .GetEnumNames()
    .Select(
        field => Enum.Parse<AppCategory>(field)
    );

foreach (var category in categories){
    packageCategoryInfo.Add(category, []);
}


for (int i = 0; i < packages.Count; i++) {
    var packageCategory = new AppCategoryMapping(packages[i]).Result;
    packageCategoryInfo[packageCategory].Add(packages[i]);
}

var objectBytes = JsonSerializer.SerializeToUtf8Bytes(packageCategoryInfo);

var stream = new FileStream("tmp.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);

await stream.WriteAsync(objectBytes);

