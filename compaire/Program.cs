using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace compaire
{
    static class MainClass
    {
        // ENUMERATION OF DIFFERENT LOG TYPES
        enum logTypes
        {
            warning, info, error, success
        }

        // method that handles log events
        static void handleLog(this logTypes logType, string text, bool space)
        {
            // print correct symbol and color
            switch (logType)
            {
                case logTypes.info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(" [i] ");
                    break;
                case logTypes.success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" [$] ");
                    break;
                case logTypes.warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" [?] ");
                    break;
                case logTypes.error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" [!] ");
                    break;
            }

            // print message including current date / time
            Console.Write(System.DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss - "));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);

            // adds an empty line under the log output
            if (space == true)
            {
                Console.WriteLine();
            }

            // apply log event to file if bool is true
            if (createLog == true)
            {
                // init textwriter with append mode enabled
                using (TextWriter tw = new StreamWriter((jobid + ".log"), true))
                {
                    // similar to above
                    switch (logType)
                    {
                        case logTypes.info:
                            tw.Write(" [i] ");
                            break;
                        case logTypes.success:
                            tw.Write(" [$] ");
                            break;
                        case logTypes.warning:
                            tw.Write(" [?] ");
                            break;
                        case logTypes.error:
                            tw.Write(" [!] ");
                            break;
                    }
                    // apply information to new line
                    tw.Write(System.DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss - "));
                    tw.WriteLine(text);

                    // same as some lines above for log output
                    if (space == true)
                    {
                        tw.WriteLine("");
                    }
                    // close textwriter
                    tw.Close();
                }
            }
        }

        // method that prints debug information if the bool is true
        static void debugPrint(string text)
        {
            if (debugMode == true)
            {
                Console.WriteLine(text);
            }
        }

        // method that prints the logo
        static void drawLogo()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("   _____                            _");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  / ____|                          (_)");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" | |     ___  _ __ ___  _ __   __ _ _ _ __ ___");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" | |    / _ \\| '_ ` _ \\| '_ \\ / _` | | '__/ _ \\");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" | |___| (_) | | | | | | |_) | (_| | | | |  __/");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  \\_____\\___/|_| |_| |_| .__/ \\__,_|_|_|  \\___| v" + version.ToString("0.0").Replace(",", ".")); // print version (+ fixed style of output)
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("                       | | Developed by Kai Borchert");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("                       |_|");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");
        }

        // DOUBLE - VERSION OF THE PROGRAM
        private const double version = 1.1;

        // STRING LISTS - COLLECTING QUERIED FOLDERS AND FILES
        private static List<string> queriedFolders = new List<string>();
        private static List<string> queriedFiles = new List<string>();
        // STRING LIST - GENERATED CHECKSUMS
        private static List<string> checksums = new List<string>();

        // STRING ARRAY - PATHS TO SCAN
        private static string[] paths = new string[2];

        // INT - COUNTER FOR DIFFERENT LOG EVENTS
        private static int successCounter = 0;
        private static int warningCounter = 0;
        private static int errorCounter = 0;

        // BOOL - SKIP MISSING FILES?
        private static bool autoSkipMissing;

        // BOOL - CREATE LOG FILE?
        private static bool createLog;

        // BOOL - RECURSIVE QUERY
        private static bool recursive;

        // STRING - CONTAINS GENERATED ID OF CURRENT JOB FOR LOG FILE
        private static string jobid;

        // BOOL - CREATE MASTER HASH FILE?
        private static bool generateHashFile;

        // DEBUG MODE
        private static bool debugMode = false;

        // VAR FOR CHOOSEN ALGORITHM
        private static string alg;

        // ENUMERATION OF AVAILABLE ALGORITHMS
        enum algorithms
        {
            MD5, SHA1, SHA256, SHA384, SHA512
        }

        static void loopFolderQuery()
        {

            for (int x = 0; x < queriedFolders.Count; x++)
            {
                queryFolders(paths[0] + queriedFolders[x]);
            }
        }

        // print question and throw choice as bool
        static bool questionHandler(string question)
        {
            while (true)
            {
                Console.Write(" " + question + " [y/n]: ");

                // init helper to get pressed key
                ConsoleKeyInfo keyhelper;
                keyhelper = Console.ReadKey();

                Console.WriteLine("");

                if (keyhelper.KeyChar.Equals('y') || keyhelper.KeyChar.Equals('Y'))
                {
                    return true;
                }
                else if (keyhelper.KeyChar.Equals('n') || keyhelper.KeyChar.Equals('N'))
                {
                    return false;
                }
            }
        }

        // method that checks if the given arguments are valid
        static bool argumentsValid()
        {
            // return 'false' if there are not enough paths given
            for (byte x = 0; x < paths.Length; x++)
            {
                if (paths[x] == null)
                {
                    return false;
                }
            }

            // return 'false' if algorithm is missing
            if (alg == null)
            {
                return false;
            }

            // return 'true' if everything is okay
            return true;
        }

        // method which throws the current os as a string
        static String requestOS()
        {
            if (Environment.OSVersion.ToString().Contains("Unix"))
            {
                return "darwin";
            }
            else if (Environment.OSVersion.ToString().Contains("Windows"))
            {
                return "windows";
            }
            else if (Environment.OSVersion.ToString().Contains("Linux"))
            {
                return "linux";
            }
            else
            {
                return "unsupported";
            }
        }

        // method that prints coloured brackets with normal text in it
        static void colouredBrackets(string text, byte loff = 0, byte roff = 0)
        {
            // left offset
            for (byte x = 0; x < loff; x++)
            {
                Console.Write(" ");
            }

            // print brackets and text
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("]");
            Console.ForegroundColor = ConsoleColor.White;

            // right offset
            for (byte x = 0; x < roff; x++)
            {
                Console.Write(" ");
            }
        }

        /// MAIN METHOD
        public static void Main(string[] args)
        {
            // creates id for log 
            jobid = "compaire_" + System.DateTime.Now.ToString("dd_MM_yyyy-HH_mm_ss");

            // optical output (clean console, title, logo)
            Console.Clear();
            Console.Title = "Compaire v" + version.ToString("0.0").Replace(",", ".");
            drawLogo();

            // are there at least two arguments given?
            if ((args.Count() >= 2) /*&& (args.Count() <= 6)*/)
            {
                // query arguments
                foreach (string arg in args)
                {

                    // handle parameters
                    switch (arg.ToLower())
                    {
                        // create log file?
                        case "/l":
                            createLog = true;
                            debugPrint("createLog = true");
                            break;
                        // skip missing files automatically?
                        case "/s":
                            autoSkipMissing = true;
                            debugPrint("autoSkipMissing = true");
                            break;
                        // generate file with master hashes?
                        case "/m":
                            generateHashFile = true;
                            debugPrint("generateHashFile = true");
                            break;
                        // recursive query?
                        case "/r":
                            recursive = true;
                            debugPrint("recursive = true");
                            break;
                        // handle checksums
                        case "md5":
                        case "/md5":
                            if (alg == null)
                            {
                                alg = "MD5";
                                debugPrint("alg = MD5");
                            }
                            break;
                        case "sha1":
                        case "/sha1":
                            if (alg == null)
                            {
                                alg = "SHA1";
                                debugPrint("alg = SHA1");
                            }
                            break;
                        case "sha256":
                        case "/sha256":
                            if (alg == null)
                            {
                                alg = "SHA256";
                                debugPrint("alg = SHA256");
                            }
                            break;
                        case "sha384":
                        case "/sha384":
                            if (alg == null)
                            {
                                alg = "SHA384";
                                debugPrint("alg = SHA384");
                            }
                            break;
                        case "sha512":
                        case "/sha512":
                            if (alg == null)
                            {
                                alg = "SHA512";
                                debugPrint("alg = SHA512");
                            }
                            break;

                        // could it be a directory?
                        default:
                            if (Directory.Exists(arg))
                            {
                                for (byte x = 0; x < paths.Length; x++)
                                {
                                    if (paths[x] == null)
                                    {
                                        paths[x] = arg;
                                        debugPrint(paths[x]);
                                        break;
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            // output info page if arguments are not valid
            if (argumentsValid() == false)
            {
                // available parameters
                Console.ForegroundColor = ConsoleColor.Cyan;
                colouredBrackets("AVAILABLE PARAMETERS", 1, 0);
                Console.WriteLine();
                colouredBrackets("/l", 1, 1);
                Console.WriteLine("\tCreate log file");
                colouredBrackets("/s", 1, 1);
                Console.WriteLine("\tSkip missing files");
                colouredBrackets("/m", 1, 1);
                Console.WriteLine("\tGenerate seperate file with master hashes");
                colouredBrackets("/r", 1, 1);
                Console.WriteLine("\tRecursive Mode\n");

                // available algorithms
                Console.ForegroundColor = ConsoleColor.Cyan;
                colouredBrackets("AVAILABLE ALGORITHMS", 1, 0);
                Console.WriteLine();
                colouredBrackets("MD5", 1, 2);
                colouredBrackets("SHA1", 1, 2);
                colouredBrackets("SHA256", 1, 2);
                colouredBrackets("SHA384", 1, 2);
                colouredBrackets("SHA512", 1, 0);
                Console.WriteLine("\n");

                // usage of the cli
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(" Usage: ");
                // add additional 'mono' term if the os equals macos or linux
                if (requestOS().Equals("darwin") || requestOS().Equals("linux"))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("mono ");
                }
                // print name of current executable including available options
                Console.Write(System.AppDomain.CurrentDomain.FriendlyName + " ");
                colouredBrackets("Path1", 0, 1);
                colouredBrackets("Path2", 0, 1);
                colouredBrackets("Algorithm", 0, 1);
                colouredBrackets("Parameter", 0, 1);
                Console.WriteLine("\n");
            }

            // starting the application if arguments are valid
            else
            {
                // log handler - create log file?
                if (createLog == true)
                {
                    handleLog(logTypes.info, ("Log enabled. Save as " + jobid + ".log"), true);
                }
                else
                {
                    handleLog(logTypes.info, ("Log disabled."), true);
                }

                // log handler - skip missing files automatically?
                if (autoSkipMissing == true)
                {
                    handleLog(logTypes.info, ("Skip missing files automatically."), true);
                }
                else
                {
                    handleLog(logTypes.info, ("Ask for paths of missing files."), true);
                }

                // log handler - print paths
                for (byte x = 0; x < paths.Length; x++)
                {
                    handleLog(logTypes.info, ("Path " + (x + 1) + ": " + paths[x]), true);
                }

                // log handler - index creation
                handleLog(logTypes.info, ("Creating index of " + paths[0] + " ..."), true);

                // query maindirectories
                queryFolders(paths[0]);

                // query subdirectories?
                if (recursive == true)
                {
                    loopFolderQuery();
                }

                // log handler - begin with hashing process
                handleLog(logTypes.info, ("Begin with the " + alg + " hashing process ..."), true);

                // start hashing process with matching algorithm
                switch (alg)
                {
                    case "MD5":
                        hashingProcess(algorithms.MD5);
                        break;
                    case "SHA1":
                        hashingProcess(algorithms.SHA1);
                        break;
                    case "SHA256":
                        hashingProcess(algorithms.SHA256);
                        break;
                    case "SHA384":
                        hashingProcess(algorithms.SHA384);
                        break;
                    case "SHA512":
                        hashingProcess(algorithms.SHA512);
                        break;
                }

                // log handler - done job and counted events
                handleLog(logTypes.info, ("Compaire has finished it's work!"), false);
                handleLog(logTypes.success, ("Successes: " + successCounter), false);
                handleLog(logTypes.warning, ("Warnings: " + warningCounter), false);
                handleLog(logTypes.error, ("Errors: " + errorCounter), true);

                // generate seperate file with master hashes if 'true'
                if (generateHashFile == true)
                {
                    // log handler - generate seperate file
                    handleLog(logTypes.info, ("Generate seperate file with master hashes ..."), true);

                    // init textwriter with append mode enabled
                    using (TextWriter tw = new StreamWriter((jobid + ".sums"), true))
                    {
                        // loop queried files
                        for (int x = 0; x < queriedFiles.Count; x++)
                        {
                            // apply queried files and checksums to the file
                            tw.WriteLine(queriedFiles[x]);
                            tw.WriteLine(checksums[x]);
                            tw.WriteLine("");
                        }
                        // close textwriter
                        tw.Close();
                    }
                }

                // log handler - work done
                handleLog(logTypes.info, ("My work is done here. Have a nice day!"), true);

                // optional: exit application
                // Environment.Exit(0);
            }
        }

        // method that cares about the hashing process - parse algorithm
        static void hashingProcess(this algorithms algorithm)
        {
            // loop queried files
            for (int x = 0; x < queriedFiles.Count; x++)
            {
                if (checksumMethod(algorithm, x, queriedFiles[x]) == false)
                {
                    // endless loop
                    while (true)
                    {
                        // log handler - file could not found
                        handleLog(logTypes.warning, ("File could not found!"), false);

                        // skip question for alternate path if autoskip is enabled
                        if (autoSkipMissing == false)
                        {
                            // ask for alternate path of missed file
                            if (questionHandler("Do you want to enter an alternate path?") == true)
                            {
                                Console.Write(" Please enter your new path: ");
                                // request new path
                                if (checksumMethod(algorithm, x, Console.ReadLine()) == true)
                                {
                                    // break endless loop
                                    break;
                                }
                            }
                            else
                            {
                                // add placeholder for skipped file checksum
                                checksums.Add("skipped");
                                // log handler - skipped file
                                handleLog(logTypes.warning, ("Skipped file."), true);
                                // increase warning counter
                                warningCounter++;
                                // break endless loop
                                break;
                            }
                        }
                        else
                        {
                            // same as above
                            checksums.Add("skipped");
                            handleLog(logTypes.warning, ("Skipped file."), true);
                            warningCounter++;
                            // break endless loop
                            break;
                        }
                    }
                }
            }
        }

        // method which checks if second file exists and calculate checksums
        static bool checksumMethod(this algorithms algorithm, int x, string secpath)
        {
            // log handler - search for second file
            handleLog(logTypes.info, ("Search for " + paths[1] + secpath + " ..."), false);
            // check if second file exists
            if (File.Exists(paths[1] + secpath))
            {
                // string variable for temporary checksum
                string tempsum;

                // log handler - generate checksums
                handleLog(logTypes.info, ("Generate checksums ..."), false);

                // calculate checksums for both files
                checksums.Add(hashFile(algorithm, (paths[0] + queriedFiles[x]), debugMode));
                tempsum = hashFile(algorithm, (paths[1] + secpath), debugMode);

                // if both checksums are equal
                if (checksums[x].Equals(tempsum))
                {
                    // log handler - checksum is equal
                    handleLog(logTypes.success, ("Checksum " + tempsum + " is equal!"), true);
                    // increase success counter
                    successCounter++;
                }
                else
                {
                    // log handler - checksums are not equal
                    handleLog(logTypes.error, ("Checksums " + checksums[x] + " and " + tempsum + " are not equal!"), true);
                    // increase error counter
                    errorCounter++;
                }
                // return 'true' if file exists
                return true;
            }
            // else return 'false'
            return false;
        }

        // method to query folders and files of a path
        static void queryFolders(string path)
        {
            // log handler - query path
            handleLog(logTypes.info, ("Query " + path + " ..."), false);
            // query folders 
            for (int x = 0; x < (Directory.GetDirectories(path).Length); x++)
            {
                // add queried folders to list
                queriedFolders.Add(Directory.GetDirectories(path)[x].Replace(paths[0], ""));
            }
            // query files
            for (int x = 0; x < (Directory.GetFiles(path).Length); x++)
            {
                // ignore useless files from windows and macos
                if ((Directory.GetFiles(path)[x].Contains(".DS_Store") == false) && (Directory.GetFiles(path)[x].Contains("desktop.ini") == false))
                {
                    // add queried files to list
                    queriedFiles.Add(Directory.GetFiles(path)[x].Replace(paths[0], ""));
                }
            }
        }

        // method for hash value creation
        private static string hashFile(this algorithms algorithm, string Path, bool debug = false)
        {
            // handle different algorithms
            switch (algorithm)
            {
                // md5 algorithm
                case algorithms.MD5:
                    try
                    {
                        // open and handle file as buffered stream
                        using (FileStream fs = new FileStream(Path, FileMode.Open))
                        using (BufferedStream bs = new BufferedStream(fs))
                        using (var md = System.Security.Cryptography.MD5.Create())
                        {
                            // compute hash
                            var comp = md.ComputeHash(bs);
                            // build string
                            var hash = new StringBuilder();
                            foreach (byte b in comp)
                            {
                                hash.Append(b.ToString("X2"));
                            }
                            // return hash as string
                            return hash.ToString();
                        }
                    }
                    // return 'exception'
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return "exception";
                    }
                // sha1 algorithm
                case algorithms.SHA1:
                    try
                    {
                        // open and handle file as buffered stream
                        using (FileStream fs = new FileStream(Path, FileMode.Open))
                        using (BufferedStream bs = new BufferedStream(fs))
                        using (SHA1Managed sha = new SHA1Managed())
                        {
                            // compute hash
                            var comp = sha.ComputeHash(bs);
                            // build string
                            var hash = new StringBuilder(comp.Length * 2);
                            foreach (byte b in comp)
                            {
                                hash.Append(b.ToString("X2"));
                            }
                            // return hash as string
                            return hash.ToString();
                        }
                    }
                    // return 'exception'
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return "exception";
                    }
                // sha256 algorithm
                case algorithms.SHA256:
                    try
                    {
                        // open and handle file as buffered stream
                        using (FileStream fs = new FileStream(Path, FileMode.Open))
                        using (BufferedStream bs = new BufferedStream(fs))
                        using (SHA256Managed sha = new SHA256Managed())
                        {
                            // compute hash
                            var comp = sha.ComputeHash(bs);
                            // build string
                            var hash = new StringBuilder(comp.Length * 2);

                            foreach (byte b in comp)
                            {
                                hash.Append(b.ToString("X2"));
                            }
                            // return hash as string
                            return hash.ToString();
                        }
                    }
                    // return 'exception'
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return "exception";
                    }
                // sha384 algorithm
                case algorithms.SHA384:
                    try
                    {
                        // open and handle file as buffered stream
                        using (FileStream fs = new FileStream(Path, FileMode.Open))
                        using (BufferedStream bs = new BufferedStream(fs))
                        using (SHA384Managed sha = new SHA384Managed())
                        {
                            // compute hash
                            var comp = sha.ComputeHash(bs);
                            // build string
                            var hash = new StringBuilder(comp.Length * 2);
                            foreach (byte b in comp)
                            {
                                hash.Append(b.ToString("X2"));
                            }
                            // return hash as string
                            return hash.ToString();
                        }
                    }
                    // return 'exception'
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return "exception";
                    }
                // sha512 algorithm
                case algorithms.SHA512:
                    try
                    {
                        // open and handle file as buffered stream
                        using (FileStream fs = new FileStream(Path, FileMode.Open))
                        using (BufferedStream bs = new BufferedStream(fs))
                        using (SHA512Managed sha = new SHA512Managed())
                        {
                            // compute hash
                            var comp = sha.ComputeHash(bs);
                            // build string
                            var hash = new StringBuilder(comp.Length * 2);
                            foreach (byte b in comp)
                            {
                                hash.Append(b.ToString("X2"));
                            }
                            // return hash as string
                            return hash.ToString();
                        }
                    }
                    // return 'exception'
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return "exception";
                    }
            }
            // handle undefined request (shouldn't be possible)
            debugPrint("undefined");
            return "undefined";
        }
    }
}