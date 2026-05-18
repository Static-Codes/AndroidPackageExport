namespace AndroidPackageExport.Core.Types.ADB;

using static Global.Logging;
using static Helpers.InputHelper;
using static Helpers.FileHelper;
using System.Text.Json;

public class Whitelist
{
    private readonly string WhitelistFilePath = GetWhitelistFilePath();
    private readonly string WhitelistBackupFilePath = GetWhitelistBackupFilePath();
    private HashSet<object> AuthorizedDevices { get; set; } = [];
    public Whitelist() {
        AuthorizedDevices = LoadWhitelist();
    }
    public Whitelist(HashSet<object> Devices) {
        AuthorizedDevices = Devices.Count > 0 ? Devices : LoadWhitelist();
    }

    /// <summary> 
    ///     Attempts to add the passed device to AuthorizedDevices. <br/> 
    ///     Returns a boolean representing the status of this request.
    /// </summary>
    public bool AddDevice(object device) 
    {
        if (device.GetType() == typeof(Device)) {
            return ProcessDevice(ref device, NeedsAuthorization: true);
        }

        else if (device.GetType() == typeof(AuthorizedDevice)) {
            return ProcessDevice(ref device, NeedsAuthorization: false);
        }

        WriteWarningMessage("Invalid object type passed to Whitelist.AddDevice.");
        WriteInformation("The expected object must of the type Device or AuthorizedDevice");
        return false;
    }

    /// <summary> Performing a semantic cast from the type Device to the type AuthorizedDevice. </summary>
    private static AuthorizedDevice AuthorizeDevice(object device) => (AuthorizedDevice)device;

    /// <summary> 
    ///     Performs a cast on AuthorizedDevices, going from HashSet of objects to an IEnumerable of AuthorizedDevice.
    /// </summary>
    public IEnumerable<AuthorizedDevice> GetAuthorizedDevices() => AuthorizedDevices.Select(d => AuthorizeDevice(d));


    /// <summary> Returns a boolean representing the whitelist status of the current device. </summary>
    public bool IsWhitelistedDevice(object device) {
        return 
            device.GetType() == typeof(AuthorizedDevice) &&
            AuthorizedDevices.Contains((AuthorizedDevice)device);
    }


    private HashSet<object> LoadWhitelist(HashSet<object>? list = null) 
    {
        list ??= [];
        
        if (File.Exists(WhitelistBackupFilePath) && list.Count != 0) 
        {
            using FileStream? stream = File.Open(WhitelistBackupFilePath, FileMode.Open);
            var items = JsonSerializer.Deserialize<HashSet<object>>(stream) ?? []; 
            foreach (var device in AuthorizedDevices) 
            {
                if (items.Contains(device)) {
                    continue;
                }
                items.Add(device);
            }
            return list;
        }


        if (!File.Exists(WhitelistFilePath)) {
            // If windows support is ever added, this needs to be dynamically initialized in a variable.
            // Currently, Carriage Returns are not required as this is a Unix-Only application.
            File.WriteAllText(WhitelistFilePath, "[\n]\n");
            // File.WriteAllText(WhitelistFilePath, "{\n\t\"devices\": []\n}\n");
            return list;
        }

        try {
            using FileStream? stream = File.Open(WhitelistFilePath, FileMode.OpenOrCreate);
            list = JsonSerializer.Deserialize<HashSet<object>>(stream);
            ArgumentNullException.ThrowIfNull(list);
        }

        catch (Exception ex) {
            WriteWarningMessage("An exception has occured while attempting to load the device whitelist.");
            var exc = ex;
            throw exc;
        }

        return list;
    }

    /// <summary> 
    ///     Authorizes a device passed via reference parameter (if it's not already authorized). <br/>
    ///     The authorized device is then added it to the whitelist. <br/>
    ///     This is a helper method for Whitelist.AddDevice()
    /// </summary>
    private bool ProcessDevice(ref object device, bool NeedsAuthorization) 
    {
        if (NeedsAuthorization) 
        {
            WriteInformation("You are trying to add an unauthorized device to the application whitelist.");
            
            var selection = AskForSelection(
                message: "Do you wish to continue?",
                options: ["I wish to authorize this device.", "No, I made a mistake."]
            );

            if (selection == "No, I made a mistake.") {
                return false;
            }
        
            device = AuthorizeDevice(device);
        }

        AuthorizedDevices.Add(device);
        return true;
    }

    private void ProcessWhitelistBackup() 
    {
        try 
        {
            File.Copy(WhitelistFilePath, WhitelistBackupFilePath);
            File.Delete(WhitelistFilePath);
            AuthorizedDevices = LoadWhitelist(AuthorizedDevices);
        }

        catch (Exception ex) 
        {
            WriteWarningMessage("Unable to create a Backup of Device Whitelists");
            var exc = ex;
            throw exc;
        }
    }

    public void UpdateWhitelistFile(object device) 
    {
        ProcessWhitelistBackup();
        ProcessDevice(ref device, NeedsAuthorization: false);
    }
}
