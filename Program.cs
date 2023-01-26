using System.Text.RegularExpressions;

class Program {
    class Mode {
        public const int Error = -1;
        public const int Split = 1;
        public const int Recombine = 2;
    }
    static void Main(string[] args) {
        if (args.Length < 2) {
            Console.WriteLine("usage:\n" +
                "required first argument is 1 to split files, or 2 to recombine files\n" +
                "required second argument is a location (a file for mode 1, or a directory for mode 2). both paths must be RELATIVE to the executable.\n" +
                "optional third argument determines the chunk size in split mode, in mb (default 20mb)");
            return;
        }

        int checkMode = Int32.Parse(args[0]);
        string inputPath = args[1];

        bool valid = false;
        int validMode = -1;
        switch (checkMode) {
            case Mode.Split: {
                if (File.Exists(inputPath)) {
                    valid = true;
                }
                break;
            }
            case 2: {
                if (Directory.Exists(inputPath)) {
                    valid = true;
                }
                break;
            }
            default: {
                Console.WriteLine("invalid mode (must be 1 or 2)");
                return;
            }
        }

        if (valid) {
            validMode = checkMode;
        }
        else {
            Console.Write("invalid input (make sure to use relative paths)");
        }

        switch (validMode) {
            case Mode.Error: {
                return;
            }
            case Mode.Split: {
                int chunkSize = 1024 * 1024 * 20;
                string no = inputPath;
                no.Replace(".", "");
                if (args.Length == 3) {
                    chunkSize = Int32.Parse(args[2]);
                    chunkSize *= (1024 * 1024);
                }
                Directory.CreateDirectory(no + "_split");
                SplitFile(inputPath, chunkSize, no + "_split");
                break;
            }
            case Mode.Recombine: {
                string format = "";
                if (args.Length == 3) {
                    format = args[2];
                }
                RecombineFile(inputPath, Directory.GetCurrentDirectory() + "\\" + "recombined");
                break;
            }
        }

    }
    public static void SplitFile(string inputFile, int chunkSize, string path) {
        const int BUFFER_SIZE = 20 * 1024;
        byte[] buffer = new byte[BUFFER_SIZE]; //20kb
        int index = 0;
        using (Stream input = File.OpenRead(inputFile)) {
            
            while (input.Position < input.Length) {
                if (index == 0) {
                    string ext = Regex.Replace(Path.GetExtension(inputFile), @"\r\n?|\n", String.Empty);
                    string[] asArray = {
                        ext
                    };
                    File.WriteAllLines(path + "\\format", asArray);
                }
                string fileName = index.ToString();
                int initialLength = fileName.Length;
                for (int i = 0; i < 10 - initialLength; i++) {
                    fileName = "0" + fileName; //prepend 0s so chunks dont get read out of order
                }
                using (Stream output = File.Create(path + "\\" + fileName)) {
                    int remaining = chunkSize, bytesRead;
                    while (remaining > 0 && (bytesRead = input.Read(buffer, 0,
                            Math.Min(remaining, BUFFER_SIZE))) > 0) {
                        output.Write(buffer, 0, bytesRead);
                        remaining -= bytesRead;
                    }
                }
                index++;
            }
        }
        Console.WriteLine("Created " + (index) + " chunks");
    }
    public static void RecombineFile(string inputDirectory, string output) {
        string[] inputFilePaths = Directory.GetFiles(inputDirectory);
        Console.WriteLine("Found {0} chunks", inputFilePaths.Length - 1);
        string formatPath = inputDirectory + "\\format";
        if (File.Exists(formatPath)) {
            string format = Regex.Replace(File.ReadAllText(formatPath), @"\r\n?|\n\r?", string.Empty);
            output += format;
            inputFilePaths = inputFilePaths.Take(inputFilePaths.Count() - 1).ToArray(); //dont process in the format
            Console.WriteLine("Format is " + format);
        }
        else {
            Console.WriteLine("Format detection failed");
        }
        using (var outputStream = File.Create(output)) {
            foreach (var inputFilePath in inputFilePaths) {
                if (inputFilePath.Equals("format")) continue;
                using (var inputStream = File.OpenRead(inputFilePath)) {
                    inputStream.CopyTo(outputStream);
                }
                Console.WriteLine("Chunk {0} has been processed", inputFilePath);
            }
            Console.WriteLine("File {0} was created", output);
        }
    }
}