using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler.Model;

//VIR: Virtual Intermediate Representation
//>>>> NODE ATTACHMENTS <<<<
public class VIR() : VAssemblyEdit(-1)
{
    private Dictionary<int, FileNodeId> _sourceByNode = [];
    public void SetSourceNode(int nodeId, FileNodeId sourceId)
    => _sourceByNode[nodeId] = sourceId;

    public bool TryGetSourceNode(int nodeId, out FileNodeId sourceId)
    => _sourceByNode.TryGetValue(nodeId, out sourceId);
    public bool HasSourceNode(int nodeId)
    => _sourceByNode.ContainsKey(nodeId);
    public FileNodeId GetSourceNode(int nodeId)
    => _sourceByNode[nodeId];
}