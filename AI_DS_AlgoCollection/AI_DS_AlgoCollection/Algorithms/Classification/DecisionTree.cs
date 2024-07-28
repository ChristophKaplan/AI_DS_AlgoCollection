using AI_DS_AlgoCollection.DataStructures;

namespace AI_DS_AlgoCollection.Algorithms.Classification;
public abstract class DtNode<TDataType, TDataValue> where TDataType : notnull {
    private class DtEdge {
        public readonly TDataValue Value;
        private readonly DtNode<TDataType, TDataValue> _node;

        public DtEdge(DtNode<TDataType, TDataValue> node, TDataValue value) {
            _node = node;
            Value = value;
        }
        
        public override string ToString() => Value.ToString();
        public DtNode<TDataType, TDataValue> GetNode() => _node;
    }
    
    private DtNode<TDataType, TDataValue> Parent;
    private readonly List<DtEdge> _edges = new ();
    protected readonly DataTable<TDataType, TDataValue> Data; //for debug

    protected DtNode(DtNode<TDataType, TDataValue> parent, DataTable<TDataType, TDataValue> data) {
        Parent = parent;
        Data = data;
    }

    private bool IsRoot() => Parent == null;
    public abstract override string ToString();
    
    private void AddEdge(DtEdge e) {
        var to = e.GetNode();
        to.Parent = this;
        if (!_edges.Contains(e)) _edges.Add(e);
    }

    public void AddEdgeTo(DtNode<TDataType, TDataValue> nodeTo, TDataValue value) {
        AddEdge(new DtEdge(nodeTo, value));
    }

    private DtEdge GetIncomingEdge() => IsRoot() ? null : Parent.GetOutgoingEdge(this);
    private DtEdge GetOutgoingEdge(DtNode<TDataType, TDataValue> node) => _edges.FirstOrDefault(edge => edge.GetNode().Equals(node));
    
    public string Explain() {
        if (IsRoot()) return "root";
        return $"{ToString()} because {Parent} is {GetIncomingEdge().Value.ToString()}, {Parent.Explain()}";
    }
}

public class DtNodeAttribute<TDataType, TDataValue> : DtNode<TDataType, TDataValue> where TDataType : notnull {
    private readonly TDataType _attribute;
    public DtNodeAttribute(TDataType attribute, DtNode<TDataType, TDataValue> parent, DataTable<TDataType, TDataValue> data) : base(parent, data) {
        _attribute = attribute;
    }
    public override string ToString() => _attribute.ToString();
}

public class DtNodeClassification<TDataType, TDataValue> : DtNode<TDataType, TDataValue> where TDataType : notnull {
    private readonly bool _truthValue;
    public DtNodeClassification(bool truthValue, DtNode<TDataType, TDataValue> parent, DataTable<TDataType, TDataValue> data) : base(parent, data) {
        _truthValue = truthValue;
    }
    public override string ToString() => _truthValue.ToString();
}

public class DecisionTree<TDataType, TDataValue> where TDataType : notnull {
    public readonly DtNode<TDataType, TDataValue> Root;
    
    public DecisionTree(ClassifictaionData<TDataType, TDataValue> data) {
        Root = ConstructDecisionTree(data, true);
    }

    private DtNode<TDataType, TDataValue> ConstructDecisionTree(ClassifictaionData<TDataType, TDataValue> dtData, bool defaultVal, DtNode<TDataType, TDataValue> parent = null) {
        //Console.WriteLine(data.PrintInfo());
        
        if (dtData.RowCount == 0) {
            return new DtNodeClassification<TDataType, TDataValue>(defaultVal, parent, dtData);
        }
        if (dtData.IsSameClassificationForExamples()) return new DtNodeClassification<TDataType, TDataValue>(dtData.Classification[0], parent, dtData);
        if (dtData.ColCount == 0) {
            Console.WriteLine("Leftover examples ?");
            return new DtNodeClassification<TDataType, TDataValue>(defaultVal, parent, dtData);
        }

        var curAttribute = dtData.ChooseAttribute();
        var curTree = new DtNodeAttribute<TDataType, TDataValue>(curAttribute, parent, dtData);

        var values = dtData.PossibleValuesForType(curAttribute);
        
        foreach (var curValue in values) {
            var dataSubset = dtData.SelectRows(curAttribute, curValue);
            var nextNode = ConstructDecisionTree(dataSubset, dtData.MajorityVal(), curTree);
            curTree.AddEdgeTo(nextNode, curValue);
        }

        return curTree;
    }
}

