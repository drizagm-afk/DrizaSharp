namespace DrzSharp.Compiler.Diagnostics;

public class GroupDiagnostics<T> where T : struct
{
    protected readonly List<DiagnosticEntry<T>> _reports = [];
    public IReadOnlyList<DiagnosticEntry<T>> Reports => _reports;
}
public class ProjectDiagnostics : GroupDiagnostics<bool>
{
    internal void ReportUnexpected(SourceSlice source, string message)
    => _reports.Add(new(DiagnosticCode.Unexpected, source, null, message));
    internal void ReportUnhandled(SourceSlice source, string message)
    => _reports.Add(new(DiagnosticCode.Unhandled, source, null, message));

    public void AddError(SourceSlice source, string message)
    => _reports.Add(new(DiagnosticCode.UserError, source, null, message));
    public void AddWarning(SourceSlice source, string message)
    => _reports.Add(new(DiagnosticCode.UserWarning, source, null, message));
    public void AddInfo(SourceSlice source, string message)
    => _reports.Add(new(DiagnosticCode.UserInfo, source, null, message));
}
public class FileDiagnostics<T> : GroupDiagnostics<T> where T : struct
{
    internal void ReportUnexpected(SourceSlice source, string message)
    => _reports.Add(new(DiagnosticCode.Unexpected, source, null, message));
    internal void ReportUnhandled(SourceSlice source, string message)
    => ReportUnhandled(source, null, message);
    internal void ReportUnhandled(SourceSlice source, T? caller, string message)
    => _reports.Add(new(DiagnosticCode.Unhandled, source, caller, message));

    public void AddError(SourceSlice source, string message)
    => AddError(source, null, message);
    public void AddError(SourceSlice source, T? caller, string message)
    => _reports.Add(new(DiagnosticCode.UserError, source, caller, message));
    public void AddWarning(SourceSlice source, string message)
    => AddWarning(source, null, message);
    public void AddWarning(SourceSlice source, T? caller, string message)
    => _reports.Add(new(DiagnosticCode.UserWarning, source, caller, message));
    public void AddInfo(SourceSlice source, string message)
    => AddInfo(source, null, message);
    public void AddInfo(SourceSlice source, T? caller, string message)
    => _reports.Add(new(DiagnosticCode.UserInfo, source, caller, message));
}

public readonly struct DiagnosticEntry<T>
(DiagnosticCode code, SourceSlice source, T? caller, string? message) where T : struct
{
    public readonly DiagnosticCode Code = code;
    public readonly SourceSlice Source = source;

    public readonly T? Caller = caller;
    public readonly string? Message = message;
}
public enum DiagnosticCode
{
    Unexpected, Unhandled,
    UserError, UserWarning, UserInfo
}