namespace AI_DS_AlgoCollection.Algorithms.Clustering;

public interface IClusterInterface
{
    float Distance(IClusterInterface other);
    static float Distance(IClusterInterface a, IClusterInterface b)
    {
        return a.Distance(b);
    }
    IClusterInterface Add(IClusterInterface other);
    IClusterInterface Divide(int n);
}

public static class DBScan
{
    const string Undefined = "Undefined";
    const string Noise = "Noise";
    
    public static void DoDBScan(this List<IClusterInterface> data)
    {
        var result = DBScanAlgorithm(data, 3 , 1.0f);
        result.GroupBy(res => res.Value).ToList().ForEach(group =>
        {
            Console.WriteLine($"Cluster {group.Key}");
            
            group.ToList().ForEach(y => Console.WriteLine(y.Key));
        });
    }
    
    private static Dictionary<IClusterInterface, string> DBScanAlgorithm(List<IClusterInterface> data, int minPts, float epsilon)
    {
        var id = 0;
        var result = data.ToDictionary(dataPoint => dataPoint, _ => Undefined);
        
        foreach (var dataPoint in data)
        {
            if (result[dataPoint] == Undefined)
            {
                var epsNeighborhood = RangeQuery(dataPoint, data, epsilon);
                if (epsNeighborhood.Count < minPts)
                {
                    //not core point
                    result[dataPoint] = Noise;
                }
                else
                {
                    id++;
                    result[dataPoint] = id.ToString(); //core point

                    //expansion
                    var set = new List<IClusterInterface>(epsNeighborhood);
                    set.Remove(dataPoint);

                    for (var i = 0; i < set.Count; i++)
                    {
                        var current = set[i];
                        if (result[current] == Noise)
                        {
                            result[current] = id.ToString(); //border point
                        }

                        if (result[current] != Undefined) continue;
                        
                        epsNeighborhood = RangeQuery(current, data, epsilon);
                        result[current] = id.ToString();
                        if (epsNeighborhood.Count >= minPts)
                        {
                            set.AddRange(epsNeighborhood);
                        }
                    }
                }
            }
        }

        return result;
    }
    
    private static List<IClusterInterface> RangeQuery(IClusterInterface dataPoint, List<IClusterInterface> data, float epsilon)
    {
        return data.Where(point => IClusterInterface.Distance(dataPoint, point) <= epsilon).ToList();
    }
}