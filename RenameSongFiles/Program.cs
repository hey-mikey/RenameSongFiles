using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Id3;

namespace RenameSongFiles
{
    class Program
    {
        private static readonly string DirectoryPath = "SET YOUR PATH HERE";
        static void Main()
        {
            var directories = Directory.EnumerateDirectories(DirectoryPath);
            foreach (var dir in directories)
            {
                if (DirectoryHasAudioFiles(dir))
                {
                    var files = Directory.GetFiles(dir).Where(f=>f.EndsWith("mp3"));
                    var oldFileNamesToNewFileNames = new Dictionary<string, string>();
                    foreach (var file in files)
                    {
                        using var mp3 = new Mp3(file);
                        var tagV1 = mp3.GetTag(Id3TagFamily.Version1X);
                        var tagV2 = mp3.GetTag(Id3TagFamily.Version2X);
                        var fileInfo = new FileInfo(file);

                        Id3Tag tag = tagV1 ?? tagV2;

                        if (tag == null || fileInfo.Name == $"{tag.Title}.mp3") continue;
                        var newName = $"{ReplaceInvalidChars(tag.Title)}.mp3";
                        oldFileNamesToNewFileNames.Add(Path.Combine(dir, fileInfo.Name), Path.Combine(dir, newName));
                    }

                    if (oldFileNamesToNewFileNames.Count > 0)
                    {
                        Console.WriteLine("About to rename some files. Hang on!");
                        foreach (var (oldFileName, newFileName) in oldFileNamesToNewFileNames)
                        {
                            File.Move(oldFileName, newFileName);
                        }
                    }
                }
            }
            Console.WriteLine("All done!");
        }

        /// <summary>
        /// Checks for mp3 files in the given directory
        /// TODO: include other audio file types
        /// </summary>
        /// <returns></returns>
        private static bool DirectoryHasAudioFiles(string s)
        {
            var files = Directory.GetFiles(s);

            return Directory.Exists(s) && files.Length > 0
                && files.Any(f => f.EndsWith("mp3"));
        }

        private static string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
