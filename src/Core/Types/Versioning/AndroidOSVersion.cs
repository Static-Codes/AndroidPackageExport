namespace AndroidPackageExport.Core.Types.Versioning;

/// <summary> 
///     Contains an integer representing the AndroidAPILevel. <br/>
///     Exceptions: <br/>
///     UNKNOWN returns -1 <br/>
///     LESS_THAN_ANDROID_10 returns 0. <br/> 
/// </summary>
public enum AndroidOSVersion
{
    UNKNOWN = -1,
    LESS_THAN_ANDROID_10 = 0,
    ANDROID_10 = 29,
    ANDROID_11 = 30,
    ANDROID_12 = 31,
    ANDROID_12L = 32,
    ANDROID_13 = 33,
    ANDROID_14 = 34,
    ANDROID_15 = 35,
    ANDROID_16 = 36

}