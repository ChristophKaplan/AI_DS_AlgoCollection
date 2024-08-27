using AI_DS_AlgoCollection.Utilities;

namespace AI_DS_AlgoCollection.DataStructures;

public class ItemSet<TDataType>
{
    public static readonly ItemSet<TDataType> EmptySet = new();

    public ItemSet(params TDataType[] items)
    {
        ItemList = new List<TDataType>(items);
        ItemList.Sort();
    }

    public List<TDataType> ItemList { get; set; } // need to be able to sort

    public bool IsEmptySet()
    {
        return ItemList.Count <= 0;
    }

    public ItemSet<TDataType> Clone()
    {
        return new ItemSet<TDataType>(ItemList.ToArray());
    }

    public bool Contains(ItemSet<TDataType> other)
    {
        return other.ItemList.All(t => ItemList.Contains(t));
    }

    public bool Contains(TDataType other)
    {
        return ItemList.Contains(other);
    }

    public bool Contains<TDataType>(TDataType other)
    {
        return Contains(other);
    }

    public List<ItemSet<TDataType>> GetPossibleSubsets(int maxLength)
    {
        return ItemList.GetPowerSet(maxLength).Select(itemList => new ItemSet<TDataType>(itemList.ToArray())).ToList();
    }

    public static ItemSet<TDataType> Union(ItemSet<TDataType> a, ItemSet<TDataType> b)
    {
        var joinedList = new HashSet<TDataType>(a.ItemList.Count + b.ItemList.Count);
        joinedList.UnionWith(a.ItemList);
        joinedList.UnionWith(b.ItemList);
        return new ItemSet<TDataType>(joinedList.ToArray());
    }

    public static ItemSet<TDataType> GetSubtract(ItemSet<TDataType> a, ItemSet<TDataType> b)
    {
        var result = new List<TDataType>(a.ItemList);
        var removeThis = new List<TDataType>();
        for (var i = 0; i < result.Count; i++)
        for (var j = 0; j < b.ItemList.Count; j++)
            if (result[i].Equals(b.ItemList[j]) && !removeThis.Contains(result[i]))
                removeThis.Add(result[i]);

        for (var i = 0; i < removeThis.Count; i++) result.Remove(removeThis[i]);

        return new ItemSet<TDataType>(result.ToArray());
    }

    public static bool IsEqualUntil(ItemSet<TDataType> p, ItemSet<TDataType> q, int until)
    {
        for (var i = 0; i < until; i++)
            if (!p.ItemList[i].Equals(q.ItemList[i]))
                return false;

        return true;
    }

    public override bool Equals(object obj)
    {
        var other = (ItemSet<TDataType>)obj;
        return Contains(other) && other.Contains(this);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override string ToString()
    {
        var s = "(";
        for (var i = 0; i < ItemList.Count - 1; i++) s += $"{ItemList[i]}, ";
        return $"{s}{ItemList[^1]})";
    }
}