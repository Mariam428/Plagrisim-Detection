using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;

namespace ConsoleApp1
{
    class Program
    {
        static Dictionary<KeyValuePair<string, string>, List<string>> myDictionary = new Dictionary<KeyValuePair<string, string>, List<string>>();
        static KeyValuePair<string, string> key;
        static List<string> valueList;
        static void readFile(string fileName)
        {

            using (ExcelPackage package = new ExcelPackage(new FileInfo(fileName)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // get the first worksheet

                // Loop through rows
                for (int i = worksheet.Dimension.Start.Row; i < worksheet.Dimension.End.Row; i++)
                {
                    string fileOneData = worksheet.Cells[i + 1, 1].Value.ToString();
                    string fileTwoData = worksheet.Cells[i + 1, 2].Value.ToString();
                    string lineMatchesString = worksheet.Cells[i + 1, 3].Value.ToString();
                    int lineMatches = int.Parse(worksheet.Cells[i + 1, 3].Value.ToString());

                    string[] fileOneParts = fileOneData.Split(new string[] { "(", ")" }, StringSplitOptions.None);
                    string hyperlink1 = fileOneParts[0].Trim();
                    string similarity1 = fileOneParts[1];

                    string[] fileTwoParts = fileTwoData.Split(new string[] { "(", ")" }, StringSplitOptions.None);
                    string hyperlink2 = fileTwoParts[0].Trim();
                    string similarity2 = fileTwoParts[1];
                   
                    
                    // Create a KeyValuePair for the key
                    key = new KeyValuePair<string, string>(hyperlink1, hyperlink2);

                    valueList = new List<string> { similarity1, similarity2, lineMatchesString};

                    // Add the entry to the dictionary
                    myDictionary.Add(key, valueList);

                    //Console.WriteLine($"File one: {hyperlink1}, Similarity: ({similarity1})");
                    //Console.WriteLine($"File two: {hyperlink2}, Similarity: ({similarity2})");
                    //Console.WriteLine($"Lines matched: {lineMatches}");
                    //Console.WriteLine();
                }
            }
        }

        static Dictionary<KeyValuePair<string, string>, string> ConstructTheGraph(Dictionary<KeyValuePair<string, string>, List<string>> dictionary)
        {
            Dictionary<KeyValuePair<string, string>, string> Graph = new Dictionary<KeyValuePair<string, string>, string>();
            string firstKey;
            string secondKey;
            string value1;
            string value2;
            KeyValuePair<string, string> key1;
            KeyValuePair<string, string> key2;
            foreach (var keyValuePair in dictionary.Keys)
            {
                firstKey = keyValuePair.Key;
                secondKey = keyValuePair.Value;
                value1 = dictionary[keyValuePair][0];
                value2 = dictionary[keyValuePair][1];
                key1 = new KeyValuePair<string, string>(firstKey, secondKey);
                key2 = new KeyValuePair<string, string>(secondKey, firstKey);
                Graph.Add(key1, value1);
                Graph.Add(key2, value2);
            }
            return Graph;
        }

        static void Main(string[] args)
        {
            readFile(@"D:\3rd\Algorithm\Plagrisim-Detection\ConsoleApp1\Sample\1-Input.xlsx");
            Dictionary<KeyValuePair<string, string>, string> graph;
            graph=ConstructTheGraph(myDictionary);
            Console.WriteLine(graph.Keys.Count);
            foreach (var entry in graph)
            {
                Console.WriteLine($"Hyperlink 1: {entry.Key.Key}");
                Console.WriteLine($"Hyperlink 2: {entry.Key.Value}");
                Console.WriteLine($"Similarity : {entry.Value}");
                Console.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
            }
        }
    }
}
