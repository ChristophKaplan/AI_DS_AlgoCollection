using AI_DS_AlgoCollection.Algorithms.Association;
using AI_DS_AlgoCollection.DataStructures;

namespace AI_DS_AlgoCollection.Utilities;

public static class RuleUtilities
{
    public static List<AssociationRule<TDataType>> CreateRulesFromItemSets<TDataType>(
        this List<ItemSet<TDataType>> frequentItemSets, float minConf, List<ItemSet<TDataType>> itemSetsOrigin)
        where TDataType : IComparable
    {
        var rules = new List<AssociationRule<TDataType>>();
        foreach (var itemSet in frequentItemSets)
            rules.AddRange(CreateRulesFromItemSet(itemSet, minConf, itemSetsOrigin));

        return rules;
    }

    private static List<AssociationRule<TDataType>> CreateRulesFromItemSet<TDataType>(
        ItemSet<TDataType> frequentItemSet, float minConf, List<ItemSet<TDataType>> itemSetsOrigin)
        where TDataType : IComparable
    {
        var maxLen = frequentItemSet.ItemList.Count;
        var genConclusion = frequentItemSet.ItemList.Select(item => new ItemSet<TDataType>(item)).ToList();
        var rules = new List<AssociationRule<TDataType>>();

        for (var i = 1; i < maxLen; i++)
        {
            foreach (var conclusion in genConclusion)
            {
                var premise = ItemSet<TDataType>.GetSubtract(frequentItemSet.Clone(), conclusion);
                var rule = new AssociationRule<TDataType>(premise, conclusion);
                if (itemSetsOrigin.Confidence(rule) < minConf) continue;

                rules.Add(rule);
            }

            genConclusion = genConclusion.AprioriGen();
        }

        return rules;
    }
}