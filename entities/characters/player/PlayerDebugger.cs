using Godot;

public partial class PlayerDebugger : Node
{
    private Player _player;

    public override void _Ready()
    {
        _player = GetOwner<Player>();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("show_player_attributes_debug_info"))
        {
            GD.Print("---------------------------------");
            GD.Print("玩家属性：");
            GD.Print("Strength     - " + _player.CharacterData.Strength);
            GD.Print("Constitution - " + _player.CharacterData.Constitution);
            GD.Print("Agility      - " + _player.CharacterData.Agility);
            GD.Print("Health    - " + _player.CharacterData.Health);
            GD.Print("MaxHealth - " + _player.CharacterData.MaxHealth);
            GD.Print("Attack    - " + _player.CharacterData.Attack);
            GD.Print("Defend    - " + _player.CharacterData.Defend);
            GD.Print("Dodge     - " + _player.CharacterData.Dodge);
            GD.Print("Crit      - " + _player.CharacterData.Crit);
            GD.Print("---------------------------------");
        }

        if (Input.IsActionJustPressed("show_player_inventory_debug_info"))
        {
            GD.Print("---------------------------------");
            GD.Print("玩家背包：");
            foreach (var item in (_player.CharacterData as PlayerData).Inventory)
            {
                GD.Print("* " + item.Name_);
            }
            GD.Print("---------------------------------");
        }
    }
}
