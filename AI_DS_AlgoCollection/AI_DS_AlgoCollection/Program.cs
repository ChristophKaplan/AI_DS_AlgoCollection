using AI_DS_AlgoCollection.Algorithms.Association;
using AI_DS_AlgoCollection.DataStructures;

var types = new AttributeType[] {
    new ("Bereich"), new ("Aufwand"), new ("Attraktivität"), new ("Bauchgefühl")
};

var examples = new List<string[]> {
  new []{"Handwerker", "groß", "gering", "gut"}, 
  new []{"Handwerker", "gering", "gering", "neutral"},
  new []{"Handwerker", "mittel", "mittel", "gut"}, 
  new []{"Handwerker", "mittel", "mittel", "schlecht"},
  new []{"Beratungsnetz", "mittel", "hoch", "neutral"},
  new []{"Beratungsnetz", "gering", "mittel", "neutral"},
  new []{"Beratungsnetz", "groß", "mittel", "schlecht"},
  new []{"Beratungsnetz", "mittel", "gering", "gut"},
  new []{"Online-Shop", "groß", "hoch", "schlecht"},
  new []{"Online-Shop", "mittel", "mittel", "schlecht"},
  new []{"Online-Shop", "mittel", "gering", "gut"},
  new []{"Online-Shop", "groß", "hoch", "gut"}
};

var classification = new [] {
    false, false, true, false, true, false, true, true, false, false, true, true
};

//var data = new ClassifictaionData<AttributeType, string>(types, examples, classification);
//data.GetDT();
   
/*
 T1 {A,B,C,E, F} T2 {B,E, F} T3 {A,B,C,E} T4 {B,C,E, F} T5 {A,C,D, F} T6 {C, F}
 */

var types3 = new AttributeType[] {
    new ("A"), new ("B"), new ("C"), new ("D"), new ("E"), new ("F")
};

var examples3 = new List<bool[]> {
    new []{true, true, true, false, true, true}, 
    new []{false, true, false, false, true, true},
    new []{true, true, true, false, true, false},
    new []{false, true, true, false, true, true},
    new []{true, false, true, true, false, true},
    new []{false, false, true, false, false, true}
};

var data3 = new EventData<AttributeType, bool>(types3, examples3);
data3.DoEclat();
data3.DoFrequentPattern();
data3.DoApriori();

/*
 T1 {A,C,D,F} T2 {B,C,D,E,F,G} T3 {A,B,C,E, F} T4 {A,B,C, F} T5 {A, F} T6 {A,C,E, F,G}
 */
 
 var types4 = new AttributeType[] {
    new ("A"), new ("B"), new ("C"), new ("D"), new ("E"), new ("F"), new ("G")
};

var examples4 = new List<bool[]> {
    new []{true, false, true, true, false, true, false}, 
    new []{false, true, true, true, true, true, true},
    new []{true, true, true, false, true, true, false},
    new []{true, true, true, false, false, true, false},
    new []{true, false, false, false, false, true, false},
    new []{true, false, true, false, true, true, true}
};

var data4 = new EventData<AttributeType, bool>(types4, examples4);
data4.DoFrequentPattern();
data4.DoEclat();
data4.DoApriori();