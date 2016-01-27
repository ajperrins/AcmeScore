using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AcmeScore
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1 || !File.Exists(args[0]))
            {
                Console.WriteLine($"No file argument, or non-existent file. Usage: {Process.GetCurrentProcess().ProcessName}.exe [filePath]");
                return;
            }

            CalculateScores(File.ReadAllLines(args[0]), Console.Out);
        }


        internal static List<WeightedNormalizedContactScore> CalculateScores(string[] lines, TextWriter output)
        {
            // Parse lines using `ScoreRecord` class, and group by contact ID and sum their weighted scores
            // Project the result into `WeighedNormalizedContactScore` instances so we can easily normalize
            // in a subsequent step, by setting that class's static min/max properties
            var result = (from line in lines
                          let score = ScoreRecord.Parse(line)
                          where score != null
                          group score by score.ContactId into contactGroup
                          select new WeightedNormalizedContactScore
                          {
                              WeightedSum = contactGroup.Sum(r => r.WeightedScore),
                              ContactId = contactGroup.Key
                          })
                          .OrderByDescending(record => record.WeightedSum)
                          .ToList();

            // Set the min and max static properties, which will affect instances' NormalizedScore result
            WeightedNormalizedContactScore.MaxWeightedSum = result.Max(i => i.WeightedSum);
            WeightedNormalizedContactScore.MinWeightedSum = result.Min(i => i.WeightedSum);

            foreach (var item in result)
            {
                output.WriteLine($"{item.ContactId}, {item.QuartileLabel}, {item.NormalizedScore}");
            }

            return result;
        }
    }
}