using AI_DS_AlgoCollection.DataStructures;

namespace AI_DS_AlgoCollection.Utilities;

public static class ItemSetsUtilities
{
    public static Dictionary<ItemSet<TDataType>, List<int>> VerticalFormat<TDataType>(this List<ItemSet<TDataType>> itemSets) where TDataType : IComparable
    {
        return itemSets.AppearingItems().Select(item => new ItemSet<TDataType>(item))
            .ToDictionary(singularItemSet => singularItemSet, itemSets.IndicesOfSetsThatContainItem);
    }

    private static List<int> IndicesOfSetsThatContainItem<TDataType>(this List<ItemSet<TDataType>> itemSets, ItemSet<TDataType> itemSet) where TDataType : IComparable
    {
        var indices = new List<int>();
        for (var i = 0; i < itemSets.Count; i++)
        {
            if (!itemSets[i].Contains(itemSet)) continue;
            indices.Add(i);
        }

        return indices;
    }

    private static List<TDataType> AppearingItems<TDataType>(this List<ItemSet<TDataType>> itemSets) where TDataType : IComparable
    {
        var appeared = new List<TDataType>();
        
        foreach (var itemSet in itemSets)
        {
            foreach (var item in itemSet.ItemList)
            {
                if (!appeared.Contains(item))
                {
                    appeared.Add(item);
                }
            }
        }

        return appeared;
    }

    internal static List<ItemSet<TDataType>> GetFrequentSingleItemSetsByMinNum<TDataType>(this List<ItemSet<TDataType>> itemSets, float minNum)
        where TDataType : IComparable
    {
        return itemSets.AppearingItems().Select(appearingItem => new ItemSet<TDataType>(appearingItem)).
            Where(singular => itemSets.Num(singular) >= minNum).OrderByDescending(itemSets.Num).ToList();
    }
        
    public static int Num<TDataType>(this List<ItemSet<TDataType>> itemSets, ItemSet<TDataType> itemSet) where TDataType : IComparable
    {
        return itemSets.Count(set => set.Contains(itemSet));
    }

    public static int Num<TDataType>(this List<ItemSet<TDataType>> itemSets, TDataType item) where TDataType : IComparable
    {
        return itemSets.Count(itemSet => itemSet.Contains(item));
    }

    public static float Support<TDataType>(this List<ItemSet<TDataType>> itemSets, ItemSet<TDataType> itemSet) where TDataType : IComparable => itemSets.Num(itemSet) / (float)itemSets.Count;

    public static float Support<TDataType>(this List<ItemSet<TDataType>> itemSets, AssociationRule<TDataType> rule) where TDataType : IComparable => itemSets.Num(rule.GetJoined()) / (float)itemSets.Count;

    public static float Confidence<TDataType>(this List<ItemSet<TDataType>> itemSets, AssociationRule<TDataType> rule) where TDataType : IComparable =>
        itemSets.Support(rule) / itemSets.Support(rule.X);

    public static IEnumerable<IEnumerable<T>> GetPowerSet<T>(this IEnumerable<T> input, int n)
    {
        for (var i = 0; i < 1 << n; i++) yield return input.Where((_, index) => (i & (1 << index)) != 0);
    }

    public static List<ItemSet<TDataType>> SortingBy<TDataType>(this List<ItemSet<TDataType>> itemSets, List<ItemSet<TDataType>> sortBy) where TDataType : IComparable
    {
        foreach (var itemSet in itemSets)
        {
            itemSet.ItemList = itemSet.ItemList.OrderByDescending(sortBy.Num).ToList();
        }
            
        return itemSets.OrderBy(sortBy.Num).ToList();
    }
}