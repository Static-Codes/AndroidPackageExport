namespace AndroidPackageExport.Core.Extensions;

public static class StringExtension 
{
    public static bool StartsWithAny(this string str, string[] Options) {
        return Options.Any(option => option.StartsWith(str));
    }
}