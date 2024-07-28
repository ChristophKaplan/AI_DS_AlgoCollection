using System.Drawing;
using System.Numerics;

public class DBScan {
    private List<Vector3> _data;
    private int _minPts;
    private float _epsilon; //radius einer umgebung
    private int _id = 0;

    public void Test() {
        _data = MakeTestData(200,20f);
        _minPts = 3;
        _epsilon = 3.0f;
        var result = Scan();

        foreach (var o in result.Keys) {
            /*GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.position = o;
            obj.name = result[o] + " " + o;
            
            obj.GetComponent<Renderer>().material.color = GetColor(result[o]);*/
        }
    }

    private Color GetColor(string s) {
        if(s == "undefined") return Color.Crimson;
        if (s == "R") return Color.Aqua;
        Int32.TryParse(s,out var n);
        var v = n / 10;
        return Color.FromArgb(v, v, v);
    }

    public Dictionary<Vector3, string> Scan() {
        Dictionary<Vector3, string> result = new();

        foreach (var d in _data) {
            result.Add(d, "undefined");
        }

        foreach (var d in _data) {
            if (result[d] == "undefined") {
                var U = GetEUmgebung(d);
                if (U.Count < _minPts) { //KEIN KERNPUNKT
                    result[d] = "R";
                }
                else { //KERNPUNKT
                    _id++;
                    result[d] = _id.ToString();
                    
                    //expansion
                    var S = new List<Vector3>(U);
                    S.Remove(d);

                    for (var i = 0; i < S.Count; i++) { //andere punkte der Umgebung werden durchlaufen
                        var y = S[i];
                        if (result[y] == "R") {
                            result[y] = _id.ToString(); //randpunkt  ist kein kernpunkt aber in umgebung eines
                        }

                        if (result[y] == "undefined") {
                            U = GetEUmgebung(y);
                            result[y] = _id.ToString();
                            if (U.Count >= _minPts) { S.AddRange(U); }
                        }
                    }
                }
            }
        }

        return result;
    }

    private int EUmgebung(Vector3 x) {
        return _data.Where(point => Vector3.Distance(x, point) <= _epsilon).ToList().Count;
    }

    private bool IsKernpunkt(Vector3 x) {
        return EUmgebung(x) >= _minPts;
    }

    private bool IsRandpunkt(Vector3 x) {
        return !IsKernpunkt(x) && IsEUmgebungOfAKernpunkt(x);
    }

    private bool IsRauschpunkt(Vector3 x) {
        return !IsKernpunkt(x) && !IsRandpunkt(x);
    }

    private bool IsEUmgebungOfAKernpunkt(Vector3 x) {
        foreach (var point in _data) {
            if (IsKernpunkt(point) && Vector3.Distance(x, point) <= _epsilon) return true;
        }

        return false;
    }

    private List<Vector3> GetEUmgebung(Vector3 x) {
        List<Vector3> list = new List<Vector3>();
        foreach (var point in _data) {
            if (Vector3.Distance(x, point) <= _epsilon) list.Add(point);
        }

        return list;
    }

    private float RandomRange(float min, float max) {
        Random rnd = new Random();
        return (float)rnd.NextDouble() * (max - min) + min;
    }

    private List<Vector3> MakeTestData(int amount, float size = 10f) {
        List<Vector3> list = new();
        for (int i = 0; i < amount; i++) {
            var rnd = new Vector3(RandomRange(0f, size), RandomRange(0f, size), RandomRange(0f, size));
            list.Add(rnd);
        }

        return list;
    }
}