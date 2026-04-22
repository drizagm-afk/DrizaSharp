namespace DrzSharp.Compiler;

internal class AbortException(string? msg = null) : Exception(msg) { }