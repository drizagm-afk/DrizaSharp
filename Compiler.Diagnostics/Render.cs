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
        public static void Debug(DzProject project, Config config)
        => new Render().DebugProject(project, config);

        public static void Debug(DzFile file, Config config)
        => new Render().DebugFile(file, config);
    }
}

namespace DrzSharp.Compiler.Diagnostics
{
    public class Render
    {
        private Config Config = null!;
        private StringBuilder Stream = null!;

        private DzFile File = null!;
        private TAST TAST => File.TAST;
        private TASI TASI => File.TASI;

        private DebugSource Source = null!;

        public void DebugProject(DzProject project, Config config)
        {
            Config = config;
            Stream = new();

            foreach (var file in project.Files)
            {
                File = file;
                Source = new(file.Content, Stream);

                Debug();
                Stream.AppendLine();
            }
            RenderDebug();
        }
        public void DebugFile(DzFile file, Config config)
        {
            Config = config;
            Stream = new();

            File = file;
            Source = new(File.Content, Stream);

            Debug();
            RenderDebug();
        }
        private void RenderDebug()
        {
            if (Config.Output == OutputMode.CONSOLE)
                Console.WriteLine(Stream.ToString());
            else
                System.IO.File.WriteAllText(@"C:\Driza\DrizaSharp\.dzdiag", Stream.ToString());
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
            {
                var showStruct = Config.Lexer.ShowTokenList;
                var showLogs = Config.Lexer.ShowLogs;

                if (showStruct || showLogs)
                    printTitle("LEXER DIAGNOSTICS");

                if (showStruct)
                    DebugTokens();
                if (showLogs)
                    DebugLogs("Lexer", File.Diagnostics.Lexer);
            }
            //PARSER
            {
                var showStruct = Config.Parser.ShowTAST != TASTMode.Hidden;
                var showLogs = Config.Parser.ShowLogs;

                if (showStruct || showLogs)
                    printTitle("PARSER DIAGNOSTICS");

                if (showStruct)
                    DebugTAST();
                if (showLogs)
                    DebugLogs("Parser", File.Diagnostics.Parser);
            }
            //LOWERER
            {
                var showStruct = Config.Lowerer.ShowTASI;
                var showLogs = Config.Lowerer.ShowLogs;

                if (showStruct || showLogs)
                    printTitle("LOWERER DIAGNOSTICS");

                if (showStruct)
                    DebugTASI();
                if (showLogs)
                    DebugLogs("Lowerer", File.Diagnostics.Lowerer);
            }
        }

        //DEBUG TOKENS
        private void DebugTokens()
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
        private void DebugTAST()
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
                var header = printHeader(node.Id, newRealm);
                printContent(node, header);

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
            string printHeader(int nodeId, bool newRealm)
            {
                if (nodeId == 0)
                    return "VIRTUAL ROOT ";
                
                var info = TAST.InfoAt(nodeId);
                string header = "";

                if (newRealm)
                    header += $"Entering {ParserManager.Realms[info.RealmId]} ";

                header += $"<{nodeId:D3}> ";

                if (info.IsRewritten)
                    header += "REWRITTEN ";

                if (TAST.TryGetApplyRule(nodeId, out var inst))
                    header += ParserManager.GetRuleName(inst.RuleId) + " ";

                return header;
            }
            void printContent(in TASTNode node, string header)
            {
                if (Config.Parser.ShowTAST == TASTMode.TextOriented)
                {
                    //**TEXT**
                    var start = TAST.TokenAt(node.Start);
                    var end = TAST.TokenAt(node.Start + node.Length - 1);

                    PrintGConn($"{header}{Source.Interval(start.Start, end.Start + end.Length)}", tabs);
                    return;
                }

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
                PrintGConn($"{header}[{string.Join(", ", listSpans)}]", tabs);

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
                PrintGTab($"<{string.Join(", ", listTokens)}>", tabs);
            }

            realms.Push(0);
            printNode(TAST.Root);
            WriteLine();
        }

        //DEBUG TASI
        private void DebugTASI()
        {
            WriteLine(">> TASI (Abstract Stratified Instruction Tree): ");

            int tabs = 0;
            PrintGConn("VIRTUAL ROOT", tabs);
            void printSibs(int nodeId)
            {
                if (TASI.TryNodeAt(nodeId, out var child))
                {
                    printSibs(child.NextSiblingId);
                    DebugTASI(child, ref tabs);
                }
            }
            printSibs(TASI.Root.FirstChildId);
            WriteLine();
        }
        private void DebugTASI(in TASINode node, ref int tabs)
        {
            tabs++;

            //HEADER
            var source = TASI.InfoAt(node.Id).SourceNodeId;
            PrintGConn($"From <{source:D3}> {ParserManager.GetRuleName(TAST.GetApplyRule(source).RuleId)}", tabs);

            //STACKING CHILDREN
            Stack<NodeRef> children = [];
            var childExists = TASI.TryNodeAt(node.FirstChildId, out var child);
            while (childExists)
            {
                children.Push(new(child.Id, child.RelIndex));
                childExists = TASI.TryNodeAt(child.NextSiblingId, out child);
            }

            //PRINTING
            for (int i = 0; i < node.Length; i++)
            {
                var instr = TASI.InstructionAt(node.Start + i);
                PrintGTab($"[{i}] {LowererManager._rules[instr.RuleId].Method.Name}", tabs);

                while (children.TryPeek(out var next) && next.RelIndex == i)
                {
                    children.Pop();
                    DebugTASI(TASI.NodeAt(next.NodeId), ref tabs);
                }
            }

            tabs--;
        }
        private readonly struct NodeRef(int nodeId, int relIndex)
        {
            public readonly int NodeId = nodeId;
            public readonly int RelIndex = relIndex;
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