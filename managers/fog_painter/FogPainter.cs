using Godot;
using Godot.Collections;
using System;

public partial class FogPainter : Node, ILoadable
{
	private MapData _mapData;
	private TileMap _tileMap;
	private Player _player;

	private Array<Vector2I> _previousInSightCells = new();
	private Array<Vector2I> currentInSightCells = new();

	private Node _enemyContainer;
	private SaveLoadManager _saveLoadManager;
	private Shadowcasting _shadowcasting;
	public async void Initialize()
	{
		_mapData = this.GetUnique<MapManager>().MapData;
		_tileMap = this.GetUnique<TileMap>();
		_player = this.GetUnique<Player>();
		_enemyContainer = this.GetUnique("%EnemyContainer");
		_saveLoadManager = this.GetUnique<SaveLoadManager>();
		_shadowcasting = new Shadowcasting(BlocksLights, SetVisibile, GetDistance);

		if (!InitializeByLoadedData())
		{
			FullFillWithUnexploredFog();
			await ToSignal(GetTree(), "process_frame");
			RefreshFog();
		}
	}

	public void Update()
	{
		RefreshFog();
	}

	/// <summary>
	/// 填满未探险区域
	/// </summary>
	private void FullFillWithUnexploredFog()
	{
		var allCells = new Array<Vector2I>();
		for (int y = 0; y < _mapData.MapSize.Y; y++)
		{
			for (int x = 0; x < _mapData.MapSize.X; x++)
			{
				allCells.Add(new Vector2I(x, y));
			}
		}

		_tileMap.SetCellsTerrainConnect(
				(int)TileMapLayer.Fog,
				allCells,
				(int)TerrainSet.Fog,
				(int)FogTerrain.Unexplored,
				false
			);
	}

	private void RefreshEnemiesVisbility()
	{
		for (int i = 0; i < _enemyContainer.GetChildCount(); i++)
		{
			var enemy = _enemyContainer.GetChild(i) as Node2D;
			var enemyCell = (enemy as Enemy).GetCell();
			var enemyTileData = _tileMap.GetCellTileData((int)TileMapLayer.Fog, enemyCell);
			if (enemyTileData.TerrainSet == (int)TerrainSet.Fog &&
				enemyTileData.Terrain == (int)FogTerrain.InSight)
			{
				enemy.Visible = true;
			}
			else
			{
				enemy.Visible = false;
			}
		}
	}

	private void RefreshFog()
	{
		currentInSightCells.Clear();
		var playerCell = _player.GetCell();
		var playerSight = _player.CharacterData.Sight;
		_shadowcasting.Compute(playerCell, playerSight);

		for (int i = 0; i < currentInSightCells.Count; i++)
		{
			_previousInSightCells.Remove(currentInSightCells[i]);
		}

		_tileMap.SetCellsTerrainConnect(
		(int)TileMapLayer.Fog,
		_previousInSightCells,
		(int)TerrainSet.Fog,
		(int)FogTerrain.OutOfSight,
		false
	);

		_tileMap.SetCellsTerrainConnect(
			(int)TileMapLayer.Fog,
			currentInSightCells,
			(int)TerrainSet.Fog,
			(int)FogTerrain.InSight,
			false
		);


		for (int i = 0; i < currentInSightCells.Count; i++)
		{
			_previousInSightCells.Add(currentInSightCells[i]);
		}
		RefreshEnemiesVisbility();

	}

	private bool BlocksLights(int x, int y)
	{
		if (x < 0 || y < 0 || x >= _mapData.MapSize.X || y >= _mapData.MapSize.Y)
			return true;
		var tile = _tileMap.GetCellTileData((int)TileMapLayer.Default, new Vector2I(x, y));
		if (tile.GetCollisionPolygonsCount(0) > 0)
			return true;
		return false;
	}

	private void SetVisibile(int x, int y)
	{
		currentInSightCells.Add(new Vector2I(x, y));
	}
	private int GetDistance(int x, int y)
	{
		int c = Mathf.Abs(x - y);
		int d = Mathf.Min(x, y);
		return c + d / 2 * 2 + (d - (d / 2));
	}


	public bool InitializeByLoadedData()
	{
		if (!_saveLoadManager.IsInitialized("maps"))
			return false;

		var maps = _saveLoadManager.maps;
		foreach (var map in maps)
		{
			if (map["scene_name"].AsString() == GetTree().CurrentScene.Name)
			{
				var unexploredFogCells = map["unexplored_fog_cells"].AsGodotArray<Vector2I>();
				var outOfSightFogCells = map["out_of_sight_fog_cells"].AsGodotArray<Vector2I>();
				var inSightFogCells = map["in_sight_fog_cells"].AsGodotArray<Vector2I>();

				_tileMap.SetCellsTerrainConnect
					(
						(int)TileMapLayer.Fog,
						unexploredFogCells,
						(int)TerrainSet.Fog,
						(int)FogTerrain.Unexplored,
						false
					);
				_tileMap.SetCellsTerrainConnect
				   (
					   (int)TileMapLayer.Fog,
					   outOfSightFogCells,
					   (int)TerrainSet.Fog,
					   (int)FogTerrain.OutOfSight,
					   false
				   );
				_tileMap.SetCellsTerrainConnect
				   (
					   (int)TileMapLayer.Fog,
					   inSightFogCells,
					   (int)TerrainSet.Fog,
					   (int)FogTerrain.InSight,
					   false
				   );


				return true;
			}
		}
		return false;
	}
}
