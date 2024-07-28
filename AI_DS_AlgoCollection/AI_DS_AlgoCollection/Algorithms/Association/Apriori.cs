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

public class AprioriAlgo<TItem> where TItem : IComparable {
    private readonly ItemSet<TItem> _itemBase;
    private readonly List<ItemSet<TItem>> _transactions;
    readonly float _minsupp;
    readonly float _minconf;

    public AprioriAlgo(List<ItemSet<TItem> > Transactions, float minsupp, float minconf) {
        _itemBase = new ItemSet<TItem>(Transactions.AppearingItems().ToArray());
        _transactions = Transactions;
        _minconf = minconf;
        _minsupp = minsupp;
    }

    private List<ItemSet<TItem> > GetMostFrequentSubsetsWithOneItem(float minsupp) {
        var itemSet = new List<ItemSet<TItem> >();
        foreach (var item in _itemBase.ItemList) {
            var singular = new ItemSet<TItem> (item);
            if (_transactions.Support(singular) >= minsupp) itemSet.Add(singular);
        }
        return itemSet;
    }

    public List<List<ItemSet<TItem> >> Apriori(List<ItemSet<TItem>> Transactions) {
        var L = new List<List<ItemSet<TItem> >>();
        L.Add(GetMostFrequentSubsetsWithOneItem(_minsupp));

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
            for (var i = 0; i < Transactions.Count; i++) {
                for (var j = 0; j < candidates.Count; j++) {
                    if (!counter.ContainsKey(candidates[j])) counter.Add(candidates[j], 0);
                    if (Transactions[i].Contains(candidates[j])) { counter[candidates[j]]++; }
                }
            }

            //Step 3, if the support of a candidate is greater or equal than minsupp
            List<ItemSet<TItem> > Lk = new List<ItemSet<TItem> >();
            for (var i = 0; i < candidates.Count; i++) {
                var count = (float)counter[candidates[i]];
                var dms = ((float)Transactions.Count * _minsupp);
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

    public List<ItemSet<TItem> > AprioriGen(List<ItemSet<TItem> > L, int preLenght) {
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

    private bool LcontainsAnyOfSubset(List<ItemSet<TItem> > L, ItemSet<TItem>  candidate, int length) {
        var sub = candidate.GetPossibleSubsets(length);
        var contains = false;
        for (var k = 0; k < L.Count; k++) {
            for (var j = 0; j < sub.Count; j++) {
                if (L[k].Contains(sub[j])) { contains = true; }
            }
        }

        return contains;
    }

    public List<(AssociationRule<TItem>, float)> GetRulesWithConfidence() {
        var L = Apriori(this._transactions);
        var extractedRules = ExtractRules(L);
        var rulesWithConfidence = new List<(AssociationRule<TItem>, float)>();

        for (var i = 0; i < extractedRules.Count; i++) {
            rulesWithConfidence.Add(new(extractedRules[i], _transactions.Confidence(extractedRules[i])));
        }

        return rulesWithConfidence;
    }

    private List<AssociationRule<TItem>> ExtractRules(List<List<ItemSet<TItem> >> L) {
        var allCurrentRules = new List<AssociationRule<TItem>>();
        List<ItemSet<TItem>> currentH = default;

        for (int n = 0; n < L[1].Count; n++) {
            currentH = L[1][n].GetPossibleSubsets(1);
            allCurrentRules.AddRange(GetAllRules(L[1][n], currentH));
            RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH);
        }

        for (int n = 0; n < L[2].Count; n++) {
            currentH = L[2][n].GetPossibleSubsets(1); //warum 1 ?
            allCurrentRules.AddRange(GetAllRules(L[2][n], currentH));
            RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH);
            RecursiveExtraction(n, 2, L, currentH, allCurrentRules);
        }

        return allCurrentRules;
    }

    private void RecursiveExtraction(int n, int m, List<List<ItemSet<TItem> >> L, List<ItemSet<TItem> > currentH, List<AssociationRule<TItem>> allCurrentRules) {
        if (m >= L.Count) return;
        if (L[m].Count == 0) return; //leer ?
        currentH = AprioriGen(currentH, m - 1);
        allCurrentRules.AddRange(GetAllRules(L[m][n], currentH));
        RemoveConclusionAndRulesNotMatchingConf(allCurrentRules, currentH);
        for (int i = 0; i < L[m].Count; i++) { RecursiveExtraction(i, m + 1, L, currentH, allCurrentRules); }
    }

    private void RemoveConclusionAndRulesNotMatchingConf(List<AssociationRule<TItem>> currentRules, List<ItemSet<TItem> > currentH) {
        for (int i = currentRules.Count - 1; i >= 0; i--) {
            float conf = _transactions.Confidence(currentRules[i]);
            if (conf < _minconf) {
                currentH.Remove(currentRules[i].GetY());
                currentRules.RemoveAt(i);
            }
        }
    }

    private List<AssociationRule<TItem>> GetAllRules(ItemSet<TItem>  set, List<ItemSet<TItem> > Hm) {
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