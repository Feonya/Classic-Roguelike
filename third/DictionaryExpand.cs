using Godot;
using Godot.Collections;


public static class DictionaryExpand
{
    public static bool IsNullOrEmpty<[MustBeVariant] TKey, [MustBeVariant] TValue>(this Dictionary<TKey,TValue>  dictionary)
    {
        return dictionary == null || dictionary.Count == 0;
    }
}
