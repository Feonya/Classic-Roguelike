using Godot;
using Godot.Collections;
using System;

public partial class PickableObject : Node2D, IEntity, ISavable
{
    protected MapData _mapData;
    protected TileMap _tileMap;
    protected Player _player;

    [Export]
    protected string _name;
    [Export]
    protected string _description;

    public string Name_ => _name;
    public string Description => _description;

    public Dictionary<string, Variant> GetDataForSave()
    {
        return new Dictionary<string, Variant>
        {
            ["scene_path"] = SceneFilePath,
            ["position"] = GlobalPosition
        };
    }

    public virtual void Initialize()
    {
        _mapData = this.GetUnique<MapManager>().MapData;
        _tileMap = this.GetUnique<TileMap>();
        _player = this.GetUnique<Player>();
    }

    public virtual void Update()
    {
       
    }
}
