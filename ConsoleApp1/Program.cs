using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;
using static OfficeOpenXml.ExcelErrorValue;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using OfficeOpenXml.Style.XmlAccess;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    class Program
    {
        public struct ComponentValues
        {
            public double sim1;
            public double sim2;
            public double maxsim;
            public int linesmatch;
            public ComponentValues(int sim1, int sim2, int Linesmatch, double maxsim)
            {
                this.sim1 = sim1;
                this.sim2 = sim2;
                linesmatch = Linesmatch;
                this.maxsim = maxsim;
            }
        };
        public struct vert
        {
            public int setno;
            public double totalsim;
            public int count;
            public vert(int setno, double avgsim, int count)
            {
                this.setno = setno;
                this.totalsim = avgsim;
                this.count = count;
            }
        };
        public static float mstTime;
        static Dictionary<Tuple<string, string>, ComponentValues> ReadGraph = new Dictionary<Tuple<string, string>, ComponentValues>();
        static Dictionary<string, vert> vertices = new Dictionary<string, vert>();
        static HashSet<string> keysIndex = new HashSet<string>();
        static Tuple<string, string> key;
        static Dictionary<Tuple<string, string>, double[]> sortedmaxst = new Dictionary<Tuple<string, string>, double[]>();
        static void readFile(string fileName)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            int temp = 1;
            string fileOneData;
            string fileTwoData;
            string lineMatchesString;
            int lineMatches;
            string hyperlink1;
            string similarity1;
            string hyperlink2;
            string similarity2;
            double sim1; double sim2;
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fileName)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // get the first worksheet
                // Loop through rows
                for (int i = worksheet.Dimension.Start.Row; i < worksheet.Dimension.End.Row; i++)
                {
                    fileOneData = worksheet.Cells[i + 1, 1].Value.ToString();
                    fileTwoData = worksheet.Cells[i + 1, 2].Value.ToString();
                    lineMatchesString = worksheet.Cells[i + 1, 3].Value.ToString();
                    lineMatches = int.Parse(worksheet.Cells[i + 1, 3].Value.ToString());
                    string[] fileOneParts = fileOneData.Split(new string[] { "(", ")" }, StringSplitOptions.None);
                    hyperlink1 = fileOneParts[0];
                    similarity1 = fileOneParts[1];
                    string[] fileTwoParts = fileTwoData.Split(new string[] { "(", ")" }, StringSplitOptions.None);
                    hyperlink2 = fileTwoParts[0];
                    similarity2 = fileTwoParts[1];
                    similarity1 = similarity1.Replace("%", "");
                    sim1 = double.Parse(similarity1);
                    similarity2 = similarity2.Replace("%", "");
                    sim2 = double.Parse(similarity2);
                    key = Tuple.Create(hyperlink1, hyperlink2);
                    ComponentValues component = new ComponentValues
                    {
                        linesmatch = int.Parse(lineMatchesString),
                        sim1 = sim1,
                        sim2 = sim2,
                        maxsim = Math.Max(sim1, sim2)
                    };
                    ReadGraph.Add(key, component);
                    //assign each vertex to a set no initially  
                    if (!keysIndex.Contains(hyperlink1))
                    {
                        vert v = new vert(temp, 0, 0);
                        vertices.Add(hyperlink1, v);
                        keysIndex.Add(hyperlink1);
                        temp++;
                    }
                    if (!keysIndex.Contains(hyperlink2))
                    {
                        vert v = new vert(temp, 0, 0);
                        vertices.Add(hyperlink2, v);
                        keysIndex.Add(hyperlink2);
                        temp++;
                    }
                }
            }
            return;
        }
      
        
        static void maxST(Dictionary<Tuple<string, string>, ComponentValues> readgraph)
        {
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch2.Start();
            var sortedReadGraph = readgraph.OrderByDescending(kv => kv.Value.maxsim).ThenByDescending(kv => kv.Value.linesmatch);
            var maxstGraph = new Dictionary<Tuple<string, string>, double[]>();
            double bothsim;
            double sum;
            int setnumold;
            int newsetno;
            int cnt;
            foreach (var kvp in sortedReadGraph)
            {
                // Calculate the sum of similarities
                bothsim = kvp.Value.sim1 + kvp.Value.sim2;
                sum = 0;
                // Retrieve vertices or create new ones if they don't exist
                vertices.TryGetValue(kvp.Key.Item1, out var vertex1);
                vertices.TryGetValue(kvp.Key.Item2, out var vertex2);
                setnumold = vertex1.setno;
                newsetno = vertex2.setno;
                double[] Array = new double[3];
                // Calculate the sum and count for the new vertex
                if (vertex1.totalsim == 0)
                {
                    sum = bothsim + vertex2.totalsim;
                    cnt = vertex2.count + 2;
                }
                else if (vertex2.totalsim == 0)
                {
                    sum = bothsim + vertex1.totalsim;
                    cnt = vertex1.count + 2;
                }
                else if (vertex1.totalsim == vertex2.totalsim)
                {
                    sum = bothsim + vertex1.totalsim;
                    cnt = vertex1.count + 2;
                }
                else
                {
                    sum = bothsim + vertex1.totalsim + vertex2.totalsim;
                    cnt = vertex1.count + vertex2.count + 2;
                }
                vert v = new vert(newsetno, sum, cnt);
                // Update vertices with the same set numbers
                foreach (var key in keysIndex)
                {
                    if (vertices[key].setno == setnumold || vertices[key].setno == newsetno)
                    {
                        vertices[key] = v;
                    }
                }
                
                // Add edges to the readgraph
                Array[0] = kvp.Value.sim1;
                Array[1] = kvp.Value.sim2;
                Array[2] = kvp.Value.linesmatch;
                if (setnumold != newsetno)
                {
                    maxstGraph.Add(new Tuple<string, string>(kvp.Key.Item1, kvp.Key.Item2), Array);
                }
            }
            sortedmaxst = maxstGraph
                .OrderByDescending(kv => (vertices[kv.Key.Item1].totalsim) / (vertices[kv.Key.Item1].count))
                .ThenByDescending(kv => kv.Value[2])
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            mstTime += stopwatch2.ElapsedMilliseconds;

            return;
        }


        static void MSTWrite()
        {
            Stopwatch stopwatch3 = new Stopwatch();
            stopwatch3.Start();

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("MST");

                // Write column headers
                worksheet.Cells[1, 1].Value = "File 1";
                worksheet.Cells[1, 2].Value = "File 2";
                worksheet.Cells[1, 3].Value = "Line Matches";

                // Write readgraph data
                int row = 2;
                foreach (var kvp in sortedmaxst)
                {
                    Tuple<string, string> key = kvp.Key;
                    double[] values = kvp.Value;

                    // Write vertex 1 with concatenated value and make it clickable
                    worksheet.Cells[row, 1].Value = $"{key.Item1}({values[0]}%)";
                    worksheet.Cells[row, 1].Hyperlink = new Uri("file:///" + key.Item1.Replace("\\", "/"));

                    // Write vertex 2 with concatenated value and make it clickable
                    worksheet.Cells[row, 2].Value = $"{key.Item2}({values[1]}%)";
                    worksheet.Cells[row, 2].Hyperlink = new Uri("file:///" + key.Item2.Replace("\\", "/"));

                    // Write the third value
                    worksheet.Cells[row, 3].Value = values[2];

                    row++;
                }

                // Resize columns to fit content
                worksheet.Cells.AutoFitColumns();

                FileInfo excelFile = new FileInfo(@"C:\Users\momen\Desktop\algo\New folder (2)\Plagrisim-Detection\Test Cases results\Sample\5-mst_file.xlsx");
                package.SaveAs(excelFile);
            }
            mstTime += stopwatch3.ElapsedMilliseconds;
            Console.WriteLine("MST write time: " + stopwatch3.ElapsedMilliseconds);
            Console.WriteLine("MST generation and write time: " + mstTime);

        }
        static void StatWrite()
        {
            Stopwatch stopwatch4 = new Stopwatch();
            stopwatch4.Start();
            var statDict = vertices.OrderByDescending(kvp => (kvp.Value.totalsim / kvp.Value.count))
                                   .ThenBy(kvp => int.Parse((ExtractNumber(kvp.Key))));
            int temp1;
            int temp2 = statDict.First().Value.setno;

            // Create a new Excel package
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Components");

                // Set column names
                worksheet.Cells[1, 1].Value = "Component index";
                worksheet.Cells[1, 2].Value = "Keys";
                worksheet.Cells[1, 3].Value = "Average";
                worksheet.Cells[1, 4].Value = "Count";

                int row = 2;
                int componentIndex = 1;
                string cell = "";
                int counter = 0;
                double average = 0;
                foreach (var kvp in statDict)
                {
                    temp1 = kvp.Value.setno;
                    // Calculate average

                    // If keys have the same average, add to the list
                    if (temp1 == temp2)
                    {
                        average = kvp.Value.totalsim / kvp.Value.count;
                        // Write keys to worksheet  
                        cell += ExtractNumber(kvp.Key) + ", ";
                        counter++;
                    }
                    else
                    {
                        worksheet.Cells[row, 1].Value = componentIndex;
                        worksheet.Cells[row, 3].Value = Math.Round(average, 1);
                        worksheet.Cells[row, 4].Value = counter;
                        worksheet.Cells[row, 2].Value = cell.TrimEnd(',',' ');
                        row++;
                        counter = 1;
                        componentIndex++;
                        cell = ExtractNumber(kvp.Key) + ", ";
                    }
                    temp2 = kvp.Value.setno;
                }

                // Write the last group of keys to the worksheet
                worksheet.Cells[row, 1].Value = componentIndex;
                worksheet.Cells[row, 3].Value = Math.Round(average, 1);
                worksheet.Cells[row, 4].Value = counter;
                worksheet.Cells[row, 2].Value = cell.TrimEnd(',',' ');

                worksheet.Cells.AutoFitColumns();

                // Save the Excel package to the specified file name
                FileInfo excelFile = new FileInfo(@"C:\Users\momen\Desktop\algo\New folder (2)\Plagrisim-Detection\Test Cases results\Sample\5-StatFile.xlsx");
                excelPackage.SaveAs(excelFile);
            }
            Console.WriteLine("Stat generation and write time:" + stopwatch4.ElapsedMilliseconds);
        }


        static string ExtractNumber(string key)
        {
            string pattern = @"(\d+)";
            Match match = Regex.Match(key, pattern);

            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return " ";
            }
        }

        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            readFile(@"C:\Users\momen\Desktop\algo\New folder (2)\Plagrisim-Detection\Test Cases\Sample\5-Input.xlsx");

            stopwatch.Start();
            maxST(ReadGraph);
            MSTWrite();
            Console.WriteLine("Elapsed Time total: " + stopwatch.ElapsedMilliseconds);
            StatWrite();
        }

    }

}