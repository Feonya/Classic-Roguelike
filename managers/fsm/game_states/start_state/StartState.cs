using System;
using Godot;

public partial class StartState : Node, IGameState
{
    public event Action Updated;

    private SaveLoadManager _saveLoadManager;
    private InputHandler _inputHandler;
    private MapManager _mapManager;
    private EnemySpawner _enemySpawner;
    private PickableObjectSpawner _pickableObjectSpawner;
    private AStarGridManager _aStarGridManager;
    private CombatManager _combatManager;
    private FogPainter _fogPainter;
    private StairManager _stairManager;
    private Player _player;
    private InventoryWindow _inventoryWindow;
    private VictoryWindow _victoryWindow;
    private DefeatWindow _defeatWindow;
    private AttributePanel _attributePanel;

    public async void Initialize()
    {
        // GD.Print("在这里初始化其他Manager或Entity");

        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");
        _inputHandler = GetTree().CurrentScene.GetNode<InputHandler>("%InputHandler");
        _mapManager = GetTree().CurrentScene.GetNode<MapManager>("%MapManager");
        _enemySpawner = GetTree().CurrentScene.GetNode<EnemySpawner>("%EnemySpawner");
        _pickableObjectSpawner = GetTree().CurrentScene.GetNode<PickableObjectSpawner>("%PickableObjectSpawner");
        _aStarGridManager = GetTree().CurrentScene.GetNode<AStarGridManager>("%AStarGridManager");
        _combatManager = GetTree().CurrentScene.GetNode<CombatManager>("%CombatManager");
        _fogPainter = GetTree().CurrentScene.GetNode<FogPainter>("%FogPainter");
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
        _combatManager.Initialize();
        _mapManager.Initialize();
        _fogPainter.Initialize();
        _inventoryWindow.Initialize();
        _victoryWindow.Initialize();
        _defeatWindow.Initialize();
        _attributePanel.Initialize();
        await ToSignal(GetTree(), "process_frame");
        _aStarGridManager.Initialize();
        _stairManager.Initialize();
    }

    public void Update(double delta)
    {
        Updated.Invoke();
    }
}