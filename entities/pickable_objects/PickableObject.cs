using Godot;
using Godot.Collections;

public partial class PickableObject : Node2D, IEntity, ISavable
{
    protected MapData _mapData;

    protected TileMap _tileMap;

    protected Player _player;

    [Export]
    protected string _name;
    protected string _description;

    public string Name_ { get => _name; }
    public string Description { get => _description; }

    public virtual void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _tileMap = GetTree().CurrentScene.GetNode<TileMap>("%TileMap");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");
    }

    public virtual void Update()
    {
    }

    public Dictionary<string, Variant> GetDataForSave()
    {
        return new Dictionary<string, Variant>
        {
            { "scene_path", SceneFilePath },
            { "position", GlobalPosition }
        };
    }
}
