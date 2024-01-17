
using System;

public interface IGameState
{
    public void Initialize();
    public void Update();

    public event Action Updated;

}
