using System;
using Godot;
using Godot.Collections;

public partial class PlayerData : CharacterData
{
    public event Action<int> LevelChanged;
    public event Action<float> ExperienceChanged;

    protected int _level = 1;
    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            LevelChanged?.Invoke(value);
        }
    }
    protected float _experience = 0f;
    public float Experience
    {
        get => _experience;
        set
        {
            // 升级检测。
            var currentExperienceThreshold = CurrentLevelUpExperienceThreshold;
            if (value >= currentExperienceThreshold)
            {
                Level += 1;
                for (int i = 0; i < BaseAttributePointsGainPerLevelUp; i++)
                {
                    switch (GD.RandRange(0, 2))
                    {
                        case 0:
                            Strength += 1;
                            break;
                        case 1:
                            Constitution += 1;
                            break;
                        case 2:
                            Agility += 1;
                            break;
                    }
                }
                value -= currentExperienceThreshold;
            }

            _experience = value;
            ExperienceChanged?.Invoke(value);
        }
    }

    public float BaseLevelUpExperienceThreshold = 10f;
    public float LevelUpExperienceThresholdIncrementRate = 0.2f;
    public float CurrentLevelUpExperienceThreshold
    {
        get
        {
            var currentExperienceThreshold = BaseLevelUpExperienceThreshold;
            for (int i = 0; i < _level; i++)
            {
                currentExperienceThreshold +=
                    currentExperienceThreshold * LevelUpExperienceThresholdIncrementRate;
            }

            return currentExperienceThreshold;
        }
    }

    public int BaseAttributePointsGainPerLevelUp = 5;

    public Array<PickableObject> Inventory = new();

    public ILeftHandHoldEquipment LeftHandHoldEquipment;
    public IRightHandHoldEqiupment RightHandHoldEquipment;
    public IBodyWearEquipment BodyWearEquipment;
    public INeckWearEquipment NeckWearEquipment;
    public IFingerWearEquipment FingerWearEquipment;
}
