using AI_DS_AlgoCollection.DataStructures;

namespace DataMining {
    
    public static class TransactionUtilities {

        public static List<ItemSet<TItem> > GetOnlyAboveAnzMin<TItem> (this List<ItemSet<TItem> > transactions, int anzMin) where TItem : IComparable{
            var above = new List<ItemSet<TItem> >();
            
            foreach (var itemSet in transactions) {
                if (transactions.Num(itemSet) >= anzMin) {
                    above.Add(itemSet);
                }
            }

            return above;
        }
        public static Dictionary<ItemSet<TItem> ,List<int>> VerticalFormat<TItem> (this List<ItemSet<TItem> > transactions) where TItem : IComparable{
            var d = new Dictionary<ItemSet<TItem> , List<int>>();
            foreach (var item in transactions.AppearingItems()) {
                var oneItemSet = new ItemSet<TItem> (item);
                d.Add(oneItemSet,transactions.IndicesOfSetsThatContainItem(oneItemSet));
            }

            return d;
        }

        public static List<int> IndicesOfSetsThatContainItem<TItem> (this List<ItemSet<TItem> > transactions,ItemSet<TItem>  itemSet) where TItem : IComparable{
            var lst = new List<int>();
            for (var i = 0; i < transactions.Count; i++) {
                var set = transactions[i];
                if (set.Contains(itemSet)) lst.Add(i);
            }

            return lst;
        }
        public static List<TItem> AppearingItems<TItem> (this List<ItemSet<TItem> > Transactions) where TItem : IComparable{
            var appeared = new List<TItem>();
            foreach (var transaction in Transactions) {
                foreach (var item in transaction.ItemList) {
                    if(appeared.Contains(item)) continue;
                    appeared.Add(item);
                }
            }

            return appeared;
        }
    
        public static int Num<TItem> (this List<ItemSet<TItem> > Transactions,ItemSet<TItem>  x) where TItem : IComparable{
            return Transactions.Count(t => t.Contains(x));
        }
        public static int Num<TItem> (this List<ItemSet<TItem> > Transactions,TItem x) where TItem : IComparable{
            return Transactions.Count(t => t.Contains(x));
        }
    
        public static float Support<TItem> (this List<ItemSet<TItem> > Transactions,ItemSet<TItem>  x) where TItem : IComparable => (float)Transactions.Num(x) / (float)Transactions.Count;
        public static float Support<TItem> (this List<ItemSet<TItem> > Transactions, TItem x) where TItem : IComparable => (float)Transactions.Num(x) / (float)Transactions.Count;
        public static float Support<TItem> (this List<ItemSet<TItem> > Transactions,AssociationRule<TItem>  rule) where TItem : IComparable => (float)Transactions.Num(rule.GetJoined()) / (float)Transactions.Count;
        public static float Confidence<TItem> (this List<ItemSet<TItem> > Transactions,AssociationRule<TItem>  rule) where TItem : IComparable => Transactions.Support(rule) / Transactions.Support(rule.GetX());
    
        public static IEnumerable<IEnumerable<T>> GetPowerSet<T>(this IEnumerable<T> input) {
            var n = input.Count();
            for (var i = 0; i < 1 << n; i++) yield return input.Where((item, index) => (i & (1 << index)) != 0);
        }
    }
}