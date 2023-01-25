class Program{
    static void Main(string[] args) {
        if (args.Length < 2) {
            Console.WriteLine("usage:\n" +
                "required first argument is 1 to split files, or 2 to recombine files\n" +
                "required second argument is a location (a file for mode 1, or a directory for mode 2). both paths must be RELATIVE to the executable.\n" +
                "optional third argument determines the chunk size in split mode, in mb (default 20mb)\n" +
                "and the file format to recombine to in recombine mode (default none)");
            return;
        }

        int checkMode = Int32.Parse(args[0]);
        string inputPath = args[1];

        bool valid = false;
        int validMode = -1;
        switch (checkMode) {
            case 1: {
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
            case -1: {
                return;
            }
            case 1: {
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
            case 2: {
                string format = "";
                if (args.Length == 3) {
                    format = args[2];
                }
                RecombineFile(inputPath, Directory.GetCurrentDirectory() + "\\" + "recombined", format);
                break;
            }
        }

    }
    public static void SplitFile(string inputFile, int chunkSize, string path) {
        const int BUFFER_SIZE = 20 * 1024;
        byte[] buffer = new byte[BUFFER_SIZE]; //20kb

        using (Stream input = File.OpenRead(inputFile)) {
            int index = 0;
            while (input.Position < input.Length) {
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
    }
    public static void RecombineFile(string inputDirectory, string output, string format) {
        string[] inputFilePaths = Directory.GetFiles(inputDirectory);
        Console.WriteLine("Number of files: {0}.", inputFilePaths.Length);
        if (format != string.Empty) {
            output += ".";
            output += format;
        }
        using (var outputStream = File.Create(output)) {
            foreach (var inputFilePath in inputFilePaths) {
                using (var inputStream = File.OpenRead(inputFilePath)) {
                    inputStream.CopyTo(outputStream);
                }
                Console.WriteLine("The file {0} has been processed.", inputFilePath);
            }
        }
    }
}