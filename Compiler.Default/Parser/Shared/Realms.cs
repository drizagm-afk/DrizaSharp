using DrzSharp.Compiler.Parser;

namespace DrzSharp.Compiler.Default.Parser;

public static class Realms
{
    //VIRTUAL PHASE
    public const int VIRTUAL = ParserRealms.VIRTUAL;

    //LOGIC PHASE
    public static int ASMLogic { get; internal set; }
    public static int Logic { get; internal set; }
}