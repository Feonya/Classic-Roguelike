using Godot;
using Godot.Collections;

public partial class SaveLoadManager : Node, IManager
{
    private MapManager _mapManager;

    private Player _player;

    private string _persistentFilePath = "user://classic_roguelike.sav";

    private Dictionary<string, Variant> _persistentData;

    public Dictionary<string, Variant> PersistentData { get => _persistentData; }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            TryDeletePersistentFile();
        }
    }

    public void Initialize()
    {
        _mapManager = GetTree().CurrentScene.GetNode<MapManager>("%MapManager");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");

        _persistentData = Load();
    }

    public void Update(double delta)
    {
    }

    public void TryDeletePersistentFile()
    {
        if (!FileAccess.FileExists(_persistentFilePath)) { return; }

        var persistentFile = FileAccess.Open(_persistentFilePath, FileAccess.ModeFlags.Write);
        var absoluteFilePath = persistentFile.GetPathAbsolute();

        GD.Print("删除存档文件：" + absoluteFilePath);
        System.IO.File.Delete(absoluteFilePath);
    }

    public void Save()
    {
        var persistentData = new Dictionary<string, Variant>();
        // {
        //      { "maps", [Array<Dictionary>] },
        //      { "player", [Dictionary] },
        // }

        if (FileAccess.FileExists(_persistentFilePath))
        {
            var persistentFileForRead = FileAccess.Open(_persistentFilePath, FileAccess.ModeFlags.Read);

            if (persistentFileForRead.GetLength() > 0)
            {
                persistentData = persistentFileForRead.GetVar().AsGodotDictionary<string, Variant>();

                if (persistentData.ContainsKey("maps"))
                {
                    var scenes = persistentData["maps"].AsGodotArray<Dictionary<string, Variant>>();
                    for (int i = 0; i < scenes.Count; i++)
                    {
                        var sceneName = scenes[i]["scene_name"].AsString();
                        if (sceneName == GetTree().CurrentScene.Name)
                        {
                            persistentData["maps"].AsGodotArray().RemoveAt(i);
                            break;
                        }
                    }
                }

                if (persistentData.ContainsKey("player"))
                {
                    persistentData.Remove("player");
                }
            }

            persistentFileForRead.Dispose();
        }

        var persistentFileForWrite = FileAccess.Open(_persistentFilePath, FileAccess.ModeFlags.Write);

        var mapPersistentData = _mapManager.GetPersistentData();
        var playerPersistentData = _player.GetPersistentData();

        if (persistentData.ContainsKey("maps"))
        {
            persistentData["maps"].AsGodotArray<Dictionary<string, Variant>>().Add(mapPersistentData);
        }
        else
        {
            persistentData["maps"] = new Array<Dictionary<string, Variant>> { mapPersistentData };
        }
        persistentData["player"] = playerPersistentData;

        persistentFileForWrite.StoreVar(persistentData, true);

        persistentFileForWrite.Dispose();
    }

    public Dictionary<string, Variant> Load()
    {
        if (!FileAccess.FileExists(_persistentFilePath)) { return null; }

        var persistentFile = FileAccess.Open(_persistentFilePath, FileAccess.ModeFlags.Read);

        if (persistentFile.GetLength() == 0) { return null; }

        var persistentData = persistentFile.GetVar(true).AsGodotDictionary<string, Variant>();

        persistentFile.Dispose();

        return persistentData;
    }
}
