using DrzSharp.Compiler.Diagnostics;
using DrzSharp.Compiler.Model;
using DrzSharp.Compiler.Project;

namespace DrzSharp.Compiler.Parser;

public enum ParserPhase { SYNTACTIC, SEMANTIC }
public partial class ParserProcess
{
    //===== PARSING PROCESS =====
    public void ParseProject(DzProject project)
    {
        ParseProject(project, ParserPhase.SYNTACTIC);
        ParseProject(project, ParserPhase.SEMANTIC);
    }
    public void ParseProject(DzProject project, ParserPhase phase)
    {
        foreach (var file in project.Files)
            ParseFile(file, phase);
    }

    public void ParseFile(DzFile file)
    {
        ParseFile(file, ParserPhase.SYNTACTIC);
        ParseFile(file, ParserPhase.SEMANTIC);
    }
    public void ParseFile(DzFile file, ParserPhase phase)
    {
        //SYNTACTIC
        if (phase == ParserPhase.SYNTACTIC)
            Match(file);
        //SEMANTIC
        else
        {
            Validate(file);
            Emit(file);
        }
    }

    public void Reset()
    {
        File = null!;
    }

    //===== PARSER PHASE =====
    private DzFile File = null!;

    private GroupDiagnostics Diagnostics => File.Diagnostics.Parser;
    private TAST TAST => File.TAST;
    private TASI TASI => File.TASI;
}