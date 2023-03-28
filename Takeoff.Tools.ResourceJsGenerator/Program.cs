using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Resources;
using System.Collections;

namespace Takeoff.Tools.ResourceJsGenerator
{
    /// <summary>
    /// Reads resx resource files from a web app and generates javascript files for client-side localization.
    /// Arguments:  
    /// 1) directory containing resx files.  Each resx should have a cooresponding text file (name of resx + "-js.txt", so Strings.resx should have Strings-js.txt.  That file will list all of the names of strings in the resource to include in javascript.  The first line is the variable to assign the strings to.
    /// 2) directory to place target js files.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string resxDirectory = args[0];
            if (!Directory.Exists(resxDirectory))
            {
                Console.WriteLine("Directory '" + resxDirectory + "' not found.  Exiting.");
                return;
            }
            string outDir = args[1];
            if (!Directory.Exists(outDir))
            {
                Console.WriteLine("Directory '" + outDir + "' not found.  Exiting.");
                return;
            }

            var resxFiles = Directory.GetFiles(resxDirectory, "*.resx").Where(resxPath =>
            {
                return File.Exists(Path.Combine(resxDirectory, Path.GetFileName(resxPath).Before(".") + "-js.txt"));
            });
            
            foreach (var resxPath in resxFiles)
            {
                var jsInstructions = Path.Combine(resxDirectory, Path.GetFileName(resxPath).Before(".") + "-js.txt");
                var jsInstructionLines = File.ReadAllLines(jsInstructions);
                var variableToAssign = jsInstructionLines.First();
                var keysToInclude = jsInstructionLines.Skip(1).ToDictionary(k => k);
                var jsFileName = Path.GetFileNameWithoutExtension(resxPath).EndWith(".") + "js";
                var jsPath = Path.Combine(outDir, jsFileName);

                var resources = new Dictionary<string, string>();
                foreach (DictionaryEntry de in new ResXResourceReader(resxPath))
                {
                    string key = de.Key as string;
                    if (jsInstructionLines.Contains(key))
                    {
                        resources.Add(key, de.Value.CastTo<string>());
                    }
                }

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(resources, Newtonsoft.Json.Formatting.Indented);

                var contents = new StringBuilder();
                contents.AppendLine("(function () {");
                contents.AppendLine("\tvar resources = " + json + ";");
                contents.AppendLine(string.Format("\t{0} = $.extend({0} || {{}}, resources);", variableToAssign));
                contents.AppendLine("})();");

                File.WriteAllText(jsPath, contents.ToString(), Encoding.UTF8);

            }
        }

    }
}
