using AI_DS_AlgoCollection.DataStructures;

namespace AI_DS_AlgoCollection.Algorithms.Clustering;

public static class DbScan
{
    const string Undefined = "Undefined";
    const string Noise = "Noise";
    
    public static void DoDbScan<TDataValue>(this ClusterData<TDataValue> data)
    {
        var result = DbScanAlgorithm(data.Data, 3 , 1.0f, data.DistanceFunc);
        result.GroupBy(res => res.Value).ToList().ForEach(group =>
        {
            Console.WriteLine($"Cluster {group.Key}");
            group.ToList().ForEach(y => Console.WriteLine(y.Key));
        });
    }
    
    private static Dictionary<TDataValue, string> DbScanAlgorithm<TDataValue>(List<TDataValue> data, int minPts, float epsilon, Func<TDataValue, TDataValue, float> distanceFunc)
    {
        var id = 0;
        var result = data.ToDictionary(dataPoint => dataPoint, _ => Undefined);
        
        foreach (var dataPoint in data)
        {
            if (result[dataPoint] == Undefined)
            {
                var epsNeighborhood = RangeQuery(dataPoint, data, epsilon, distanceFunc);
                if (epsNeighborhood.Count < minPts)
                {
                    result[dataPoint] = Noise;
                }
                else
                {
                    id++;
                    result[dataPoint] = id.ToString(); //core point

                    //expansion
                    var set = new List<TDataValue>(epsNeighborhood);
                    set.Remove(dataPoint);

                    for (var i = 0; i < set.Count; i++)
                    {
                        var current = set[i];
                        if (result[current] == Noise)
                        {
                            result[current] = id.ToString(); //border point
                        }

                        if (result[current] != Undefined) continue;
                        
                        epsNeighborhood = RangeQuery(current, data, epsilon, distanceFunc);
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
    
    private static List<TDataValue> RangeQuery<TDataValue>(TDataValue dataPoint, List<TDataValue> data, float epsilon, Func<TDataValue, TDataValue, float> distanceFunc)
    {
        return data.Where(point => distanceFunc(dataPoint, point) <= epsilon).ToList();
    }
}