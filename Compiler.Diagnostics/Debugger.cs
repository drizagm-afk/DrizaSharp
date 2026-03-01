using DrzSharp.Compiler.Lexer;
using DrzSharp.Compiler.Parser;
using DrzSharp.Compiler.Lowerer;

using DrzSharp.Compiler.Project;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Text;

using System.Text;
using DrzSharp.Compiler.Diagnostics;

namespace DrzSharp.Compiler
{
    public static partial class Compiler
    {
        public static string Debug(DzProject project)
        => new Render().DebugProject(project);

        public static string Debug(DzFile file)
        => new Render().DebugFile(file);
    }
}

namespace DrzSharp.Compiler.Diagnostics
{
    public class Render
    {
        private StringBuilder Stream = null!;
        private DebugSource Source = null!;
        private DzFile File = null!;

        public string DebugProject(DzProject project)
        => string.Join("\n", project.Files.Select(DebugFile));
        public string DebugFile(DzFile file)
        {
            File = file;
            Stream = new();
            Source = new(File.Content, Stream);

            Debug();
            return Stream.ToString();
        }

        public void WriteLine(string? cont = null)
        => Stream.AppendLine(cont);
        public void Write(string? cont)
        => Stream.Append(cont);

        private void Debug()
        {
            const int barSize = 50;

            //**FILE HEADER**
            WriteLine('/'.Repeat(barSize));
            WriteLine($"FILE:   {File.Path}");
            WriteLine($"MODULE: <default>");
            WriteLine('/'.Repeat(barSize));

            //**VERBOSE DEBUGGING**
            void printTitle(string title)
            {
                int padd = barSize - title.Length;
                int left = padd / 2;
                int right = padd - left;

                WriteLine(
                    $"{'='.Repeat(left - 1)} {title} {'='.Repeat(right - 1)}"
                );
            }

            WriteLine();
            //LEXER
            printTitle("LEXER DIAGNOSTICS");
            DebugTokens(File.TAST);
            DebugLogs("Lexer", File.Diagnostics.Lexer);
            //PARSER
            printTitle("PARSER DIAGNOSTICS");
            DebugTAST(File.TAST);
            DebugLogs("Parser", File.Diagnostics.Parser);
            //LOWERER
            printTitle("LOWERER DIAGNOSTICS");
            DebugLogs("Lowerer", File.Diagnostics.Lowerer);
        }

        //DEBUG TOKENS
        private void DebugTokens(TAST TAST)
        {
            WriteLine(">> BASE TOKENS: ");

            //LOOP
            for (int i = 0; i < TAST.TokenCount; i++)
            {
                var token = TAST.TokenAt(i);
                string cont = $"[{token.Id:D3} {LexerManager.TokenTypes[token.Type]}] ";
                if (token.Type != Token.NULL && token.Type != Token.NEWLINE)
                    cont += "\"" + TAST.GetText(token.Id) + "\" ";
                cont += Source.Interval(token.Start, token.Start + token.Length);

                WriteLine(cont);
            }
            WriteLine();
        }

        //DEBUG GRAPHS
        const string GRAPH_CONN = " ├─ ";
        const string GRAPH_TAB = " │  ";

        private void PrintGConn(string cont, int tabs)
        {
            if (tabs == 0)
                WriteLine(cont);
            else
            {
                WriteLine(
                    GRAPH_TAB.Repeat(tabs - 1) + GRAPH_CONN + cont
                );
            }
        }
        private void PrintGTab(string cont, int tabs)
        => WriteLine(GRAPH_TAB.Repeat(tabs) + cont);

