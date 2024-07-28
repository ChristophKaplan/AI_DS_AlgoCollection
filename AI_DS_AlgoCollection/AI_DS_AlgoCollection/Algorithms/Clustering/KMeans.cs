using System.Drawing;
using System.Numerics;

namespace DataMining {
    public class KCluster {
        public class KObject {
            public readonly Vector2 Point;

            public KObject(Vector2 point) {
                Point = point;
            }

            public float Dist(KObject other) {
                return Vector2.Distance(Point, other.Point);
            }

            public override bool Equals(object obj) {
                if (obj is KObject other) { return Point.Equals(other.Point); }

                return false;
            }

            public override int GetHashCode() {
                return Point.GetHashCode();
            }
        }

        private int _index;
        public List<KObject> ClusterObjects = new();

        public KCluster(int index) {
            _index = index;
        }

        public KObject Mean() {
            var sum = Vector2.Zero;
            for (int i = 0; i < ClusterObjects.Count; i++) { sum += ClusterObjects[i].Point; }
            
            var result = sum * (1 / (float)ClusterObjects.Count);

            return new KObject(result);
        }

        public void ViewCluster() {
            foreach (var o in ClusterObjects) {
                /*GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = o.Point;
                obj.name = _index + " " + o.Point;
                var v = _index / 10f * 2f;
                obj.GetComponent<Renderer>().material.color = new Color(v,v,v);*/
            }
        }
        
    }

    public class KMeans {
        private readonly int _k = 2;
        private List<KCluster.KObject> _data;
        private List<KCluster.KObject> _centers;
        private List<KCluster> _clusterSet;

        public void Test() {
            //_data = MakeTestData(500);

            _data = new List<KCluster.KObject>() {
                new KCluster.KObject(new Vector2(2,2)),
                new KCluster.KObject(new Vector2(3,2)),
                new KCluster.KObject(new Vector2(4,1)),
                new KCluster.KObject(new Vector2(5,8)),
                new KCluster.KObject(new Vector2(1,3)),
                new KCluster.KObject(new Vector2(6,4)),
                new KCluster.KObject(new Vector2(8,4)),
                new KCluster.KObject(new Vector2(7,5)),
            };
            
            var clusters = GetClusters();

            Console.WriteLine("Varianz: " +ClusterVarianz());
            foreach (var cluster in clusters) { cluster.ViewCluster(); }
        }

        private List<KCluster.KObject> MakeTestData(int amount, float size = 10f) {
            List<KCluster.KObject> list = new();
            for (int i = 0; i < amount; i++) {
                var rnd = new Vector2(RandomRange(0f, size), RandomRange(0f, size));
                list.Add(new KCluster.KObject(rnd));
            }

            return list;
        }

        private float RandomRange(float min, float max) {
            Random rnd = new Random();
            return (float)rnd.NextDouble() * (max - min) + min;
        }
        
        private float ClusterVarianz() {
            var sum = 0f;
            for (var i = 0; i < _clusterSet.Count; i++) {
                var ci = _clusterSet[i];
                for (int j = 0; j < ci.ClusterObjects.Count; j++) {
                    var p = ci.ClusterObjects[j];
                    sum += (float)Math.Pow(p.Dist(_centers[i]),2);
                }
            }

            return sum;
        }
        public List<KCluster> GetClusters() {

            var somethingChanged = true;
            _centers = _data.OrderBy(c => Random.Shared.NextDouble()).Take(_k).ToList();

            _clusterSet = new();

            var saftey = 0;
            while (somethingChanged) {
                _clusterSet.Clear();
                
                for (var i = 0; i < _k; i++) {
                    var cluster = new KCluster(i);
                    _clusterSet.Add(cluster);

                    foreach (var x in _data) {
                        if (PrevClustersContains(_clusterSet, x)) continue;

                        if (CenterDistSmallerThanAllOthersCenters(x, _centers[i])) {
                            _clusterSet[i].ClusterObjects.Add(x);
                        }
                    }
                }
                
                List<KCluster.KObject> newCenters = new();
                for (var i = 0; i < _centers.Count; i++) {
                    if (_clusterSet.Count <= i) {
                        newCenters.Add(_centers[i]); //keep old if empty cluster ?...
                        continue;
                    }

                    var newCenter = _clusterSet[i].Mean();
                    newCenters.Add(newCenter);
                }

                if (!Same(_centers, newCenters)) {
                    _centers = newCenters;
                    //Debug.Log("new centers found!");
                }
                else {
                    somethingChanged = false;
                }
            }


            return _clusterSet;
        }

        private bool CenterDistSmallerThanAllOthersCenters(KCluster.KObject x, KCluster.KObject center) {
            for (var j = 0; j < _k; j++) {
                var cj = _centers[j];
                if (!(Math.Pow(x.Dist(center), 2) <= Math.Pow(x.Dist(cj), 2))) { return false; }
            }

            return true;
        }

        private bool Same(List<KCluster.KObject> a, List<KCluster.KObject> b) {
            if (a.Count != b.Count) Console.WriteLine("error");

            for (var index = 0; index < a.Count; index++) {
                if (!a[index].Equals(b[index])) { return false; }
            }
            
            return true;
        }

        private bool PrevClustersContains(List<KCluster> clusterSet, KCluster.KObject x) {
            for (var i = 0; i < clusterSet.Count - 1; i++) {
                if (clusterSet[i].ClusterObjects.Contains(x)) return true;
            }

            return false;
        }
    }
}