namespace AI_DS_AlgoCollection.DataStructures;

public class AssociationRule<TDataType>
{
    public AssociationRule(ItemSet<TDataType> x, ItemSet<TDataType> y)
    {
        X = x;
        Y = y;
        IsDisjunct();
    }

    public ItemSet<TDataType> X { get; }
    public ItemSet<TDataType> Y { get; }

    private bool IsDisjunct()
    {
        return !X.Contains(Y) && !Y.Contains(X);
    }

    public ItemSet<TDataType> GetJoined()
    {
        return ItemSet<TDataType>.Union(X, Y);
    }

    public override string ToString()
    {
        return $"{X} -> {Y}";
    }

    public override bool Equals(object obj)
    {
        var other = (AssociationRule<TDataType>)obj;
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}