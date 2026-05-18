namespace AndroidPackageExport.Core.Types.ADB;


public class Connection 
{
    public enum ConnectionMethod { USB, WIFI }
    
    /// <summary> An object representing key values that will be pulled from calls to adb. <br/>
    /// Index 0 -> Full line associated with the match. <br/>
    /// Index 1 -> "device_id" <br/>
    /// Index 2 -> "ip <br/>
    /// Index 3 -> "port" <br/>
    /// Index 4 -> "device_name" <br/>
    /// Index 5 -> "codename" <br/>
    /// Index 6 -> "transport_id" <br/>
    /// </summary>
    public class ConnectionOutput(
        string? DeviceID = null, 
        string? DeviceIP = null, 
        string? DebugPort = null, 
        string? DeviceName = null, 
        string? Codename = null, 
        string? TransportID = null
    ) 
    { 
        // Index 0 -> Full line associated with the match. 
        // Index 1 -> "device_id"
        // Index 2 -> "ip
        // Index 3 -> "port"
        // Index 4 -> "device_name"
        // Index 5 -> "codename"
        // Index 6 -> "transport_id"

        public string DeviceID { get; set; } = DeviceID ?? "Unknown";
        public string IP { get; set; } =  DeviceIP ?? "";
        public string Port { get; set; } = DebugPort ?? "";
        public string DeviceName { get; set; } = DeviceName ?? "Unknown";
        public string Codename { get; set; } = Codename ?? "Unknown";
        public string TransportID { get; set; } = TransportID ?? "Unknown";

    }

    public class ConnectionStatus(bool Connected, ConnectionMethod? Method, ConnectionOutput? Output, ProcessResult? Result, string? Identifier = null) 
    {
        /// <summary> If a device is currently connected </summary>
        public bool Connected { get; set; } = Connected;

        /// <summary> The current connection method (if a device is connected) </summary>
        public ConnectionMethod? Method { get; set; } = Method;


        /// <summary> 
        ///     The sanitized output from the StandardOutput of the process associated with the call to "adb connect 'ID'"
        /// </summary>
        public ConnectionOutput? Output { get; set; } = Output;

        /// <summary> The raw ProcessResult object associated with the connecting process. </summary>
        public ProcessResult? Result { get; set; } = Result;

        /// <summary> The Identifier associated with the connected device (either a GUID or Device Address) </summary>
        public string? Identifier { get; set; } = Identifier;

    }
    
    public enum ConnectionType { Existing, New }

    
}