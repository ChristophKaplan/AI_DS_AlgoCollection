using AI_DS_AlgoCollection.DataStructures;
using DataMining;

public static class AprioriAlgorithm {
    public static void DoApriori<TDataType, TDataValue>(this EventData<TDataType, TDataValue> data, float minSupp = 0.5f) where TDataType : IComparable
    {
       var itemSetsBase =  data.GetItemSets();
       var frequentItemSets = itemSetsBase.Select(s => s.Clone()).ToList().Apriori(minSupp);
       var result = new List<ItemSet<TDataType>>();
       foreach (var itemSets in frequentItemSets)
       {
            result.AddRange(itemSets);   
       }

       result = result.SortingBy(itemSetsBase);
       Console.WriteLine(result.Aggregate("Apriori: \t", (current, itemSet) => current + itemSet + itemSetsBase.Num(itemSet) + ", "));
    }
    
    internal static List<List<ItemSet<TDataType>>> Apriori<TDataType>(this List<ItemSet<TDataType>> itemSets, float minsupp) where TDataType : IComparable
    {
        var L = new List<List<ItemSet<TDataType>>>();
        L.Add(itemSets.GetFrequentSingleItemSetsByMinSupp(minsupp));

        var k = 2;
        while (L[k - 2].Count > 0) {
            //normally -1, but indices from 0
            //Step 1
            List<ItemSet<TDataType> > candidates = L[k - 2].AprioriGen(k - 1);

            //string c = "candidates: ";
            //for (var i = 0; i < candidates.Count; i++) { c += candidates[i] + ", "; }
            //Console.WriteLine("STEP" + (k - 1) + ": " + c);
            
            Dictionary<ItemSet<TDataType> , int> counter = new Dictionary<ItemSet<TDataType> , int>();

            //Step 2, messure how often a candidate i in itemSets
            for (var i = 0; i < itemSets.Count; i++) {
                for (var j = 0; j < candidates.Count; j++) {
                    if (!counter.ContainsKey(candidates[j])) counter.Add(candidates[j], 0);
                    if (itemSets[i].Contains(candidates[j])) { counter[candidates[j]]++; }
                }
            }

            //Step 3, if the support of a candidate is greater or equal than minsupp
            List<ItemSet<TDataType> > Lk = new List<ItemSet<TDataType> >();
            for (var i = 0; i < candidates.Count; i++) {
                var count = (float)counter[candidates[i]];
                var dms = ((float)itemSets.Count * minsupp);
                if (count >= dms) Lk.Add(candidates[i]);
            }

            L.Add(Lk);
            k++;
        }
        
        return L;
    }

    internal static List<ItemSet<TDataType> > AprioriGen<TDataType>(this List<ItemSet<TDataType> > L, int preLenght) where TDataType : IComparable
    {
        var newCandidates = new List<ItemSet<TDataType> >();

        //check every set with every set
        foreach (var p in L)
        {
            foreach (var q in L)
            {
                int kMinus2 = p.ItemList.Count - 1;

                if (!ItemSet<TDataType>.IsEqualUntil(p, q, kMinus2)) continue;
                
                var eP = p.ItemList[^1];
                var eQ = q.ItemList[^1];

                if ((eP.CompareTo(eQ) < 0)) {
                    var specialJoin = new List<TDataType>();
                    for (var k = 0; k < kMinus2; k++) { specialJoin.Add(p.ItemList[k]); }

                    specialJoin.Add(eP);
                    specialJoin.Add(eQ);
                    var  join = new ItemSet<TDataType> (specialJoin.ToArray());
                    if (!newCandidates.Contains(join)) newCandidates.Add(join);
                }
            }
        }

        //Subset check
        for (var i = 0; i < newCandidates.Count; i++) {
            if (!LcontainsAnyOfSubset(L, newCandidates[i], preLenght))
            {
                newCandidates.RemoveAt(i);
            }
        }

        return newCandidates;
    }

    private static bool LcontainsAnyOfSubset<TDataType>(List<ItemSet<TDataType> > L, ItemSet<TDataType>  candidate, int length) where TDataType : IComparable
    {
        //stimmt was nicht
        var possibleSubsets = candidate.GetPossibleSubsets(length);
        var contains = false;
        foreach (var set1 in L)
        {
            foreach (var subset in possibleSubsets)
            {
                if (set1.Contains(subset))
                {
                    contains = true;
                }
            }
        }

        return contains;
    }
}