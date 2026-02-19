using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Lexer;

public partial class LexerProcess
{
    private const byte NEWLINE = 0;

    //LEX PROJECT
    public void LexProject(DzProject project)
    {
        foreach (var file in project.Files) LexFile(file);
    }

    //LEX FILE
    private DzFile File = null!;
    private TAST TAST => File.TAST;
    private int iter;
    public void LexFile(DzFile file)
    {
        File = file;
        var cont = file.Content;

        //STARTING NEWLINE
        NewToken(NEWLINE, -1, -1);

        //LOOP
        iter = 0;
        while (iter < cont.Length)
        {
            char c = cont[iter];

            //NEWLINE RULE
            if (c == '\n' || c == ';')
            {
                if (LastToken().Type != NEWLINE)
                    NewToken(NEWLINE, iter, 1);

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
                rule.TryMatch(this, cont.AsSpan(iter));

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
                //ADD AN ERROR TO THE PIPELINE OR SMTH
                iter++;
            }
        }

        //ENDING NEWLINE
        if (LastToken().Type != NEWLINE)
            NewToken(NEWLINE, -1, -1);

        //BUILD FLAT-TAST
        file.TAST.BuildFlatTAST();
    }
}