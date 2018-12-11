using System;
using System.Collections.Generic;
using System.IO;

/* IDE3.cs
 * October 17, 2018
 * CS 460 - Machine Learning
 * Alfred Nehme
 */

namespace Algorithm
{
    /// <summary>
    /// Class that contains IDE3 object which includes various data structures
    /// and various 
    /// </summary>
    public class IDE3
    {
        private AttributeHeader[] attributes;
        private int classifierIndex;
        private static string[,] data;
        private Node root;

        /// <summary>
        /// IDE3 type's only constructor that is used to make an IDE3 object. 
        /// </summary>
        /// <param name="dataRows">Rows of data (including header).</param>
        /// <param name="omitAttribute">
        /// Boolean array of attributes needed to be omitted. 
        /// </param>
        public IDE3(string[] dataRows, bool[] omitAttribute)
        {
            SetAttribute(dataRows[0].Split(','), omitAttribute);
            PopulateData(dataRows);
            IdentifyClassifierAttribute();
            StartRecursion(omitAttribute);
        }

        /// <summary>
        /// Initializes the attribute field with set attribute names and omits 
        /// any attribute that are not supposed to be apparent in the IDE3 
        /// tree-building process. 
        /// </summary>
        /// <param name="headerLabel">
        /// String array of all of the attribute labels. 
        /// </param>
        /// <param name="omitRows">
        /// Boolean array of attributes needed to be omitted. 
        /// </param>
        private void SetAttribute(string[] headerLabel, bool[] omitRows)
        {
            this.attributes = new AttributeHeader[headerLabel.Length];
            for (int i = 0; i < attributes.Length; i++)
            {
                this.attributes[i] = new AttributeHeader(headerLabel[i], i);
                if (omitRows[i])
                    this.attributes[i].FlipAttributeOmissionStatus();
            }
        }

        /// <summary>
        /// Method populates the 2D array with hash value of the respective 
        /// attribute value string. Please note lines.Length and the IDE3 field
        /// should have equivalent attribute lengths. 
        /// </summary>
        /// <param name="dataRows">Rows of data (including header).</param>
        private void PopulateData(string[] dataRows)
        {
            //-2 due to the omission of the last empty line and the 
            //header/attribute labels
            data = new string[dataRows.Length - 2, this.attributes.Length];

            //-1 due to the omission of the last empty line
            for (int i = 1; i < dataRows.Length - 1; i++)
            {
                string[] line = dataRows[i].Split(',');
                for (int j = 0; j < line.Length; j++) {
                    data[i - 1, j] = line[j];
                    if(line[j]==null)
                        data[i - 1, j] = this.attributes[j].getMostCommonFeature();
                }
            }

            //for each attribute, set the most popular item for the attribute. 
            for (int i = 0; i < this.attributes.Length; i++)
                this.attributes[i].MostPopular();
        }

        /// <summary>
        /// Method asks user to assign the Classifier Attribute Index for 
        /// any future IDE3 calculation. 
        /// </summary>
        private void IdentifyClassifierAttribute()
        {
            Console.Write(this.attributes[0].GetAttribute());
            for (int i = 1; i < this.attributes.Length; i++)
                Console.Write(" | " + this.attributes[i].GetAttribute());
            Console.WriteLine();
            Console.WriteLine("With the leftmost attribute as 1 and the " +
                "right-most as " + this.attributes.Length + ", " +
                "please specify classifier index. ");
            Console.WriteLine("Enter classifier index: ");
            while (!int.TryParse(Console.ReadLine(), out classifierIndex) && 
                classifierIndex > 0 &&
                classifierIndex <= this.attributes.Length && 
                !this.attributes[classifierIndex-1].GetOmissionStatus())
                Console.Write("Value invalid. Respecifiy clasifier index " +
                    "again: ");
            this.classifierIndex--;
        }

        /// <summary>
        /// This is the starting/helper method for the tree-building program. 
        /// </summary>
        /// <param name="omittedCollumns">
        /// An array of attributes/columns to omit. 
        /// </param>
        public void StartRecursion(bool[] omittedCollumns)
        {
            this.root = new Node(ref this.attributes, 1, 
                new bool[data.GetLength(0)], omittedCollumns, ref data, this.classifierIndex, null, -1);
        }
        
