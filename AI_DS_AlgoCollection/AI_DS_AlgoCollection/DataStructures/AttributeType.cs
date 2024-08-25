using System.Numerics;
using AI_DS_AlgoCollection.Algorithms.Clustering;

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

public struct ClusterVector : IClusterInterface
{
    private readonly float _x;
    private readonly float _y;
    public ClusterVector(float x, float y) {
        _x = x;
        _y = y;
    }

    public IClusterInterface Add(IClusterInterface other)
    {
        var rightCluster = (ClusterVector)other;
        return new ClusterVector(_x + rightCluster._x, _y + rightCluster._y);
    }

    public IClusterInterface Divide(int n) => new ClusterVector(_x/n, _y/n);
    
    public float Distance(IClusterInterface other) {
        var otherCluster = (ClusterVector)other;
        return (float)Math.Sqrt(Math.Pow(_x - otherCluster._x, 2) + Math.Pow(_y - otherCluster._y, 2));
    }
    
    public override string ToString() => $"({_x}, {_y})";
}