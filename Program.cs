using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HtmlToText.ConsoleTools;
using HtmlAgilityPack;

namespace HtmlToText {
    public static class Program {
        public static void Main(string[] args) {
            try {
                new CommandEngine().Execute(typeof(Program).GetMethod("Process"), args);
            }
            catch (Exception ex) {
                WriteError(ex.ToString());
            }
        }

        public static void Process(string input, string output, bool recursive = false, string xpath = "body")
        {
            string inputDirectory;
            var inputFiles = GetInputFiles(input, recursive, out inputDirectory);
            foreach (var inputFile in inputFiles) {
                var document = new HtmlDocument();
                document.Load(Path.Combine(inputDirectory, inputFile));

                var outputFile = Path.Combine(output, inputFile + ".txt");
                var outputDirectory = Path.GetDirectoryName(outputFile);
                Directory.CreateDirectory(outputDirectory);

                var nodes = document.DocumentNode.SelectNodes(xpath);
                if (nodes == null || nodes.Count == 0)
                    continue;

                using (var writer = new StreamWriter(outputFile)) {                    
                    new TextExtractor().ExtractTextAndWrite(nodes, writer);
                }
            }
        }

        private static string[] GetInputFiles(string input, bool recursive, out string inputDirectory) {
            if (Directory.Exists(input)) {
                inputDirectory = input;
                return Directory.GetFiles(input, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                                .Where(f => f.EndsWith("htm") || f.EndsWith("html"))
                                .Select(f => f.Replace(input, ""))
                                .ToArray();
            }

            if (!File.Exists(input))
                throw new ArgumentException(input + " was not found.");

            if (recursive)
                throw new ArgumentException(input + " is a file, can not use recursive processing.");

            inputDirectory = Path.GetDirectoryName(input);
            return new[] { Path.GetFileName(input) };
        }



        private static void WriteError(string error, params object[] args) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error, args);
            Console.ResetColor();
        }
    }
}
