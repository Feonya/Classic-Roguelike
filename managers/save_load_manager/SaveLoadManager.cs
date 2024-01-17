using Godot;
using Godot.Collections;
using System;

public partial class SaveLoadManager : Node,IManager
{
    private MapManager _mapManager;
    private Player _player;
    private string _saveFilePath = "user://NETHACKLIKE.sav";
    private Dictionary<string, Variant> _loadedData;
    public Dictionary<string, Variant> LoadedData => _loadedData;
    public Array<Dictionary<string ,Variant>> maps
    {
        get
        {
            if (!IsInitialized("maps"))
                return null;
            return LoadedData["maps"].AsGodotArray<Dictionary<string, Variant>>();
        }
    }

    public override void _Notification(int what)
    {
        if(what == NotificationWMCloseRequest)
            TryDeleteSavedFile();
    }

    public Dictionary<string, Variant> player
    {
        get
        {
            if (!IsInitialized("maps","player"))
                return null;
            return LoadedData["player"].AsGodotDictionary<string, Variant>(); 
        }
    }

    public void Initialize()
    {
        _mapManager = this.GetUnique<MapManager>();
        _player = this.GetUnique<Player>();
        _loadedData = Load();
    }

    public bool IsInitialized(params string[] keys)
    {
       
        if (LoadedData.IsNullOrEmpty())
            return false;
        foreach (var key in keys)
            if (!LoadedData.ContainsKey(key))
                return false;
        return true;
    }

    public void Update()
    {
    }

    public void Save()
    {
        var dataForSave = new Dictionary<string, Variant>();

        CheckDataBeforeSave(ref dataForSave);

        var savedFileForWrite = FileAccess.Open(_saveFilePath, FileAccess.ModeFlags.Write);
        var mapDataForSave = _mapManager.GetDataForSave();
        var playerDataForSave = _player.GetDataForSave();

        if(!dataForSave.ContainsKey("maps"))
            dataForSave["maps"] = new Array<Dictionary<string, Variant>>();

        dataForSave["maps"].AsGodotArray<Dictionary<string, Variant>>().Add(mapDataForSave);
        dataForSave["player"] = playerDataForSave;

        savedFileForWrite.StoreVar(dataForSave, true);
        savedFileForWrite.Dispose();
    }

    private void CheckDataBeforeSave(ref  Dictionary<string, Variant> dataForSave)
    {
        if (FileAccess.FileExists(_saveFilePath))
        {
            var savedFileForRead = FileAccess.Open(_saveFilePath,FileAccess.ModeFlags.Read);
            if (savedFileForRead.GetLength() > 0)
            {
                dataForSave = savedFileForRead.GetVar(true).AsGodotDictionary<string,Variant>();
                if (dataForSave.ContainsKey("maps"))
                {
                    var maps = dataForSave["maps"].AsGodotArray<Dictionary<string,Variant>>();
                    for (int i = 0; i < maps.Count; i++)
                    {
                        var sceneName = maps[i]["scene_name"].AsString();
                        if(sceneName == GetTree().CurrentScene.Name)
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

    private Dictionary<string,Variant> Load()
    {
        if (!FileAccess.FileExists(_saveFilePath))
            return null;
        var savedFile = FileAccess.Open(_saveFilePath, FileAccess.ModeFlags.Read);
        if(savedFile.GetLength() == 0)
        {
            savedFile.Dispose();
            return null;
        }

        var loadedData = savedFile.GetVar(true).AsGodotDictionary<string,Variant>();
        savedFile.Dispose();
        return loadedData;
    }

    public void TryDeleteSavedFile()
    {
        if (!FileAccess.FileExists(_saveFilePath))
            return;
        var savedFile = FileAccess.Open(_saveFilePath, FileAccess.ModeFlags.Read);
        var absoluteFilePath = savedFile.GetPathAbsolute();
        savedFile.Dispose();

        GD.Print($"删除存档文件：{absoluteFilePath}");
        System.IO.File.Delete(absoluteFilePath);
    }
}
