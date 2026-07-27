using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;

namespace PipeLoom.Engine;

public class WeavePlan
{
    public IPipeLoomEngine Engine { get; }
    public WeaveNode RootNode { get; }

    public IReadOnlyCollection<WeaveNode> Nodes => _nodes;

    public bool IsFused => _nodes.Count > 0 && _nodes.All(d => d.IsFused);

    public PlTypeDef OutputType => this.RootNode.ReturnType;
    
    private readonly HashSet<WeaveNode> _nodes = [];
    
    public WeavePlan(IPipeLoomEngine engine)
    {
        this.Engine = engine;
        this.RootNode = new WeaveNode(this, "sequence");
    }

    public async ValueTask Fuse<TOutput>()
    {
        foreach (var weaveNode in this.Nodes)
        {
            weaveNode.ResetFuse(true);
        }
        
        this.RootNode.RequiredReturnType = this.Engine.TypeOf<TOutput>();
        await this.RootNode.Fuse();
    }

    internal void AddNode(WeaveNode node)
    {
        _nodes.Add(node);
    }
}