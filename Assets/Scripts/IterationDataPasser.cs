using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IterationList;


namespace DataPasser
{

    public class IterationDataPasser
    {

        public static Sequence sequence;
        public List<DictResemblerList> iterationSequencesList;


        public List<DictResemblerList> GenerateIterationSeqList()
        {
            IterationSequenceList.GenerateSeedFile();
            int seednum = PlayerPrefs.GetInt("seedNum");
            var sequenceOrder = IterationSequenceList.GenerateRandomSequence(seednum);

            var iterationSequenceList = new IterationSequenceList();

            var iterationsDict = iterationSequenceList.iterationDict;

            iterationSequencesList = new List<DictResemblerList>();


            for (int iterationCount = 0; iterationCount < sequenceOrder.Length; iterationCount++)
            {
                int itKey = sequenceOrder[iterationCount];
                var wordSequenceDict = iterationsDict[itKey];
                PopulateIterationSequencesList(wordSequenceDict);
            }

            return iterationSequencesList;
        }

        private void PopulateIterationSequencesList(Dictionary<string, Sequence> dict)
        {
            foreach (KeyValuePair<string, Sequence> item in dict)
            {
                iterationSequencesList.Add(new DictResemblerList() { key = item.Key, sequence = item.Value });
            }

        }



    }
}
