using AI_DS_AlgoCollection.DataStructures;
using DataMining;

namespace AI_DS_AlgoCollection.Algorithms.Association;

public static class Eclat {
    public static void DoEclat<TDataType, TDataValue>(this EventData<TDataType, TDataValue> data, float minSupp = 0.5f) where TDataType : IComparable {
        var itemSets = data.GetItemSets();
        var numItems = itemSets.AppearingItems().Count;
        var result = EclatAlgo(itemSets.Select(s => s.Clone()).ToList(), (int) (minSupp * numItems));
        
        var einser = itemSets.GetFrequenTDataTypesetsCardinalityOneByMinSupp(minSupp);
        result.AddRange(einser);
        
        result = result.Sorting(itemSets);
        
        Console.WriteLine(result.Aggregate("Eclat: ", (current, itemSet) => current + itemSet + itemSets.Num(itemSet) + ", "));
    }

    private static List<ItemSet<TDataType>> EclatAlgo<TDataType>(List<ItemSet<TDataType>> itemSets, int minNum) where TDataType : IComparable {
        var frequenTDataTypeSets = new List<ItemSet<TDataType>>();
        var verticalFormat = itemSets.VerticalFormat();
        Remove(verticalFormat, minNum);
        var k = 1;

        while (verticalFormat.Keys.Count >= 2) {
            verticalFormat = JoinAll(verticalFormat, k + 1);
            Remove(verticalFormat, minNum);
            frequenTDataTypeSets.AddRange(verticalFormat.Keys);
            k++;
        }

        return frequenTDataTypeSets;
    }
    
    private static void Remove<TDataType>(Dictionary<ItemSet<TDataType>, List<int>> vert, int minNum) where TDataType : IComparable {
        foreach (var item in vert.Keys.Where(item => vert[item].Count < minNum)) { vert.Remove(item); }
    }

    private static Dictionary<ItemSet<TDataType>, List<int>> JoinAll<TDataType>(Dictionary<ItemSet<TDataType>, List<int>> verticalDictionary, int k) where TDataType : IComparable {
        var joined = new Dictionary<ItemSet<TDataType>, List<int>>();
        var itemSets = verticalDictionary.Keys.ToList();

        for (var i = 0; i < itemSets.Count; i++) {
            for (var j = i + 1; j < itemSets.Count; j++) {
                var itemSet1 = itemSets[i];
                var itemSet2 = itemSets[j];
                
                var union = ItemSet<TDataType>.Union(itemSet1, itemSet2);
                if (union.ItemList.Count != k) {
                    continue;
                }
                
                var intersection = verticalDictionary[itemSet1].Intersect(verticalDictionary[itemSet2]).ToList();
                joined.TryAdd(union, intersection);
            }
        }

        return joined;
    }
}