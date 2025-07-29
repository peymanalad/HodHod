using System;
using System.Collections.Generic;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace BlackListTool
{
    public static class BlackListManager
    {
        public static HashSet<string> Load(string path)
        {
            var numbers = new HashSet<string>();
            if (!File.Exists(path))
                return numbers;
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var workbook = new XSSFWorkbook(fs);
            var sheet = workbook.GetSheetAt(0);
            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var cell = sheet.GetRow(i)?.GetCell(0);
                if (cell != null)
                {
                    numbers.Add(cell.ToString());
                }
            }
            return numbers;
        }

        public static void Save(IEnumerable<string> numbers, string path)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Numbers");
            var header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("PhoneNumber");
            int rowIndex = 1;
            foreach (var num in numbers)
            {
                var row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(num);
            }
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            workbook.Write(fs, true);
        }
    }

    class Program
    {
        static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  import <excel> -> reads numbers from excel and prints them");
            Console.WriteLine("  export <excel> <number1> [number2 ...] -> creates excel with numbers");
        }

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage();
                return 1;
            }
            var cmd = args[0].ToLower();
            if (cmd == "import" && args.Length >= 2)
            {
                var list = BlackListManager.Load(args[1]);
                foreach (var num in list)
                    Console.WriteLine(num);
                return 0;
            }
            if (cmd == "export" && args.Length >= 3)
            {
                var excel = args[1];
                var nums = args[2..];
                BlackListManager.Save(nums, excel);
                Console.WriteLine($"Saved {nums.Length} numbers to {excel}");
                return 0;
            }
            ShowUsage();
            return 1;
        }
    }
}