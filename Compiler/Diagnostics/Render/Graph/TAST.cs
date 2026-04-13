using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Diagnostics;

public partial class Render
{
    private void DebugTAST()
    {
        PrintSectionHeader("PARSER");
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
                header += $"Entering {Rules.Parser.RuleExt.RealmAt(Project, info.RealmId).Name} ";

            header += $"<{nodeId:D3}> ";

            if (info.IsAppended)
                header += "APPENDED ";
            if (info.IsRewritten)
                header += "REWRITTEN ";
            if (TAST.TryGetApplyRule(nodeId, out var inst))
                header += $"{Rules.Parser.RuleExt.GetRule(Project, inst.RuleId).Name} ";

            return header;
        }
        void printContent(in TASTNode node, string header)
        {
            bool isTextOriented = false;
            if (isTextOriented)
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
                var tokenType = Rules.Lexer.RuleExt.TokenTypeAt(Project, token.Type);

                var log = tokenType.Name;
                if (tokenType.ShowValue)
                    log += $" \"{TAST.GetText(id)}\"";

                return log;
            });
            PrintGTab($"<{string.Join(", ", listTokens)}>", tabs);
        }

        realms.Push(0);
        printNode(TAST.Root);
        WriteLine();
    }
}