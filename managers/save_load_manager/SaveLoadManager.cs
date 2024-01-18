using Godot;
using Godot.Collections;

public partial class SaveLoadManager : Node, IManager
{
    private MapManager _mapManager;

    private Player _player;

    private string _savedFilePath = "user://classic_roguelike.sav";

    private Dictionary<string, Variant> _loadedData;

    public Dictionary<string, Variant> LoadedData { get => _loadedData; }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            TryDeleteSavedFile();
        }
    }

    public void Initialize()
    {
        _mapManager = GetTree().CurrentScene.GetNode<MapManager>("%MapManager");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");

        _loadedData = Load();
    }

    public void Update()
    {
    }

    public void TryDeleteSavedFile()
    {
        if (!FileAccess.FileExists(_savedFilePath)) { return; }

        var savedFile = FileAccess.Open(_savedFilePath, FileAccess.ModeFlags.Read);

        var absoluteFilePath = savedFile.GetPathAbsolute();

        savedFile.Dispose();

        GD.Print("删除存档文件：" + absoluteFilePath);
        System.IO.File.Delete(absoluteFilePath);
    }

    public void Save()
    {
        var dataForSave = new Dictionary<string, Variant>();
        // {
        //     { "player", [Dictionary] },
        //     { "maps", [Array<Dictionary>]}
        // }

        CheckDataBeforeSave(ref dataForSave);

        var savedFileForWrite = FileAccess.Open(_savedFilePath, FileAccess.ModeFlags.Write);

        var mapDataForSave = _mapManager.GetDataForSave();
        var playerDataForSave = _player.GetDataForSave();

        if (!dataForSave.ContainsKey("maps"))
        {
            dataForSave.Add("maps", new Array<Dictionary<string, Variant>>());
        }

        dataForSave["maps"].AsGodotArray<Dictionary<string, Variant>>().Add(mapDataForSave);
        dataForSave["player"] = playerDataForSave;

        savedFileForWrite.StoreVar(dataForSave, true);

        savedFileForWrite.Dispose();
    }

    private void CheckDataBeforeSave(ref Dictionary<string, Variant> dataForSave)
    {
        if (FileAccess.FileExists(_savedFilePath))
        {
            var savedFileForRead = FileAccess.Open(_savedFilePath, FileAccess.ModeFlags.Read);
            if (savedFileForRead.GetLength() > 0)
            {
                dataForSave = savedFileForRead.GetVar(true).AsGodotDictionary<string, Variant>();
                if (dataForSave.ContainsKey("maps"))
                {
                    var maps = dataForSave["maps"].AsGodotArray<Dictionary<string, Variant>>();
                    for (int i = 0; i < maps.Count; i++)
                    {
                        var sceneName = maps[i]["scene_name"].AsString();
                        if (sceneName == GetTree().CurrentScene.Name)
                        {
                            dataForSave["maps"].AsGodotArray().RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            savedFileForRead.Dispose();
        }
    }

    private Dictionary<string, Variant> Load()
    {
        if (!FileAccess.FileExists(_savedFilePath)) { return null; }

        var savedFile = FileAccess.Open(_savedFilePath, FileAccess.ModeFlags.Read);
        if (savedFile.GetLength() == 0)
        {
            savedFile.Dispose();
            return null;
        }

        var loadedData = savedFile.GetVar(true).AsGodotDictionary<string, Variant>();

        savedFile.Dispose();

        return loadedData;
    }
}
