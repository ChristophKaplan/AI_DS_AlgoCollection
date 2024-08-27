using AI_DS_AlgoCollection.DataStructures;
using AI_DS_AlgoCollection.Utilities;

namespace AI_DS_AlgoCollection.Algorithms.Association;

public static class FrequentPatternGrowth
{
    public static void DoFrequentPattern<TDataType, TDataValue>(this EventData<TDataType, TDataValue> data,
        int minNum = 3)
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

        Console.WriteLine(result.Aggregate("FP: \t\t",
            (current, itemSet) => current + itemSet + itemSets.Num(itemSet) + ", "));
    }

    private static FPTree<TDataType> BuildFpTree<TDataType>(List<ItemSet<TDataType>> itemSets, int minNum)
    {
        var fpTree = new FPTree<TDataType>();
        var sortedSingleItemSets = itemSets.GetFrequentSingleItemSetsByMinNum(minNum);

        foreach (var itemSet in sortedSingleItemSets)
            fpTree.SideArray.Add(itemSet.ItemList.First(), new List<FPTree<TDataType>.Node>());

        foreach (var itemSet in itemSets)
        {
            var currentNode = fpTree.Root;
            var sortedItemSet = itemSet.ItemList.Where(item => itemSets.Num(item) >= minNum)
                .OrderByDescending(itemSets.Num).ToList();

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

    private static List<ItemSet<TDataType>> Growth<TDataType>(FPTree<TDataType> tree, ItemSet<TDataType> itemSetG,
        int minNum)
    {
        var result = new List<ItemSet<TDataType>>();

        if (tree.HasSingularPath())
        {
            var pathItems = tree.GetPathItems(minNum);
            var frequentItemSets = new List<ItemSet<TDataType>>();
            foreach (var subset in pathItems.GetPowerSet(pathItems.Count))
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
                    //here we have I = {E, F} and the frequency is node.ItemCount
                    result.Add(itemSetExtended);

                var condPatternBase = ConditionalPatternBase(itemSetExtended, tree);
                var reducedTree = BuildFpTree(condPatternBase, minNum);

                //Console.WriteLine(condPatternBase.Aggregate($"R({itemSetExtended}): ", (a, b) => $"{a}, {b}"));
                //Console.WriteLine(reducedTree);

                if (reducedTree.IsEmpty()) continue;

                var growthSets = Growth(reducedTree, itemSetExtended, minNum);
                result.AddRange(growthSets);
            }
        }

        return result;
    }

    private static List<ItemSet<TDataType>> ConditionalPatternBase<TDataType>(ItemSet<TDataType> itemSetR,
        FPTree<TDataType> tree)
    {
        var patternBase = new List<ItemSet<TDataType>>();
        var lowestNodes = tree.GetLowestNodeOf(itemSetR).Value;

        foreach (var lowNode in lowestNodes)
        {
            var path = lowNode.GetPathToRoot();
            var frequentItemsWithoutFirst = path.Skip(1).Select(n => n.Item).ToArray();

            if (frequentItemsWithoutFirst.Length == 0) continue;

            for (var i = 0; i < lowNode.ItemCount; i++)
                patternBase.Add(new ItemSet<TDataType>(frequentItemsWithoutFirst));
        }

        return patternBase;
    }
}