        /// <summary>
        /// Testing method that will be used to test a testing csv file and 
        /// output its results onto a file. 
        /// </summary>
        /// <param name="testSource">
        /// String filepath of the testing data. 
        /// </param>
        /// <param name="output">
        /// A StreamWriter to write the results of the testing into a text file. 
        /// </param>
        /// <returns>
        /// A double value of its accuracy over the training set that was given
        /// via source. 
        /// </returns>
        public double Test(string testSource, StreamWriter output)
        {
            string[] data = File.ReadAllText(testSource).Split('\n');
            double average = 0.0;
            for (int i = 1; i < data.Length - 1; i++)
            {
                string[] line = data[i].Split(',');
                string result = treeTraverse(ref line);
                output.WriteLine(data[i] + ", Classified " +
                    this.attributes[this.classifierIndex].GetAttribute() + " as: " + result);
                if(line[line.Length-1].Equals(result))
                    average+= 1 / data.Length * 1.0 - 1;
            }
            return average;
        }

        /// <summary>
        /// Method takes in a string array of answers to create the . 
        /// </summary>
        /// <param name="line">
        /// Takes in a string array that has been broken up. 
        /// </param>
        /// <returns></returns>
        private string treeTraverse(ref string[] line)
        {
            return this.root.Answer(ref line, 0);
        }
    }

    /// <summary>
    /// Node object that creates a basis of travelling for decisions and 
    /// answers to traverse in. 
    /// </summary>
    internal class Node
    {
        private bool[] unavailableRowList, unavailableColList;
        private string[,] data;
        private Dictionary<string, Node> possibilities;
        private AttributeHeader header;

        //Only exists if this is an answer node. 
        private string answerText;
        private bool answer;

        /// <summary>
        /// Main constructor for node. It is a recursive constructor and will 
        /// keep repeating until a threshhold node depth has been reached or 
        /// if it has met a place where it can make 
        /// </summary>
        /// <param name="attributes">
        /// A reference to the list of attributes. 
        /// </param>
        /// <param name="level">
        /// Integer representative of the tree's depth. 
        /// </param>
        /// <param name="omitRows">
        /// A boolean array representative of rows to omit during 
        /// tree-processing. 
        /// </param>
        /// <param name="omitCols">
        /// A boolean array representative of columns to omit during 
        /// tree-processing. 
        /// </param>
        /// <param name="data">
        /// Reference to the entire training dataset. 
        /// </param>
        /// <param name="subClassifyingIndex">
        /// Integer value of index used to continue the classifying/sub-tree 
        /// process. 
        /// </param>
        /// <param name="featureRemove">
        /// String of the feature to be used in the removal of rows. 
        /// </param>
        /// <param name="featureRemoveAttribute">
        /// Integer of the attribute to cross-reference with the feature so it 
        /// can be used in the removal of rows. 
        /// </param>
        public Node(ref AttributeHeader[] attributes, int level, bool[] omitRows, 
            bool[] omitCols, ref string[,] data, int subClassifyingIndex, 
            string featureRemove, int featureRemoveAttribute)
        {
            this.data = data;
            this.unavailableColList = omitCols;
            this.unavailableRowList = omitRows;

            if (featureRemoveAttribute == -1)
                removeRows(ref unavailableRowList, ref data, featureRemove, featureRemoveAttribute);

            int maxIGColumnIndex = -1;
            double max = -1;
            if (level <= 10)
            {
                for (int indexOfArray = 0; indexOfArray < attributes.Length;
                    indexOfArray++)
                {
                    if (!this.unavailableColList[indexOfArray])
                    {
                        double value = setEntropy(ref data, subClassifyingIndex);
                        if (value > max)
                        {
                            max = value;
                            maxIGColumnIndex = indexOfArray;
                        }
                    }
                }
                if (maxIGColumnIndex != -1)
                    this.unavailableColList[maxIGColumnIndex] = !this.unavailableColList[maxIGColumnIndex];
            }
            if ((max - 1 >= -.0001 && max - 1 < .0001) || (level>10 && max>0.5))
            {
                this.answer = true;
                this.answerText = "yes";

            }
            else if ((max - 1 <= -.9999 && max - 1 > -1.0001) || (level > 10 && max <= 0.5))
            {
                this.answer = true;
                this.answerText = "no";
            }
            this.header = attributes[maxIGColumnIndex];
            foreach (string feature in this.header.getValues().Keys)
            {
                this.possibilities.Add(feature, new Node(ref attributes, level + 1, omitRows, omitCols, ref data, maxIGColumnIndex, feature, maxIGColumnIndex));
            }
        }

