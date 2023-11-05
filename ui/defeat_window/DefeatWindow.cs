using Godot;

public partial class DefeatWindow : CanvasLayer, IUi
{
    public void Initialize()
    {
        GetTree().CurrentScene.GetNode<Player>("%Player").Losing += () => Visible = true;
    }

    public void Update(double delta)
    {
    }
}