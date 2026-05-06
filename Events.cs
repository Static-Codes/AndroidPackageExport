using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AndroidPackageExport;
public class ProcessDataReceivedEventArgs : EventArgs
{
    public string? Data { get; set; }

    public ProcessDataReceivedEventArgs(string _Data) {
        Data = _Data;
    }

    public ProcessDataReceivedEventArgs(DataReceivedEventArgs Source) 
    {
        Data = Source.Data;
    }
}

public class ExceptionEvents 
{ 
    [DoesNotReturn]
    public static void ThrowDeviceNotConnectedException() {
        throw new Exception(
            "No connected devices were detected.\n\n" +
            "Please reference the 'Connecting Your Device' section in the project's repository."
        );
    }

    [DoesNotReturn]
    public static void ThrowMultipleDevicesConnectedException(string[] addresses) {
        throw new Exception(
            "Multiple devices were detected, please ensure only one is plugged in.\n" +
            "Device Addresses:\n\n" +
            $"{string.Join('\n', addresses)}\n"
        );
    }
}