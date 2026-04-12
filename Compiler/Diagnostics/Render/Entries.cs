namespace DrzSharp.Compiler.Diagnostics;

public partial class Render
{
    private bool DebugEntries<T>(Stage stage, GroupDiagnostics<T> diag) where T : struct
    {
        if (diag.Reports.Count <= 0)
            return false;

        WriteLine(">> COMPILER ENTRIES: ");

        //>>>> RENDER LOGIC <<<<
        List<DiagnosticEntry<T>> unhandledEntries = [];
        List<DiagnosticEntry<T>> unexpectedEntries = [];

        List<DiagnosticEntry<T>> userEntries = [];

        foreach (var entry in diag.Reports)
        {
            if (entry.Code == DiagnosticCode.Unhandled)
                unhandledEntries.Add(entry);
            else if (entry.Code == DiagnosticCode.Unexpected)
                unexpectedEntries.Add(entry);
            else
                userEntries.Add(entry);
        }

        //UNHANDLED ENTRIES
        foreach (var entry in unhandledEntries)
        {
            PrintEntryHeader(stage, entry);
            Source.Print(0, ConsoleColor.Blue, entry.Source);
        }

        //UNEXPECTED ENTRIES
        if (unexpectedEntries.Count > 0)
        {
            PrintEntryHeader(stage, unexpectedEntries.First());
            Source.Print(0, ConsoleColor.Blue, [.. unexpectedEntries
                .DistinctBy(e => e.Source.Start)
                .OrderBy(e => e.Source.Start)
                .Select(e => e.Source)
            ]);
        }

        //USER ENTRIES
        foreach (var entry in userEntries)
        {
            PrintEntryHeader(stage, entry);
            Source.Print(0, ConsoleColor.Blue, entry.Source);
        }

        WriteLine();
        return true;
    }
    private string? CallerName<T>(Stage stage, DiagnosticEntry<T> entry) where T : struct
    {
        string? caller = null;
        if (stage == Stage.Lexer && entry.Caller is RuleId ruleId)
        {
            caller = Rules.Lexer.RuleExt.GetRule(Project, ruleId).Method.Name;
        }
        else if (stage == Stage.Parser && entry.Caller is int nodeId)
        {
            if (TAST.TryGetApplyRule(nodeId, out var inst))
                caller = Rules.Parser.RuleExt.GetRule(Project, inst.RuleId).Name;
        }
        else if (stage == Stage.Lowerer && entry.Caller is int instrId)
        {
            caller = TASI.InstructionAt(instrId).Type.ToString();
        }

        return caller;
    }
    private void PrintEntryHeader<T>(Stage stage, DiagnosticEntry<T> entry) where T : struct
    {
        (var oldColor, Color) = (Color, ConsoleColor.Red);
        WriteLine($"[{stage} {entry.Code} | {CallerName(stage, entry) ?? "SYSTEM"}] {(entry.Message is string msg ? msg + ":" : "")}");

        Color = oldColor;
    }
}