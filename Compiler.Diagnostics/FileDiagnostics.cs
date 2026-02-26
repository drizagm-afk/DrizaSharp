namespace DrzSharp.Compiler.Diagnostics;

public class FileDiagnostics
{
    private bool _enabled = true;
    public bool IsEnabled => _enabled;
    internal void Enable() => _enabled = true;
    internal void Disable() => _enabled = false;

    public readonly GroupDiagnostics Lexer = new();
    public readonly GroupDiagnostics Parser = new();
    public readonly GroupDiagnostics Lowerer = new();
}
public class GroupDiagnostics
{
    //REPORTS
    private readonly List<DiagnosticEntry> _reports = [];
    public IReadOnlyList<DiagnosticEntry> Reports => _reports;

    internal void ReportUnexpected(Slice span, string? caller, string message)
    => _reports.Add(new(span, caller, message, DiagnosticCode.Unexpected));
    public void ReportInvalid(Slice span, string? caller, string message)
    => _reports.Add(new(span, caller, message, DiagnosticCode.Invalid));
}

public readonly struct DiagnosticEntry(Slice span, string? caller, string? message, DiagnosticCode code)
{
    public readonly Slice Span = span;
    public readonly string? Caller = caller;
    public readonly string? Message = message;
    public readonly DiagnosticCode Code = code;
}
public enum DiagnosticCode
{ Unexpected, Invalid }