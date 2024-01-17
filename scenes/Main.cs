using Godot;
using System;

public partial class Main : Node
{
	private Fsm _fsm;

	public override void _Ready()
	{
		RenderingServer.SetDefaultClearColor(Colors.Black);

		_fsm = GetNode<Fsm>("Fsm");
		_fsm.Initialize();
	}

	public override void _Process(double delta)
	{
		_fsm.Update();
	}
}

public enum PhysicsLayer
{
	BlockMovement = 1 << 0,
	BlockSight = 1 << 1,
	PickableObject = 1 << 2,
	Fog = 1 << 3,
}

public enum TileMapLayer
{
	Default = 0,
	Fog = 1,

}

public enum TerrainSet
{
	Default = 0,
	Fog = 1,
	Stair= 2,
}

public enum DungeonTerrain
{
	Floor = 0,
	Wall = 1,
}

public enum ForestTerrain
{
	Ground,
	Grass,
	Tree,
	DeadTree,
}


public enum FogTerrain
{
	Unexplored,
	OutOfSight,
	InSight,
}

public enum StairTerrain
{
	UpStair,
	DownStair,
}