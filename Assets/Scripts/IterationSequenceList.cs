using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;


namespace IterationList{

public class Sequence
{
    public List<string> sequence{ get; set;}
    public string description { get; set;}
}

public class DictResemblerList
{
    public string key {get;set;}
    public Sequence sequence {get;set;}
}

public class IterationSequenceList
{

public Dictionary<string, Sequence> iteration1 = new Dictionary<string, Sequence>();
public Dictionary<string, Sequence> iteration2 = new Dictionary<string, Sequence>();
public Dictionary<string, Sequence> iteration3 = new Dictionary<string, Sequence>();
public Dictionary<string, Sequence> iteration4 = new Dictionary<string, Sequence>();

public Dictionary<int, Dictionary<string, Sequence>> iterationDict = new Dictionary<int, Dictionary<string, Sequence>>();


public List<Sequence> sequenceIteration1 = new List<Sequence>{
    new Sequence(){sequence = new List<string>{"Platine", "Fließband", "Hebe"}, description = "Hebe die Platine vom Fließband"},
    new Sequence(){sequence = new List<string>{"Schraube", "Werkbank", "Lege"}, description = "Lege die Schraube auf die Werkbank"},
    new Sequence(){sequence = new List<string>{"Gehäuse", "Boden", "Halte"}, description = "Halte das fertige Gehäuse über dem Boden, weil dort kein Platz ist"},
};

public List<Sequence> sequenceIteration2 = new List<Sequence>{
    new Sequence(){sequence = new List<string>{"Schraube", "Fließband", "Hebe"}, description = "Hebe die Schraube vom Fließband"},
    new Sequence(){sequence = new List<string>{"Platine", "Werkbank", "Halte"}, description = "Halte die Platine über die Werkbank, während du arbeitest"},
    new Sequence(){sequence = new List<string>{"Gehäuse", "Boden", "Lege"}, description = "Lege das fertige Gehäuse auf den Boden"},
};

public List<Sequence> sequenceIteration3 = new List<Sequence>{
    new Sequence(){sequence = new List<string>{"Gehäuse", "Boden", "Hebe"}, description = "Hebe das Gehäuse vom Boden und inspiziere es"},
    new Sequence(){sequence = new List<string>{"Schraube", "Werkbank", "Halte"}, description = "Halte die Schraube über die Werkbank, während du arbeitest"},
    new Sequence(){sequence = new List<string>{"Platine", "Fließband", "Lege"}, description = "Lege die defekte Platine auf das Fließband, um sie zur Reparatur zu schicken"},
};

public List<Sequence> sequenceIteration4 = new List<Sequence>{
    new Sequence(){sequence = new List<string>{"Platine", "Boden", "Hebe"}, description = "Hebe die Platine vom Boden und inspiziere sie"},
    new Sequence(){sequence = new List<string>{"Schraube", "Werkbank", "Lege"}, description = "Lege die Schraube auf die Werkbank"},
    new Sequence(){sequence = new List<string>{"Gehäuse", "Fließband", "Halte"}, description = "Halte das defekte Gehäuse über dem Fließband, weil es nicht bereit ist"},
};

public IterationSequenceList(){
    iteration1.Add("it1st1", sequenceIteration1[0]);
    iteration1.Add("it1st2", sequenceIteration1[1]);
    iteration1.Add("it1st3", sequenceIteration1[2]);
    iteration2.Add("it2st1", sequenceIteration2[0]);
    iteration2.Add("it2st2", sequenceIteration2[1]);
    iteration2.Add("it2st3", sequenceIteration2[2]);
    iteration3.Add("it3st1", sequenceIteration3[0]);
    iteration3.Add("it3st2", sequenceIteration3[1]);
    iteration3.Add("it3st3", sequenceIteration3[2]);
    iteration4.Add("it4st1", sequenceIteration4[0]);
    iteration4.Add("it4st2", sequenceIteration4[1]);
    iteration4.Add("it4st3", sequenceIteration4[2]);
    iterationDict.Add(1, iteration1);
    iterationDict.Add(2, iteration2);
    iterationDict.Add(3, iteration3);
    iterationDict.Add(4, iteration4);
}

public static void GenerateSeedFile() {
    int? seednum = PlayerPrefs.GetInt("seedNum");
    if(seednum <= 0 || seednum == null) {
    System.Random seedGenerator = new System.Random();
    var seed = seedGenerator.Next(100,300);
    FileStream fs = new FileStream(@"Assets/Scripts/seedfiles/seed"+seed+".dat", FileMode.Create);
    BinaryWriter bin = new BinaryWriter(fs);
    bin.Write(seed);
    bin.Close();
    PlayerPrefs.SetInt("seedNum", seed);
    PlayerPrefs.Save();
    Debug.Log("Seed File created!");
    } else {
        Debug.Log("------Using existing seed number-------");
    }
}

public static int[] GenerateRandomSequence(int seednum) {
    int[] iterationArray = {1,2,3,4};
    System.Random rnd = new System.Random(seednum);
    int[] sequence = new int[10];
    int j=0;
    for(int i= 0; i<8;i++) {
        if(j == 4)
            j=0;
        sequence[i]=iterationArray[j];
        j++;
    }
    for(int ctr=8;ctr<10;ctr++) {
        sequence[ctr] = rnd.Next(1,5);
    }
    int[] randomSequence = sequence.OrderBy(val => rnd.Next()).ToArray();
    return randomSequence;
}

}
}
