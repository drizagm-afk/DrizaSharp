using DrzSharp.Compiler.Core;

namespace DrzSharp.Compiler.Default.Parser;

public static class Realms
{
    //PHASE 0
    public static RealmId Virtual { get; internal set; }

    //PHASE 1
    public static RealmId ASMLogic { get; internal set; }
    public static RealmId Logic { get; internal set; }
}