        /// <summary>
        /// Function does the actual boolean-wise marking of rows that will 
        /// not be calculated. 
        /// </summary>
        /// <param name="rowList">
        /// A reference to the boolean array with a list of rows. 
        /// </param>
        /// <param name="data">
        /// Reference to the entire training dataset. 
        /// </param>
        /// <param name="featureRemove">
        /// String of the feature to be used in the removal of rows. 
        /// </param>
        /// <param name="featureRemoveAttribute">
        /// Integer of the attribute to cross-reference with the feature so it 
        /// can be used in the removal of rows. 
        /// </param>
        private void removeRows(ref bool[] rowList, ref string[,] data, 
            string featureRemove, int featureRemoveAttribute)
        {
            for (int i = 0; i < data.GetLength(0); i++)
                if (!rowList[i] && !data[i, featureRemoveAttribute].Equals(featureRemove))
                    rowList[i] = true;
        }

        /// <summary>
        /// Updates the booleans of the column and row limiter arrays in the 
        /// set. It ensures that the columns and rows won't be counted twice 
        /// during tree creation. 
        /// </summary>
        /// <param name="columnIndex">
        /// Index to the column that needs to be taken off of the 
        /// decision-making process
        /// </param>
        /// <param name="featureKeep">
        /// String of the feature that is needed to be kept active during 
        /// subset creation. 
        /// </param>
        /// <param name="data">
        /// A reference array to the data. 
        /// </param>
        private void updateUnavailablityDimensions(int columnIndex, 
            ref string featureKeep, ref string[,] data)
        {
            unavailableColList[columnIndex] = !unavailableColList[columnIndex];
            for(int i=0; i<data.GetLength(0); i++)
            {
                int j = 0;
                if (!data[i,j].Equals(featureKeep))
                    unavailableRowList[j] = !unavailableRowList[j];
            }
        }

        /// <summary>
        /// Node constructor that takes in a dictionary
        /// </summary>
        /// <param name="dictionary"></param>
        public Node(Dictionary<string, int> dictionary)
        {
            foreach (string line in dictionary.Keys)
                this.possibilities.Add(line, null);

        }
        
        /// <summary>
        /// Returns the answer to a testing data string array. 
        /// </summary>
        /// <param name="answer">string array of the test data. </param>
        /// <returns>Returns a string of the IDE3's prediction</returns>
        internal string Answer(ref string[] answers, int index)
        {
            if (!this.answer)
                return this.possibilities[answers[index]].Answer(ref answers, 
                    index+1);
            else
                return this.answerText;
        }

        /// <summary>
        /// This method calculates information gain based on a given setEntropy
        /// and a counted-up array of attribute counts against the base set. 
        /// </summary>
        /// <param name="setEntropy">
        /// Entropy value of the current set. 
        /// </param>
        /// <param name="numbers">
        /// An integer array that comes from 
        /// </param>
        /// <returns>
        /// A double value of the difference of entropy values form the 
        /// original set to the subset. 
        /// </returns>
        private double InformationGain(double originalSetEntropy, int[,] numbers)
        {
            double sum = 0.0;
            for (int line = 0; line < numbers.Length; line++)
                for (int col = 0; col < numbers.Length; col++)
                    sum += Entropy(numbers[line, col], numbers[line, col]);
            return originalSetEntropy - sum;
        }

        /// <summary>
        /// Calculates entropy of the whole set. 
        /// </summary>
        /// <param name="data">
        /// Reference to the collection dataset of training instances. 
        /// </param>
        /// <param name="columnIndex">
        /// Column index of the dataset. 
        /// </param>
        /// <returns></returns>
        private double setEntropy(ref string[,] data, int columnIndex)
        {
            double holder = 0.0;
            Dictionary<string, int> trueAndFalse = CountAttributes(ref data, columnIndex);
            int total = 0;
            foreach (int temp in trueAndFalse.Values)
                total += temp;
            foreach (int temp in trueAndFalse.Values)
                holder += Entropy(temp, total-temp);
            return holder;
        }

        /// <summary>
        /// Method takes in the amount of trues and falses from a recent 
        /// attribute count and then outputs an entropy value based on  
        /// those amounts. 
        /// </summary>
        /// <param name="trues">
        /// Integer amount of the number of trues in a set.
        /// </param>
        /// <param name="falses">
        /// Integer amount of the number of falses in a set.
        /// </param>
        /// <returns>A calculated double Entropy value. </returns>
        private double Entropy(int trues, int falses)
        {
            return -trues / this.data.Length * Math.Log(trues /
                (trues + falses)) - falses / this.data.Length *
                Math.Log(falses / (trues + falses));
        }

