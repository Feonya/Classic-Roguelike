using System;
using System.Linq;
using Godot;
using Godot.Collections;

public partial class Player : Character
{
    public event Action Losing;

    public override void Initialize()
    {
        base.Initialize();

        _mapManager.Initialized += On_MapManager_Initialized;
        _combatManager.CharacterDied += On_CombatManager_CharacterDied;
    }

    public override Dictionary<string, Variant> GetPersistentData()
    {
        var persistentData = base.GetPersistentData();

        var playerData = _characterData as PlayerData;

        persistentData.Add("level", playerData.Level);
        persistentData.Add("experience", playerData.Experience);

        var inventoryPersistentData = new Array<string>();
        var inventoryEquipPersistentData = new Array<bool>();
        foreach (var pickableObject in playerData.Inventory)
        {
            inventoryPersistentData.Add(pickableObject.SceneFilePath);
            inventoryEquipPersistentData.Add(
                pickableObject is Equipment &&
                (pickableObject as Equipment).IsEquipped
            );
        }
        persistentData.Add("inventory", inventoryPersistentData);
        persistentData.Add("inventory_equip", inventoryEquipPersistentData);

        // persistentData.Add(
        //     "left_hand_hold_equipment",
        //     (playerData.LeftHandHoldEquipment as Equipment).SceneFilePath
        // );
        // persistentData.Add(
        //     "right_hand_hold_equipment",
        //     (playerData.RightHandHoldEquipment as Equipment).SceneFilePath
        // );
        // persistentData.Add(
        //     "body_wear_equipment",
        //     (playerData.BodyWearEquipment as Equipment).SceneFilePath
        // );
        // persistentData.Add(
        //     "finger_wear_equipment",
        //     (playerData.FingerWearEquipment as Equipment).SceneFilePath
        // );
        // persistentData.Add(
        //     "neck_wear_equipment",
        //     (playerData.NeckWearEquipment as Equipment).SceneFilePath
        // );

        return persistentData;
    }

    private void On_MapManager_Initialized(
        Vector2I playerStartCell, Callable GetEnemySpawnCell)
    {
        if (TryInitializePlayerOnMapManagerInititalizedByPersistentData()) { return; }

        GlobalPosition = playerStartCell * _mapData.CellSize + _mapData.CellSize / 2;
    }

    private bool TryInitializePlayerOnMapManagerInititalizedByPersistentData()
    {
        if (_saveLoadManager.PersistentData == null ||
            _saveLoadManager.PersistentData.Count == 0 ||
            !_saveLoadManager.PersistentData.ContainsKey("maps") ||
            !_saveLoadManager.PersistentData.ContainsKey("player"))
        {
            return false;
        }

        var playerPersistentData = _saveLoadManager
            .PersistentData["player"].AsGodotDictionary<string, Variant>();

        (_characterData as PlayerData).Level = playerPersistentData["level"].AsInt32();
        (_characterData as PlayerData).Experience = playerPersistentData["experience"].AsSingle();

        var inventoryPersistentData = playerPersistentData["inventory"].AsGodotArray<string>();
        var inventoryEquipPersistentData = playerPersistentData["inventory_equip"].AsGodotArray<bool>();
        for (int j = 0; j < inventoryPersistentData.Count; j++)
        {
            var pickableObjectScenePath = inventoryPersistentData[j];
            var pickableObjectEquip = inventoryEquipPersistentData[j];

            var pickableObjectInstance =
                GD.Load<PackedScene>(pickableObjectScenePath).Instantiate<PickableObject>();
            pickableObjectInstance.Visible = false;
            GetTree().CurrentScene.AddChild(pickableObjectInstance);
            pickableObjectInstance.Initialize();
            GetTree().CurrentScene.RemoveChild(pickableObjectInstance);

            (_characterData as PlayerData).Inventory.Add(pickableObjectInstance);

            if (pickableObjectEquip)
            {
                (pickableObjectInstance as IEquipableEquipment).EquipWithoutEffects();
            }
        }

        var mapsPersistentData = _saveLoadManager
            .PersistentData["maps"].AsGodotArray<Dictionary<string, Variant>>();
        for (int i = 0; i < mapsPersistentData.Count; i++)
        {
            var mapPersistentData = mapsPersistentData[i];
            if (mapPersistentData["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                GlobalPosition = mapPersistentData["player_last_position"].AsVector2();

                return true;
            }
        }

        return false;
    }

    private async void On_CombatManager_CharacterDied(Character character)
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

        Losing.Invoke();
    }
}
