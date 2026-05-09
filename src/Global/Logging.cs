namespace AndroidPackageExport.Global;


// A <PackageReference> to Spectre.Console must be included prior to Referencing Logging.cs in a .csproj
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;
using static Global.Constants;

internal static class Logging
{
    public static void WriteDebugMessage(string message) {
        AnsiConsole.MarkupLine($"[{OrangeHex}]{DebugTag}[/] {message.EscapeMarkup()}");
    }
    public static void WriteErrorMessage(string message, int? exitCode = null, [DoesNotReturnIf(true)] bool exit = false)
    {
        AnsiConsole.MarkupLine($"[red]{ErrorTag}[/] {message.EscapeMarkup()}");
        if (exit) {
            Environment.Exit(exitCode ?? 1);
        }
    }

    public static void WriteInformation(string whiteText = "", string coloredText = "", string textColor = "blue", string tagName = InfoTag, string tagNameColor = "blue") 
    {
        string[] validColors = ["blue", "purple", "orange"];

        if (!validColors.Contains(textColor)) {
            WriteErrorMessage("Invalid textColor passed to WriteInformation.");
            WriteWarningMessage("Supported Colors:");
            WriteInformation(coloredText: "\t blue", textColor: "blue");
            WriteInformation(coloredText: "\t purple", textColor: "purple");
            WriteInformation(coloredText: "\t orange", textColor: "orange");
            return;
        }

        textColor = textColor == "orange" ? OrangeHex : textColor;
        tagNameColor = tagNameColor == "orange" ? OrangeHex : tagNameColor;

        AnsiConsole.MarkupLine($"[{tagNameColor}]{tagName}[/] [{textColor}]{coloredText.EscapeMarkup()}[/] {whiteText.EscapeMarkup()}");
    }

    public static void WriteStateMessage(string message) => AnsiConsole.MarkupLine($"[blue]{message}[/]");

    public static void WriteSuccessMessage(string message) => AnsiConsole.MarkupLine($"[green]{SuccessTag}[/] {message.EscapeMarkup()}");

    public static void WriteWarningMessage(string message) => AnsiConsole.MarkupLine($"[yellow]{WarningTag}[/] {message.EscapeMarkup()}");

}
