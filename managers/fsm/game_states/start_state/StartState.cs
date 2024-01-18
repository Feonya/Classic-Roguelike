using System;
using Godot;

/// <summary>
/// 状态循环启动时首先进入本状态，并在本状态中初始化除状态机及状态模块外的所有其他Entity和Mananger，在切换到其他主场景前不会再次进入本状态。
/// </summary>
public partial class StartState : Node, IGameState
{
    public event Action Updated;

    private SaveLoadManager _saveLoadManager;
    private InputHandler _inputHandler;
    private MapManager _mapManager;
    private AStarGridManager _aStarGridManager;
    private FogPainter _fogPainter;
    private EnemySpawner _enemySpawner;
    private PickableObjectSpawner _pickableObjectSpawner;
    private StairManager _stairManager;
    private Player _player;
    private InventoryWindow _inventoryWindow;
    private VictoryWindow _victoryWindow;
    private DefeatWindow _defeatWindow;
    private AttributePanel _attributePanel;

    public async void Initialize()
    {
        // GD.Print("初始化Entiy和Manager");

        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");
        _inputHandler = GetTree().CurrentScene.GetNode<InputHandler>("%InputHandler");
        _mapManager = GetTree().CurrentScene.GetNode<MapManager>("%MapManager");
        _aStarGridManager = GetTree().CurrentScene.GetNode<AStarGridManager>("%AStarGridManager");
        _fogPainter = GetTree().CurrentScene.GetNode<FogPainter>("%FogPainter");
        _enemySpawner = GetTree().CurrentScene.GetNode<EnemySpawner>("%EnemySpawner");
        _pickableObjectSpawner = GetTree().CurrentScene.GetNode<PickableObjectSpawner>("%PickableObjectSpawner");
        _stairManager = GetTree().CurrentScene.GetNode<StairManager>("%StairManager");
        _player = GetTree().CurrentScene.GetNode<Player>("%Player");
        _inventoryWindow = GetTree().CurrentScene.GetNode<InventoryWindow>("%InventoryWindow");
        _victoryWindow = GetTree().CurrentScene.GetNode<VictoryWindow>("%VictoryWindow");
        _defeatWindow = GetTree().CurrentScene.GetNode<DefeatWindow>("%DefeatWindow");
        _attributePanel = GetTree().CurrentScene.GetNode<AttributePanel>("%AttributePanel");

        _saveLoadManager.Initialize();
        _inputHandler.Initialize();
        _player.Initialize();
        _enemySpawner.Initialize();
        _pickableObjectSpawner.Initialize();
        _mapManager.Initialize();
        _inventoryWindow.Initialize();
        _victoryWindow.Initialize();
        _defeatWindow.Initialize();
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
}
