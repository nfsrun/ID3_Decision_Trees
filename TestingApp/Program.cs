using System;
using System.Collections.Generic;
using System.IO;
using Algorithm;
using Randomizer;

/* Program.cs
 * October 17, 2018
 * CS 460 - Machine Learning
 * Alfred Nehme
 */

/// <summary>
/// Class that will conduct the commands of file pre-processing, IDE3 tree 
/// making, tests, and testing result output(s). 
/// </summary>
class Program
{
    private static int trainRows, testRows, trials, classifier;
    private static string sourcePath = "";

    /// <summary>
    /// Main method that will run the whole program from file pre-processing to
    /// IDE3 tree creation to testing. 
    /// </summary>
    /// <param name="args">Default argument for main method. </param>
    static void Main(string[] args)
    {
        Console.WriteLine("ID3 Application");
        if (!FilePath())
            return;
        string[] dataRows = File.ReadAllText(sourcePath).Split('\n');
        SetFieldParams(dataRows.Length);
        SortedDictionary<int, List<int>> reBinData = 
            binAttributeValues(dataRows[0].Split(','));
        List<string>[] TrainingAndTestingFiles = 
            Randomization.trialPrep(dataRows, trainRows, testRows, trials, reBinData);
        bool[] nonActiveAttributes = 
            produceAttributeOmissions(dataRows[0].Split(','));
        RunTrials(dataRows, TrainingAndTestingFiles, nonActiveAttributes);
        Console.WriteLine("Trials completed. Press any key to close. ");
        Console.ReadKey();
    }

    /// <summary>
    /// This method starts the tree-making and testing process for a specified 
    /// number of trials. 
    /// </summary>
    /// <param name="dataRows">
    /// Intakes a string array of all rows in a .csv file used for training. 
    /// </param>
    /// <param name="TrainingAndTestingFiles">
    /// Array of two lists that contain full file paths to testing or training 
    /// data. 
    /// </param>
    /// <param name="omitAttribute">Default argument for main method. </param>
    private static void RunTrials(string[] dataRows, 
        List<string>[] TrainingAndTestingFiles, bool[] omitAttribute)
    {
        double[] percentages = new double[trials];
        StreamWriter outputFile = new StreamWriter(
            AppDomain.CurrentDomain.BaseDirectory + 
            "Files\\Output\\results.csv", false);
        for (int i = 0; i < TrainingAndTestingFiles[0].Count; i++)
        {
            outputFile.WriteLine("Trial " + i + " Testing Results");
            IDE3 algorithm = new IDE3(dataRows, omitAttribute);
            percentages[i] = algorithm.Test(TrainingAndTestingFiles[1][i], 
                outputFile);
            outputFile.WriteLine("---------------------------------------");
        }
        outputFile.Write("Average Total: ");
        percentages[0] /= percentages.Length;
        for (int i = 1; i < percentages.Length; i++)
            percentages[0] += percentages[i] / percentages.Length;
        outputFile.Write(percentages[0] + "");
        outputFile.Close();
    }

    /// <summary>
    /// Method below ensures that a valid file path/file existed or if a quit 
    /// was indicated. 
    /// </summary>
    /// <returns>
    /// Whether the file path/file exists or if there was a quit 
    /// indicated(false). 
    /// </returns>
    private static Boolean FilePath()
    {
        Console.Write("Please insert source file into local location's " +
            "\"bin/Debug/Files/\" folder and type file name (type q! to quit): ");
        sourcePath = AppDomain.CurrentDomain.BaseDirectory + "Files\\" + 
            Console.ReadLine();
        while (!File.Exists(sourcePath) && !sourcePath.Equals("q!"))
        {
            Console.Write(sourcePath + " was not a valid entry. Please try again.");
            sourcePath = Console.ReadLine();
        }
        if (sourcePath.Equals("q!"))
            return false;
        return true;
    }

    /// <summary>
    /// This program changes the class fields in the testing program.
    /// It ensures that the number of trials, training rows, and testing rows 
    /// are valid positive integers. Then it also ensures training and testing
    /// rows are set so that it won't exceed training rows. 
    /// </summary>
    /// <param name="dataRows">
    /// Intakes an int of all rows in a .csv file used for training. 
    /// </param>
    private static void SetFieldParams(int dataRows)
    {
        Console.WriteLine("Enter amount of trials: ");
        while (!int.TryParse(Console.ReadLine(), out trials) && trainRows > -1)
            Console.Write("Value invalid. Add trial amount again: ");
        Console.WriteLine("Enter amount of training rows: ");
        while (!int.TryParse(Console.ReadLine(), out trainRows) && trainRows > 1)
            Console.Write("Value invalid. Add training row amount again: ");
        Console.WriteLine("Enter amount of testing rows: ");
        while (!int.TryParse(Console.ReadLine(), out testRows) && testRows > 1)
            Console.Write("Value invalid. Insert test row amount again: ");
        //-2 due to the omission of the last empty line and the 
        //header/attribute labels
        if (dataRows - 2 < testRows + trainRows)
        {
            Console.WriteLine("Sum of Testing and Training Arguments are too " +
                "large. Resetting set parameters... ");

            SetFieldParams(dataRows);
        }
    }

