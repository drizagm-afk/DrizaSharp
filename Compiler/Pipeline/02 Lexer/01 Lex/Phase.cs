using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Rules.Lexer;

namespace DrzSharp.Compiler.Lexer;

public partial class LexerProcess
{
    //>>>> LEX PROJECT <<<<
    public partial void Lex()
    {
        foreach (var file in Project.Files)
            Lex(file);
    }

    //>>>> LEX FILE <<<<
    private int iter;
    private void Lex(DzFile file)
    {
        File = file;

        //STARTING NEWLINE
        NewToken(Tokens.NEWLINE, -1, 1);

        //LOOP
        iter = 0;
        while (iter < Source.Length)
        {
            char c = Source[iter];

            //NEWLINE RULE
            if (c == '\n' || c == ';')
            {
                if (LastToken().Type != Tokens.NEWLINE)
                    NewToken(Tokens.NEWLINE, iter, 1);

                iter++;
                continue;
            }
            //WHITESPACE RULE
            if (c == '\t' || c == '\r' || c == ' ')
            {
                iter++;
                continue;
            }

            //OTHER RULES
            bool match = false;
            int count = TokenCount;
            foreach(var rule in Project.Rules(Module))
            {
                rule(this, Source.AsSpan(iter));

                if (count < TokenCount)
                {
                    var ltk = LastToken();
                    iter = ltk.Start + ltk.Length;

                    match = true;
                    break;
                }
            }

            if (!match)
            {
                Diagnostics.ReportUnexpected(new(iter, 1), "Unexpected Characters");
                iter++;
            }
        }

        //ENDING NEWLINE
        if (LastToken().Type != Tokens.NEWLINE)
            NewToken(Tokens.NEWLINE, -2, 1);

        //BUILD FLAT-TAST
        TAST.BuildFlatTAST();
    }
}