        //DEBUG TAST
        private void DebugTAST(TAST TAST)
        {
            WriteLine(">> TAST (Abstract Stratified Token Tree): ");

            //LOOP
            Stack<int> realms = [];
            int tabs = 0;

            void printNode(in TASTNode node)
            {
                var info = TAST.InfoAt(node.Id);
                var newRealm = realms.Peek() != info.RealmId;

                //HEADER
                printHeader(node.Id, newRealm);
                printContent(node);

                //BODY
                tabs++;
                if (newRealm)
                    realms.Push(info.RealmId);

                var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
                while (childExists)
                {
                    printNode(child);
                    childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
                }

                tabs--;
                if (newRealm)
                    realms.Pop();
            }
            void printHeader(int nodeId, bool newRealm)
            {
                if (nodeId == 0)
                {
                    PrintGConn("VIRTUAL ROOT", tabs);
                    return;
                }
                var info = TAST.InfoAt(nodeId);
                string header = "";

                if (newRealm)
                    header += $"Entering {ParserManager.Realms[info.RealmId]} ";

                header += $"<{nodeId:D3}> ";

                if (info.IsRewritten)
                    header += "REWRITTEN ";

                if (TAST.TryGetApplyRule(nodeId, out var inst))
                    header += ParserManager.GetRuleName(inst.RuleId);
                else
                    header += "GROUP";

                PrintGConn(header, tabs);
            }
            void printContent(in TASTNode node)
            {
                //**TEXT**
                var start = TAST.TokenAt(node.Start);
                var end = TAST.TokenAt(node.Start + node.Length - 1);

                PrintGTab($"//Text: {Source.Interval(start.Start, end.Start + end.Length)}", tabs);

                //**TOKENS**
                List<int?> tokens = [];

                var childExists = TAST.TryNodeAt(node.FirstChildId, out var child);
                int i = 0;
                while (i < node.Length)
                {
                    if (childExists && child.RelStart == i)
                    {
                        tokens.Add(null);
                        i += child.RelLength;

                        childExists = TAST.TryNodeAt(child.NextSiblingId, out child);
                        continue;
                    }

                    tokens.Add(node.Start + i);
                    i++;
                }

                //TOKEN SPAN
                List<(int start, int end)> spans = [];
                int lastStart = -1, lastEnd = -1;

                int j = 0;
                while (j < tokens.Count)
                {
                    var nullId = tokens[j];
                    if (nullId is not int id)
                    {
                        j++;
                        continue;
                    }

                    if (lastStart < 0)
                        lastStart = id;

                    lastEnd = id;

                    j++;
                    if (j >= tokens.Count || tokens[j] == null)
                    {
                        spans.Add((lastStart, lastEnd));
                        lastStart = lastEnd = -1;
                    }
                }

                var listSpans = spans.Select(span =>
                {
                    if (span.start == span.end)
                        return $"{span.start:D3}";

                    return $"{span.start:D3} - {span.end:D3}";
                });
                PrintGTab($"//Tokens: ({string.Join(", ", listSpans)})", tabs);

                //TOKENS
                var listTokens = tokens.Select(nullId =>
                {
                    if (nullId is not int id)
                        return "...";

                    var token = TAST.TokenAt(id);
                    if (token.Type == Token.NEWLINE)
                        return "NEWLINE";

                    return $"{LexerManager.TokenTypes[token.Type]} \"{TAST.GetText(id)}\"";
                });
                PrintGTab($"//{string.Join(", ", listTokens)}", tabs);
            }

            realms.Push(0);
            printNode(TAST.Root);
            WriteLine();
        }

        //DEBUG LOGS
        private void DebugLogs(string group, GroupDiagnostics diag)
        {
            Write(">> COMPILER LOGS: ");
            if (diag.Reports.Count <= 0)
            {
                WriteLine("NONE\n");
            }

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
                Source.Print(0, ConsoleColor.Blue, [.. sysEntries
                    .DistinctBy(e => e.Span.Start)
                    .OrderBy(e => e.Span.Start)
                    .Select(e => e.Span)
                ]);
            }

            //OTHER ENTRIES
            foreach (var entry in entries)
            {
                DebugTitle(group, entry);
                Source.Print(0, ConsoleColor.Blue, entry.Span);
            }
        }
        private void DebugTitle(string group, DiagnosticEntry entry)
        {
            (var defColor, Console.ForegroundColor) = (Console.ForegroundColor, ConsoleColor.Red);
            WriteLine($"[{group} {entry.Caller ?? "SYSTEM"}] {entry.Message}:");

            Console.ForegroundColor = defColor;
        }
    }

    public class DebugSource
    {
        private readonly SourceSpan Source;
        private readonly StringBuilder Stream;
        private readonly List<int> Lines = [0];

        public DebugSource(SourceSpan source, StringBuilder stream)
        {
            Stream = stream;

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
                        Stream.Append(tabs + $"{curLine + 1} | ");
                        newline = false;
                    }

                    char c = text[curPos];
                    if (c == '\n')
                    {
                        Stream.AppendLine();
                        curLine++;
                        newline = true;
                    }
                    else if (c != '\r')
                    {
                        Stream.Append(c);
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

            Stream.AppendLine("\n");
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
}