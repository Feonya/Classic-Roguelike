using Godot;
using Godot.Collections;

public interface ISavable
{
    public Dictionary<string, Variant> GetDataForSave();
}