    /// <summary>
    /// Method produces a boolean array of what attributes are needed to be 
    /// omitted from the IDE3 tree calculation. 
    /// </summary>
    /// <param name="attributes">
    /// Uses the string array of attributes to output representation. 
    /// </param>
    /// <returns>
    /// Boolean array of what needs to be omitted from the tree calculation. 
    /// </returns>
    private static bool[] produceAttributeOmissions(string[] attributes)
    {
        bool[] omit = new bool[attributes.Length];
        int attributeIndex = 0;
        while(attributeIndex!=-1)
        {
            Console.Write('\n' + attributes[0]);
            for (int i = 1; i < attributes.Length; i++)
                Console.Write(" | " + attributes[i]);
            Console.WriteLine('\n');
            Console.WriteLine("With the right-most index being 1 and increasing" +
                " as the index goes left, please indicate what attribute to " +
                "reclassify. Enter 0 to quit: ");
            Console.WriteLine("Enter classifier index: ");
            while (!int.TryParse(Console.ReadLine(), out attributeIndex) 
                || classifier < -1 || classifier > attributes.Length )
                Console.Write("Value invalid. Insert attribute value again: ");
            attributeIndex--;
            if (attributeIndex != -1)
            {
                omit[attributeIndex] = !omit[attributeIndex];
                Console.WriteLine("Attribute at index " + attributeIndex + 1 + 
                    " will be omitted?" + omit[attributeIndex]);
            }
        }
        return omit;
    }

    /// <summary>
    /// This method changes the class field that identifies which attribute is 
    /// the classifier. 
    /// </summary>
    /// <param name="attributes">
    /// Intakes a string array of worded attributes so program can output 
    /// choices for user. 
    /// </param>
    private static void ClassifierParam(string[] attributes)
    {
        for (int i=0; i>attributes.Length; i++)
            Console.Write("|" + attributes[i]);
        Console.WriteLine("|");
        Console.WriteLine("With the right-most index being 1 and increasing as " +
            "the index goes left, please indicate the classifier attribute. ");
        while (!int.TryParse(Console.ReadLine(), out classifier) && classifier 
            < 0 && classifier > attributes.Length)
            Console.Write("Value invalid. Insert test row amount again: ");
        classifier--;
    }

    /// <summary>
    /// Method below returns a complex data structure where it holds the 
    /// attribute indexes that need to perform re-binning and its respective 
    /// list of attribute values to range upon. 
    /// </summary>
    /// <param name="attributes">
    /// Intakes a string array of worded attributes so program can output 
    /// choices for user. 
    /// </param>
    /// <returns>
    /// Returns a SortedDictionary of integers (the attribute indicative of 
    /// re-binning) and its list of integers (to impose categorical ranges). 
    /// </returns>
    private static SortedDictionary<int, List<int>> binAttributeValues(string[] attributes)
    {
        SortedDictionary<int, List<int>> bins = new SortedDictionary<int, List<int>>();
        int attributeIndex = 0;
        while (attributeIndex != -1)
        {
            Console.Write(attributes[0]);
            for (int i = 1; i < attributes.Length; i++)
                Console.Write(" | " + attributes[i]);
            Console.WriteLine('\n');
            Console.WriteLine("With the right-most index being 1 and increasing as the index " +
                        "goes left, please indicate what attribute to bin. Enter 0 to quit: ");
            Console.WriteLine("Enter classifier index: ");
            while (!int.TryParse(Console.ReadLine(), out attributeIndex) || attributeIndex < 0 ||
                attributeIndex > attributes.Length)
                Console.Write("Value invalid. Insert attribute value again: ");
            attributeIndex--;
            if (attributeIndex != -1)
            {
                int rangeIndex;
                bins[attributeIndex] = new List<int>();
                Console.WriteLine("Continue inputting list of rangeIndexes until" +
                    " done. Finish by entering a non-integer. ");

                while (int.TryParse(Console.ReadLine(), out rangeIndex))
                {
                    if (!bins[attributeIndex].Contains(rangeIndex))
                        bins[attributeIndex].Add(rangeIndex);
                }

                bins[attributeIndex].Sort();
            }
        }
        return bins;
    }
}