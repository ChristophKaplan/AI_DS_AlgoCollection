namespace AI_DS_AlgoCollection.DataStructures;

public struct AttributeType : IComparable{ //Could be Enum ?
    private readonly string _className;
    public AttributeType(string className) {
        _className = className;
    }

    public override bool Equals(object? obj) {
        var other = (AttributeType)obj;
        return ToString().Equals(other.ToString());
    }
    public override int GetHashCode() => _className.GetHashCode();
    public override string ToString() => _className;
    
    public int CompareTo(object? obj) {
        var other = (AttributeType)obj;
        return ToString().CompareTo(other.ToString());
    }
}