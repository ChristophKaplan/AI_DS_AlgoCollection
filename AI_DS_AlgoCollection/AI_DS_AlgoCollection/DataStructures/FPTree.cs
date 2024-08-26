namespace AI_DS_AlgoCollection.DataStructures;

public class FPTree<TDataType>
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