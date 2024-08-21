using AI_DS_AlgoCollection.DataStructures;
using DataMining;

public class AssociationRule<TItem> where TItem : IComparable{
    private ItemSet<TItem>  x, y;
    public ItemSet<TItem>  GetX() => x;
    public ItemSet<TItem>  GetY() => y;

    public AssociationRule(ItemSet<TItem>  x, ItemSet<TItem>  y) {
        this.x = x;
        this.y = y;
        IsDisjunct();
    }

    private bool IsDisjunct() => !x.Contains(y) && !y.Contains(x);

    public ItemSet<TItem>  GetJoined() {
        return ItemSet<TItem> .Union(x, y);
    }

    public override string ToString() {
        return GetX().ToString() + " -> " + GetY().ToString();
    }

    public override bool Equals(object obj) {
        AssociationRule<TItem>  other = (AssociationRule<TItem> )obj;
        return (this.GetX().Equals(other.GetX()) && this.GetY().Equals(other.GetY()));
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }
}

public static class AprioriAlgo {
    public static void DoApriori<TDataType, TDataValue>(this EventData<TDataType, TDataValue> data, float minsupp = 0.5f) where TDataType : IComparable
    {
       var outcome = Apriori(data.GetItemSets(), minsupp);

       foreach (var itemSets in outcome)
       {
           itemSets.Aggregate("Apriori: ", (current, itemSet) => current + itemSet + " , ");
       }
    }

    private static List<ItemSet<TItem>> GetMostFrequentSubsetsWithOneItem<TItem>(List<ItemSet<TItem>> transactions, float minsupp) where TItem : IComparable
    {
        var itemSet = new List<ItemSet<TItem> >();
        foreach (var item in transactions.AppearingItems()) {
            var singular = new ItemSet<TItem> (item);
            if (transactions.Support(singular) >= minsupp) itemSet.Add(singular);
        }
        return itemSet;
    }

    private static List<List<ItemSet<TItem>>> Apriori<TItem>(List<ItemSet<TItem>> transactions, float minsupp) where TItem : IComparable
    {
        var L = new List<List<ItemSet<TItem>>>();
        L.Add(GetMostFrequentSubsetsWithOneItem(transactions, minsupp));

        var k = 2;
        while (L[k - 2].Count > 0) {
            //normally -1, but indices from 0
            //Step 1
            List<ItemSet<TItem> > candidates = AprioriGen(L[k - 2], k - 1);

            string c = "candidates: ";
            for (var i = 0; i < candidates.Count; i++) { c += candidates[i] + ", "; }

            Console.WriteLine("STEP" + (k - 1) + ": " + c);
            Dictionary<ItemSet<TItem> , int> counter = new Dictionary<ItemSet<TItem> , int>();

            //Step 2, messure how often a candidate i in transactions
            for (var i = 0; i < transactions.Count; i++) {
                for (var j = 0; j < candidates.Count; j++) {
                    if (!counter.ContainsKey(candidates[j])) counter.Add(candidates[j], 0);
                    if (transactions[i].Contains(candidates[j])) { counter[candidates[j]]++; }
                }
            }

            //Step 3, if the support of a candidate is greater or equal than minsupp
            List<ItemSet<TItem> > Lk = new List<ItemSet<TItem> >();
            for (var i = 0; i < candidates.Count; i++) {
                var count = (float)counter[candidates[i]];
                var dms = ((float)transactions.Count * minsupp);
                if (count >= dms) Lk.Add(candidates[i]);
            }

            L.Add(Lk);
            k++;
        }

        string el = "";
        for (int i = 0; i < L.Count; i++) {
            el += i + "\n";
            for (int j = 0; j < L[i].Count; j++) { el += L[i][j] + ", "; }

            el += "\n";
        }

        Console.WriteLine("el:" + el);

        return L;
    }

    private static List<ItemSet<TItem> > AprioriGen<TItem>(List<ItemSet<TItem> > L, int preLenght) where TItem : IComparable
    {
        var newCandidates = new List<ItemSet<TItem> >();

        //need to be sorted ?

        //check every set with every set
        for (int i = 0; i < L.Count; i++) {
            var  p = L[i];
            for (int j = 0; j < L.Count; j++) {
                var  q = L[j];

                int kMinus2 = p.ItemList.Count - 1;

                if (ItemSet<TItem> .IsEqualUntil(p, q, kMinus2)) {
                    var eP = p.ItemList[p.ItemList.Count - 1];
                    var eQ = q.ItemList[q.ItemList.Count - 1];

                    if ((eP.CompareTo(eQ) < 0)) {
                        var specialJoin = new List<TItem>();
                        for (var k = 0; k < kMinus2; k++) { specialJoin.Add(p.ItemList[k]); }

                        specialJoin.Add(eP);
                        specialJoin.Add(eQ);
                        ItemSet<TItem>  join = new ItemSet<TItem> (specialJoin.ToArray());
                        if (!newCandidates.Contains(join)) newCandidates.Add(join);
                    }
                }
            }
        }

        //Subset check
        for (var i = 0; i < newCandidates.Count; i++) {
            if (!LcontainsAnyOfSubset(L, newCandidates[i], preLenght)) { newCandidates.RemoveAt(i); }
        }

        return newCandidates;
    }

