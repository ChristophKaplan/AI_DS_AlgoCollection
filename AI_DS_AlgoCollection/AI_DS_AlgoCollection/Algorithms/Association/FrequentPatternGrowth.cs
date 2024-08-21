using AI_DS_AlgoCollection.DataStructures;
using DataMining;

public class FPTree<TDataType> where TDataType : notnull {
    public class Node {
        public readonly List<Node> Children = new();
        public TDataType Item;
        public int ItemCount;

        public Node(TDataType item) {
            Item = item;
            ItemCount = 1;
        }

        public bool TryGetChild(TDataType item, out Node child) {
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
    
    public readonly Dictionary<TDataType, List<Node>> SideArray = new();
    
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
    public static void DoFrequentPattern<TDataType, TDataValue>(this EventData<TDataType, TDataValue> data, int minSup = 3) where TDataType : IComparable {
        var itemSets = data.GetItemSets();

        //remove items that are not frequent
        foreach (var itemSet in itemSets) {
            itemSet.ItemList = itemSet.ItemList.Where(item => itemSets.Num(item) >= minSup).ToList();
        }
        
        var tree = BuildTree(itemSets, minSup);
        Console.WriteLine(tree);
        var growth = Growth(tree, new(), minSup, itemSets);
        
        var result = new List<ItemSet<TDataType>>(growth);

        var einser = FrequentOneItemSubsets(itemSets, minSup); //itemSets is 0 ??
        result.AddRange(einser);
        result = result.Sorting(itemSets);
        
        Console.WriteLine(result.Aggregate("FP: ", (current, itemSet) => current + itemSet + result.Num(itemSet) + ", "));
    }

    private static List<ItemSet<TDataType>> FrequentOneItemSubsets<TDataType>(List<ItemSet<TDataType>> itemSets, float anz_min)
        where TDataType : IComparable {
        var itemSetList = new List<ItemSet<TDataType>>();
        foreach (var appearingItem in itemSets.AppearingItems()) {
            var singular = new ItemSet<TDataType>(appearingItem);
            if (itemSets.Num(singular) >= anz_min) itemSetList.Add(singular);
        }
        
        return itemSetList;
    }

    private static FPTree<TDataType> BuildTree<TDataType>(List<ItemSet<TDataType>> itemSets, int minSub) where TDataType : IComparable {
        var oneItemSets = FrequentOneItemSubsets(itemSets, minSub);
        var sortedSets = oneItemSets.OrderByDescending(itemSets.Num);
        var tree = new FPTree<TDataType>();

        foreach (var itemSet in sortedSets) {
            var item = itemSet.ItemList[0];
            tree.SideArray[item] = new();
        }

        foreach (var transaction in itemSets) {
            var currentNode = tree.Root;
            var sortedSet = transaction.ItemList.Where(i => itemSets.Num(i) >= minSub).OrderByDescending(itemSets.Num).ToList();
            
            for (var j = 0; j < sortedSet.Count; j++) {
                if (currentNode.TryGetChild(sortedSet[j], out var child)) {
                    child.ItemCount++;
                }
                else {
                    child = new FPTree<TDataType>.Node(sortedSet[j]);
                    currentNode.Children.Add(child);
                    tree.SideArray[sortedSet[j]].Add(child);
                }

                currentNode = child;
            }
        }

        return tree;
    }

    private static List<ItemSet<TDataType>> Growth<TDataType>(FPTree<TDataType> tree, ItemSet<TDataType> itemSet, int anzMin, List<ItemSet<TDataType>> itemSets) where TDataType : IComparable {
        var returnOutcome = new List<ItemSet<TDataType>>();
        
        if (tree.HasOnlyOnePath(out var path)) {
            var asItems = path.Where(n => n.ItemCount >= anzMin).Select(n => n.Item).ToList();
            foreach (var subset in asItems.GetPowerSet()) {
                var set = new ItemSet<TDataType>(subset.ToArray());
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

                var itemSetCopy = new ItemSet<TDataType>(itemSet.ItemList.ToArray());
                itemSetCopy.ItemList.Add(node.Item);

                if (itemSetCopy.ItemList.Count > 1 && node.ItemCount >= anzMin) {
                    returnOutcome.Add(itemSetCopy);
                }

                var condPatternBase = ConditionalPatternBase(itemSetCopy, itemSets);
                var reducedTree = BuildTree(condPatternBase, anzMin);

                //Console.WriteLine(condPatternBase.Aggregate($"R({itemSetCopy}): ", (a, b) => $"{a}, {b}"));
                //Console.WriteLine(reducedTree);

                if (reducedTree.IsEmpty()) {
                    continue;
                }
                
                var growthSets = Growth(reducedTree, itemSetCopy, anzMin, itemSets);
                returnOutcome.AddRange(growthSets);
            }
        }

        return returnOutcome;
    }

    private static List<ItemSet<TDataType>> ConditionalPatternBase<TDataType>(ItemSet<TDataType> itemSet, List<ItemSet<TDataType>> itemSets)
        where TDataType : IComparable {
        
        var transactionContainsItemSet = itemSets.Where(trans => trans.Contains(itemSet)).ToList();

        for (var i = transactionContainsItemSet.Count - 1; i >= 0; i--) {
            var contains = transactionContainsItemSet[i];
            
            foreach (var removeMe in itemSet.ItemList) {
                contains.ItemList.Remove(removeMe);
            }

            contains.ItemList = contains.ItemList.OrderByDescending(itemSets.Num).ToList();
            
            //remove if is now emptySet
            if (contains.ItemList.Count == 0) {
                transactionContainsItemSet.Remove(contains);
            }
        }

        return transactionContainsItemSet;
    }
}