using Godot;

public partial class DefeatWindow : CanvasLayer, IUi
{
    public void Initialize()
    {
        GetTree().CurrentScene.GetNode<Player>("%Player").DiedForSure += () => Visible = true;
    }

    public void Update()
    {
    }
}
