using AI_DS_AlgoCollection.DataStructures;
using DataMining;

namespace AI_DS_AlgoCollection.Algorithms.Association;

public static class Eclat {
    public static void DoEclat<TDataType, TDataValue>(this EventData<TDataType, TDataValue> data, float minSupp = 0.5f) where TDataType : IComparable {

        var transactions = data.GetItemSets();
        var numItems = transactions.AppearingItems().Count();
        var result = EclatAlgo(transactions, (int) (minSupp * numItems));
        
        string s = result.Aggregate("Eclat: ", (current, itemSet) => current + (itemSet + " : " + transactions.Num(itemSet) + ", "));
        Console.WriteLine(s);
    }

    private static List<ItemSet<TItem>> EclatAlgo<TItem>(List<ItemSet<TItem>> transactions, int anzMin) where TItem : IComparable {
        var frequentItemSets = new List<ItemSet<TItem>>();
        var verticalFormat = transactions.VerticalFormat();
        Remove(verticalFormat, anzMin);
        //PrintVert(verticalFormat);
        var k = 1;

        while (verticalFormat.Keys.Count >= 2) {
            verticalFormat = JoinAll(verticalFormat, k + 1);
            Remove(verticalFormat, anzMin);
            //PrintVert(verticalFormat);
            frequentItemSets.AddRange(verticalFormat.Keys);
            k++;
        }

        return frequentItemSets;
    }

    private static void PrintVert<TItem>(Dictionary<ItemSet<TItem>, List<int>> vert) where TItem : IComparable {
        foreach (var item in vert) {
            Console.WriteLine(item.Key + " : " + string.Join(", ", item.Value));
        }
    }
    
    private static void Remove<TItem>(Dictionary<ItemSet<TItem>, List<int>> vert, int anzMin) where TItem : IComparable {
        foreach (var item in vert.Keys.Where(item => vert[item].Count < anzMin)) { vert.Remove(item); }
    }

    private static Dictionary<ItemSet<TItem>, List<int>> JoinAll<TItem>(Dictionary<ItemSet<TItem>, List<int>> verticalDictionary, int k) where TItem : IComparable {
        var joined = new Dictionary<ItemSet<TItem>, List<int>>();
        var itemSets = verticalDictionary.Keys.ToList();

        for (var i = 0; i < itemSets.Count; i++) {
            for (var j = i + 1; j < itemSets.Count; j++) {
                var itemSet1 = itemSets[i];
                var itemSet2 = itemSets[j];
                
                var union = ItemSet<TItem>.Union(itemSet1, itemSet2);
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