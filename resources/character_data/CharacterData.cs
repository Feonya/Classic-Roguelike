using System;
using Godot;
using Godot.Collections;

public partial class CharacterData : Resource
{
    public event Action<int> SightChanged;
    public event Action<int> StrengthChanged;
    public event Action<int> ConstitutionChanged;
    public event Action<int> AgilityChanged;
    public event Action<float> HealthChanged;
    public event Action<float> MaxHealthChanged;
    public event Action<float> AttackChanged;
    public event Action<float> DefendChanged;
    public event Action<float> DodgeChanged;
    public event Action<float> CritChanged;

    [Export]
    public string Name = "玩家";

    [Export]
    protected int _sight = 6;
    public int Sight
    {
        get => _sight;
        set
        {
            _sight = value;
            SightChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// 基础属性（Base Attributes）。
    /// </summary>
    [Export]
    protected int _strength = 8;
    public int Strength
    {
        get => _strength;
        set
        {
            var valueDifference = value - _strength;
            Attack += valueDifference * StrengthIncrementEffects["attack"];
            Defend += valueDifference * StrengthIncrementEffects["defend"];

            _strength = value;
            StrengthChanged?.Invoke(value);
        }
    }

    [Export]
    protected int _constitution = 8;
    public int Constitution
    {
        get => _constitution;
        set
        {
            var valueDifference = value - _constitution;
            Health += valueDifference * ConstitutionIncrementEffects["health"];
            MaxHealth += valueDifference * ConstitutionIncrementEffects["max_health"];

            _constitution = value;
            ConstitutionChanged?.Invoke(value);
        }
    }

    [Export]
    protected int _agility = 8;
    public int Agility
    {
        get => _agility;
        set
        {
            var valueDifference = value - _agility;
            Dodge += valueDifference * AgilityIncrementEffects["dodge"];
            Crit += valueDifference * AgilityIncrementEffects["crit"];

            _agility = value;
            AgilityChanged?.Invoke(value);
        }
    }

    [Export]
    public Dictionary<string, float> StrengthIncrementEffects = new()
    {
        { "attack", 2f }, { "defend", 1f }
    };

    [Export]
    public Dictionary<string, float> ConstitutionIncrementEffects = new()
    {
        { "max_health", 3f }, { "health", 3f }
    };

    [Export]
    public Dictionary<string, float> AgilityIncrementEffects = new()
    {
        { "dodge", 0.01f }, { "crit", 0.005f }
    };

    /// <summary>
    /// 战斗属性（Combat Attributes）。
    /// </summary>
    protected float _health;
    public float Health
    {
        get => _health;
        set
        {
            _health = value;
            HealthChanged?.Invoke(value);
        }
    }

    protected float _maxHealth;
    public float MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            MaxHealthChanged?.Invoke(value);
        }
    }

    protected float _attack;
    public float Attack
    {
        get => _attack;
        set
        {
            _attack = value;
            AttackChanged?.Invoke(value);
        }
    }

    protected float _defend;
    public float Defend
    {
        get => _defend;
        set
        {
            _defend = value;
            DefendChanged?.Invoke(value);
        }
    }

    protected float _dodge;
    public float Dodge
    {
        get => _dodge;
        set
        {
            _dodge = value;
            DodgeChanged?.Invoke(value);
        }
    }

    protected float _crit;
    public float Crit
    {
        get => _crit;
        set
        {
            _crit = value;
            CritChanged?.Invoke(value);
        }
    }
}
