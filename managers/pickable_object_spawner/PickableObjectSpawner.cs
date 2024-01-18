using Godot;
using Godot.Collections;

public partial class PickableObjectSpawner : Node, IManager, ILoadable
{
    private SaveLoadManager _saveLoadManager;

    private Node _pickableObjectContainer;

    public void Initialize()
    {
        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        _pickableObjectContainer = GetTree().CurrentScene.GetNode<Node>("%PickableObjectContainer");

        InitializeByLoadedData();
    }

    public void Update()
    {
    }

    public bool InitializeByLoadedData()
    {
        if (_saveLoadManager.LoadedData == null ||
            _saveLoadManager.LoadedData.Count == 0 ||
            !_saveLoadManager.LoadedData.ContainsKey("maps"))
        {
            return false;
        }

        var maps = _saveLoadManager.LoadedData["maps"].AsGodotArray<Dictionary<string, Variant>>();
        for (int i = 0; i < maps.Count; i++)
        {
            var map = maps[i];
            if (map["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                var pickableObjects = map["pickable_objects"]
                    .AsGodotArray<Dictionary<string, Variant>>();
                for (int j = 0; j < pickableObjects.Count; j++)
                {
                    var pickableObject = pickableObjects[j];

                    var pickableObjectInstance = GD.Load<PackedScene>(
                        pickableObject["scene_path"].AsString()
                    ).Instantiate<PickableObject>();
                    _pickableObjectContainer.AddChild(pickableObjectInstance);

                    pickableObjectInstance.GlobalPosition = pickableObject["position"].AsVector2();

                    pickableObjectInstance.Initialize();
                }

                return true;
            }
        }

        return false;
    }
}
