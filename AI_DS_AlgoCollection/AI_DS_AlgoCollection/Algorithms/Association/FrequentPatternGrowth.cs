using AI_DS_AlgoCollection.DataStructures;
using DataMining;

public class FPTree<TItem> where TItem : notnull {
    public class Node {
        public readonly List<Node> Children = new();
        public TItem Item;
        public int ItemCount;

        public Node(TItem item) {
            Item = item;
            ItemCount = 1;
        }

        public bool TryGetChild(TItem item, out Node child) {
            foreach (var currentChild in Children) {
                if (!currentChild.Item.Equals(item)) continue;
                child = currentChild;
                return true;
            }

            child = default;
            return false;
        }

        public override string ToString() {
            return $"{Item}[{ItemCount}]";
        }
    }
    
    public readonly Node Root = new (default);
    
    public readonly Dictionary<TItem, List<Node>> SideArray = new();
    
    public bool IsEmpty() => Root.Children.Count == 0;
    
    public bool HasOnlyOnePath(out List<Node> path) {
        path = new();
        
        foreach (var key in SideArray.Keys) {
            if (SideArray[key].Count > 1) {
                path = default;
                return false;
            }
            
            path.Add(SideArray[key][0]);
        }
        
        return true;
    }

    public override string ToString() {
        return SideArray.Keys.Aggregate("", (current, key) => current + (SideArray[key].Aggregate($"{key} - ", (a, b) => $"{a}, {b}") + "\n"));
    }
}

public static class FrequentPatternGrowth {
    public static void DoFrequentPattern<TDataType, TDataValue>(this EventData<TDataType, TDataValue> data, int anzMin = 3) where TDataType : IComparable {
        var itemSets = data.GetItemSets();

        //remove items that are not frequent
        foreach (var itemSet in itemSets) {
            itemSet.ItemList = itemSet.ItemList.Where(i => itemSets.Num(i) >= anzMin).ToList();
        }
        
        var tree = BuildTree(itemSets, anzMin);
        Console.WriteLine(tree);
        var growth = Growth(tree, new(), anzMin, itemSets);

        Console.WriteLine(growth.Aggregate("fp-growth: ", (a, b) => $"{a}, {b}"));
    }

    private static List<ItemSet<TItem>> FrequentOneItemSubsets<TItem>(List<ItemSet<TItem>> transactions, float anz_min)
        where TItem : IComparable {
        var itemSetList = new List<ItemSet<TItem>>();
        foreach (var appearingItem in transactions.AppearingItems()) {
            var singular = new ItemSet<TItem>(appearingItem);
            if (transactions.Num(singular) >= anz_min) itemSetList.Add(singular);
        }
        
        return itemSetList;
    }

    private static FPTree<TItem> BuildTree<TItem>(List<ItemSet<TItem>> transactions, int anzMin) where TItem : IComparable {
        var oneItemSets = FrequentOneItemSubsets(transactions, anzMin);
        var sortedSets = oneItemSets.OrderByDescending(transactions.Num);
        var tree = new FPTree<TItem>();

        foreach (var itemSet in sortedSets) {
            var item = itemSet.ItemList[0];
            tree.SideArray[item] = new();
        }

        foreach (var transaction in transactions) {
            var currentNode = tree.Root;
            var sortedSet = transaction.ItemList.Where(i => transactions.Num(i) >= anzMin).OrderByDescending(transactions.Num).ToList();
            
            for (var j = 0; j < sortedSet.Count; j++) {
                if (currentNode.TryGetChild(sortedSet[j], out var child)) {
                    child.ItemCount++;
                }
                else {
                    child = new FPTree<TItem>.Node(sortedSet[j]);
                    currentNode.Children.Add(child);
                    tree.SideArray[sortedSet[j]].Add(child);
                }

                currentNode = child;
            }
        }

        return tree;
    }

    private static List<ItemSet<TItem>> Growth<TItem>(FPTree<TItem> tree, ItemSet<TItem> itemSet, int anzMin, List<ItemSet<TItem>> transactions) where TItem : IComparable {
        var returnOutcome = new List<ItemSet<TItem>>();
        
        if (tree.HasOnlyOnePath(out var path)) {
            var asItems = path.Where(n => n.ItemCount >= anzMin).Select(n => n.Item).ToList();
            foreach (var subset in asItems.GetPowerSet()) {
                var set = new ItemSet<TItem>(subset.ToArray());
                if (set.ItemList.Count > 0)
                {
                    set.ItemList.AddRange(itemSet.ItemList);
                    returnOutcome.Add(set);
                }
            }
            
            //Console.WriteLine(asItems.Aggregate($"One Path, ({returnOutcome}): ", (a, b) => $"{a}, {b}"));
        }
        else {
            var sideArrayKeys = tree.SideArray.Keys.ToList();

            for (var i = sideArrayKeys.Count - 1; i >= 0; i--) {
                var node = tree.SideArray[sideArrayKeys[i]][0]; //(i,0) is lowest-first, even if its not the most "left".

                var itemSetCopy = new ItemSet<TItem>(itemSet.ItemList.ToArray());
                itemSetCopy.ItemList.Add(node.Item);

                if (itemSetCopy.ItemList.Count > 1 && node.ItemCount >= anzMin) {
                    returnOutcome.Add(itemSetCopy);
                }

                var condPatternBase = ConditionalPatternBase(itemSetCopy, transactions);
                var reducedTree = BuildTree(condPatternBase, anzMin);

                //Console.WriteLine(condPatternBase.Aggregate($"R({itemSetCopy}): ", (a, b) => $"{a}, {b}"));
                //Console.WriteLine(reducedTree);

                if (reducedTree.IsEmpty()) {
                    continue;
                }
                
                var growthSets = Growth(reducedTree, itemSetCopy, anzMin, transactions);
                returnOutcome.AddRange(growthSets);
            }
        }

        return returnOutcome;
    }

    private static List<ItemSet<TItem>> ConditionalPatternBase<TItem>(ItemSet<TItem> itemSet, List<ItemSet<TItem>> transactions)
        where TItem : IComparable {
        
        var transactionContainsItemSet = transactions.Where(trans => trans.Contains(itemSet)).ToList();

        for (var i = transactionContainsItemSet.Count - 1; i >= 0; i--) {
            var contains = transactionContainsItemSet[i];
            
            foreach (var removeMe in itemSet.ItemList) {
                contains.ItemList.Remove(removeMe);
            }

            contains.ItemList = contains.ItemList.OrderByDescending(transactions.Num).ToList();
            
            //remove if is now emptySet
            if (contains.ItemList.Count == 0) {
                transactionContainsItemSet.Remove(contains);
            }
        }

        return transactionContainsItemSet;
    }
}