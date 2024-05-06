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
                    string fileOneData = worksheet.Cells[i+1, 1].Value.ToString();
                    string fileTwoData = worksheet.Cells[i+1, 2].Value.ToString();
                    string lineMatches = worksheet.Cells[i+1, 3].Value.ToString();
                    string[]fileOneParts = fileOneData.Split(new string[] { "/" }, StringSplitOptions.None);
                    string hyperlink1 = string.Join("/", fileOneParts. Take(4));
                    string similarity1 = fileOneParts[3].Trim().Replace("(", "").Replace(")", "").Replace("%", ""); // remove brackets
                    string[] fileTwoParts = fileTwoData.Split(new string[] { "/" }, StringSplitOptions.None);
                    string hyperlink2 = string.Join("/", fileTwoParts.Take(4));
                    string similarity2 = fileTwoParts[3].Trim().Replace("(", "").Replace(")", "").Replace("%", ""); // remove brackets

                    // Create a KeyValuePair for the key
                    key = new KeyValuePair<string, string>(hyperlink1, hyperlink2);

                    valueList = new List<string> { similarity1, similarity2, lineMatches};

                    // Add the entry to the dictionary
                    myDictionary.Add(key, valueList);

                    //Console.WriteLine($"File one: {hyperlink1}, Similarity: {similarity1}");
                    //Console.WriteLine($"File two: {hyperlink2}, Similarity: {similarity2}");
                    //Console.WriteLine($"Lines matched: {lineMatches}");
                    //Console.WriteLine();
                }
            }
        }
        
        static Dictionary<KeyValuePair<string, string>, double> ConstructTheGraph(Dictionary<KeyValuePair<string, string>, List<string>> dictionary)
        {
            Dictionary<KeyValuePair<string, string>, double> Graph = new Dictionary<KeyValuePair<string, string>, double>();
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
                double Value1 = double.Parse(value1);
                double Value2 = double.Parse(value2);
                if (Value1 > Value2)
                 Graph.Add(key1, Value1);
                else
                 Graph.Add(key2, Value2);
            }
            //foreach (KeyValuePair<KeyValuePair<string, string>, double> kvp in Graph)
            //{
            //    Console.WriteLine("Key: {0}, {1}, Value: {2}", kvp.Key.Key, kvp.Key.Value, kvp.Value);
            //}
            return Graph;
        }
        static HashSet<HashSet<string>> GetComponents(Dictionary<KeyValuePair<string, string>, string> graph)
        {

            HashSet<HashSet<string>> components = new HashSet<HashSet<string>>();
            HashSet<string> visited = new HashSet<string>();

            foreach (var node in graph.Keys)
            {
                if (!visited.Contains(node.Key))
                {
                    HashSet<string> component = new HashSet<string>();
                    DepthFirstSearch(graph, node.Key, visited, component);
                    components.Add(component);
                }
            }
            return components;
        }
        static void DepthFirstSearch(Dictionary<KeyValuePair<string, string>, string> graph, string currentNode, HashSet<string> visited, HashSet<string> component)
        {
            visited.Add(currentNode);
            component.Add(currentNode);
            var adjacentNodes = new List<string>();
            foreach (var edge in graph)
            {
                if ((edge.Key.Key == currentNode || edge.Key.Value == currentNode) && !visited.Contains(edge.Key.Key))
                {
                    if (edge.Key.Key == currentNode)
                    {
                        adjacentNodes.Add(edge.Key.Value);
                    }
                    else
                    {
                        adjacentNodes.Add(edge.Key.Key);
                    }
                }
            }
            foreach (var node in adjacentNodes)
            {
                DepthFirstSearch(graph, node, visited, component);
            }
        }

        static void calculateStat(Dictionary<KeyValuePair<string, string>, string> graph)
        {
            HashSet<HashSet<string>> components = GetComponents(graph);

            double total=0;
            double avg = 0;
            double count = 0;
            foreach (var component in components)
            {
                total = 0;
                count = 0;
              //  Console.WriteLine("Component:");
                foreach (var node in component)
                {
                    Console.WriteLine(node);
                    // Loop through edges in the graph
                    foreach (var edge in graph)
                    {
                        // Check if the edge connects to the current node and if the other end of the edge is also in the component
                        if (edge.Key.Key == node)
                        {
                            // Access the weight of the edge
                            string weight = edge.Value;
                            //weight = weight.Replace("%", "");
                            double w = double.Parse(weight); 
                            total += w;
                            count++;
                        }
                    }
                }
                    avg = total / count;
            }
        }

        static void maxST(Dictionary<KeyValuePair<string, string>, double> graph)
        {
            //var sortedGraph = from entry in graph orderby entry.Value descending select entry;
            int temp = 1;
            Dictionary<KeyValuePair<int, int>, double> newgraph= new Dictionary<KeyValuePair<int, int>, double>();
            Dictionary<string, int> vertices = new Dictionary<string, int>();
            foreach (KeyValuePair<KeyValuePair<string, string>, double> kvp in graph)
            {
                string[] parts1 = kvp.Key.Key.Split(new string[] { "/" }, StringSplitOptions.None);
                string key1 = string.Join("/", parts1.Take(3));

                string[] parts2 = kvp.Key.Value.Split(new string[] { "/" }, StringSplitOptions.None);
                string key2 = string.Join("/", parts2.Take(3));
                if (!vertices.ContainsKey(key1))
                {
                    vertices.Add(key1, temp);
                    //Console.WriteLine(key1);
                    temp++;
                }
                if (!vertices.ContainsKey(key2))
                {
                    vertices.Add(key2,temp);
                    //Console.WriteLine(key2);
                    temp++;
                }
                //code for making new graph with pairs as number assigned in verytices dictionary 
                if (vertices.ContainsKey(key1) && vertices.ContainsKey(key2))
                {
                    int vertex1 = vertices[key1];
                    int vertex2 = vertices[key2];

                    KeyValuePair<int, int> newKey = new KeyValuePair<int, int>(vertex1, vertex2);
                    newgraph.Add(newKey, kvp.Value);
                }
                
            }
            //Console.WriteLine("Vertices:");
            //foreach (KeyValuePair<string, int> kvp in vertices)
            //{
            //    Console.WriteLine("Key: {0}, Value: {1}", kvp.Key, kvp.Value);
            //}

            //// Print newGraph
            //Console.WriteLine("\nNew Graph:");
            //foreach (KeyValuePair<KeyValuePair<int, int>, double> kvp in newgraph)
            //{
            //    Console.WriteLine("Key: ({0}, {1}), Value: {2}", kvp.Key.Key, kvp.Key.Value, kvp.Value);
            //}
            //foreach (<string,int> kvp in vertices)
            //{
            //    Console.WriteLine("Edge: {0}, Weight: {2}", kvp.Key, kvp.Value);
            //}
        }

        static void Main(string[] args)
        {
            readFile(@"D:\Plagrisim\Plagrisim-Detection\Test Cases\Sample\1-Input.xlsx");
            Dictionary<KeyValuePair<string, string>, double> graph;
            graph=ConstructTheGraph(myDictionary);
            maxST(graph);
            //calculateStat(graph);
        }
    }
}
