using Godot;
using System;
using System.Collections.Generic;

public partial class AiComponent : Node, IComponent
{
    public List<IAi> _aiList = new List<IAi>();

    public void Initialize()
    {
        for (int i = 0; i < GetChildCount(); i++)
        {
            var child = GetChild(i);
            if(child is IAi ai)
            {
                ai.Initialize();
                _aiList.Add(ai);
            }
        }
    }

    public void Update()
    {
        foreach (var ai in _aiList)
        {
            if (ai.Execute())
                break;
        }
    }
}
