namespace AndroidPackageExport.Core.Types.ADB;


public class Connection 
{
    public enum ConnectionMethod { USB, WIFI }
    
    public class ConnectionStatus(bool Connected, ConnectionMethod? Method, ProcessResult? Result, string? Identifier = null) 
    {
        /// <summary> If a device is currently connected </summary>
        public bool Connected { get; set; } = Connected;

        /// <summary> The current connection method (if a device is connected) </summary>
        public ConnectionMethod? Method { get; set; } = Method;

        /// <summary> The ProcessResult object associated with the connecting process. </summary>
        public ProcessResult? Result { get; set; } = Result;

        /// <summary> The Identifier associated with the connected device (either a GUID or Device Address) </summary>
        public string? Identifier { get; set; } = Identifier;
    }
    
    public enum ConnectionType { Existing, New }
}