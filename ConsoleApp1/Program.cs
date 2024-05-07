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
        static Dictionary<Tuple<string, string>, List<string>> myDictionary = new Dictionary<Tuple<string, string>, List<string>>();
        static Tuple<string, string> key;
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
                    string lineMatches = worksheet.Cells[i + 1, 3].Value.ToString();
                    string[] fileOneParts = fileOneData.Split(new string[] { "/" }, StringSplitOptions.None);
                    string hyperlink1 = string.Join("/", fileOneParts.Take(4));
                    string similarity1 = fileOneParts[3].Trim().Replace("(", "").Replace(")", "").Replace("%", ""); // remove brackets
                    string[] fileTwoParts = fileTwoData.Split(new string[] { "/" }, StringSplitOptions.None);
                    string hyperlink2 = string.Join("/", fileTwoParts.Take(4));
                    string similarity2 = fileTwoParts[3].Trim().Replace("(", "").Replace(")", "").Replace("%", ""); // remove brackets
                                                                                                                    // Create a Tuple for the key
                    key = Tuple.Create(hyperlink1, hyperlink2);
                    valueList = new List<string> { similarity1, similarity2, lineMatches };
                    // Add the entry to the dictionary
                    myDictionary.Add(key, valueList);
                    //Console.WriteLine($"File one: {hyperlink1}, Similarity: {similarity1}");
                    //Console.WriteLine($"File two: {hyperlink2}, Similarity: {similarity2}");
                    //Console.WriteLine($"Lines matched: {lineMatches}");
                    //Console.WriteLine();
                }
            }
        }

        static Dictionary<Tuple<string, string>, string> ConstructTheGraph(Dictionary<Tuple<string, string>, List<string>> dictionary)
        {
            Dictionary<Tuple<string, string>, string> Graph = new Dictionary<Tuple<string, string>, string>();
            foreach (var keyValuePair in dictionary.Keys)
            {
                string firstKey = keyValuePair.Item1;
                string secondKey = keyValuePair.Item2;
                string value1 = dictionary[keyValuePair][0];
                string value2 = dictionary[keyValuePair][1];
                Tuple<string, string> key1 = Tuple.Create(firstKey, secondKey);
                Tuple<string, string> key2 = Tuple.Create(secondKey, firstKey);
                Graph.Add(key1, value1);
                Graph.Add(key2, value2);
            }
            //foreach (KeyValuePair<KeyValuePair<string, string>, double> kvp in Graph)
            //{
            //    Console.WriteLine("Key: {0}, {1}, Value: {2}", kvp.Key.Key, kvp.Key.Value, kvp.Value);
            //}
            return Graph;
        }

        public struct ComponentValues
        {
            public double Sum;
            public int Count;
            public HashSet<string> HashSet;

            public ComponentValues(double sum, int count)
            {
                Sum = sum;
                Count = count;
                HashSet = new HashSet<string>();
            }
        }



        static HashSet<ComponentValues> GetComponents(Dictionary<Tuple<string, string>, string> graph)
        {
            HashSet<ComponentValues> components = new HashSet<ComponentValues>();
            HashSet<Tuple<string, string>> visited = new HashSet<Tuple<string, string>>();

            foreach (var node in graph.Keys)
            {
                if (!visited.Contains(node))
                {
                    ComponentValues component = new ComponentValues(0.0, 0); // Initialize with default values
                    DepthFirstSearch(graph, node, visited, ref component);
                    //Console.WriteLine("$$$$$$$$$$$$$$$$$");
                    components.Add(component);
                }
            }
            return components;
        }


        static void DepthFirstSearch(Dictionary<Tuple<string, string>, string> graph, Tuple<string, string> currentNode, HashSet<Tuple<string, string>> visited, ref ComponentValues component)
        {
            visited.Add(currentNode);
            component.HashSet.Add(currentNode.Item1);
            Tuple<string, string> k = Tuple.Create(currentNode.Item1, currentNode.Item2);
            string value;
            //Console.WriteLine("in the graph" + currentNode);
            //Console.WriteLine(k);
            // Check if the key exists in the graph dictionary
            graph.TryGetValue(k, out value);
            value = value.Replace("%", "");
            //Console.WriteLine(currentNode);
            //Console.WriteLine(value);

            component.Sum += double.Parse(value);
            //Console.WriteLine(component.Sum);
            component.Count += 1;
            // Value is found, you can use it here
            // Console.WriteLine("Value found: " + value);



            var adjacentNodes = new HashSet<Tuple<string, string>>();
            foreach (var edge in graph)
            {
                if ((edge.Key.Item1 == currentNode.Item1 || edge.Key.Item2 == currentNode.Item1) && !visited.Contains(edge.Key))
                {
                    if (edge.Key.Item1 == currentNode.Item1)
                    {
                        Tuple<string, string> kvp = Tuple.Create(edge.Key.Item2, edge.Key.Item1);
                        adjacentNodes.Add(kvp);
                    }
                    else
                    {
                        Tuple<string, string> kvp = Tuple.Create(edge.Key.Item1, edge.Key.Item2);
                        adjacentNodes.Add(kvp);
                    }
                }
            }
            foreach (var node in adjacentNodes)
            {
                if (!visited.Contains(node))
                {
                    DepthFirstSearch(graph, node, visited, ref component);
                }
            }
        }



        static void maxST(Dictionary<Tuple<string, string>, string> graph)
        {
            int temp = 1;
            Dictionary<KeyValuePair<int, int>, string> newgraph = new Dictionary<KeyValuePair<int, int>, string>();
            Dictionary<string, int> vertices = new Dictionary<string, int>();
            foreach (KeyValuePair<Tuple<string, string>, string> kvp in graph)
            {
                string[] parts1 = kvp.Key.Item1.Split(new string[] { "/" }, StringSplitOptions.None);
                string key1 = string.Join("/", parts1.Take(3));

                string[] parts2 = kvp.Key.Item2.Split(new string[] { "/" }, StringSplitOptions.None);
                string key2 = string.Join("/", parts2.Take(3));
                if (!vertices.ContainsKey(key1))
                {
                    vertices.Add(key1, temp);
                    temp++;
                }
                if (!vertices.ContainsKey(key2))
                {
                    vertices.Add(key2, temp);
                    temp++;
                }
                if (vertices.ContainsKey(key1) && vertices.ContainsKey(key2))
                {
                    int vertex1 = vertices[key1];
                    int vertex2 = vertices[key2];

                    KeyValuePair<int, int> newKey = new KeyValuePair<int, int>(vertex1, vertex2);
                    newgraph.Add(newKey, kvp.Value);
                }
            }
            // Print newGraph
            Console.WriteLine("\nNew Graph:");
            foreach (KeyValuePair<KeyValuePair<int, int>, string> kvp in newgraph)
            {
                Console.WriteLine("Key: ({0}, {1}), Value: {2}", kvp.Key.Key, kvp.Key.Value, kvp.Value);
            }
        }

        static void Main(string[] args)
        {
            readFile(@"C:\Users\momen\Desktop\New folder (2)\Plagrisim-Detection\Test Cases\Complete\Hard\1-Input.xlsx");
            Dictionary<Tuple<string, string>, string> graph;
            graph = ConstructTheGraph(myDictionary);
            HashSet<ComponentValues> statics = new HashSet<ComponentValues>();
            statics = GetComponents(graph);
            foreach (var x in statics)
            {
                foreach (var y in x.HashSet)
                {
                    Console.WriteLine(y);
                }
                Console.WriteLine("#############################");
                Console.WriteLine("Total: " + x.Sum);
                Console.WriteLine("Count: " + x.Count);
                Console.WriteLine("finaaaaaaaaaal  " + x.Sum / x.Count);
            }
        }
    }
}