public static class DtExtensions {
    public static DecisionTree<TDataType, TDataValue> GetDT<TDataType, TDataValue>(this ClassifictaionData<TDataType, TDataValue> data) where TDataType : notnull {
        return new DecisionTree<TDataType, TDataValue>(data);
    }
    
    public static TDataType ChooseAttribute<TDataType, TDataValue>(this ClassifictaionData<TDataType, TDataValue> data, bool id3 = false) {
        return id3 ? data.ID3_GetMaxGain().Item1 : data.C4_5_GetMaxGainRatio().Item1;
    }
    
    private static float Entropy(params float[] probabilities) {
        return probabilities.Where(prob => prob != 0f).Sum(prob => -prob * (float)Math.Log(prob, 2));
    }
    
    private static float InformationEntropy<TDataType, TDataValue>(this ClassifictaionData<TDataType, TDataValue> data) {
        var exampleCount = (float)data.RowCount;
        var probabilityToPickANegativeExample = data.GetPosOrNegExamples(false).Count / exampleCount;
        var probabilityToPickAPositiveExample = data.GetPosOrNegExamples(true).Count / exampleCount;
        return Entropy(probabilityToPickAPositiveExample, probabilityToPickANegativeExample );
    }

    private static float ConditionalEntropy<TDataType, TDataValue>(this ClassifictaionData<TDataType, TDataValue> data, TDataType type) {
        var possibleValues = data.PossibleValuesForType(type);
        var exampleCount = (float)data.RowCount;

        var sum = 0f;
        foreach (var posVal in possibleValues) {
            var sub = data.SelectRows(type, posVal);
            var prob = sub.RowCount / exampleCount;
            sum += prob * InformationEntropy(sub);
        }

        return sum;
    }

    private static float InformationGain<TDataType, TDataValue>(this ClassifictaionData<TDataType, TDataValue> data, TDataType attribute) {
        return data.InformationEntropy() - data.ConditionalEntropy(attribute);
    }

    private static float GainRatio<TDataType, TDataValue>(this ClassifictaionData<TDataType, TDataValue> data, TDataType attribute) {
        var gain = data.InformationGain(attribute);
        var splitInfo = data.SplitInfo(attribute);
        if (splitInfo == 0) {
            return 0f;
        }

        return gain / splitInfo;
    }

    private static float SplitInfo<TDataType, TDataValue>(this ClassifictaionData<TDataType, TDataValue> data, TDataType attribute) {
        var probabilities = new List<float>();
        var possibleValues = data.PossibleValuesForType(attribute);
        var exampleCount = (float)data.RowCount;
        
        foreach (var posVal in possibleValues) {
            var selection = data.SelectRows(attribute, posVal);
            var prob = selection.RowCount / exampleCount;
            probabilities.Add(prob);
        }

        return Entropy(probabilities.ToArray());
    }

    private static (TDataType, float) ID3_GetMaxGain<TDataType, TDataValue>(this ClassifictaionData<TDataType, TDataValue> data) {
        (TDataType, float) max = default;
        
        foreach (var cls in data.Types) {
            var currentGain = data.InformationGain(cls);
            if (currentGain > max.Item2) max = (cls, currentGain);
        }

        if (max.Item1.ToString() == "null") throw new Exception("No max Gain found?!");
        return max;
    }

    private static (TDataType, float) C4_5_GetMaxGainRatio<TDataType, TDataValue>(this ClassifictaionData<TDataType, TDataValue> data) {
        (TDataType, float) max = default;
        
        foreach (var cls in data.Types) {
            var currentGain = data.GainRatio<TDataType, TDataValue>(cls);
            if (currentGain > max.Item2) max = (cls, currentGain);
        }

        if (max.Item1.ToString() == "null") throw new Exception($"No max GainRatio found?!");
        return max;
    }
}
