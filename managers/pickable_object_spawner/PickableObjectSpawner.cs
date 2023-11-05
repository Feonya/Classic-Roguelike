using Godot;
using Godot.Collections;

public partial class PickableObjectSpawner : Node, IManager
{
    private SaveLoadManager _saveLoadManager;

    private Node _pickableObjectContainer;

    public void Initialize()
    {
        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        _pickableObjectContainer = GetTree().CurrentScene.GetNode<Node>("%PickableObjectContainer");

        TrySpawnPickableObjectsByPersistentData();
    }

    public void Update(double delta)
    {
    }

    private void TrySpawnPickableObjectsByPersistentData()
    {
        if (_saveLoadManager.PersistentData == null ||
            _saveLoadManager.PersistentData.Count == 0 ||
            !_saveLoadManager.PersistentData.ContainsKey("maps"))
        {
            return;
        }

        var mapsPersistentData = _saveLoadManager
            .PersistentData["maps"].AsGodotArray<Dictionary<string, Variant>>();
        for (int i = 0; i < mapsPersistentData.Count; i++)
        {
            var mapPersistentData = mapsPersistentData[i];
            if (mapPersistentData["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                var pickableObjectsPersistentData =
                    mapPersistentData["pickable_objects"].AsGodotArray<Dictionary<string, Variant>>();
                for (int j = 0; j < pickableObjectsPersistentData.Count; j++)
                {
                    var pickableObjectPersistentData = pickableObjectsPersistentData[j];

                    var pickableObjectInstance = GD.Load<PackedScene>(
                        pickableObjectPersistentData["scene_path"].AsString()
                    ).Instantiate<PickableObject>();
                    _pickableObjectContainer.AddChild(pickableObjectInstance);

                    pickableObjectInstance.GlobalPosition =
                        pickableObjectPersistentData["position"].AsVector2();

                    pickableObjectInstance.Initialize();
                }
            }
        }
    }
}
