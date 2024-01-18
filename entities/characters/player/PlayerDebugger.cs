using Godot;

public partial class PlayerDebugger : Node
{
    public override void _Process(double delta)
    {
        var player = GetTree().CurrentScene.GetNode<Player>("%Player");

        if (Input.IsActionJustPressed("show_player_attributes_debug_info"))
        {
            GD.Print("----------------");
            GD.Print("姓名：" + player.CharacterData.Name);
            GD.Print("力量：" + player.CharacterData.Strength);
            GD.Print("体质：" + player.CharacterData.Constitution);
            GD.Print("敏捷：" + player.CharacterData.Agility);
            GD.Print("血量：" + player.CharacterData.Health);
            GD.Print("最大血量：" + player.CharacterData.MaxHealth);
            GD.Print("攻击：" + player.CharacterData.Attack);
            GD.Print("防御：" + player.CharacterData.Defend);
            GD.Print("闪避：" + player.CharacterData.Dodge);
            GD.Print("暴击：" + player.CharacterData.Crit);
            GD.Print("----------------");
        }

        if (Input.IsActionJustPressed("show_player_inventory_debug_info"))
        {
            GD.Print("----------------");
            GD.Print("背包：");
            foreach (var item in (player.CharacterData as PlayerData).Inventory)
            {
                GD.Print("* " + item.Name_);
            }
            GD.Print("----------------");
        }
    }
}