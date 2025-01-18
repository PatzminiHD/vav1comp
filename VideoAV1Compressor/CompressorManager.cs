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
        private uint quality, cpu_used;
        private List<string>? filesList;
        public CompressorManager(string directory, int sublevels, uint quality, uint cpu_used)
        {
            if (!Directory.Exists(directory))
                throw new Exception("The given directory is not valid");

            this.directory = directory;
            this.sublevels = sublevels;
            this.quality = quality;
            this.cpu_used = cpu_used;
        }

        public void Run()
        {
            Console.WriteLine($"Directory: {directory}");
            Console.WriteLine($"Sublevels: {sublevels}");
            Console.WriteLine($"Quality:   {quality}");
            Console.WriteLine($"Cpu-Used:  {cpu_used}\n");

            Console.WriteLine($"Searching for files in '{directory}'...");
            filesList = GetAllFiles();
            Console.WriteLine($"Found {filesList.Count} file{(filesList.Count != 1 ? "s" : "")}");
            Console.WriteLine("Filtering for video files...");
            filesList = GetVideoFiles(filesList);
            Console.WriteLine($"Found {filesList.Count} video file{(filesList.Count != 1 ? "s" : "")}");
            Console.WriteLine("Filtering out videos that have already been reencoded...");
            filesList = GetNotReencodedVideoFiles(filesList);
            Console.WriteLine($"Found {filesList.Count} video file{(filesList.Count != 1 ? "s" : "")} that {(filesList.Count != 1 ? "have" : "has")} not been reencoded");
            
            if(!PatzminiHD.CSLib.Input.Console.YesNo.Show("Do you want to continue?", true))
                return;

            Console.WriteLine("Beginning encoding...");
            ReencodeVideos(filesList);
            Console.WriteLine("Encoding finished!");
        }

        private void ReencodeVideos(List<string> files)
        {
            string file;
            bool encodingResult;
            PatzminiHD.CSLib.Output.Console.ProgressBar progressBar = new(0, files.Count - 1, 0, Console.WindowHeight - 2, 60, true);
            progressBar.Value = 0;
            for(int i = 0; i < files.Count; i++)
            {
                file = files[i];

                string newFileName = Path.Combine(Path.GetDirectoryName(file)!, Path.GetFileNameWithoutExtension(file) + "_av1.mkv");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Encoding file {file}...");

                Console.ForegroundColor = ConsoleColor.Gray;
                encodingResult = ReencodeVideo(file, newFileName);

                if(encodingResult)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Finished encoding file {file}!");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if(PatzminiHD.CSLib.Input.Console.YesNo.Show("Do you want to overwrite the original file?", true))
                    {
                        File.Delete(file);
                        File.Move(newFileName, file);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Overwriting file {file} finished!");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Encoding file {file} failed!");
                }

                Console.ResetColor();
                progressBar.Value = i;
            }
        }

        private bool ReencodeVideo(string filePath, string newFilePath)
        {
            var result = PatzminiHD.CSLib.ProgramInterfaces.FFmpeg.ReencodeVideo(filePath, newFilePath, this.quality, this.cpu_used, true, true);
            if(result == null)
                return true;
            return false;
        }

        private List<string> GetNotReencodedVideoFiles(List<string> files)
        {
            List<string> filteredFiles = new();
            string? codec;

            foreach(string file in files)
            {
                codec = PatzminiHD.CSLib.ProgramInterfaces.FFprobe.GetCodecName(file);
                if(codec == null)
                    continue;

                if(!codec.ToLower().Contains("av1") && !codec.ToLower().Contains("hevc"))
                    filteredFiles.Add(file);
                else
                    Console.WriteLine($"Has Codec AV1: {file}");
            }

            return filteredFiles;
        }

        private List<string> GetVideoFiles(List<string> files)
        {
            List<string> filteredFiles = new();

            foreach(string file in files)
            {
                if(PatzminiHD.CSLib.ProgramInterfaces.FFprobe.IsVideoFile(file))
                    filteredFiles.Add(file);
                else
                    Console.WriteLine($"Not video file: {file}");
            }

            return filteredFiles;
        }

        private List<string> GetAllFiles()
        {
            return PatzminiHD.CSLib.FileSystem.Directory.GetAllFiles(this.directory, this.sublevels).Result;
        }
    }
}
