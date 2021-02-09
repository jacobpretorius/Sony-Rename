using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sony.Rename
{
    class Program
    {
        static void Main(string[] args)
        {
            var versionString = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion
                .ToString();

            Console.WriteLine($"Sony-Rename version {versionString}. Made by Jacob Pretorius.");

            string workDir;
            if (args.Length == 0)
            {
                Console.WriteLine("Enter directory to scan for media");
                workDir = Console.ReadLine();
            }
            else
            {
                workDir = args[0];
            }

            if (!string.IsNullOrWhiteSpace(workDir))
            {
                var fileList = new List<string>();

                try
                {
                    // Lookup the filenames
                    var files = Directory.EnumerateFiles(workDir);
                    foreach (string file in files)
                    {
                        string fileName = file.Substring(workDir.Length + 1);
                        if (fileName.EndsWith("MP4") || fileName.EndsWith("XML"))
                        {
                            fileList.Add(fileName);
                        }
                    }

                    // Get a match on name pattern
                    var foundPattern = GetPattern(fileList);

                    // Get replacement
                    Console.WriteLine($"Found the pattern '{foundPattern}', what would you like it to be instead?");
                    var replacePattern = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(replacePattern))
                    {
                        throw new Exception(replacePattern + " not valid");
                    }

                    // Replace
                    foreach (string file in files)
                    {
                        string fileName = file.Substring(workDir.Length + 1);
                        if (fileName.EndsWith("MP4") || fileName.EndsWith("XML"))
                        {
                            var newFileName = fileName.Replace(foundPattern, replacePattern);
                            Console.WriteLine($"Renaming {fileName} to {newFileName}");
                            
                            Directory.Move(file, file.Substring(0, workDir.Length + 1) + newFileName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine("Done.");
        }

        private static string GetPattern(List<string> files)
        {
            var shortestFilename = files.OrderBy(s => s.Length).First().Length - 4;
            var matchAt = 0;

            for (int i = 0; i < shortestFilename; i++)
            {
                var matched = CheckDigitsMatch(files, i);
                if (matched)
                {
                    matchAt = i;
                    continue;
                }

                // Check if this non match breaks the previous match so we should stop
                if (!matched && i == (matchAt - 1))
                {
                    break;
                }
            }

            return matchAt > 0 ? files.First().Substring(0, matchAt + 1) : string.Empty;
        }

        private static bool CheckDigitsMatch(List<string> files, int index)
        {
            var checker = files.First();

            return files.All(a => a[index] == checker[index]);
        }
    }
}