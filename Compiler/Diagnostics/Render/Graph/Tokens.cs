using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Diagnostics;

public partial class Render
{
    private void DebugTokens()
    {
        PrintSectionHeader("LEXER");
        WriteLine(">> TOKEN LIST: ");

        //LOOP
        for (int i = 0; i < TAST.TokenCount; i++)
        {
            var token = TAST.TokenAt(i);
            var tokenType = Rules.Lexer.RuleExt.TokenTypeAt(Project, token.Type);

            string cont = $"[{token.Id:D3} {tokenType.Name}] ";
            if (tokenType.ShowValue)
                cont += $"\"{TAST.GetText(token.Id)}\" ";
            cont += Source.Interval(token.Start, token.Start + token.Length);

            WriteLine(cont);
        }
        WriteLine();
    }
}