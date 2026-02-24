using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Project;

namespace DrzSharp.Compiler.Lexer;

public partial class LexerProcess
{
    //LEX PROJECT
    public void LexProject(DzProject project)
    {
        foreach (var file in project.Files) LexFile(file);
    }
    public void LexFile(DzFile file)
    {
        File = file;
        Lex();

        Reset();
    }
    public void Reset()
    {
        File = null!;
    }

    //LEX FILE
    private DzFile File = null!;
    private TAST TAST => File.TAST;
    private GroupDiagnostics Diagnostics => File.Diagnostics.Lexer;

    private int iter;
    private void Lex()
    {
        var cont = File.Content;

        //STARTING NEWLINE
        NewToken(Token.NEWLINE, -1, -1);

        //LOOP
        iter = 0;
        while (iter < cont.Length)
        {
            char c = cont[iter];

            //NEWLINE RULE
            if (c == '\n' || c == ';')
            {
                if (LastToken().Type != Token.NEWLINE)
                    NewToken(Token.NEWLINE, iter, 1);

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
            for (int r = LexerManager._rules.Count - 1; r >= 0; r--)
            {
                var rule = LexerManager._rules[r];
                rule(this, cont.AsSpan(iter));

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
                Diagnostics.ReportUnexpected(new(iter, 1));
                iter++;
            }
        }

        //ENDING NEWLINE
        if (LastToken().Type != Token.NEWLINE)
            NewToken(Token.NEWLINE, -1, -1);

        //BUILD FLAT-TAST
        TAST.BuildFlatTAST();
    }
}