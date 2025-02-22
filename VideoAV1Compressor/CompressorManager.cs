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
        private string? skipListPath;
        private List<string>? filesList;
        private List<string>? skipList;
        public CompressorManager(string directory, int sublevels, uint quality, uint cpu_used, string? skipListPath)
        {
            if (!Directory.Exists(directory))
                throw new Exception("The given directory is not valid");

            this.directory = directory;
            this.sublevels = sublevels;
            this.quality = quality;
            this.cpu_used = cpu_used;
            if(skipListPath != null)
                this.skipListPath = skipListPath;
            else
                this.skipListPath = Path.Combine(directory, $"vav1comp_skip_list.txt");
        }

        public void Run()
        {
            Console.WriteLine($"Directory: {directory}");
            Console.WriteLine($"Sublevels: {sublevels}");
            Console.WriteLine($"Quality:   {quality}");
            Console.WriteLine($"Cpu-Used:  {cpu_used}");
            Console.WriteLine($"Skip-List:  {skipListPath}\n");

            Console.WriteLine($"Reading skip list...");
            if(File.Exists(skipListPath))
                skipList = File.ReadAllLines(skipListPath).ToList();
            Console.WriteLine($"Found {(skipList == null ? "no" : skipList.Count)} video{(skipList != null && skipList.Count == 1 ? "" : "s")} to skip");

            Console.WriteLine($"Searching for files in '{directory}'...");
            filesList = GetAllFiles();
            filesList.Sort();
            Console.WriteLine($"Found {filesList.Count} file{(filesList.Count != 1 ? "s" : "")}");
            Console.WriteLine("Filtering for video files...");
            filesList = GetVideoFiles(filesList);
            Console.WriteLine($"Found {filesList.Count} video file{(filesList.Count != 1 ? "s" : "")}");
            Console.WriteLine("Filtering out videos that have already been reencoded...");
            filesList = GetNotReencodedVideoFiles(filesList);
            Console.WriteLine($"Found {filesList.Count} video file{(filesList.Count != 1 ? "s" : "")} that {(filesList.Count != 1 ? "have" : "has")} not been reencoded");
            
            if(filesList.Count < 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No videos to work on!");
                Console.ResetColor();
                Environment.Exit(0);
            }

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
                        File.Move(newFileName, Path.Combine(Path.GetDirectoryName(file)!, Path.GetFileNameWithoutExtension(file) + ".mkv"));
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Overwriting file {file} finished!");
                    }
                    else
                    {
                        File.Delete(newFileName);
                        if(skipListPath != null)
                            File.AppendAllText(skipListPath, file + "\n");
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
            if(files.Count == 0)
                return filteredFiles;
                
            string? codec;


            int i = 0;
            PatzminiHD.CSLib.Output.Console.ProgressBar progressBar = new(0, files.Count, -1, -1, (uint)Console.WindowWidth - 5, false);
            progressBar.Value = i;
            Console.SetCursorPosition(0, Console.CursorTop);
            progressBar.Draw();

            foreach(string file in files)
            {
                i++;
                codec = PatzminiHD.CSLib.ProgramInterfaces.FFprobe.GetCodecName(file);
                if(codec == null)
                    continue;

                if(!codec.ToLower().Contains("av1") && !codec.ToLower().Contains("hevc"))
                    filteredFiles.Add(file);
                else
                    Console.WriteLine($"\nHas Codec AV1 or HEVC: {file}");

                Console.SetCursorPosition(0, Console.CursorTop);
                progressBar.Value = i;
                progressBar.Draw();
            }

            Console.WriteLine();

            return filteredFiles;
        }

        private List<string> GetVideoFiles(List<string> files)
        {
            List<string> filteredFiles = new();

            int i = 0;
            PatzminiHD.CSLib.Output.Console.ProgressBar progressBar = new(0, files.Count, -1, -1, (uint)Console.WindowWidth - 5, false);
            progressBar.Value = i;
            Console.SetCursorPosition(0, Console.CursorTop);
            progressBar.Draw();

            foreach(string file in files)
            {
                i++;
                if(PatzminiHD.CSLib.ProgramInterfaces.FFprobe.IsVideoFile(file))
                    filteredFiles.Add(file);
                else
                    Console.WriteLine($"Not video file: {file}");

                Console.SetCursorPosition(0, Console.CursorTop);
                progressBar.Value = i;
                progressBar.Draw();
            }

            Console.WriteLine();

            return filteredFiles;
        }

        private List<string> GetAllFiles()
        {
            var files = PatzminiHD.CSLib.FileSystem.Directory.GetAllFiles(this.directory, this.sublevels).Result;
            if(skipList != null)
            {
                files = files.Where(f => !skipList.Contains(f)).ToList();
            }
            return files;
        }
    }
}
