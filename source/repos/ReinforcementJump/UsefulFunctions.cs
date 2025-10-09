
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
#nullable enable

namespace ReinforcementJump
{
    public static class UsefulFunctions
    {
        public static double Derivative(Func<double, double> f, double x) // Derives any function with 1 parameter
        {
            double h = 0.00001;
            double Delta = f(x + h) - f(x);
            return Math.Floor(100 * Delta / h)/100;

        }
        public static string? FindFileRegardlessOfExtension(string DirectoryPath, string FileToLookFor)
        {
            var MatchingFiles = Directory.EnumerateFiles(DirectoryPath, "*.*", SearchOption.AllDirectories)
                .Where(file => Path.GetFileNameWithoutExtension(file).Equals(FileToLookFor, StringComparison.OrdinalIgnoreCase));
            //.Where is a LINQ keyword that filters the files based on a condition, often a lambda expression
            //Here it gets all files in the directory and subdirectories, then enumerates a function through each file to see if the name matches

            //Equals checks if two strings share the same value. An optional second parameter exists which you can use to specify how the comparison should be done, such as ignoring case sensitivity.
            return MatchingFiles.FirstOrDefault(); // Returns the first file that matches the condition, or null if no files match
        }
    }
}
