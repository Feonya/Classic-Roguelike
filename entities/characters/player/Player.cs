using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class Player : Character
{
    public override void Initialize()
    {
        base.Initialize();

        _mapManager.Initialized += On_MapManager_initialized;
    }

    private void On_MapManager_initialized(Vector2I playerStartCell,Func<Vector2I> _)
    {
        if (InitializeByLoadedData())
            return;

        GlobalPosition = playerStartCell * _mapData.CellSize + _mapData.CellSize /2 ;
    }

    private new bool InitializeByLoadedData()
    {
        if (!_saveLoadManager.IsInitialized("maps","player"))
            return false;

        var player = _saveLoadManager.player;
        if(_characterData is PlayerData playerData)
        {
            playerData.Level = player["level"].AsInt32();
            playerData.Experience = player["experience"].AsInt32();
        }
        var inventoryPickableObjects = player["inventory_pickable_objects"].AsGodotArray<string>();
        var inventoryEquipStates = player["inventory_equip_states"].AsGodotArray<bool>();

        for (int i = 0; i < inventoryPickableObjects.Count; i++)
        {
            var pickableObjectScenePath = inventoryPickableObjects[i];
            var pickableObjectEquipState = inventoryEquipStates[i];

            var pickableObjectInstance = GD.Load<PackedScene>(pickableObjectScenePath).Instantiate<PickableObject>();
            pickableObjectInstance.Visible = false;
            GetTree().CurrentScene.AddChild(pickableObjectInstance);
            pickableObjectInstance.Initialize();
            GetTree().CurrentScene.RemoveChild(pickableObjectInstance);

            (_characterData as PlayerData).Inventory.Add(pickableObjectInstance);
            if(pickableObjectEquipState)
            {
                (pickableObjectInstance as Equipment).EquipWithoutEffect();
            }
        }

        //恢复玩家位置
        var maps = _saveLoadManager.maps;
        foreach (var map in maps)
        {
            if (map["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                GlobalPosition = map["player_last_position"].AsVector2();
                return true;
            }
        }
        return false;
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

    protected override async void On_CombatManager_CharacterDied(Character character)
    {
        if(character != this || _isDead)
        {
            return;
        }
        GetNode<Sprite2D>("DeathSprite2D").Visible = true;

        var firstDeadEffectItem = (_characterData as PlayerData).Inventory.FirstOrDefault(
            item =>
             item is IDeadEffectItem
        );
        if (firstDeadEffectItem != null)
        {
            GD.Print("使用物品 即将复活！！");
            await (firstDeadEffectItem as IDeadEffectItem).DoDeadEffect();
            GetNode<Sprite2D>("DeathSprite2D").Visible = false;
            _isDead = false;
            return;
        }


        GD.Print("玩家被击败");
        _isDead = true;
    }
}
