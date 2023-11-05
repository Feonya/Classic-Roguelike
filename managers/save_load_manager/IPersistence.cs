
using Godot;
using Godot.Collections;

public interface IPersistence
{
    public Dictionary<string, Variant> GetPersistentData();
}
