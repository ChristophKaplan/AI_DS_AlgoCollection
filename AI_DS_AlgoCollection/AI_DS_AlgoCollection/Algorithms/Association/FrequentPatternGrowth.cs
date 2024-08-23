using AI_DS_AlgoCollection.DataStructures;
using DataMining;

public class FPTree<TDataType> where TDataType : IComparable
{
    public class Node
    {
        private Node Parent;
        public readonly List<Node> Children = new();
        public TDataType Item;
        public int ItemCount;

        public Node(TDataType item, Node parent = null)
        {
            Item = item;
            Parent = parent;
            ItemCount = 1;
        }
        
        public bool TryGetChild(TDataType item, out Node child)
        {
            foreach (var currentChild in Children)
            {
                if (!currentChild.Item.Equals(item)) continue;
                child = currentChild;
                return true;
            }

            child = default;
            return false;
        }

        public List<Node> GetPathToRoot()
        {
            var path = new List<Node> {this};
            var current = this;

            while (current.Parent != null && current.Parent.Parent != null)
            {
                path.Add(current.Parent);
                current = current.Parent;
            }

            return path;
        }
        
        public override string ToString()
        {
            return $"{Item}[{ItemCount}]";
        }
    }

    public readonly Node Root = new(default);

    public readonly Dictionary<TDataType, List<Node>> SideArray = new();

    public bool IsEmpty() => Root.Children.Count == 0;

    public KeyValuePair<TDataType,List<Node>> GetLowestNodeOf(ItemSet<TDataType> itemSetR)
    {
        foreach (var keyValuePair in SideArray.Reverse())
        {
            if (itemSetR.Contains(keyValuePair.Key))
            {
                return keyValuePair;
            }
        }

        Console.Error.WriteLine($"No lowest node found, for {itemSetR}");
        return default;
    }
    
    public List<TDataType> GetPathItems(int minNum, int pathPos = 0)
    {
        var firstPath = SideArray[SideArray.Keys.Last()][pathPos].GetPathToRoot();
        return firstPath.Where(node => node.ItemCount >= minNum).Select(n => n.Item).ToList();
    }
    
    public bool HasSingularPath() => SideArray.Keys.All(key => SideArray[key].Count <= 1);
    
    public override string ToString()
    {
        return SideArray.Keys.Aggregate("",
            (current, key) => current + (SideArray[key].Aggregate($"{key} - ", (a, b) => $"{a}, {b}") + "\n"));
    }
}

public static class FrequentPatternGrowth
{
    public static void DoFrequentPattern<TDataType, TDataValue>(this EventData<TDataType, TDataValue> data, int minNum = 3) where TDataType : IComparable
    {
        var itemSets = data.GetItemSets();

        //remove items that are not frequent???
        foreach (var itemSet in itemSets)
        {
            //itemSet.ItemList = itemSet.ItemList.Where(item => itemSets.Num(item) >= minNum).ToList();
        }

        var fpTree = BuildFpTree(itemSets, minNum);
        var growth = Growth(fpTree, ItemSet<TDataType>.EmptySet, minNum);
        
        var result = new List<ItemSet<TDataType>>(growth);
        result.AddRange(itemSets.GetFrequentSingleItemSetsByMinNum(minNum));
        result = result.SortingBy(itemSets);

        Console.WriteLine(result.Aggregate($"FP: \t\t", (current, itemSet) => current + itemSet + itemSets.Num(itemSet) + ", "));
    }

    private static FPTree<TDataType> BuildFpTree<TDataType>(List<ItemSet<TDataType>> itemSets, int minNum) where TDataType : IComparable
    {
        var fpTree = new FPTree<TDataType>();
        var sortedSingleItemSets = itemSets.GetFrequentSingleItemSetsByMinNum(minNum);
        
        foreach (var itemSet in sortedSingleItemSets)
        {
            fpTree.SideArray.Add(itemSet.ItemList.First(), new List<FPTree<TDataType>.Node>());
        }

        foreach (var itemSet in itemSets)
        {
            var currentNode = fpTree.Root;
            var sortedItemSet = itemSet.ItemList.Where(item => itemSets.Num(item) >= minNum).OrderByDescending(itemSets.Num).ToList();
            
            foreach (var item in sortedItemSet)
            {
                if (currentNode.TryGetChild(item, out var childNode))
                {
                    childNode.ItemCount++;
                }
                else
                {
                    childNode = new FPTree<TDataType>.Node(item, currentNode);
                    currentNode.Children.Add(childNode);
                    fpTree.SideArray[item].Add(childNode);
                }

                currentNode = childNode;
            }
        }

        //Console.WriteLine(fpTree);
        return fpTree;
    }

    private static List<ItemSet<TDataType>> Growth<TDataType>(FPTree<TDataType> tree, ItemSet<TDataType> itemSetG, int minNum) where TDataType : IComparable
    {
        var result = new List<ItemSet<TDataType>>();

        if (tree.HasSingularPath())
        {
            var pathItems = tree.GetPathItems(minNum);
            var frequentItemSets = new List<ItemSet<TDataType>>();
            foreach (var subset in pathItems.GetPowerSet())
            {
                var asArray = subset.ToArray();
                if (asArray.Length <= 0) continue;
                var set = new ItemSet<TDataType>(asArray);
                set.ItemList.AddRange(itemSetG.ItemList);
                frequentItemSets.Add(set);
            }
            
            result.AddRange(frequentItemSets);
            //Console.WriteLine(pathItems.Aggregate($"One Path, ({result}): ", (a, b) => $"{a}, {b}"));
        }
        else
        {
            var sideArrayKeysReverse = tree.SideArray.Keys.Reverse().ToList();

            foreach (var key in sideArrayKeysReverse)
            {
                var itemSetExtended = itemSetG.Clone();
                
                var node = tree.SideArray[key].First();
                itemSetExtended.ItemList.Add(node.Item);

                if (itemSetExtended.ItemList.Count > 1)
                {
                    //here we have I = {E, F} and the frequency is node.ItemCount
                    result.Add(itemSetExtended);
                }

                var condPatternBase = ConditionalPatternBase(itemSetExtended, tree);
                var reducedTree = BuildFpTree(condPatternBase, minNum);

                //Console.WriteLine(condPatternBase.Aggregate($"R({itemSetExtended}): ", (a, b) => $"{a}, {b}"));
                //Console.WriteLine(reducedTree);

                if (reducedTree.IsEmpty())
                {
                    continue;
                }

                var growthSets = Growth(reducedTree, itemSetExtended, minNum);
                result.AddRange(growthSets);
            }
        }

        return result;
    }

    private static List<ItemSet<TDataType>> ConditionalPatternBase<TDataType>(ItemSet<TDataType> itemSetR,
        FPTree<TDataType> tree) where TDataType : IComparable
    {
        var patternBase = new List<ItemSet<TDataType>>();
        var lowestNodes =  tree.GetLowestNodeOf(itemSetR).Value;

        foreach (var lowNode in lowestNodes)
        {
            var path = lowNode.GetPathToRoot();
            var frequentItemsWithoutFirst = path.Skip(1).Select(n => n.Item).ToArray();
            
            if(frequentItemsWithoutFirst.Length == 0) continue;
            
            for (var i = 0; i < lowNode.ItemCount; i++)
            {
                patternBase.Add(new ItemSet<TDataType>(frequentItemsWithoutFirst));
            }
        }
        
        return patternBase;
    }
}