using DrzSharp.Compiler.Lexer;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Project;
using DrzSharp.Compiler.Text;

namespace DrzSharp.Compiler
{
    public static partial class Compiler
    {
        public static void Debug(DzProject proj)
        {
            foreach (var file in proj.Files)
                Debug(file);
        }

        public static void Debug(DzFile file)
        => Diagnostics.Render.Debug(file);
    }
}

namespace DrzSharp.Compiler.Diagnostics
{
    public static class Render
    {
        public static void Debug(DzFile file)
        {
            DebugSource source = new(file.Content);
            Console.WriteLine();

            Console.WriteLine($"===== LEXER DIAGNOSTICS =====");
            DebugTokens(source, file.TAST);
            Console.WriteLine();
            DebugGroup(source, "Lexer", file.Diagnostics.Lexer);

            Console.WriteLine($"===== PARSER DIAGNOSTICS =====");
            DebugGroup(source, "Parser", file.Diagnostics.Parser);

            Console.WriteLine($"===== LOWERER DIAGNOSTICS =====");
            DebugGroup(source, "Lowerer", file.Diagnostics.Lowerer);
        }

        private static void DebugGroup(DebugSource source, string group, GroupDiagnostics diag)
        {
            if (diag.Reports.Count <= 0) return;

            List<DiagnosticEntry> sysEntries = [];
            List<DiagnosticEntry> entries = [];

            foreach (var entry in diag.Reports)
            {
                if (entry.Code == DiagnosticCode.Unexpected)
                    sysEntries.Add(entry);
                else
                    entries.Add(entry);
            }

            //UNEXPECTED-TYPE ENTRIES
            if (sysEntries.Count > 0)
            {
                DebugTitle(group, sysEntries.First());
                source.Print(0, ConsoleColor.Blue, [.. sysEntries
                    .DistinctBy(e => e.Span.Start)
                    .OrderBy(e => e.Span.Start)
                    .Select(e => e.Span)
                ]);
            }

            //OTHER ENTRIES
            foreach (var entry in entries)
            {
                DebugTitle(group, entry);
                source.Print(0, ConsoleColor.Blue, entry.Span);
            }
        }
        private static void DebugTitle(string group, DiagnosticEntry entry)
        {
            (var defColor, Console.ForegroundColor) = (Console.ForegroundColor, ConsoleColor.Red);
            Console.WriteLine($"[{group} {entry.Caller ?? "SYSTEM"}] {entry.Message}:");

            Console.ForegroundColor = defColor;
        }

        private static void DebugTokens(DebugSource source, TAST TAST)
        {
            for (int i = 0; i < TAST.TokenCount; i++)
            {
                var token = TAST.TokenAt(i);
                string cont = $"[{LexerManager.TokenTypes[token.Type]}] ";
                if (token.Type != Token.NULL && token.Type != Token.NEWLINE)
                    cont += TAST.GetText(token.Id) + " | ";
                cont += token.Start + " : " + token.Length;

                Console.WriteLine(cont);
            }
        }
    }

    public class DebugSource
    {
        private readonly SourceSpan Source;
        private readonly List<int> Lines = [0];

        public DebugSource(SourceSpan source)
        {
            Source = source;
            var text = source.Source;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                    Lines.Add(i + 1);
            }
        }

        public void Print(int tabCount, ConsoleColor color, params Slice[] spans)
        {
            if (spans.Length == 0) return;

            string tabs = new('\t', tabCount);
            var defColor = Console.ForegroundColor;

            string text = Source.Source;

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
                        Console.Write(tabs + $"{curLine + 1} | ");
                        newline = false;
                    }

                    char c = text[curPos];
                    if (c == '\n')
                    {
                        Console.WriteLine();
                        curLine++;
                        newline = true;
                    }
                    else if (c != '\r')
                    {
                        Console.Write(c);
                    }
                    curPos++;
                }
            }

            foreach (var span in spans)
            {
                int spanStart = Source.Start + span.Start;
                int spanEnd = spanStart + span.Length;

                printUntil(spanStart);

                Console.ForegroundColor = color;
                printUntil(spanEnd);
                Console.ForegroundColor = defColor;
            }

            int lastPos = curLine + 1 < Lines.Count ? Lines[curLine + 1] : text.Length;
            printUntil(lastPos);

            Console.WriteLine('\n');
        }
    }
}