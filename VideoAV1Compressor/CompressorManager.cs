using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace VideoAV1Compressor
{
    public class CompressorManager
    {
        private string directory;
        private int sublevels;
        private List<string>? filesList;
        public CompressorManager(string directory, int sublevels)
        {
            if (!Directory.Exists(directory))
                throw new Exception("The given directory is not valid");

            this.directory = directory;
            this.sublevels = sublevels;
        }

        public void Run()
        {
            Console.WriteLine($"Directory: {directory}");
            Console.WriteLine($"Sublevels: {sublevels}");

            Console.WriteLine($"Searching for files in '{directory}'...");
            filesList = GetAllFiles();
            Console.WriteLine($"Found {filesList.Count} files");
        }

        private List<string> GetAllFiles()
        {
            return PatzminiHD.CSLib.FileSystem.Directory.GetAllFiles(this.directory, this.sublevels).Result;
        }
    }
}
