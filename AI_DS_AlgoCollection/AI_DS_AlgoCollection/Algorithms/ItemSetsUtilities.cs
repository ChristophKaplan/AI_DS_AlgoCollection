using AI_DS_AlgoCollection.DataStructures;

namespace DataMining
{
    public static class ItemSetsUtilities
    {
        public static Dictionary<ItemSet<TDataType>, List<int>> VerticalFormat<TDataType>(
            this List<ItemSet<TDataType>> transactions) where TDataType : IComparable
        {
            var d = new Dictionary<ItemSet<TDataType>, List<int>>();
            foreach (var item in transactions.AppearingItems())
            {
                var oneItemSet = new ItemSet<TDataType>(item);
                d.Add(oneItemSet, transactions.IndicesOfSetsThatContainItem(oneItemSet));
            }

            return d;
        }

        public static List<int> IndicesOfSetsThatContainItem<TDataType>(this List<ItemSet<TDataType>> transactions,
            ItemSet<TDataType> itemSet) where TDataType : IComparable
        {
            var lst = new List<int>();
            for (var i = 0; i < transactions.Count; i++)
            {
                var set = transactions[i];
                if (set.Contains(itemSet)) lst.Add(i);
            }

            return lst;
        }

        public static List<TDataType> AppearingItems<TDataType>(this List<ItemSet<TDataType>> Transactions)
            where TDataType : IComparable
        {
            var appeared = new List<TDataType>();
            foreach (var transaction in Transactions)
            {
                foreach (var item in transaction.ItemList)
                {
                    if (appeared.Contains(item)) continue;
                    appeared.Add(item);
                }
            }

            return appeared;
        }

        internal static List<ItemSet<TDataType>> GetFrequenTDataTypesetsCardinalityOneByMinSupp<TDataType>(this List<ItemSet<TDataType>> itemSets, float minsupp) where TDataType : IComparable
        {
            return itemSets.AppearingItems().Select(item => new ItemSet<TDataType>(item)).
                Where(singular => itemSets.Support(singular) >= minsupp).ToList();
        }
        internal static List<ItemSet<TDataType>> GetFrequenTDataTypesetsCardinalityOneByMinNum<TDataType>(this List<ItemSet<TDataType>> itemSets, float minNum)
            where TDataType : IComparable
        {
            return itemSets.AppearingItems().Select(appearingItem => new ItemSet<TDataType>(appearingItem)).
                Where(singular => itemSets.Num(singular) >= minNum).ToList();
        }
        
        public static int Num<TDataType>(this List<ItemSet<TDataType>> Transactions, ItemSet<TDataType> x)
            where TDataType : IComparable
        {
            return Transactions.Count(t => t.Contains(x));
        }

        public static int Num<TDataType>(this List<ItemSet<TDataType>> Transactions, TDataType x)
            where TDataType : IComparable
        {
            return Transactions.Count(t => t.Contains(x));
        }

        public static float Support<TDataType>(this List<ItemSet<TDataType>> Transactions, ItemSet<TDataType> x)
            where TDataType : IComparable => (float)Transactions.Num(x) / (float)Transactions.Count;

        public static float Support<TDataType>(this List<ItemSet<TDataType>> Transactions, TDataType x)
            where TDataType : IComparable => (float)Transactions.Num(x) / (float)Transactions.Count;

        public static float Support<TDataType>(this List<ItemSet<TDataType>> Transactions,
            AssociationRule<TDataType> rule) where TDataType : IComparable =>
            (float)Transactions.Num(rule.GetJoined()) / (float)Transactions.Count;

        public static float Confidence<TDataType>(this List<ItemSet<TDataType>> Transactions,
            AssociationRule<TDataType> rule) where TDataType : IComparable =>
            Transactions.Support(rule) / Transactions.Support(rule.X);

        public static IEnumerable<IEnumerable<T>> GetPowerSet<T>(this IEnumerable<T> input)
        {
            var n = input.Count();
            for (var i = 0; i < 1 << n; i++) yield return input.Where((item, index) => (i & (1 << index)) != 0);
        }

        public static List<ItemSet<TDataType>> Sorting<TDataType>(this List<ItemSet<TDataType>> itemSetsSub, List<ItemSet<TDataType>> itemSetsSup) where TDataType : IComparable
        {
            foreach (var itemSet in itemSetsSub)
            {
                itemSet.ItemList.Sort();
            }
            
            return itemSetsSub.OrderBy(itemSetsSup.Num).ToList();
        }
    }
}