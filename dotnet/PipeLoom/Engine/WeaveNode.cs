using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipeLoom.Engine.Abstractions;
using PipeLoom.Engine.Abstractions.Errors;
using PipeLoom.Operators.Abstractions.Handlers;

namespace PipeLoom.Engine;

public sealed class WeaveNode : IWeaveNode
{
    public WeavePlan Plan { get; }
    public string OperatorName { get; }
    
    internal int Index { get; set; }
    
    public bool IsFused { get; private set; }
    
    public IPipeLoomEngine Engine => this.Plan.Engine;

    public IReadOnlyList<WeaveNode> Children => _children;
    IReadOnlyList<IWeaveNode> IWeaveNode.Children => this.Children;

    public IEnumerable<WeaveNode> Arguments => _children.Where(d => d.IsArgument);
    
    public WeaveNode? Parent { get; private set; }

    public bool IsArgument => this.IsEnabled && !this.IsFuseOnly && (this.IsForcedArgument || !this.IsVoid);
    
    internal bool IsFuseOnly { get; private set; }
    internal bool IsVoid { get; set; }
    internal bool IsEnabled { get; set; }
    internal bool IsForcedArgument { get; set; }

    public Variant ImplicitValue { get; private set; } = Variant.Undefined;
    
    internal PlTypeDef? RequiredReturnType { get; set; }
    
    internal PlTypeDef? NarrowedReturnType { get; set; }
    
    public PlTypeDef ReturnType => this.NarrowedReturnType ?? this.RequiredReturnType ?? this.Engine.WellKnown.Void;
    
    internal PlOperatorClass OperatorClass { get; private set; }
    
    internal OperatorHandler? Handler { get; private set; }

    public PlTypeDef? CarryType
    {
        get => field ?? this.Parent?.CarryType;
        set;
    }

    internal bool HasCarry => this.CarryType is not null;
    
    private readonly List<WeaveNode> _children = [];
    
    public WeaveNode(WeavePlan plan, string operatorName, WeaveNode? parent = null)
    {
        this.Plan = plan;
        this.Parent = parent;
        this.OperatorName = operatorName;
        this.OperatorClass = this.Engine.GetOperatorClass(operatorName);
        
        if (parent is not null)
        {
            parent._children.Add(this);
            this.Index = parent._children.Count - 1;
        }
        
        plan.AddNode(this);
        
        this.ResetFuse();
    }

    public int CountArguments()
    {
        var res = 0;
        var count = _children.Count;
        for (var i = 0; i < count; i++)
        {
            res += _children[i].IsArgument ? 1 : 0;
        }

        return res;
    }

    public WeaveNode MoveToFirst()
    {
        this.CheckFuse();

        if (this.Parent is null)
            return this;

        this.Parent._children.Remove(this);
        this.Parent._children.Insert(0, this);
        
        this.ReIndexChildren();
        
        return this;
    }
    
    public WeaveNode WrapChildren(string operatorName)
    {
        this.CheckFuse();

        var tmp = _children.ToArray();
        _children.Clear();
        
        var wrapper = new WeaveNode(this.Plan, operatorName, this);
        wrapper._children.AddRange(tmp);
        wrapper.ReIndexChildren();
        
        foreach (var child in wrapper._children)
        {
            child.Parent = wrapper;
        }
        
        return wrapper;
    }
    
    public WeaveNode AppendOperator(string operatorName)
    {
        this.CheckFuse();
        return new WeaveNode(this.Plan, operatorName, this);
    }

    public WeaveNode AppendValue<T>(T value)
    {
        this.CheckFuse();
            
        var node = this.AppendOperator("const");
        node.ImplicitValue = Variant.From(value, this.Engine);

        return node;
    }
    
    public async ValueTask Fuse()
    {
        this.ResetFuse();
        
        var preFuseFlags = await this.OperatorClass.PreFuse(this);

        if (this.IsFuseOnly)
        {
            return;
        }

        if (!preFuseFlags.HasFlag(PreFuseFlags.SkipChildFuse))
        {
            foreach (var child in _children)
            {
                await child.Fuse();
            }
        }

        this.Handler = this.OperatorClass.ChooseHandler(this);
        
        if (this.Handler is null)
        {
            var expectedTypeDesc =
                $"({string.Join(',', _children.Select(d => d.ReturnType.Name).Prepend(this.CarryType?.Name ?? ""))}) : {this.ReturnType.Name}";
            
            throw new PipeLoomException($"Can't find handler for {this.OperatorName}: {expectedTypeDesc}");
        }
        
        this.NarrowedReturnType = this.IsVoid
            ? this.Engine.WellKnown.Void
            : this.Handler.NarrowReturnType(this);
        
        await this.OperatorClass.PostFuse(this);

        if (this.RequiredReturnType is not null && !this.ReturnType.IsConvertibleTo(this.RequiredReturnType))
        {
            throw new PipeLoomException($"A node of '{this.OperatorName}' tries to return a type of '{this.ReturnType.Name}', but it is expected to provide '{this.RequiredReturnType.Name}'");
        }
        
        this.IsFused = true;
    }

    internal void ResetFuse(bool clearRequirements = false)
    {
        if (clearRequirements)
        {
            this.RequiredReturnType = null;
        }
        
        this.Handler = null;
        this.NarrowedReturnType = null;

        this.IsFuseOnly = this.OperatorClass.IsFuseOnly;
        this.IsVoid = this.OperatorClass.IsVoid;
        this.IsEnabled = !this.IsFuseOnly;
        
        this.IsFused = false;
    }

    private void CheckFuse()
    {
        if (this.IsFused)
            throw new PipeLoomException("WeaveNode is already fused");
    }

    private void ReIndexChildren()
    {
        for (var i = 0; i < _children.Count; i++)
        {
            _children[i].Index = i;
        }
    }
}