    private static bool LcontainsAnyOfSubset<TItem>(List<ItemSet<TItem> > L, ItemSet<TItem>  candidate, int length) where TItem : IComparable
    {
        var sub = candidate.GetPossibleSubsets(length);
        var contains = false;
        for (var k = 0; k < L.Count; k++) {
            for (var j = 0; j < sub.Count; j++) {
                if (L[k].Contains(sub[j])) { contains = true; }
            }
        }

        return contains;
    }

    public static List<(AssociationRule<TItem>, float)> GetRulesWithConfidence<TItem>(List<ItemSet<TItem>> transactions, float minsupp) where TItem : IComparable
    {
        var L = Apriori(transactions, minsupp);
        var extractedRules = ExtractRules(L, transactions, minsupp);
        var rulesWithConfidence = new List<(AssociationRule<TItem>, float)>();

        for (var i = 0; i < extractedRules.Count; i++) {
            rulesWithConfidence.Add(new(extractedRules[i], transactions.Confidence(extractedRules[i])));
        }

        return rulesWithConfidence;
    }

    private static List<AssociationRule<TItem>> ExtractRules<TItem>(List<List<ItemSet<TItem> >> L, List<ItemSet<TItem>> transactions, float minconf) where TItem : IComparable
    {
        var allCurrentRules = new List<AssociationRule<TItem>>();
        List<ItemSet<TItem>> currentH = default;

        for (int n = 0; n < L[1].Count; n++) {
            currentH = L[1][n].GetPossibleSubsets(1);
            allCurrentRules.AddRange(GetAllRules(L[1][n], currentH));
            RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH, transactions, minconf);
        }

        for (int n = 0; n < L[2].Count; n++) {
            currentH = L[2][n].GetPossibleSubsets(1); //warum 1 ?
            allCurrentRules.AddRange(GetAllRules(L[2][n], currentH));
            RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH, transactions, minconf);
            RecursiveExtraction(n, 2, L, currentH, allCurrentRules, transactions, minconf);
        }

        return allCurrentRules;
    }

    private static void RecursiveExtraction<TItem>(int n, int m, List<List<ItemSet<TItem> >> L, List<ItemSet<TItem> > currentH, List<AssociationRule<TItem>> allCurrentRules, List<ItemSet<TItem>> transactions, float minconf) where TItem : IComparable
    {
        if (m >= L.Count) return;
        if (L[m].Count == 0) return; //leer ?
        currentH = AprioriGen(currentH, m - 1);
        allCurrentRules.AddRange(GetAllRules(L[m][n], currentH));
        RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH,transactions,minconf);
        for (int i = 0; i < L[m].Count; i++) { RecursiveExtraction(i, m + 1, L, currentH, allCurrentRules, transactions, minconf); }
    }

    private static void RemoveConclusionAndRulesNotMatchingConf<TItem>(List<AssociationRule<TItem>> currentRules, List<ItemSet<TItem> > currentH, List<ItemSet<TItem>> transactions, float minconf) where TItem : IComparable
    {
        for (int i = currentRules.Count - 1; i >= 0; i--) {
            float conf = transactions.Confidence(currentRules[i]);
            if (conf < minconf) {
                currentH.Remove(currentRules[i].GetY());
                currentRules.RemoveAt(i);
            }
        }
    }

    private static List<AssociationRule<TItem>> GetAllRules<TItem>(ItemSet<TItem>  set, List<ItemSet<TItem> > Hm) where TItem : IComparable
    {
        int len = set.ItemList.Count;
        var X = set.GetPossibleSubsets(len);
        var rules = new List<AssociationRule<TItem>>();
        for (var i = 0; i < Hm.Count; i++) {
            for (var j = 0; j < X.Count; j++) {
                AssociationRule<TItem> ar = new AssociationRule<TItem>(ItemSet<TItem>.GetSubtract(X[j], Hm[i]), Hm[i]);
                if (!rules.Contains(ar)) rules.Add(ar);
            }
        }

        return rules;
    }
}