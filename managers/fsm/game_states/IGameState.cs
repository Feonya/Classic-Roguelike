using System;

public interface IGameState
{
    public event Action Updated;

    public void Initialize();
    public void Update(double delta);
}