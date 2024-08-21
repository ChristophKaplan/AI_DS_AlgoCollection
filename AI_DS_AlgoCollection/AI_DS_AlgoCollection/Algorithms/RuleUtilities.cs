using AI_DS_AlgoCollection.DataStructures;

namespace DataMining;

public static class RuleUtilities
{
    public static List<(AssociationRule<TDataType>, float)> GetRulesWithConfidence<TDataType>(List<ItemSet<TDataType>> transactions, float minsupp) where TDataType : IComparable
    {
        var L = transactions.Apriori(minsupp);
        var extractedRules = ExtractRules(L, transactions, minsupp);
        return extractedRules.Select(exRule => new ValueTuple<AssociationRule<TDataType>, float>(exRule, transactions.Confidence(exRule))).ToList();
    }

    private static List<AssociationRule<TDataType>> ExtractRules<TDataType>(List<List<ItemSet<TDataType> >> L, List<ItemSet<TDataType>> transactions, float minconf) where TDataType : IComparable
    {
        var allCurrentRules = new List<AssociationRule<TDataType>>();
        List<ItemSet<TDataType>> currentH = default;

        for (var n = 0; n < L[1].Count; n++) {
            currentH = L[1][n].GetPossibleSubsets(1);
            allCurrentRules.AddRange(GetAllRules(L[1][n], currentH));
            RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH, transactions, minconf);
        }

        for (var n = 0; n < L[2].Count; n++) {
            currentH = L[2][n].GetPossibleSubsets(1); //warum 1 ?
            allCurrentRules.AddRange(GetAllRules(L[2][n], currentH));
            RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH, transactions, minconf);
            RecursiveExtraction(n, 2, L, currentH, allCurrentRules, transactions, minconf);
        }

        return allCurrentRules;
    }

    private static void RecursiveExtraction<TDataType>(int n, int m, List<List<ItemSet<TDataType> >> L, List<ItemSet<TDataType> > currentH, List<AssociationRule<TDataType>> allCurrentRules, List<ItemSet<TDataType>> transactions, float minconf) where TDataType : IComparable
    {
        if (m >= L.Count) return;
        if (L[m].Count == 0) return; //leer ?
        currentH = currentH.AprioriGen(m - 1);
        allCurrentRules.AddRange(GetAllRules(L[m][n], currentH));
        RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH,transactions,minconf);
        for (int i = 0; i < L[m].Count; i++) { RecursiveExtraction(i, m + 1, L, currentH, allCurrentRules, transactions, minconf); }
    }

    private static void RemoveConclusionAndRulesNotMatchingConf<TDataType>(List<AssociationRule<TDataType>> currentRules, List<ItemSet<TDataType> > currentH, List<ItemSet<TDataType>> transactions, float minconf) where TDataType : IComparable
    {
        for (int i = currentRules.Count - 1; i >= 0; i--) {
            float conf = transactions.Confidence(currentRules[i]);
            if (conf < minconf) {
                currentH.Remove(currentRules[i].Y);
                currentRules.RemoveAt(i);
            }
        }
    }

    private static List<AssociationRule<TDataType>> GetAllRules<TDataType>(ItemSet<TDataType>  set, List<ItemSet<TDataType> > Hm) where TDataType : IComparable
    {
        int len = set.ItemList.Count;
        var X = set.GetPossibleSubsets(len);
        var rules = new List<AssociationRule<TDataType>>();
        for (var i = 0; i < Hm.Count; i++) {
            for (var j = 0; j < X.Count; j++) {
                var ar = new AssociationRule<TDataType>(ItemSet<TDataType>.GetSubtract(X[j], Hm[i]), Hm[i]);
                if (!rules.Contains(ar)) rules.Add(ar);
            }
        }

        return rules;
    }
}