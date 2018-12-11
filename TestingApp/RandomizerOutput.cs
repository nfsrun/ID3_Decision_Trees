using System;
using System.IO;
using System.Collections.Generic;

/* RandomizerOutput.cs
 * October 17, 2018
 * CS 460 - Machine Learning
 * Alfred Nehme
 */ 

namespace Randomizer
{
    /// <summary>
    /// Class that will contain methods to randomize given set data into 
    /// unorganized training and testing data. 
    /// </summary>
    class Randomization
    {
        /// <summary>
        /// Method initiates all file writing and file path keeping objects and
        /// uses another random method to keep track of randomized row lines. 
        /// </summary>
        /// <param name="dataRows">
        /// Intakes an int of all rows in a .csv file used for training. 
        /// </param>
        /// <param name="train">
        /// Integer indicating training rows to grab and randomize. 
        /// </param>
        /// <param name="test">
        /// Integer indicating testing rows to grab and randomize. 
        /// </param>
        /// <param name="trial">
        /// Integer indicating how many repeats should be done on dataset. 
        /// </param>
        /// <returns>
        /// An array of string lists. Array index 0 will be training while 1 
        /// index will be testing. 
        /// </returns>
        /// <param name="reBinData">
        /// Holds the attribute indexes that need to perform re-binning and its
        /// respective list of attribute values to range upon. 
        /// </param>
        internal static List<string>[] trialPrep(string[] dataRows, int train, 
            int test, int trial, SortedDictionary<int, List<int>> reBinData)
        {
            List<string>[] paths = new List<string>[2];

            //0 index will be training, 1 index will be testing
            paths[0] = new List<string>();
            paths[1] = new List<string>();

            while (trial != 0)
            {
                DirectoryInfo d = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Files\\Output\\Trial_" + trial);
                StreamWriter outputTrain = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Files\\Output\\Trial_" + trial + "\\_trialTrainData_" + trial + ".csv", false);
                StreamWriter outputTest = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Files\\Output\\Trial_" + trial + "\\_trialTestData_" + trial + ".csv", false);
                List<int>[] TrainAndTestRows = new List<int>[2];
                TrainAndTestRows[0] = new List<int>();
                TrainAndTestRows[1] = new List<int>();
                outputTrain.Write(dataRows[0]);
                outputTest.Write(dataRows[0]);
                populate(TrainAndTestRows, train, test, dataRows.Length);
                foreach (int lineNumber in TrainAndTestRows[0])
                    outputTrain.Write(reBinning(dataRows[lineNumber].Split(','), reBinData));
                foreach (int lineNumber in TrainAndTestRows[1])
                    outputTest.Write(reBinning(dataRows[lineNumber].Split(','), reBinData));
                outputTrain.Close();
                outputTest.Close();
                paths[0].Add(AppDomain.CurrentDomain.BaseDirectory + "Files\\Output\\Trial_" + trial + "\\_trialTrainData_" + trial);
                paths[1].Add(AppDomain.CurrentDomain.BaseDirectory + "Files\\Output\\Trial_" + trial + "\\_trialTestData_" + trial);
                trial--;
            }
            return paths;
        }

        /// <summary>
        /// Goes through the argument given for each attribute and then rebins 
        /// the values based on the reBinData list associated to that 
        /// attribute. 
        /// </summary>
        /// <param name="reBinData">
        /// Holds the attribute indexes that need to perform re-binning and its
        /// respective list of attribute values to range upon. 
        /// </param>
        private static string reBinning(string[] line, SortedDictionary<int, List<int>> reBinData)
        {
            string ret = "";
            for (int i = 0; i < line.Length; i++)
            {
                if (reBinData.ContainsKey(i))
                {
                    int temp = int.Parse(line[i]);

                    //context of data is 0 and positive
                    int min = 0, max = 0;
                    for (int j = 0; j < reBinData[i].Count; j++)
                    {
                        max = reBinData[i][j];
                        if (max < temp)
                        {
                            min = max;
                        }
                        else
                            break;
                    }
                    line[i] = min + " - " + max;

                }
                ret += line[i] + ",";
            }
            return ret.Substring(0,ret.Length-1);
        }

        /// <summary>
        /// Method will append random numbers without repeat to the training 
        /// data first, and then append numbers to the testing list. 
        /// </summary>
        /// <param name="TrainAndTestRows">
        /// Array of Train and Test list of their respective rows. 
        /// </param>
        /// <param name="train">
        /// Integer indicating training rows to grab and randomize. 
        /// </param>
        /// <param name="test">
        /// Integer indicating testing rows to grab and randomize. 
        /// </param>
        /// <param name="typeLayer">
        /// Array index indicating whether data belongs to Train or Test list 
        /// in list array. 
        /// </param>
        private static void populate(List<int>[] TrainAndTestRows, int train, int test, int typeLayer)
        {
            bool[] Possibilities = new bool[typeLayer];
            populateRandom(TrainAndTestRows, Possibilities, train, 0);
            populateRandom(TrainAndTestRows, Possibilities, test, 1);
        }

        /// <summary>
        /// Method will add numbers without repeat to either the training or 
        /// testing list from a given mutable list array. 
        /// </summary>
        /// <param name="TrainAndTestRows">
        /// Array of Train and Test list of their respective rows. 
        /// </param>
        /// <param name="Possibilities">
        /// A static boolean array that holds a 'length' amount of booleans to 
        /// keep track on what has or not has been used in either train or test
        /// rows. 
        /// </param>
        /// <param name="length">
        /// Holds integer amount of elements needed to populate a train or 
        /// test list. 
        /// </param>
        /// <param name="typeLayer">
        /// Array index indicating whether data belongs to Train or Test list 
        /// in list array. 
        /// </param>
        private static void populateRandom(List<int>[] TrainAndTestRows, 
            bool[] Possibilities, int length, int typeLayer)
        {
            Random r = new Random();
            for (int i = 0; i < length; i++)
            {
                int current = r.Next(1, Possibilities.Length);
                while (Possibilities[current])
                {
                    current = r.Next(1, Possibilities.Length);
                }
                Possibilities[current] = true;
                TrainAndTestRows[typeLayer].Add(current);
            }
        }
    }
}