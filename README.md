```
NetHackLike
├─ assets
│  └─ textures
│     ├─ quale's_spritesheet.png
│     ├─ quale's_spritesheet.png.import
│     ├─ tile_sheet.png
│     ├─ tile_sheet.png.import
│     ├─ vonwaon_bitmap_16px.ttf
│     └─ vonwaon_bitmap_16px.ttf.import
├─ components
│  ├─ IComponent.cs
│  └─ movement_component
│     ├─ MovementComponent.cs
│     └─ movement_component.tscn
├─ entities
│  ├─ characters
│  │  ├─ Character.cs
│  │  ├─ character.tscn
│  │  ├─ enemy
│  │  │  ├─ Enemy.cs
│  │  │  ├─ enemy.tscn
│  │  │  ├─ goblin
│  │  │  │  └─ goblin.tscn
│  │  │  ├─ orc
│  │  │  │  └─ orc.tscn
│  │  │  ├─ rat
│  │  │  │  └─ rat.tscn
│  │  │  ├─ skeleton
│  │  │  │  └─ skeleton.tscn
│  │  │  ├─ skeleton_king
│  │  │  │  └─ skeleton_king.tscn
│  │  │  └─ skeleton_magician
│  │  │     └─ skeleton_magician.tscn
│  │  └─ player
│  │     ├─ Player.cs
│  │     └─ player.tscn
│  └─ IEntity.cs
├─ managers
│  ├─ enemy_spawner
│  │  ├─ EnemySpawner.cs
│  │  └─ enemy_spawner.tscn
│  ├─ fsm
│  │  ├─ Fsm.cs
│  │  ├─ fsm.tscn
│  │  └─ game_states
│  │     ├─ action_state
│  │     │  ├─ ActionState.cs
│  │     │  └─ action_state.tscn
│  │     ├─ combat_state
│  │     │  ├─ CombatState.cs
│  │     │  └─ combat_state.tscn
│  │     ├─ IGameState.cs
│  │     ├─ start_state
│  │     │  ├─ StartState.cs
│  │     │  └─ start_state.tscn
│  │     └─ wait_for_input_state
│  │        ├─ WaitForInputState.cs
│  │        └─ wait_for_input_state.tscn
│  ├─ IManager.cs
│  ├─ input_handler
│  │  ├─ InputHandler.cs
│  │  └─ input_handler.tscn
│  └─ map_manager
│     ├─ MapManager.cs
│     ├─ map_generators
│     │  ├─ dungeon_generator
│     │  │  ├─ DungeonGenerator.cs
│     │  │  └─ dungeon_generator.tscn
│     │  └─ IMapGenerator.cs
│     └─ map_manager.tscn
├─ NetHackLike.csproj
├─ NetHackLike.csproj.old
├─ NetHackLike.sln
├─ project.godot
├─ README.md
├─ resources
│  ├─ map_data
│  │  ├─ dungeon_data
│  │  │  ├─ DungeonData.cs
│  │  │  └─ dungeon_data.tres
│  │  └─ MapData.cs
│  └─ tile_sets
│     └─ dungeon_tile_set.tres
└─ scenes
   ├─ Main.cs
   └─ main.tscn

```