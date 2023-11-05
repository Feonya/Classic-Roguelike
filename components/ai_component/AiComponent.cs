using System.Collections.Generic;
using Godot;

public partial class AiComponent : Node, IComponent
{
    private List<IAi> _aiList = new();

    public void Initialize()
    {
        for (int i = 0; i < GetChildCount(); i++)
        {
            var child = GetChild(i);

            if (child is not IAi) { continue; }

            var ai = child as IAi;
            ai.Initialize();

            _aiList.Add(ai);
        }
    }

    public void Update(double delta)
    {
        foreach (var ai in _aiList)
        {
            if (ai.Execute()) { break; }
        }
    }
}
