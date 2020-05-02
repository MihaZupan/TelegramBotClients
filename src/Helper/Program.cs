﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MakeRequestAsyncModifier
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceFilePath = @"..\..\..\TelegramBotClients\BlockingClient\BlockingTelegramBotClient.cs";
            
            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine("Code file does not exist");
                Console.ReadKey(true);
                Environment.Exit(1);
            }
            
            string backupFilePath;
            int previousBackupCount = 0;
            do
            {
                backupFilePath = "backup" + (previousBackupCount != 0 ? previousBackupCount.ToString() : "") + "_" + Path.GetFileName(sourceFilePath);
                previousBackupCount++;
            }
            while (File.Exists(backupFilePath));
            
            File.Copy(sourceFilePath, backupFilePath);
            string code = File.ReadAllText(sourceFilePath);
            
            foreach (Match match in Regex.Matches(code, @"(chatId|userId)[\w\W]{0,900}?\)[\w\W]{0,30}?MakeRequestAsync[\w\W]*?\([\w\W]*?\);"))
            {
                code = code.Insert(code.OrdinalIndexAfter(match.Value, match.Index) - 2, ", " + match.Groups[1]);
            }
            foreach (Match match in Regex.Matches(code, @"default\W*?\)[\w\W]{0,50}?MakeRequestAsync[\w\W]*?\([\w\W]*?\);"))
            {
                code = code.Insert(code.OrdinalIndexOf(match.Value, match.Index) + 7, ",\r\n"+ new string(' ', 12) + "SchedulingMethod schedulingMethod = SchedulingMethod.Normal");
                code = code.Insert(code.OrdinalIndexAfter(match.Value.Last(20), match.Index) - 2, ", schedulingMethod");
            }
            
            File.WriteAllText(sourceFilePath, code);
            
            Console.WriteLine("Done");
            Console.ReadKey(true);
        }
    }
    
    public static class StringExtensions
    {
        public static int OrdinalIndexOf(this string str, string value, int startIndex)
        {
            return str.IndexOf(value, startIndex, StringComparison.Ordinal);
        }
        
        public static int OrdinalIndexAfter(this string str, string value, int startIndex)
        {
            return str.IndexOf(value, startIndex, StringComparison.Ordinal) + 1;
        }

        public static string Last(this string str, int amount)
        {
            return amount >= str.Length ? str : str.Substring(str.Length - amount);
        }
    }
}
