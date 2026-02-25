using DrzSharp.Compiler.Model;

namespace DrzSharp.Compiler.Default.Parser;

public static class Realms
{
    //VIRTUAL PHASE
    public static RealmId Virtual { get; internal set; }

    //LOGIC PHASE
    public static RealmId ASMLogic { get; internal set; }
    public static RealmId Logic { get; internal set; }
}