using Godot;
using System;

/// <summary>
/// 状态循环启动时首先进入本状态，并在本状态中初始化除状态机及状态模块外的所有其他Entity和Manager
/// 在切换到其他主场景前不会再次进入本状态。
/// </summary>
public partial class StartState : Node,IGameState
{

    public event Action Updated;

    private InputHandler _inputHandler;
    private MapManager _mapManager;
    private FogPainter _fogPainter;
    private Player _player;
    private EnemySpawner _enemySpawner;
    private AStarGridManager _aStarGridManager;
    private InventoryWindow _inventoryWindow;
    private AttributePanel _attributePanel;
    private StairManager _stairManager;
    private SaveLoadManager _saveLoadManager;
    private PickableObjectSpawner _pickableObjectSpawner;

    public async void Initialize()
    {
        GD.Print($"[{Engine.GetPhysicsFrames()}] 初始化Entity和Manager");

        
        _inputHandler = this.GetUnique<InputHandler>();
        _mapManager = this.GetUnique<MapManager>();
        _player = this.GetUnique<Player>();
        _enemySpawner = this.GetUnique<EnemySpawner>();
        _aStarGridManager = this.GetUnique<AStarGridManager>();
        _inventoryWindow = this.GetUnique<InventoryWindow>();
        _fogPainter = this.GetUnique<FogPainter>();
        _attributePanel = this.GetUnique<AttributePanel>();
        _stairManager = this.GetUnique<StairManager>();
        _saveLoadManager = this.GetUnique<SaveLoadManager>();
        _pickableObjectSpawner = this.GetUnique<PickableObjectSpawner>();

        _saveLoadManager.Initialize();
        _inputHandler.Initialize();
        _player.Initialize();
        _enemySpawner.Initialize();
        _pickableObjectSpawner.Initialize();
        _mapManager.Initialize();
        _inventoryWindow.Initialize();
        _attributePanel.Initialize();
        await ToSignal(GetTree(), "process_frame");
        _aStarGridManager.Initialize();
        _stairManager.Initialize();
        _fogPainter.Initialize();
    }

    public void Update()
    {
        Updated.Invoke();
    }


    //首先游戏会进入start state 并在下一帧进入input state
    //在input state中，状态机会等待玩家输入
    //如果玩家输入了，则进入action state
    //玩家将会在action state中进行一帧的非战斗行为比如移动、拾取等行为
    //然后会在下一帧进入combat state
    //所有符合的玩家和敌人会进入一回合的战斗，并结算伤害处理死亡。
    //随后游戏会再回到input state 如此循环
}
