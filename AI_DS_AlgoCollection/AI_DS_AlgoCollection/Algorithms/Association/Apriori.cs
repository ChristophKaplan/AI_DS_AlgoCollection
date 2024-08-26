using AI_DS_AlgoCollection.DataStructures;
using AI_DS_AlgoCollection.Utilities;

namespace AI_DS_AlgoCollection.Algorithms.Association;

public static class Apriori
{
    public static void DoApriori<TDataType, TDataValue>(this EventData<TDataType, TDataValue> data, int minNum = 3) where TDataType : IComparable
    {
        var itemSetsOrigin = data.GetItemSets();
        var aprioriResult = itemSetsOrigin.Select(itemSet => itemSet.Clone()).ToList().AprioriAlgo(minNum);
        
        var result = new List<ItemSet<TDataType>>();
        foreach (var itemSets in aprioriResult)
        {
            result.AddRange(itemSets);
        }

        result = result.SortingBy(itemSetsOrigin);
        
        Console.WriteLine(result.Aggregate("Apriori: \t", (current, itemSet) => current + itemSet + itemSetsOrigin.Num(itemSet) + ", "));
        var b = result.CreateRulesFromItemSets(0.3f, itemSetsOrigin);
    }

    private static List<List<ItemSet<TDataType>>> AprioriAlgo<TDataType>(this List<ItemSet<TDataType>> itemSets, int minNum) where TDataType : IComparable
    {
        var result = new List<List<ItemSet<TDataType>>>
        {
            itemSets.GetFrequentSingleItemSetsByMinNum(minNum)
        };
        
        var k = 0;
        while (result[k].Count > 0)
        {
            var candidates = result[k].AprioriGen();
            var frequencyCount = FrequencyCount(itemSets, candidates);
            var candidatesWithSupport = candidates.Where(candidate => frequencyCount[candidate] >= minNum).ToList();
            result.Add(candidatesWithSupport);
            k++;
        }

        return result;
    }
    
    private static Dictionary<ItemSet<TDataType>, int> FrequencyCount<TDataType>(List<ItemSet<TDataType>> itemSets, List<ItemSet<TDataType>> candidates) where TDataType : IComparable
    {
        var frequencyCount = new Dictionary<ItemSet<TDataType>, int>();
        foreach (var itemSet in itemSets)
        {
            foreach (var candidate in candidates)
            {
                frequencyCount.TryAdd(candidate, 0);
                if (itemSet.Contains(candidate))
                {
                    frequencyCount[candidate]++;
                }
            }
        }
        
        return frequencyCount;
    }
    
    internal static List<ItemSet<TDataType>> AprioriGen<TDataType>(this List<ItemSet<TDataType>> itemSets) where TDataType : IComparable
    {
        var candidates = new List<ItemSet<TDataType>>();

        foreach (var p in itemSets)
        {
            var k = p.ItemList.Count - 1;
            
            foreach (var q in itemSets)
            {
                if (!ItemSet<TDataType>.IsEqualUntil(p, q, k)) continue;
                if (p.ItemList[^1].CompareTo(q.ItemList[^1]) >= 0) continue; //lexicographic order
                
                var newSet = KeepItemsUntilK_JoinLastTwo(p, q, k);
                if (!candidates.Contains(newSet))
                {
                    candidates.Add(newSet);
                }
            }
        }
        
        SubsetCheck(itemSets, candidates);
        
        return candidates;
    }

    private static void SubsetCheck<TDataType>(List<ItemSet<TDataType>> itemSets, List<ItemSet<TDataType>> candidates) where TDataType : IComparable
    {
        foreach (var candidate in candidates.ToList())
        {
            var possibleSubsetsOfCandidate = candidate.GetPossibleSubsets(candidate.ItemList.Count - 1);
            if (!itemSets.Any(itemSet => possibleSubsetsOfCandidate.Any(itemSet.Contains)))
            {
                candidates.Remove(candidate);
            }
        }
    }
    
    private static ItemSet<TDataType> KeepItemsUntilK_JoinLastTwo<TDataType>(ItemSet<TDataType> p, ItemSet<TDataType> q, int k) where TDataType : IComparable
    {
        var joined = new List<TDataType>();
        for (var i = 0; i < k; i++)
        {
            joined.Add(p.ItemList[i]);
        }
        
        joined.Add(p.ItemList[^1]);
        joined.Add(q.ItemList[^1]);
                    
        return new ItemSet<TDataType>(joined.ToArray());
    }
}