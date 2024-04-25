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
        static void readFile(string fileName)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fileName)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // get the first worksheet

                // Loop through rows
                for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                {
                    string fileOneData = worksheet.Cells[i + 1, 1].Value.ToString();
                    string fileTwoData = worksheet.Cells[i + 1, 2].Value.ToString();
                    int lineMatches = int.Parse(worksheet.Cells[i + 1, 3].Value.ToString());

                    string[] fileOneParts = fileOneData.Split(new string[] { "(", ")" }, StringSplitOptions.None);
                    string hyperlink1 = fileOneParts[0].Trim();
                    string similarity1 = fileOneParts[1];

                    string[] fileTwoParts = fileTwoData.Split(new string[] { "(", ")" }, StringSplitOptions.None);
                    string hyperlink2 = fileTwoParts[0].Trim();
                    string similarity2 = fileTwoParts[1];

                    Console.WriteLine($"File one: {hyperlink1}, Similarity: ({similarity1})");
                    Console.WriteLine($"File two: {hyperlink2}, Similarity: ({similarity2})");
                    Console.WriteLine($"Lines matched: {lineMatches}");
                    Console.WriteLine();
                }
            }
        }

        static void Main(string[] args)
        {
            readFile(@"C:\Users\Lenovo\Downloads\1-Input.xlsx"); 
        }
    }
}
