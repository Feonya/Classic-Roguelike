using Godot;
using System;

public static class NodeExpand 
{
	public static T GetUnique<T>(this Node node) where T : Node
	{
		return node.GetTree().CurrentScene.GetNode<T>("%"+ typeof(T).Name);
	} 

	public static T GetUnique<T>(this Node node,string name) where T : Node
	{
		return node.GetTree().CurrentScene.GetNode<T>(name);
	}

    public static Node GetUnique(this Node node, string name) 
    {
        return node.GetTree().CurrentScene.GetNode<Node>(name);
    }

	public static string GetCurrentSceneName(this Node node)
	{
		return node.GetTree().CurrentScene.Name;
	}
	
}
