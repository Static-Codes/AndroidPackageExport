namespace AndroidPackageExport.Core.Types;

public class ProcessResult(List<string> output, List<string> error, uint exitCode, Exception? exception)
{
    public List<string> output { get; set; } = output;
    public List<string> error { get; set; } = error;
    public uint exitCode { get; set; } = exitCode;
    public Exception? exception { get; set; } = exception;
}