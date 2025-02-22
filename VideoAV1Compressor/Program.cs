using PatzminiHD.CSLib.Input.Console;

namespace VideoAV1Compressor
{
    internal class Program
    {
        const string VERSION_STRING = "v1.3.1";
        static string directory = "";
        static int sublevels = -1;
        static uint quality = 23, cpu_used = 1;
        static string? skipListPath = null;
        static List<(List<string> names, CmdArgsParser.ArgType type)> validArgs = new()
        {
            (new(){"h", "help"}, CmdArgsParser.ArgType.SET),
            (new(){"d", "directory"}, CmdArgsParser.ArgType.STRING),
            (new(){"s", "sublevels"}, CmdArgsParser.ArgType.INT),
            (new(){"q", "quality"}, CmdArgsParser.ArgType.UINT),
            (new(){"c", "cpu-used"}, CmdArgsParser.ArgType.UINT),
            (new(){"", "skip-list"}, CmdArgsParser.ArgType.STRING),
        };
        static int Main(string[] args)
        {
            if(args.Length == 0)
                ShowHelpAndExit();

            CmdArgsParser parser = new(args, validArgs);
            try
            {
                var parsedArgs = parser.Parse();
                if(!VerifyArgs(parsedArgs))
                    return 1;

                CompressorManager compressorManager = new(directory, sublevels, quality, cpu_used, skipListPath);
                compressorManager.Run();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 2;
            }
            return 0;
        }

        static bool VerifyArgs(Dictionary<List<string>, (Type? type, object? value)> parsedArgs)
        {
            if (parsedArgs.ContainsKey(validArgs[0].names))
                    ShowHelpAndExit();

                if (!parsedArgs.ContainsKey(validArgs[1].names))    //directory
                    directory = Environment.CurrentDirectory;
                else if (!Directory.Exists(parsedArgs.GetValueOrDefault(validArgs[1].names).value?.ToString()))
                {
                    Console.WriteLine("The given directory does not exist");
                    return false;
                }
                else
                    directory = parsedArgs.GetValueOrDefault(validArgs[1].names).value?.ToString()!;

                if (parsedArgs.ContainsKey(validArgs[2].names)) //sublevels
                    sublevels = (int)parsedArgs.GetValueOrDefault(validArgs[2].names).value!;

                if(parsedArgs.ContainsKey(validArgs[3].names))  //quality
                    quality = (uint)parsedArgs.GetValueOrDefault(validArgs[3].names).value!;

                if(parsedArgs.ContainsKey(validArgs[4].names))  //cpu-used
                    cpu_used = (uint)parsedArgs.GetValueOrDefault(validArgs[4].names).value!;

                if(parsedArgs.ContainsKey(validArgs[5].names))  //skip-list
                    skipListPath = parsedArgs.GetValueOrDefault(validArgs[5].names).value?.ToString()!;

                if (quality > 63) //Value can only range from 0 to 63 (inclusive)
                {
                    Console.WriteLine("Argument 'q', 'quality' can only range from 0 to 63 (inclusive)");
                    return false;
                }
                if(cpu_used > 8) //Value can only range from 0 to 8 (inclusive)   
                {
                    Console.WriteLine("Argument 'c', 'cpu-used' can only range from 0 to 8 (inclusive)");
                    return false;
                }
                return true;
        }
        static void ShowHelpAndExit()
        {
            Console.WriteLine($"VideoAV1Compressor {VERSION_STRING}");
            Console.WriteLine($"using {PatzminiHD.CSLib.Info.Name} {PatzminiHD.CSLib.Info.Version}");
            Console.WriteLine($"");
            Console.WriteLine($"Usage: vav1comp -d <path/to/directory> [-s] <number> [-q] <number> [-c] <number>");
            Console.WriteLine($"Command line arguments:");
            Console.WriteLine($"-h --help         Show this help");
            Console.WriteLine($"-d --directory    The base directory that should be\n" +
                              $"                  searched for videos");
            Console.WriteLine($"-s --sublevels    How many subdirectories deep to\n" +
                              $"                  search from the base directory.\n" +
                              $"                  0 means only search for videos directly\n" +
                              $"                  in the base directory. Any negative number\n" +
                              $"                  means no limit. This parameter is\n" +
                              $"                  optional. If not specified, default is {sublevels}");
            Console.WriteLine($"-q --quality      Specify the quality of the encoding.\n" +
                              $"                  Valid values are 0 to 63 (inclusive),\n" +
                              $"                  Where lower values mean a better quality.\n"+
                              $"                  Default value is {quality}");
            Console.WriteLine($"-c --cpu-used     Specify the efficiency of the encoding.\n" +
                              $"                  Valid values are 0 to 8 (inclusive),\n" +
                              $"                  Where lower values mean a slower encoding with\n"+
                              $"                  smaller file size.\n"+
                              $"                  Default value is {cpu_used}");
            Environment.Exit(0);
        }
    }
}
