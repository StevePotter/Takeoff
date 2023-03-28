using System;
using System.Linq;
using CommandLine;
using Takeoff.Data;
using Takeoff.Models;
using System.Data.Objects;
using System.Text;
using CsvHelper;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.IO.Compression;

namespace Takeoff.DataTools.Commands
{
    /*
     *TEMPORARY COMMAND USED DURING OUR CLOSING TO GET LIST OF EMAIL APOLOGIES
     *
     */

    public class ZipEachAccountData : BaseCommand
    {
        public ZipEachAccountData()
        {
            EnableXmlReport = false;
            NotifyOnErrors = true;
            LogJobInDatabase = false;
        }

        protected override void Perform(string[] commandLineArgs)
        {
            var baseDir = @"C:\Users\Steve\Dropbox (Personal)\Companies\Takeoff\Dev\Closing Down\Owner Exports With Videos";
            foreach (var accountDirectory in Directory.GetDirectories(baseDir))
            {
                var accountId = accountDirectory.Split(Path.DirectorySeparatorChar).Last();
                var outputPath = Path.Combine(baseDir, $"{accountId}.zip");
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                var files = Directory.GetFiles(accountDirectory, "*.csv");
                if (files.Length < 2)
                {
                    Console.WriteLine($"Account {accountId} Had No Active Projects");
                    continue;
                }
                if (files.Length == 2)
                {
                    files = files.Where(f => !f.EndsWith("All Comments.csv")).ToArray();
                }
                using (var fileStream = new FileStream(outputPath, FileMode.CreateNew))
                {
                    using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in files)
                        {
                            zipArchive.CreateEntryFromFile(file, Path.GetFileName(file));
                        }
                    }
                }
            }

        }

    }
}

