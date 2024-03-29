using Godot;
using Godot.Collections;

public partial class MapScroll : Item, IConsumableItem
{
    public override void Initialize()
    {
        base.Initialize();

        _description = "使用后消除本地图战争迷雾。";
    }

    public void Consume()
    {
        var unexploredFogCells = new Array<Vector2I>();

        for (int y = 0; y < _mapData.MapSize.Y; y++)
        {
            for (int x = 0; x < _mapData.MapSize.X; x++)
            {
                var cell = new Vector2I(x, y);
                var tileData = _tileMap.GetCellTileData((int)TileMapLayer.Fog, cell);
                if (tileData.TerrainSet == (int)TerrainSet.Fog &&
                    tileData.Terrain == (int)FogTerrain.Unexplored)
                {
                    unexploredFogCells.Add(cell);
                }
            }
        }

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Fog,
            unexploredFogCells,
            (int)TerrainSet.Fog,
            (int)FogTerrain.OutOfSight
        );

        (_player.CharacterData as PlayerData).Inventory.Remove(this);
    }
}
