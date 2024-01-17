using Godot;
using System;

public partial class AttributePanel : MarginContainer, IUi
{
    private PlayerData _playerData;

    private Label _levelLabel;
    private Label _experienceLabel;
    private Label _sightLabel;

    private Label _strengthLabel;
    private Label _constitutionLabel;
    private Label _agilityLabel;

    private Label _healthLabel;
    private Label _attackLabel;
    private Label _defendLabel;
    private Label _dodgeLabel;
    private Label _critLabel;


    public void Initialize()
    {
        _playerData = this.GetUnique<Player>().CharacterData as PlayerData;

        _levelLabel = GetLabel("Level");
        _experienceLabel = GetLabel("Experience");
        _sightLabel = GetLabel("Sight");

        _strengthLabel = GetLabel("Strength");
        _constitutionLabel = GetLabel("Constitution");
        _agilityLabel = GetLabel("Agility");

        _healthLabel = GetLabel("Health");
        _attackLabel = GetLabel("Attack");
        _defendLabel = GetLabel("Defend");
        _dodgeLabel = GetLabel("Dodge");
        _critLabel = GetLabel("Crit");

        InitializePlayerDataEvents();
        InitializeAttributeLabels();
    }
    private Label GetLabel(string name)
    {
        return GetNode<Label>($"%{name}Label");
    }
    public void Update()
    {
    }

    private void InitializePlayerDataEvents()
    {
        _playerData.LevelChanged += (int value) =>
        {
            _levelLabel.Text = "等级：" + value;
        };

        _playerData.ExperienceChanged += (float value) =>
        {
            _experienceLabel.Text = $"经验：{value.ToString("0.0")}/{_playerData.CurrentLevelUpExperienceThreshold.ToString("0.0")}";
        };

        _playerData.SightChanged += (int value) =>
        {
            _sightLabel.Text = "视野：" + value;
        };

        //---------------------------------------------

        _playerData.StrengthChanged += (int value) =>
        {
            _strengthLabel.Text = "力量：" + value;
        };

        _playerData.ConstitutionChanged += (int value) =>
        {
            _constitutionLabel.Text = "体质：" + value;
        };

        _playerData.AgilityChanged += (int value) =>
        {
            _agilityLabel.Text = "敏捷：" + value;
        };
        //---------------------------------------------

        _playerData.HealthChanged += (float value) =>
        {
            _healthLabel.Text = $"血量：{value.ToString("0.0")}/{_playerData.MaxHealth.ToString("0.0")}";
        };

        _playerData.AttackChanged += (float value) =>
        {
            _attackLabel.Text = "攻击：" + value.ToString("0.0");
        };

        _playerData.DefendChanged += (float value) =>
        {
            _defendLabel.Text = "防御：" + value.ToString("0.0");
        };
        _playerData.DodgeChanged += (float value) =>
        {
            _dodgeLabel.Text = "闪避：" + (value * 100f).ToString("0.0") + "%";
        };
        _playerData.CritChanged += (float value) =>
        {
            _critLabel.Text = "暴击：" + (value * 100f).ToString("0.0") + "%";
        };
    }

    private void InitializeAttributeLabels()
    {
        _levelLabel.Text = $"等级：{_playerData.Level}";
        _experienceLabel.Text = $"经验：{_playerData.Experience.ToString("0.0")}/{_playerData.CurrentLevelUpExperienceThreshold.ToString("0.0")}";
        _sightLabel.Text = $"视野：{_playerData.Sight}";

        _strengthLabel.Text = $"力量：{_playerData.Strength}";
        _constitutionLabel.Text = $"体质：{_playerData.Constitution}";
        _agilityLabel.Text = $"敏捷：{_playerData.Agility}";

        _healthLabel.Text = $"血量：{_playerData.Health.ToString("0.0")}/{_playerData.MaxHealth.ToString("0.0")}";
        _attackLabel.Text = $"攻击：{_playerData.Attack.ToString("0.0")}";
        _defendLabel.Text = $"防御：{_playerData.Defend.ToString("0.0")}";
        _dodgeLabel.Text = $"闪避：{(_playerData.Dodge * 100f).ToString("0.0")}%";
        _critLabel.Text = $"暴击：{(_playerData.Crit * 100f).ToString("0.0")}%";


    }

}