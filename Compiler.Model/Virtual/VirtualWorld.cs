using System.Diagnostics;

namespace DrzSharp.Compiler.Model;

public class VirtualWorld
{
    public readonly Dictionary<string, VirtualAssembly> Assemblies = [];

    public VirtualAssembly EnsureAssembly(string assemblyName)
    {
        if (Assemblies.TryGetValue(assemblyName, out var val)) return val;

        val = new VirtualAssembly();
        Assemblies[assemblyName] = val;
        return val;
    }
}
public sealed class VirtualAssembly
{
    public readonly Dictionary<string, VirtualTree> Namespaces = [];

    public VirtualTree EnsureNamespace(string nmspaceName)
    {
        if (Namespaces.TryGetValue(nmspaceName, out var val)) return val;

        val = new VirtualTree();
        Namespaces[nmspaceName] = val;
        return val;
    }
}

public sealed class VirtualTree
{
    public VirtualTree()
    {
        Root = NodeAt(NewNode(NewInstance(0, "ROOT", new NamespaceArgs())));
    }

    //**INSTANCES**
    private readonly List<VirtualInstance> _instances = [];
    public int NewInstance(byte type, string name, InstanceArgs data)
    {
        _instances.Add(new(type, name, data));
        return _instances.Count - 1;
    }
    public VirtualInstance InstanceAt(int id)
    {
        Debug.Assert(id >= 0 && id < _instances.Count);
        return _instances[id];
    }

    //**NODES**
    private readonly List<VirtualNode> _nodes = [];
    public const int RootId = 0;
    public readonly VirtualNode Root;

    private int NewNode(
        int instanceId, int firstChildId = -1, int nextSiblingId = -1
    )
    {
        var count = _nodes.Count;
        VirtualNode node = new(count, instanceId, firstChildId, nextSiblingId);
        _nodes.Add(node);

        return node.Id;
    }
    public VirtualNode NodeAt(int id)
    {
        Debug.Assert(id >= 0 && id < _nodes.Count);
        return _nodes[id];
    }

    public void Update(
        int nodeId, int? instanceId = null,
        int? firstChildId = null, int? nextSiblingId = null
    )
    {
        var node = NodeAt(nodeId);
        VirtualNode newNode = new(
            nodeId, instanceId ?? node.InstanceId,
            firstChildId ?? node.FirstChildId, nextSiblingId ?? node.NextSiblingId
        );
        _nodes[nodeId] = newNode;
    }

    public int AddChild(int instanceId) => AddChild(RootId, instanceId);
    public int AddChild(int nodeId, int instanceId)
    {
        var node = NodeAt(nodeId);
        var childNodeId = NewNode(instanceId, nextSiblingId: node.FirstChildId);
        Update(nodeId, firstChildId: childNodeId);

        return childNodeId;
    }

    public IEnumerable<int> Children()
    { foreach (var child in Children(RootId)) yield return child; }
    public IEnumerable<int> Children(int nodeId)
    {
        var node = NodeAt(nodeId);
        var id = node.FirstChildId;
        while (id >= 0)
        {
            yield return id;
            id = NodeAt(id).NextSiblingId;
        }
    }

    public IEnumerable<int> Children(byte type)
    { foreach (var child in Children(RootId, type)) yield return child; }
    public IEnumerable<int> Children(int nodeId, byte type)
    {
        foreach (var child in Children(nodeId))
        {
            if (_instances[NodeAt(child).InstanceId].Kind == type) yield return child;
        }
    }
}
public readonly struct VirtualNode(
    int id, int instanceId,
    int firstChildId, int nextSiblingId
)
{
    public readonly int Id = id;
    public readonly int InstanceId = instanceId;
    public readonly int FirstChildId = firstChildId;
    public readonly int NextSiblingId = nextSiblingId;
}
public sealed class VirtualInstance(byte kind, string name, InstanceArgs data)
{
    public readonly byte Kind = kind;
    public readonly string Name = name;
    public readonly InstanceArgs data = data;
}
public interface InstanceArgs;
public readonly record struct NamespaceArgs : InstanceArgs;