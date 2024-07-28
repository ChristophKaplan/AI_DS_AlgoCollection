public class BayesClassification 
{
    public void Start() {
        var o1 = new Probability.Outcome("1");
        var o2 = new Probability.Outcome("2");
        var o3 = new Probability.Outcome("3");
        var ProbabilitySystem = new Probability(new HashSet<Probability.Outcome>() {o1,o2,o3});
        ProbabilitySystem.ShowAll();
    }
}

public class Probability {
    private HashSet<Outcome> _sampleSpace; //Ergebnismenge
    private HashSet<Event> _eventSpace = new(); //Ereignismenge
    public class Outcome {
        internal string _description;
        public Outcome(string description) {
            _description = description;
        }
        public override string ToString() => _description;
    }
    
    public class Event {
        internal HashSet<Outcome> _outcomes;
        public Event(HashSet<Outcome> outcomes) {
            _outcomes = outcomes;
        }
        public Event(params Outcome[] outcomes) {
            _outcomes = outcomes.ToHashSet();
        }

        public override string ToString() {
            return _outcomes.Aggregate("{", (current, o) => current + (o + ", "))+"}";
        }
    }

    public Probability(HashSet<Outcome> sampleSpace) {
        _sampleSpace = sampleSpace;

        foreach (var set in sampleSpace.GetPowerSet()) {
            _eventSpace.Add(new Event(set.ToHashSet()));
        }
    }

    public float P(Outcome e) {
        var probability = 1/(float)_sampleSpace.Count;

        switch (e._description) {
            case "1":
            case "2":
                return 0.25f;
            case "3":
                return 0.5f;
            default:
                return probability;
        }
    }
    public float P(Event e) {
        return e._outcomes.Sum(P);
    }
    public float P(Event e,Event c) {
        var intersection = e._outcomes.Intersect(c._outcomes).ToHashSet();
        return P(new Event(intersection)) / P(c);
    }
    public float SatzVonBayes(Event e,Event c) {
        return (P(c, e) * P(e)) / P(c);
    }
    
    
    public void ShowAll() {
        foreach (var e in _eventSpace) {
            Console.WriteLine(e +" = "+ P(e));
        }
    }
}



public static class HashSetExtensions
{
    public static IEnumerable<IEnumerable<T>> GetPowerSet<T>(this HashSet<T> set)
    {
        List<T> list = set.ToList();
        int max = 1 << list.Count;
        for (int i = 0; i < max; i++)
        {
            yield return from j in Enumerable.Range(0, list.Count)
                where (i & (1 << j)) != 0
                select list[j];
        }
    }
}
