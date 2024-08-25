using AI_DS_AlgoCollection.Algorithms.Clustering;

namespace AI_DS_AlgoCollection.DataStructures;

public abstract class DataTable<TDataType, TDataValue> where TDataType : notnull {
    public int ColCount { get; }
    public int RowCount => Matrix.Count;

    protected readonly List<TDataValue[]> Matrix = new();
    public readonly List<TDataType> Types = new();

    protected DataTable(TDataType[] types, List<TDataValue[]> matrix) {
        ColCount = types.Length;
        Types.AddRange(types);
        Matrix = matrix;
    }

    public TDataValue this[int x, int y] {
        get => Matrix[y][x];
        set => Matrix[y][x] = value;
    }

    public TDataValue[] GetRow(int index) => Matrix[index];

    public IEnumerable<TDataValue> GetColumn(int index) => Matrix.Select(row => row[index]);

    public TDataType GetColumnType(int index) => Types[index];

    public void AddRow(params TDataValue[] row) {
        Matrix.Add(row);
    }

    public void AddColumn(TDataType type, params TDataValue[] column) {
        Types.Add(type);
        for (int i = 0; i < column.Length; i++) {
            if (i >= Matrix.Count) Matrix.Add(new TDataValue[ColCount]);
            Matrix[i][ColCount - 1] = column[i];
        }
    }

    public List<TDataValue> PossibleValuesForType(TDataType type) {
        var collected = new List<TDataValue>();
        var index = Types.IndexOf(type);
        foreach (var example in Matrix) {
            var valuesInExample = example[index];
            if (!collected.Contains(valuesInExample)) collected.Add(valuesInExample);
        }

        return collected;
    }

    protected virtual bool IsDataConsistent() {
        foreach (var example in Matrix.Where(example => ColCount != example.Length)) {
            Console.WriteLine($"attribute count != example count {example}");
            return false;
        }

        return true;
    }

    public string TypesToString() {
        var result = "";
        for (var i = 0; i < ColCount; i++) {
            result += Types[i] + " ";
        }

        return result;
    }

    public override string ToString() {
        var result = "";
        for (var i = 0; i < RowCount; i++) {
            for (var j = 0; j < ColCount; j++) {
                result += this[j, i] + " ";
            }

            result += "\n";
        }

        return result;
    }
}

public class ClassifictaionData<TDataType, TDataValue> : DataTable<TDataType, TDataValue> where TDataType : notnull {
    public readonly List<bool> Classification;

    public ClassifictaionData(TDataType[] types, List<TDataValue[]> matrix, bool[] classification) : base(types, matrix) {
        Classification = classification.ToList();
    }

    public ClassifictaionData<TDataType, TDataValue> SelectRows(TDataType type, TDataValue value) {
        var selectedExamples = new List<TDataValue[]>();
        var selectedTruthValues = new List<bool>();
        var index = Types.IndexOf(type);
        for (var i = 0; i < RowCount; i++) {
            if (this[index, i].Equals(value)) {
                selectedExamples.Add(GetRow(i));
                selectedTruthValues.Add(Classification[i]);
            }
        }

        return new ClassifictaionData<TDataType, TDataValue>(Types.ToArray(), selectedExamples, selectedTruthValues.ToArray());
    }

    public bool MajorityVal() => GetPosOrNegExamples().Count >= GetPosOrNegExamples(false).Count;

    public List<TDataValue[]> GetPosOrNegExamples(bool posOrNegValue = true) {
        return Matrix.Where((example, i) => Classification[i] == posOrNegValue).ToList();
    }

    public bool IsSameClassificationForExamples() {
        for (var i = 0; i < RowCount - 1; i++) {
            if (Classification[i] != Classification[i + 1]) {
                return false;
            }
        }

        return true;
    }

    protected override bool IsDataConsistent() {
        if (RowCount == Classification.Count) {
            return base.IsDataConsistent();
        }

        Console.WriteLine($"TruthValue count {Classification.Count} != example count {RowCount}!");
        return false;
    }
}

public class EventData<TDataType, TDataValue> : DataTable<TDataType, TDataValue> where TDataType : notnull, IComparable {
    public EventData(TDataType[] types, List<TDataValue[]> matrix) : base(types, matrix) {
    }

    public List<ItemSet<TDataType>> GetItemSets() {
        var itemSets = new List<ItemSet<TDataType>>();

        for (var i = 0; i < RowCount; i++) {
            var items = new List<TDataType>();

            for (var j = 0; j < ColCount; j++) {
                if (this[j, i].Equals(true)) items.Add(Types[j]);
            }

            itemSets.Add(new ItemSet<TDataType>(items.ToArray()));
        }

        
        foreach (var itemSet in itemSets)
        {
            itemSet.ItemList.Sort();
        }
        
        return itemSets;
    }
}

public class ClusterData<TDataType, TDataValue> : DataTable<TDataType, TDataValue> where TDataType : IClusterInterface, IComparable {
    public ClusterData(TDataType[] types, List<TDataValue[]> matrix) : base(types, matrix) {
    }

    public List<ItemSet<TDataType>> GetItemSets() {
        var itemSets = new List<ItemSet<TDataType>>();

        for (var i = 0; i < RowCount; i++) {
            var items = new List<TDataType>();

            for (var j = 0; j < ColCount; j++) {
                if (this[j, i].Equals(true)) items.Add(Types[j]);
            }

            itemSets.Add(new ItemSet<TDataType>(items.ToArray()));
        }

        
        foreach (var itemSet in itemSets)
        {
            itemSet.ItemList.Sort();
        }
        
        return itemSets;
    }
}