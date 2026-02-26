using DrzSharp.Compiler.Lexer;
using DrzSharp.Compiler.Project;

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
    public delegate void Rule(Context ctx, ReadOnlySpan<char> content);
    internal static class LexerManager
    {
        //PROCESSES
        public static LexerProcess NewProcess() => new();
        public static void EndProcess(this LexerProcess process) { }

        //===== RULES =====
        public static readonly List<string> TokenTypes = ["NULL", "Newline"];
        public static readonly List<Rule> _rules = [];
    }
}