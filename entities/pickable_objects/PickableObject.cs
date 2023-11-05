using Godot;
using Godot.Collections;

public partial class PickableObject : Node2D, IEntity, IPersistence
{
    protected MapData _mapData;

    [Export]
    protected string _name;
    [Export(PropertyHint.MultilineText)]
    protected string _description;

    protected TileMap _tileMap;
    protected Node _pickableObjectContainer;

    protected Player _player;

    public string Name_ { get => _name; }
    public string Description { get => _description; }

    public virtual void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _tileMap = GetTree().CurrentScene.GetNode<TileMap>("%TileMap");
        _pickableObjectContainer = GetTree().CurrentScene.GetNode<Node>("%PickableObjectContainer");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");
    }

    public virtual void Update(double delta)
    {
    }

    public Dictionary<string, Variant> GetPersistentData()
    {
        return new Dictionary<string, Variant>
        {
            { "scene_path", SceneFilePath },
            { "position", GlobalPosition }
        };
    }

    public void BePickedUp(Character character)
    {
        if (character is not Player) { return; }

        if (this is IImmediateEffectItem)
        {
            (this as IImmediateEffectItem).DoImmediateEffect();
        }

        var player = character as Player;
        (player.CharacterData as PlayerData).Inventory.Add(this);

        Visible = false;
        _pickableObjectContainer.RemoveChild(this);
    }

    public void BeDroppedDown(Character character)
    {
        if (character is not Player) { return; }

        if (this is IImmediateEffectItem)
        {
            (this as IImmediateEffectItem).UndoImmediateEffect();
        }

        var player = character as Player;
        (player.CharacterData as PlayerData).Inventory.Remove(this);

        QueueFree();
    }
}
