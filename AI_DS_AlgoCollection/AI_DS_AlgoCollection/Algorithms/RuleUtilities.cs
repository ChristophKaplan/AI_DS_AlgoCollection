using AI_DS_AlgoCollection.Algorithms.Association;
using AI_DS_AlgoCollection.DataStructures;

namespace AI_DS_AlgoCollection.Algorithms;

public static class RuleUtilities
{
    public static List<(AssociationRule<TDataType>, float)> GetRulesWithConfidence<TDataType>(List<ItemSet<TDataType>> transactions, int minNum) where TDataType : IComparable
    {
        var result = transactions.AprioriAlgo(minNum);
        var extractedRules = ExtractRules(result, transactions, minNum);
        return extractedRules.Select(exRule => new ValueTuple<AssociationRule<TDataType>, float>(exRule, transactions.Confidence(exRule))).ToList();
    }

    private static List<AssociationRule<TDataType>> ExtractRules<TDataType>(List<List<ItemSet<TDataType> >> L, List<ItemSet<TDataType>> transactions, float minconf) where TDataType : IComparable
    {
        var allCurrentRules = new List<AssociationRule<TDataType>>();
        List<ItemSet<TDataType>> currentH = default;

        for (var n = 0; n < L[1].Count; n++) {
            currentH = L[1][n].GetPossibleSubsets(1);
            allCurrentRules.AddRange( CreateRulesFrom(L[1][n], currentH) );
            RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH, transactions, minconf);
        }

        for (var n = 0; n < L[2].Count; n++) {
            currentH = L[2][n].GetPossibleSubsets(1); //warum 1 ?
            allCurrentRules.AddRange(CreateRulesFrom(L[2][n], currentH));
            RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH, transactions, minconf);
            RecursiveExtraction(n, 2, L, currentH, allCurrentRules, transactions, minconf);
        }

        return allCurrentRules;
    }

    private static void RecursiveExtraction<TDataType>(int n, int m, List<List<ItemSet<TDataType> >> L, List<ItemSet<TDataType> > curConclusion, List<AssociationRule<TDataType>> allCurrentRules, List<ItemSet<TDataType>> transactions, float minconf) where TDataType : IComparable
    {
        if (m >= L.Count || L[m].Count == 0) return;
        
        curConclusion = curConclusion.AprioriGen();
        allCurrentRules.AddRange(CreateRulesFrom(L[m][n], curConclusion));
        RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, curConclusion,transactions,minconf);
        
        for (var i = 0; i < L[m].Count; i++)
        {
            RecursiveExtraction(i, m + 1, L, curConclusion, allCurrentRules, transactions, minconf);
        }
    }

    private static void RemoveConclusionAndRulesNotMatchingConf<TDataType>(List<AssociationRule<TDataType>> currentRules, List<ItemSet<TDataType> > curConclusion, List<ItemSet<TDataType>> transactions, float minconf) where TDataType : IComparable
    {
        for (var i = currentRules.Count - 1; i >= 0; i--) {
            var conf = transactions.Confidence(currentRules[i]);
            if (conf >= minconf) continue;
            
            curConclusion.Remove(currentRules[i].Y);
            currentRules.RemoveAt(i);
        }
    }

    private static List<AssociationRule<TDataType>> CreateRulesFrom<TDataType>(ItemSet<TDataType>  itemSet, List<ItemSet<TDataType> > conclusions) where TDataType : IComparable
    {
        int len = itemSet.ItemList.Count;
        var possibleSubsets = itemSet.GetPossibleSubsets(len);
        var rules = new List<AssociationRule<TDataType>>();
        
        for (var i = 0; i < conclusions.Count; i++) {
            for (var j = 0; j < possibleSubsets.Count; j++)
            {
                var premise = ItemSet<TDataType>.GetSubtract(possibleSubsets[j], conclusions[i]);
                var rule = new AssociationRule<TDataType>(premise, conclusions[i]);
                
                
                if (!rules.Contains(rule))
                {
                    rules.Add(rule);
                }
            }
        }

        return rules;
    }

    public static List<AssociationRule<TDataType>> CreateRulesFrom2<TDataType>(ItemSet<TDataType> itemSet, float minConf, List<ItemSet<TDataType> > transactions) where TDataType : IComparable
    {
        var len = itemSet.ItemList.Count;
        var rules = new List<AssociationRule<TDataType>>();
        var frequentItemSet = itemSet.Clone();
        var genConclusion = itemSet.ItemList.Select(item => new ItemSet<TDataType>(item)).ToList();
        
        for (int i = 1; i < len; i++)
        {
            foreach (var conclusion in genConclusion)
            {
                //check if Y is always a subset of itemSet ?
                if (!conclusion.ItemList.ToHashSet().IsSubsetOf(itemSet.ItemList.ToHashSet()))
                {
                    Console.WriteLine("shit"); //seems fine
                }
                
                var premise = ItemSet<TDataType>.GetSubtract(frequentItemSet, conclusion);
                var rule = new AssociationRule<TDataType>(premise, conclusion);
                var conf = transactions.Confidence(rule);
                if (conf < minConf) continue;
                
                rules.Add(rule);
            }
            
            genConclusion = genConclusion.AprioriGen();
        }

        return rules;
    }
}