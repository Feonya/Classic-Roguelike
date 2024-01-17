using Godot;
using Godot.Collections;
using System;

public partial class PickableObjectSpawner : Node, IManager, ILoadable
{
    private SaveLoadManager _saveLoadManager;
    private Node _pickableObjectContainer;

    public void Initialize()
    {
        _saveLoadManager = this.GetUnique<SaveLoadManager>();
        _pickableObjectContainer = this.GetUnique("%PickableObjectContainer");
        InitializeByLoadedData();
    }



    public void Update()
    {

    }

    public bool InitializeByLoadedData()
    {
        if (!_saveLoadManager.IsInitialized("maps"))
            return false;

        var maps = _saveLoadManager.maps;
        foreach(var map in maps)
        {
            if (map["scene_name"].AsString() == this.GetCurrentSceneName())
            {
                var pickableObjects = map["pickable_objects"].AsGodotArray<Dictionary<string, Variant>>();
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
