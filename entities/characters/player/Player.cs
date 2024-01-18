using System;
using System.Linq;
using Godot;
using Godot.Collections;

public partial class Player : Character
{
    public event Action DiedForSure;

    public override void Initialize()
    {
        base.Initialize();

        _mapManager.Initialized += On_MapManager_Initialized;
    }

    public override Dictionary<string, Variant> GetDataForSave()
    {
        var playerDataForSave = base.GetDataForSave();

        var playerData = _characterData as PlayerData;

        playerDataForSave.Add("level", playerData.Level);
        playerDataForSave.Add("experience", playerData.Experience);

        var inventoryPickableObjects = new Array<string>();
        var inventoryEquipStates = new Array<bool>();
        foreach (var pickableObject in playerData.Inventory)
        {
            inventoryPickableObjects.Add(pickableObject.SceneFilePath);
            inventoryEquipStates.Add(
                pickableObject is Equipment && (pickableObject as Equipment).IsEquipped
            );
        }

        playerDataForSave.Add("inventory_pickable_objects", inventoryPickableObjects);
        playerDataForSave.Add("inventory_equip_states", inventoryEquipStates);

        return playerDataForSave;
    }

    private new bool InitializeByLoadedData()
    {
        if (_saveLoadManager.LoadedData == null ||
            _saveLoadManager.LoadedData.Count == 0 ||
            !_saveLoadManager.LoadedData.ContainsKey("maps") ||
            !_saveLoadManager.LoadedData.ContainsKey("player"))
        {
            return false;
        }

        var player = _saveLoadManager.LoadedData["player"].AsGodotDictionary<string, Variant>();

        (_characterData as PlayerData).Level = player["level"].AsInt32();
        (_characterData as PlayerData).Experience = player["experience"].AsInt32();

        var inventoryPickableObjects = player["inventory_pickable_objects"].AsGodotArray<string>();
        var inventoryEquipStates = player["inventory_equip_states"].AsGodotArray<bool>();

        for (int i = 0; i < inventoryPickableObjects.Count; i++)
        {
            var pickableObjectScenePath = inventoryPickableObjects[i];
            var pickableObjectEquipState = inventoryEquipStates[i];

            var pickableObjectInstance =
                GD.Load<PackedScene>(pickableObjectScenePath).Instantiate<PickableObject>();
            pickableObjectInstance.Visible = false;
            GetTree().CurrentScene.AddChild(pickableObjectInstance);
            pickableObjectInstance.Initialize();
            GetTree().CurrentScene.RemoveChild(pickableObjectInstance);

            (_characterData as PlayerData).Inventory.Add(pickableObjectInstance);

            if (pickableObjectEquipState == true)
            {
                (pickableObjectInstance as Equipment).EquipWithoutEffect();
            }
        }

        var maps = _saveLoadManager
            .LoadedData["maps"].AsGodotArray<Dictionary<string, Variant>>();
        for (int j = 0; j < maps.Count; j++)
        {
            var map = maps[j];
            if (map["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                GlobalPosition = map["player_last_position"].AsVector2();

                return true;
            }
        }

        return false;
    }

    private void On_MapManager_Initialized(Vector2I playerStartCell, Callable _)
    {
        if (InitializeByLoadedData()) { return; }

        GlobalPosition = playerStartCell * _mapData.CellSize + _mapData.CellSize / 2;
    }

    protected override async void On_CombatManager_CharacterDied(Character character)
    {
        if (character != this || _isDead) { return; }

        _isDead = true;

        GetNode<Sprite2D>("DeathSprite2D").Visible = true;

        var firstDeadEffectItem = (_characterData as PlayerData).Inventory.FirstOrDefault(
            item => item is IDeadEffectItem
        );
        if (firstDeadEffectItem != null)
        {
            GD.Print("玩家被击败！将在若干秒后复活！");

            await (firstDeadEffectItem as IDeadEffectItem).DoDeadEffect();
            GetNode<Sprite2D>("DeathSprite2D").Visible = false;
            _isDead = false;

            return;
        }

        GD.Print("玩家被击败！");

        DiedForSure.Invoke();
    }
}
