using DrzSharp.Compiler.Lexer;
using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler
{
    public partial class Compiler
    {
        public static LexerProcess NewLexer() => LexerManager.NewProcess();

        public static void LexProject(DzProject project)
        {
            var lex = NewLexer();
            lex.LexProject(project);
            lex.EndProcess();
        }
        public static void LexFile(DzFile file)
        {
            var lex = NewLexer();
            lex.LexFile(file);
            lex.EndProcess();
        }
    }
}

namespace DrzSharp.Compiler.Lexer
{
    internal static class LexerManager
    {
        //RULES
        public static readonly List<LexerRule> _rules = [];

        //PROCESSES
        public static LexerProcess NewProcess() => new();
        public static void EndProcess(this LexerProcess process) { }
    }
}