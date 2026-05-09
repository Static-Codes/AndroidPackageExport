using AndroidPackageExport.Core.Types.ADB;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;
using static AndroidPackageExport.Global.Constants;
using static AndroidPackageExport.Global.Logging;

namespace AndroidPackageExport.Core.Helpers;

public static class InputHelper 
{   
    public const string ExitChoice = "Exit";
    private static readonly Style _style = new(decoration: Decoration.Bold);


    public static string AskForInput(string message) => AnsiConsole.Prompt(MakeTextPrompt(message));

    public static ConnectionMethod AskForConnectionMethod()
    {
        // Dynamically resolving the members of ConnectionMethod and casting each member as an object.
        var connectionOptions = Enum.GetValues<ConnectionMethod>().Cast<object>();

        var promptResponse = AskForSelection(
            message: "Please select your desired connection method for your android device.", 
            options: connectionOptions
        );
        return Enum.Parse<ConnectionMethod>(promptResponse);
    }

    // Selection-based prompt function(s) entry point.
    public static string AskForSelection(string message, IEnumerable<object> options, int? pageSize = null)
    {
        var sanitizedMessage = $"[{OrangeHex}]{InputTag}[/] {message}";

        var promptResponse = AnsiConsole.Prompt(
            MakeSelectionPrompt(sanitizedMessage, [.. options], pageSize)
        );

        UserExitStatusCheck(promptResponse);

        return promptResponse;
    }
    
    public static bool IsExitOption(this string choice) => choice.Equals(ExitChoice);

    public static IEnumerable<string> MakeInputMenu(IEnumerable<string> options) {
        return options.Append(ExitChoice); 
    }


    private static SelectionPrompt<string> MakeSelectionPrompt(string message, object[] options, int? pageSize, bool search = false) 
    {
        if (options.Length == 0) {
            WriteWarningMessage("Unable to create selection menu");
            WriteErrorMessage(
                message: "Parameter 'options' is empty in InputHelper.MakeSelectionPrompt()", 
                exitCode: 1, 
                exit: true
            );
        }

        int minimumSize = 3;

        // If the page size is provided:
        //  - Its value is increased by one to account for the addition of the "Exit" option.
        //  - This new size is compared to the minimum size of 3.
        //
        // If the page size is not provided:
        //  - The number of options is compared to the minimum size of 3.
        var safePageSize =
            pageSize != null ? 
            Math.Max((int)pageSize + 1, minimumSize) : 
            Math.Max(options.Length, minimumSize);

        var sanitizedOptions = 
            options.Select(
                opt => (
                    opt.GetType() != typeof(string) ? 
                    opt.ToString().EscapeMarkup() : // Handling non string options
                    (string)opt // Handling string options
                )
            );
        
        var finalOptions = MakeInputMenu(sanitizedOptions);

        var prompt = new SelectionPrompt<string>()
            .HighlightStyle(_style)
            .Title(message)
            .PageSize(safePageSize)
            .AddChoices(finalOptions);

        if (search) {
            prompt.EnableSearch();
        }

        return prompt;
    }

    // Adding an orange [INPUT] tag to match the logic in MakeSelectionPrompt
    private static TextPrompt<string> MakeTextPrompt(string message) => new($"[{OrangeHex}]{InputTag}[/] {message}");

    public static void UserExitStatusCheck(string inputString) 
    {
        if (inputString.IsExitOption()) {
            WriteInformation("Operation cancelled by user.");
            WriteInformation("Exiting.");
            Environment.Exit(1);
        }
    }

    // The [NotNull] attribute tells roslyn, if this method returns:
    // The argument 'input' is guaranteed not to be null.
    public static void CheckForNullInput([NotNull] object? input)
    {
        if (input is null) {
            throw new ArgumentNullException(nameof(input), "Selection cannot be null.");
        }
    }

}
