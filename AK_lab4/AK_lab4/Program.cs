using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DirectoryAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || args.Contains("--help"))
            {
                ShowHelp();
                Environment.Exit(0);
            }

            string directoryPath = null;
            string filePattern = "*";
            bool includeHidden = false;
            bool includeReadOnly = false;
            bool includeArchive = false;

            foreach (var arg in args)
            {
                if (arg.StartsWith("--dir="))
                {
                    directoryPath = arg.Substring(6);
                }
                else if (arg.StartsWith("--pattern="))
                {
                    filePattern = arg.Substring(10);
                }
                else if (arg == "--include-hidden")
                {
                    includeHidden = true;
                }
                else if (arg == "--include-readonly")
                {
                    includeReadOnly = true;
                }
                else if (arg == "--include-archive")
                {
                    includeArchive = true;
                }
                else if (arg == "--help")
                {
                    ShowHelp();
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine($"Unknown parameter: {arg}");
                    ShowHelp();
                    Environment.Exit(1);
                }
            }

            if (string.IsNullOrEmpty(directoryPath))
            {
                Console.WriteLine("Directory path is required.");
                ShowHelp();
                Environment.Exit(1);
            }

            try
            {
                AnalyzeDirectory(directoryPath, filePattern, includeHidden, includeReadOnly, includeArchive);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: DirectoryAnalyzer --dir=<directory> [--pattern=<file pattern>] [--include-hidden] [--include-readonly] [--include-archive] [--help]");
            Console.WriteLine("Parameters:");
            Console.WriteLine("  --dir=<directory>          Specifies the directory to analyze.");
            Console.WriteLine("  --pattern=<file pattern>   Specifies the file pattern to match (default is '*').");
            Console.WriteLine("  --include-hidden           Includes hidden files in the analysis.");
            Console.WriteLine("  --include-readonly         Includes read-only files in the analysis.");
            Console.WriteLine("  --include-archive          Includes archive files in the analysis.");
            Console.WriteLine("  --help                     Displays this help message.");
        }

        static void AnalyzeDirectory(string directoryPath, string filePattern, bool includeHidden, bool includeReadOnly, bool includeArchive)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException($"The directory '{directoryPath}' does not exist.");
            }

            var subdirectories = directoryInfo.GetDirectories();
            foreach (var subdirectory in subdirectories)
            {
                long size = CalculateDirectorySize(subdirectory, filePattern, includeHidden, includeReadOnly, includeArchive);
                Console.WriteLine($"Directory: {subdirectory.FullName}, Size: {size} bytes");
            }
        }

        static long CalculateDirectorySize(DirectoryInfo directory, string filePattern, bool includeHidden, bool includeReadOnly, bool includeArchive)
        {
            long size = 0;

            var files = directory.GetFiles(filePattern, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (ShouldIncludeFile(file, includeHidden, includeReadOnly, includeArchive))
                {
                    size += file.Length;
                }
            }

            return size;
        }

        static bool ShouldIncludeFile(FileInfo file, bool includeHidden, bool includeReadOnly, bool includeArchive)
        {
            if (file.Attributes.HasFlag(FileAttributes.Hidden) && !includeHidden)
            {
                return false;
            }

            if (file.Attributes.HasFlag(FileAttributes.ReadOnly) && !includeReadOnly)
            {
                return false;
            }

            if (file.Attributes.HasFlag(FileAttributes.Archive) && !includeArchive)
            {
                return false;
            }

            return true;
        }
    }
}