        /// <summary>
        /// This method Counts attributes that appear in all instances that 
        /// are not blacklisted. 
        /// </summary>
        /// <param name="target_attribute_index">
        /// An integer representing the target attribute's location on the 
        /// attribute header array. 
        /// </param>
        /// <returns>
        /// A dictionary of all occurrances of each feature of the target 
        /// attribute in the subset
        /// </returns>
        private Dictionary<string, int> CountAttributes(ref string[,] data, 
            int target_attribute_index)
        {
            Dictionary<string, int> totals =
                new Dictionary<string, int>();
            for(int i=0; i<data.GetLength(0); i++)
            {
                if(!this.unavailableRowList[i])
                {
                    if (!totals.ContainsKey(data[i,target_attribute_index]))
                        totals.Add(data[i, target_attribute_index], 1);
                    else
                        totals[data[i, target_attribute_index]] = 
                            totals[data[i, target_attribute_index]] + 1;
                }
            }
            return totals;
        }

        /// <summary>
        /// Method returns this item's list of possible nodes. 
        /// </summary>
        /// <returns>Dictionary<string, Node> list of possible nodes. 
        /// </returns>
        internal Dictionary<string, Node> getPossibilityList() => 
            this.possibilities;

        /// <summary>
        /// setPossibility list sets this node's list to the provided list. 
        /// </summary>
        /// <param name="possibilities">
        /// Dictionary<string, Node> that sets this object's possibility state.
        /// </param>
        internal void setPossibilityList(Dictionary<string, Node> 
            possibilities)
        {
            this.possibilities = possibilities;
        }
    }

    /// <summary>
    /// AttributeHeader class to outline the header and header title. Also
    /// Contains information about its index in the row of columns, its 
    /// validity in the decision-tree making process, and its Dictionary of 
    /// values and integers. 
    /// </summary>
    internal class AttributeHeader
    {
        private string attributeName, mostCommonFeature;
        private int attributeIndex;
        private Dictionary<string, int> listOfValues;
        private bool notValid;

        /// <summary>
        /// Constructor for an Attribute header by inputting a header label. 
        /// </summary>
        /// <param name="name">String of an attribute's label</param>
        public AttributeHeader(String name, int index)
        {
            this.attributeIndex = index;
            this.attributeName = name;
            listOfValues = new Dictionary<string, int>();
        }

        /// <summary>
        /// Sets this attribute header object's header label. 
        /// </summary>
        /// <param name="name">String of an attribute's label</param>
        internal void SetAttribute(String name) => this.attributeName=name;

        /// <summary>
        /// Method returns the attribute header's attribute name that it holds.
        /// </summary>
        /// <returns>Attribute name (this.attributeName). </returns>
        internal string GetAttribute() => this.attributeName;

        /// <summary>
        /// Method returns true or false depending on if the attribute will be 
        /// omitted. 
        /// </summary>
        /// <returns>Attribute omission status</returns>
        internal bool GetOmissionStatus() => this.notValid;

        /// <summary>
        /// Method takes the attribute and flips the boolean associated to it 
        /// to omit or not omit the attribute from calculation. 
        /// </summary>
        internal void FlipAttributeOmissionStatus() => this.notValid = 
            !this.notValid;

        /// <summary>
        /// Method takes in a string to add features into its dictionary where 
        /// it also takes a count of how much occurences that the attributes has. 
        /// </summary>
        /// <param name="input">String parameter of an attribute's valid choice</param>
        internal void addToList(string input)
        {
                if (this.listOfValues.ContainsKey(input))
                    this.listOfValues[input] = this.listOfValues[input] + 1;
                else
                    this.listOfValues.Add(input, 1);
        }

        /// <summary>
        /// Updates the attirbute in what the current most picked field value 
        /// was. It updates the AttributHeader field: mostCommonValue. 
        /// </summary>
        internal void MostPopular()
        {
            int max = -1;
            foreach (string line in this.listOfValues.Keys)
                if(this.listOfValues[line]>max)
                {
                    this.mostCommonFeature = line;
                    max = this.listOfValues[line];
                }
        }
        
        /// <summary>
        /// Returns the size count of the dictionary of possibliities. 
        /// </summary>
        /// <returns>Returns the number of elements in the list. </returns>
        internal int getValueListSize() => this.listOfValues.Count;

        /// <summary>
        /// Returns index of an attribute by using its string. 
        /// </summary>
        /// <param name="input">A string to cross reference an attribute to its
        /// location on the array. </param>
        /// <returns>
        /// An integer of the string input's index value as seen on the dictionary. 
        /// </returns>
        internal int getIndexFrom(string input) => this.listOfValues[input];

        /// <summary>
        /// Method returns a copy of the listOfValues dictionary. 
        /// </summary>
        /// <returns>A copy of this Header's dictionary. </returns>
        internal Dictionary<string, int> getValues() => this.listOfValues;

        /// <summary>
        /// Method to get the common feature for an attribute
        /// </summary>
        /// <returns></returns>
        internal string getMostCommonFeature() => this.mostCommonFeature;
    }
}