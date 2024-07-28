namespace AI_DS_AlgoCollection.DataStructures;

public class ItemSet<TItem> where TItem : IComparable {
    public List<TItem> ItemList { get; set; } // need to be able to sort

    public ItemSet(params TItem[] items) {
        ItemList = new List<TItem>(items);
        ItemList.Sort();
    }

    public bool Contains(ItemSet<TItem> other) => other.ItemList.All(t => ItemList.Contains(t));
    public bool Contains(TItem other) => ItemList.Contains(other);

    public List<ItemSet<TItem>> GetPossibleSubsets(int maxLength) {
        return GetPermutations(ItemList, maxLength).Select(itemList => new ItemSet<TItem>(itemList.ToArray())).ToList();
    }
    
    private IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length) where T : IComparable {
        if (length == 1) return list.Select(t => new T[] { t });
        return GetPermutations(list, length - 1)
            .SelectMany(t => list.Where(e => t.All(g => g.CompareTo(e) != 0)), (t1, t2) => t1.Concat(new T[] { t2 }));
    }
    
    public static ItemSet<TItem> Union(ItemSet<TItem> a, ItemSet<TItem> b) {
        var joinedList = new HashSet<TItem>(a.ItemList.Count + b.ItemList.Count);
        joinedList.UnionWith(a.ItemList);
        joinedList.UnionWith(b.ItemList);
        return new ItemSet<TItem>(joinedList.ToArray());
    }
    
    public static ItemSet<TItem> GetSubtract(ItemSet<TItem> a, ItemSet<TItem> b) {
        var result = new List<TItem>(a.ItemList);
        var removeThis = new List<TItem>();
        for (var i = 0; i < result.Count; i++) {
            for (var j = 0; j < b.ItemList.Count; j++) {
                if (result[i].Equals(b.ItemList[j]) && !removeThis.Contains(result[i])) {
                    removeThis.Add(result[i]);
                }
            }
        }

        for (var i = 0; i < removeThis.Count; i++) { result.Remove(removeThis[i]); }

        return new ItemSet<TItem>(result.ToArray());
    }

    public static bool IsEqualUntil(ItemSet<TItem> p, ItemSet<TItem> q, int until) {
        for (var i = 0; i < until; i++) {
            if (!p.ItemList[i].Equals(q.ItemList[i])) {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object obj) {
        var other = (ItemSet<TItem>)obj;
        return Contains(other) && other.Contains(this);
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override string ToString() {
        var s = "(";
        for (var i = 0; i < ItemList.Count-1; i++) { s += $"{ItemList[i]}, "; }
        return $"{s}{ItemList[^1]})";
    }
}