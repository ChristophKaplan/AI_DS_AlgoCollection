namespace AI_DS_AlgoCollection.DataStructures;

public class AssociationRule<TDataType>{
    public ItemSet<TDataType> X { get; }
    public ItemSet<TDataType> Y { get; }

    public AssociationRule(ItemSet<TDataType>  x, ItemSet<TDataType>  y) {
        X = x;
        Y = y;
        IsDisjunct();
    }

    private bool IsDisjunct() => !X.Contains(Y) && !Y.Contains(X);

    public ItemSet<TDataType>  GetJoined() {
        return ItemSet<TDataType> .Union(X, Y);
    }

    public override string ToString() => $"{X} -> {Y}";
    
    public override bool Equals(object obj) {
        var  other = (AssociationRule<TDataType> )obj;
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }
}