using DrzSharp.Compiler.Model;
using System.Text;

namespace DrzSharp.Compiler.Diagnostics;

public static class Manager
{
    public static bool HasError(this DzProject project)
    {
        if (HasError(project.LoaderDiagnostics))
            return true;
        if (HasError(project.ParserDiagnostics))
            return true;
        if (HasError(project.LowererDiagnostics))
            return true;

        foreach (var file in project.Files)
        {
            if (HasError(file.LexerDiagnostics))
                return true;
            if (HasError(file.ParserDiagnostics))
                return true;
            if (HasError(file.LowererDiagnostics))
                return true;
        }
        return false;
    }
    private static bool HasError<T>(GroupDiagnostics<T> diag) where T : struct
    {
        foreach(var entry in diag.Reports)
        {
            if (entry.Code 
            is DiagnosticCode.Unhandled 
            or DiagnosticCode.Unexpected 
            or DiagnosticCode.UserError)
                return true;
        }
        return false;
    }

    public static void Debug(this DzProject project)
    => new Render().DebugProject(project);
}
public partial class Render
{
    //>>>> PROJECT DEBUG <<<<
    private DzProject Project = null!;

    private bool renderInConsole;
    private StringBuilder Stream = null!;
    internal ConsoleColor Color
    {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }

    private DebugSource Source = null!;

    public void DebugProject(DzProject project)
    {
        Project = project;

        //ENTRIES
        renderInConsole = true;

        PrintProjectHeader();
        foreach (var file in project.Files)
        {
            File = file;
            Source = new(this, file.Source);

            DebugFileEntries();
        }

        //STRUCTURES
        renderInConsole = false;

        Stream = new();
        PrintProjectHeader();
        foreach (var file in project.Files)
        {
            File = file;
            Source = new(this, file.Source);

            DebugFileStructures();
        }
        System.IO.File.WriteAllText(@"C:\Driza\DrizaSharp\.dzdiag", Stream.ToString());
    }

    internal void WriteLine(string? value = null)
    {
        if (renderInConsole)
            Console.WriteLine(value);
        else
            Stream.AppendLine(value);
    }
    internal void Write(string? value)
    {
        if (renderInConsole)
            Console.Write(value);
        else
            Stream.Append(value);
    }
    internal void Write(char value)
    {
        if (renderInConsole)
            Console.Write(value);
        else
            Stream.Append(value);
    }

    //>>>> FILE DEBUG <<<<
    public enum Stage
    { Loader, Lexer, Parser, Lowerer }

    private DzFile File = null!;
    private DzModule Module => Project.Modules[File.ModuleId];
    private TAST TAST => File.TAST;
    private TASI TASI => File.TASI;

    private void DebugFileEntries()
    {
        PrintFileHeader();

        PrintSectionHeader("FILE ENTRIES");
        if (DebugEntries(Stage.Lexer, File.LexerDiagnostics))
            return;
        if (DebugEntries(Stage.Parser, File.ParserDiagnostics))
            return;
        if (DebugEntries(Stage.Lowerer, File.LowererDiagnostics))
            return;

        WriteLine();
    }
    private void DebugFileStructures()
    {
        PrintFileHeader();

        DebugTokens();
        DebugTAST();
        DebugTASI();

        WriteLine();
    }
}

public class DebugSource
{
    private readonly Render Render;

    private readonly SourceText Source;
    private readonly List<int> Lines = [0];

    public DebugSource(Render render, SourceText source)
    {
        Render = render;

        Source = source;
        var text = source.Text;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                Lines.Add(i + 1);
        }
    }

    public void Print(int tabCount, ConsoleColor color, params SourceSlice[] spans)
    {
        if (spans.Length == 0) return;

        string tabs = new('\t', tabCount);

        var oldColor = Render.Color;

        string text = Source.Text;

        //START OFFSET
        int startPos = Source.Start + spans[0].Start;
        int curLine = 0;
        while (curLine + 1 < Lines.Count && Lines[curLine + 1] <= startPos)
            curLine++;

        //LOOP
        int curPos = Lines[curLine];
        bool newline = true;

        void printUntil(int endPos)
        {
            while (curPos < endPos && curPos < text.Length)
            {
                if (newline)
                {
                    Render.Write(tabs + $"{curLine + 1} | ");
                    newline = false;
                }

                char c = text[curPos];
                if (c == '\n')
                {
                    Render.WriteLine();
                    curLine++;
                    newline = true;
                }
                else if (c != '\r')
                {
                    Render.Write(c);
                }
                curPos++;
            }
        }

        foreach (var span in spans)
        {
            int spanStart = Source.Start + span.Start;
            int spanEnd = spanStart + span.Length;

            printUntil(spanStart);

            Render.Color = color;
            printUntil(spanEnd);
            Render.Color = oldColor;
        }

        int lastPos = curLine + 1 < Lines.Count ? Lines[curLine + 1] : text.Length;
        printUntil(lastPos);

        Render.WriteLine("\n");
    }

    //START: INCLUSIVE
    //END: EXCLUSIVE
    public string Interval(int start, int end)
    {
        string pos(int i, bool end = false)
        {
            if (i == -1) return "START";
            if (i == -2) return "END";

            var line = 1;
            while (line < Lines.Count && i >= Lines[line] - 1)
                line++;

            var col = i - Lines[line - 1] + 1;
            if (end) col++;

            if (col == 0)
                return $"L{line}";
            else
                return $"L{line}:C{col}";
        }

        end--;
        if (start == end)
            return $"({pos(start)})";
        else
            return $"({pos(start)} - {pos(end, true)})";
    }
}