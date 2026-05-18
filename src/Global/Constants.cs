namespace AndroidPackageExport.Global;

using System.Reflection;

internal class Constants 
{
    public const string ADBPath = "/usr/bin/adb";
    
    public static string ApplicationName = Assembly.GetExecutingAssembly().FullName?.Split(',')[0] ?? "AndroidPackageExport";

    public const BindingFlags _privateFlag = BindingFlags.NonPublic;
    public const BindingFlags _privateStaticFlag = BindingFlags.NonPublic | BindingFlags.Static;
    public const BindingFlags _publicFlag = BindingFlags.Public;
    public const BindingFlags _publicInstanceFlag = _publicFlag | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
    
    public const string ConnectionSection = "Please reference the 'Connecting Your Device' section in the project's repository.";
    public const string USBSetting = "System -> Developer options -> USB Debugging.";
    public const string WIFISetting = "System -> Developer options -> Wireless debugging -> Pair device with pairing code.";
    public const string DebugTag = "[[DEBUG]]";
    public const string ErrorTag = "[[ERROR]]:";
    public const string InfoTag = "[[INFO]]:";
    public const string InputTag = "[[INPUT]]:";
    public const string WarningTag = "[[WARNING]]:";
    public const string SuccessTag = "[[SUCCESS]]:";
    public const string OrangeHex = "#FFAF00";
    public const string ProjectIssueLink = "https://github.com/Static-Codes/AndroidPackageExport/issues";


}