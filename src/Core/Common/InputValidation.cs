namespace AndroidPackageExport.Core.Common;

using Core.Types.ADB.Wireless;
using System.Net;
using static Core.Helpers.InputHelper;
using static Global.Logging;

public class InputValidation 
{
    private static (bool status, string menuChoice) CheckPort(string port) 
    {
        var menuChoice = "N/A";

        if (port.Length != 5) 
        {
            WriteInformation("WIFI Pairing over ADB normally utilizes a 5 digit port number.");
            WriteInformation(
                "If you are positive your device is utilizing a port under 30000, please select \"This is correct\" below."
            );

            menuChoice = AskForSelection(
                $"Are you positive you want to continue with port: {port}",
                options: ["This is correct", "I made a mistake"]
            );

            UserExitStatusCheck(menuChoice);

            
            if (menuChoice.Equals("I made a mistake")) {
                return (false, menuChoice);
            }

            return (true, menuChoice);

        }

        if (!int.TryParse(port, out int parsedPort)) { 
            return (false, menuChoice);
        }
        
        var status = parsedPort > IPEndPoint.MinPort && parsedPort <= IPEndPoint.MaxPort;
        return (status, menuChoice);
    }

    private static (bool status, string menuChoice, string issue) CheckPairingCode(string code) 
    {
        var menuChoice = "N/A";

        var tooManyDuplicates = code
            .GroupBy(c => c)
            .Any(group => group.Count() > 3);

        var invalidLength = code.Length != 6;
        var issue = "N/A";

        string? inputMessage;

        // Skipping the selection menu for codes that are known to be invalid.
        if (invalidLength) {
            WriteWarningMessage("WIFI Pairing over ADB utilizes a 6 digit pairing code.");
            return (status: false, menuChoice: "I made a mistake", issue: nameof(invalidLength));
        } 

        else if (tooManyDuplicates) {
            inputMessage = "The provided code has 4 or more of the same characters, did you intend to do this?";
        }

        else {
            inputMessage = $"Are you positive you want to continue with the code: {code}";
        }

        menuChoice = AskForSelection(
            $"Are you positive you want to continue with the code: {code}",
            options: ["This is correct", "I made a mistake"]
        );

        UserExitStatusCheck(menuChoice);

        if (menuChoice.Equals("I made a mistake")) {
            return (false, menuChoice, issue);
        }

        if (!int.TryParse(code, out int parsedCode)) {
            issue = "Unable to parse"; 
            return (false, menuChoice, issue);
        }

        return (true, menuChoice, issue);
    }

    /// <summary> Will exit if the port provided is invalid </summary>
    public static string DoCodeValidation(ref PairingInfo pairingInfo) {
        var (status, menuChoice, issue) = CheckPairingCode(pairingInfo.Code);
        
        // This loop continues while the code fails basic validation testing, and the user continues to select "I made a mistake".
        while (!status && menuChoice.Equals("I made a mistake")) {
            pairingInfo.Code = AskForInput("Please enter the correct pairing code: ");
            (status, menuChoice, issue) = CheckPairingCode(pairingInfo.Code);
        }

        var errorText = issue switch {
            "invalidLength" => "The pairing code provided must be 6 digits in length.",
            "Unable to parse" => "The pairing code provided must contain only numbers.",
            _ => $"Invalid code provided: '{pairingInfo.Code}'"
        };

        // If this condition is executed, it indicates the user entered an invalid value, and selected "This is correct".
        if (!status) {
            WriteErrorMessage(errorText, exit: true, exitCode: 1);
        }

        // If the code has passed initial validation testing, no further user input is required.
        return pairingInfo.Code; 
    }

    public static string DoPortValidation(ref string port) {
        var (status, menuChoice) = CheckPort(port);
        
        // While the port is invalid, and the user has indicated this was a mistake, a loop persists.
        while (!status && menuChoice.Equals("I made a mistake")) {
            port = AskForInput("Please enter the correct port number: ");
            (status, menuChoice) = CheckPort(port);
        }


        // If this condition is executed, it indicates the user entered an invalid value, and selected "This is correct".
        if (!status) {
            WriteErrorMessage($"Invalid port provided: '{port}'");
            WriteErrorMessage(
                message: $"The port must be between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort}.", 
                exit: true, 
                exitCode: 1
            );
        }

        // If the port is valid, no further user input is required.
        return port; 
    }


    /// <summary> Will exit if the port provided is invalid </summary>
    public static PairingInfo DoPortValidation(ref PairingInfo pairingInfo) {
        var (status, menuChoice) = CheckPort(pairingInfo.Port);
        
        // While the port is invalid, and the user has indicated this was a mistake, a loop persists.
        while (!status && menuChoice.Equals("I made a mistake")) {
            pairingInfo.Port = AskForInput("Please enter the correct port number: ");
            (status, menuChoice) = CheckPort(pairingInfo.Port);
        }


        // If this condition is executed, it indicates the user entered an invalid value, and selected "This is correct".
        if (!status) {
            WriteErrorMessage($"Invalid port provided: '{pairingInfo.Port}'");
            WriteErrorMessage(
                message: $"The port must be between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort}.", 
                exit: true, 
                exitCode: 1
            );
        }

        // If the port is valid, no further user input is required.
        return pairingInfo; 
    }

}