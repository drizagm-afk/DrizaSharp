namespace DrzSharp.Compiler.Model;

//>>>> REALMS <<<<
public static class Realms
{
    public const string VIRTUAL = "\0";
    public const byte VIRTUAL_ID = 0;
}
public readonly struct RealmData(string name)
{
    public readonly string Name = name;
}
public readonly record struct RealmKey(string Name);