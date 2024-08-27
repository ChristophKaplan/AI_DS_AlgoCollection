using AI_DS_AlgoCollection.DataStructures;
using AI_DS_AlgoCollection.Utilities;

namespace AI_DS_AlgoCollection.Algorithms.Association;

public static class Eclat
{
    public static void DoEclat<TDataType, TDataValue>(this EventData<TDataType, TDataValue> data, int minNum = 3)
    {
        var itemSets = data.GetItemSets();

        //remove?

        var result = EclatAlgo(itemSets.Select(itemSet => itemSet.Clone()).ToList(), minNum);

        result.AddRange(itemSets.GetFrequentSingleItemSetsByMinNum(minNum));
        result = result.SortingBy(itemSets);

        Console.WriteLine(result.Aggregate("Eclat: \t\t",
            (current, itemSet) => current + itemSet + itemSets.Num(itemSet) + ", "));
    }

    private static List<ItemSet<TDataType>> EclatAlgo<TDataType>(List<ItemSet<TDataType>> itemSets, int minNum)
    {
        //itemSets.ForEach(itemSet => itemSet.ItemList.Sort());

        var frequenTDataTypeSets = new List<ItemSet<TDataType>>();

        var verticalFormat = itemSets.VerticalFormat();
        RemoveBelow(verticalFormat, minNum);
        var k = 1;

        while (verticalFormat.Keys.Count >= 2)
        {
            verticalFormat = JoinAll(verticalFormat, k + 1);
            RemoveBelow(verticalFormat, minNum);
            frequenTDataTypeSets.AddRange(verticalFormat.Keys);
            k++;
        }

        return frequenTDataTypeSets;
    }

    private static void RemoveBelow<TDataType>(Dictionary<ItemSet<TDataType>, List<int>> verticalFormat, int minNum)
    {
        foreach (var item in verticalFormat.Keys.Where(item => verticalFormat[item].Count < minNum))
            verticalFormat.Remove(item);
    }

    private static Dictionary<ItemSet<TDataType>, List<int>> JoinAll<TDataType>(
        Dictionary<ItemSet<TDataType>, List<int>> verticalDictionary, int k)
    {
        var joined = new Dictionary<ItemSet<TDataType>, List<int>>();
        var itemSets = verticalDictionary.Keys.ToList();

        for (var i = 0; i < itemSets.Count; i++)
        for (var j = i + 1; j < itemSets.Count; j++)
        {
            var itemSet1 = itemSets[i];
            var itemSet2 = itemSets[j];

            var union = ItemSet<TDataType>.Union(itemSet1, itemSet2);
            union.ItemList.Sort();

            if (union.ItemList.Count != k) continue;

            var intersection = verticalDictionary[itemSet1].Intersect(verticalDictionary[itemSet2]).ToList();
            joined.TryAdd(union, intersection);
        }

        return joined;
    }
}