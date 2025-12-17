using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace junmidsen
{
    internal class Program
    {
        private static readonly string TASK3_FILE_NAME_LOGS_ORIGINAL = "logs.txt";
        private static readonly string TASK3_FILE_NAME_LOGS_STANDARD = "logs_standard.txt";
        private static readonly string TASK3_FILE_NAME_LOGS_WITH_PROBLEMS = "problems.txt";

        //10.03.2025 15:14:49.523 INFORMATION Версия программы: '3.4.0.48729'
        private static readonly Regex TASK3_REGEX_FORMAT1 = new Regex(
            @"^(?<date>\d{2}\.\d{2}\.\d{4})\s+(?<time>\d{2}:\d{2}:\d{2}\.\d+)\s+(?<level>\w+)\s+(?<message>.+)$");

        //2025-03-10 15:14:51.5882| INFO|11|MobileComputer.GetDeviceId| Код устройства: '@MINDEO-M40-D-410244015546'
        private static readonly Regex TASK3_REGEX_FORMAT2 = new Regex(
                @"^(?<date>\d{4}-\d{2}-\d{2})\s+(?<time>\d{2}:\d{2}:\d{2}\.\d+)\|\s*(?<level>\w+)\|\d+\|(?<methodName>[^|]+)\|\s*(?<message>.+)$");

        static int Main(string[] args)
        {
            string valueStr = "aaabbcccdde";
            valueStr = Task1("aaabbcccdde");
            Console.WriteLine(valueStr);
            System.Diagnostics.Debug.Assert(valueStr.Equals("a3b2c3d2e"), "Задание 1 компрессия решено неверно");

            valueStr = Task1(valueStr, true);
            Console.WriteLine(valueStr);
            System.Diagnostics.Debug.Assert(valueStr.Equals("aaabbcccdde"), "Задание 1 декомпрессия решено неверно");

            using (StreamWriter writer = new StreamWriter(TASK3_FILE_NAME_LOGS_STANDARD))
            using (StreamWriter writerProblems = new StreamWriter(TASK3_FILE_NAME_LOGS_WITH_PROBLEMS))
            using (StreamReader reader = new StreamReader(TASK3_FILE_NAME_LOGS_ORIGINAL))
            {
                string line, lineStandard;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        lineStandard = Task3LogsToStandard(line);
                        writer.WriteLine(lineStandard);
                    } catch(LogToStandardWrongFormatException e)
                    {
                        writerProblems.WriteLine(line);
                    }
                }
            }


            return 0;
        }

        private static string Task3LogsToStandard(string lineStr)
        {
            Match match;
            if (TASK3_REGEX_FORMAT1.IsMatch(lineStr))
            {
                match = TASK3_REGEX_FORMAT1.Match(lineStr);

                return $"{DateTime.ParseExact(match.Groups["date"].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")}"
                    + $"\t{match.Groups["time"].Value}"
                    + $"\t{Task3LevelLabelToStandard(match.Groups["level"].Value)}\tDEFAULT"
                    + $"\t{match.Groups["message"].Value}"
                    ;
            }

            if (TASK3_REGEX_FORMAT2.IsMatch(lineStr))
            {
                match = TASK3_REGEX_FORMAT2.Match(lineStr);

                return $"{match.Groups["date"].Value}" 
                    + $"\t{match.Groups["time"].Value}" 
                    + $"\t{Task3LevelLabelToStandard(match.Groups["level"].Value)}" 
                    + $"\t{match.Groups["methodName"].Value}" 
                    + $"\t{match.Groups["message"].Value}";
            }

            throw new LogToStandardWrongFormatException(lineStr);
        }

        public static string Task3LevelLabelToStandard(string levelValue)
        {
            levelValue = levelValue.ToUpper();
            if (levelValue == "INFORMATION" || levelValue == "INFO")
            {
                return "INFO";
            }
            else if (levelValue == "WARNING" || levelValue == "WARN")
            {
                return "WARN";
            }
            else if(levelValue == "ERROR")
            {
                return "ERROR";
            } else if(levelValue == "DEBUG")
            {
                return "DEBUG";
            } else
            {
                return "INFO";
            }
        }

        /// <summary>
        /// Ошибка трансформации лога к стандартному виду
        /// </summary>
        public class LogToStandardWrongFormatException : Exception
        {
            public LogToStandardWrongFormatException(string value) : base($"Ошибка: не найден допустимый формат для лога: {value}") { }
        }


        private static class Task2Server
        {
            private static int count;
            private static readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

            public static int GetCount()
            {
                readerWriterLockSlim.EnterReadLock();
                try
                {
                    return count;
                }
                finally
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }

            public static void AddToCount(int value)
            {
                readerWriterLockSlim.EnterWriteLock();
                try
                {
                    count += value;
                }
                finally
                {
                    readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        private static string Task1(string valueStr, in bool isDecompression = false)
        {
            if (string.IsNullOrEmpty(valueStr))
            {
                return valueStr;
            }
            StringBuilder stringBuilder = new StringBuilder();
            char? characterLast = valueStr[0];
            int characterNumber = 1;
            if (!isDecompression)
            {
                characterLast = valueStr[0];
                characterNumber = 1;
                for (int i = 1; i < valueStr.Length; i++)
                {
                    if (valueStr[i] == characterLast)
                    {
                        characterNumber++;
                    } else
                    {
                        stringBuilder.Append(characterLast);
                        if(characterNumber > 1)
                        {
                            stringBuilder.Append(characterNumber);
                        }
                        characterLast = valueStr[i];
                        characterNumber = 1;
                    }
                }
                stringBuilder.Append(characterLast);
                if (characterNumber > 1)
                {
                    stringBuilder.Append(characterNumber);
                }

                return stringBuilder.ToString();
            }


            StringBuilder repeatNumberStr = new StringBuilder();
            for (int i = 1; i < valueStr.Length; i++)
            {
                if (char.IsLetter(valueStr[i]))
                {
                    if(repeatNumberStr.Length != 0)
                    {
                        characterNumber = Int32.Parse(repeatNumberStr.ToString());
                        repeatNumberStr.Clear();
                    }
                    stringBuilder.Append(characterLast.Value, characterNumber);
                    characterLast = valueStr[i];
                    characterNumber = 1;
                } else
                {
                    repeatNumberStr.Append(valueStr[i]);
                }
            }

            if (repeatNumberStr.Length != 0)
            {
                characterNumber = Int32.Parse(repeatNumberStr.ToString());
                repeatNumberStr.Clear();
            }
            stringBuilder.Append(characterLast.Value, characterNumber);


            return stringBuilder.ToString();

        }
    }
}
