using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using com.pigdawg.utils.FileSystem;

namespace com.pigdawg.Hasher
{
    public static class Global
    {
        private static class EnvironmentVariable
        {
            public const string DumpExceptionStack = "DUMP_EXECPTION_STACK";
        }
        
        private static readonly bool s_dumpExceptionStack;
        
        static Global()
        {
            string dumpExceptionStack = Environment.GetEnvironmentVariable(EnvironmentVariable.DumpExceptionStack);
            
            // The actual value of the environment variable is insignificant.
            // If the variable exists and has any value it is considered to be "on"
            s_dumpExceptionStack = !string.IsNullOrEmpty(dumpExceptionStack);
        }
        
        public static bool DumpExceptionStack
        {
            get
            {
                return s_dumpExceptionStack;
            }
        }
    }
    
    public enum ReportMode
    {
        None = 0,
        DuplicateHashes,
        AllHashes,
    }
    
    [Flags]
    public enum ReportColumns
    {
        Size =          0x01,
        HashValue =     0x02,
        FileName =      0x04,
        DirectoryName = 0x08,
        Attributes =    0x10,
        Created =       0x20,
        Modified =      0x40,
        Accessed =      0x80,
    }
    
    public enum InputProcessingMode
    {
        None = 0,
        DirectSource,
        FileList,
    }
    
    public struct OperatingContext
    {
        private ReportMode m_reportMode;
        private ReportColumns m_reportColumns;
        private HashingAlgorithm m_hashingAlgorithm;
        private DataFormatter.DataColumns m_dataColumns;
        private FileSystemInfo m_inputPath;
        private FileInfo m_outputFile;
        private bool m_helpRequested;
        

        public ReportMode ReportMode
        {
            get
            {
                return m_reportMode;
            }
            set
            {
                m_reportMode = value;
            }
        }

        public ReportColumns ReportColumns
        {
            get
            {
                return m_reportColumns;
            }
            set
            {
                m_reportColumns = value;
            }
        }

        public HashingAlgorithm HashingAlgorithm
        {
            get
            {
                return m_hashingAlgorithm;
            }
            set
            {
                m_hashingAlgorithm = value;
            }
        }

        public DataFormatter.DataColumns DataColumns
        {
            get
            {
                return m_dataColumns;
            }
            set
            {
                m_dataColumns = value;
            }
        }

        public FileSystemInfo InputPath
        {
            get
            {
                return m_inputPath;
            }
            set
            {
                m_inputPath = value;
            }
        }

        public FileInfo OutputFile
        {
            get
            {
                return m_outputFile;
            }
            set
            {
                m_outputFile = value;
            }
        }

        public bool HelpRequested
        {
            get
            {
                return m_helpRequested;
            }
            set
            {
                m_helpRequested = value;
            }
        }
    }
    
    public static class Helper
    {
        public static string HashValueToString(byte[] hashValue)
        {
            return BitConverter.ToString(hashValue).Replace("-", "");
//            StringBuilder sb = new StringBuilder();
//            Array.ForEach<byte>(hashValue, b => { sb.Append(b.ToString("X")); });
//            return sb.ToString();
        }
    }

    
    public class MainClass
    {
        public const string HelpMessage = @"
Hasher [-all | -dupes] [-md5 | -sha256] path [outputfile]

    -all         Show the hash values for all files.
    -dupes       Show the hash values for duplicate files only. (Default)
    -md5         Calculate the hash value using the MD5 algorithm
    -sha256      Calculate the hash value using the SHA256 algorithm. (Default)
    path         The path to either a file or a directory to hash.
    outputfile   The path to a file that will contain the hash results.
                 If not specified the output is sent to the console window.
";
        
        public static int Main (string[] args)
        {
            //DumpArray(args);
                    
            try
            {
                //
                // Parse arguments
                //
                
                OperatingContext operatingContext; // A struct needs no initialization
                
                try
                {
                    operatingContext = ParseArguments(args);
                }
                catch (ArgumentParsingException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    if (Global.DumpExceptionStack)
                    {
                        Console.Error.WriteLine(ex.StackTrace);
                    }
                    return ex.ErrorCode;
                }
                
                if (operatingContext.HelpRequested)
                {
                    Console.Out.WriteLine(MainClass.HelpMessage);
                    return 0;
                }
                
                // If no statistics option was specified as an argument
                // use the default value
                if (ReportMode.None == operatingContext.ReportMode)
                {
                    operatingContext.ReportMode = ReportMode.DuplicateHashes;
                }
                
                // If no hashing algorithm option was specified as an argument
                // use the default value
                if (HashingAlgorithm.None == operatingContext.HashingAlgorithm)
                {
                    operatingContext.HashingAlgorithm = HashingAlgorithm.SHA256;
                }
                
                
                //
                // Validate arguments
                //

                TextWriter outputWriter = null;

                if (null == operatingContext.InputPath)
                {
                    Console.Error.WriteLine("Invalid argument: No onput file specified.");
                    Console.Error.WriteLine(MainClass.HelpMessage);
                    return 2;
                }

                if (null == operatingContext.OutputFile)
                {
                    outputWriter = System.Console.Out;
                }
                else
                {
                    outputWriter = new StreamWriter(operatingContext.OutputFile.FullName, true /*append*/);
                }
                
                using (outputWriter)
                {
                    Program prog = new Program();
                    prog.Run(operatingContext, outputWriter);
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Unexpected error: " + ex.ToString());
                return 1;
            }
        }
        
        private static OperatingContext ParseArguments (string[] args)
        {
            OperatingContext operatingContext = new OperatingContext();
                        
            operatingContext.ReportMode = ReportMode.None;
            operatingContext.ReportColumns = default(ReportColumns);
            operatingContext.HashingAlgorithm = HashingAlgorithm.None;
            operatingContext.DataColumns = ((DataFormatter.DataColumns)(-1));
            operatingContext.InputPath = null;
            operatingContext.OutputFile = null;
            
            foreach (string arg in args)
            {
                if ((arg.Trim().Equals("/all", StringComparison.OrdinalIgnoreCase)) ||
                    (arg.Trim().Equals("-all", StringComparison.OrdinalIgnoreCase)))
                {
                    if (ReportMode.DuplicateHashes != operatingContext.ReportMode)
                    {
                        operatingContext.ReportMode = ReportMode.AllHashes;
                    }
                    else
                    {
                        string message = string.Format("Invalid argument: '{0}' option cannot be combined with argument '-dupes'.", arg.Trim());

                        throw new ArgumentParsingException(message, "args", 2);
                    }
                }
                else if ((arg.Trim().Equals("/dupes", StringComparison.OrdinalIgnoreCase)) ||
                         (arg.Trim().Equals("-dupes", StringComparison.OrdinalIgnoreCase)))
                {
                    if (ReportMode.AllHashes != operatingContext.ReportMode)
                    {
                        operatingContext.ReportMode = ReportMode.DuplicateHashes;
                    }
                    else
                    {
                        string message = string.Format("Invalid argument: '{0}' option cannot be combined with argument '-all'.", arg.Trim());

                        throw new ArgumentParsingException(message, "args", 2);
                    }
                }
                else if ((arg.Trim().Equals("/md5", StringComparison.OrdinalIgnoreCase)) ||
                         (arg.Trim().Equals("-md5", StringComparison.OrdinalIgnoreCase)))
                {
                    if (HashingAlgorithm.SHA256 != operatingContext.HashingAlgorithm)
                    {
                        operatingContext.HashingAlgorithm = HashingAlgorithm.MD5;
                    }
                    else
                    {
                        string message = string.Format("Invalid argument: '{0}' option cannot be combined with argument '-sha256'.", arg.Trim());
                        
                        throw new ArgumentParsingException(message, "args", 2);
                    }
                }
                else if ((arg.Trim().Equals("/sha256", StringComparison.OrdinalIgnoreCase)) ||
                         (arg.Trim().Equals("-sha256", StringComparison.OrdinalIgnoreCase)))
                {
                    if (HashingAlgorithm.MD5 != operatingContext.HashingAlgorithm)
                    {
                        operatingContext.HashingAlgorithm = HashingAlgorithm.SHA256;
                    }
                    else
                    {
                        string message = string.Format("Invalid argument: '{0}' option cannot be combined with argument '-md5'.", arg.Trim());
                        
                        throw new ArgumentParsingException(message, "args", 2);
                    }
                }
                else if ((arg.Trim().Equals("/?", StringComparison.OrdinalIgnoreCase)) ||
                         (arg.Trim().Equals("-?", StringComparison.OrdinalIgnoreCase)))
                {
                    operatingContext.HelpRequested = true;
                    break;
                }
                else if (null == operatingContext.InputPath)
                {
                    string inputFile = arg.Trim();
                    Debug.Assert(!inputFile.StartsWith("-", false/*ignoreCase*/, CultureInfo.InvariantCulture),
                                 "Disallowed character in file name: " + inputFile);
                    
                    // Check to see if the inputFile is a file or directory
                    // and check to see whether that file or directory exists
                    if (Directory.Exists(inputFile))
                    {
                        operatingContext.InputPath = new DirectoryInfo(inputFile);
                    }
                    else if (File.Exists(inputFile))
                    {
                        operatingContext.InputPath = new FileInfo(inputFile);
                    }
                    else
                    {
                        string message = string.Format("Invalid argument: Input path does not exist: '{0}'", inputFile);
                        
                        throw new ArgumentParsingException(message, "args", 2);
                    }
                }
                else if (null == operatingContext.OutputFile)
                {
                    string outputFile = arg.Trim(); 
                    Debug.Assert(!outputFile.StartsWith("-", false/*ignoreCase*/, CultureInfo.InvariantCulture),
                                 "Disallowed character in file name: " + outputFile);
                    
                    FileInfo fi = new FileInfo(arg);
                    
                    if (fi.Exists)
                    {
                        string message = string.Format("ERROR: Output file already exists: '{0}'", fi.FullName);
                        
                        throw new ArgumentParsingException(message, "args", 2);
                    }
                    else
                    {
                        operatingContext.OutputFile = fi;
                    }
                }
                else
                {
                    string message = string.Format("Unknown argument: '{0}'", arg.Trim());
                    
                    throw new ArgumentParsingException(message, "args", 2);
                }
            }
            return operatingContext;
        }

        private static void DumpArray (string[] args)
        {
            for (int count = 0; count < args.Length; count++)
            {
                Console.Error.WriteLine("args[{0}]: '{1}'", count, args[count]);
            }
        }
    }
    
    internal class Program
    {
        private StatisticsManager statsManager;
        private FileSystemWalker fsWalker;
        private DataTracker dataTracker;
        private HashWorker hashWorker;
        
        
        /// <summary>
        /// Performs the actual file system traversal and hashing of each file found
        /// </summary>
        /// <param name="operatingContext">A bag of properties describing how the program should behave.</param>
        /// <param name="outputWriter">The writer to which all output will be written</param>
        internal void Run(OperatingContext operatingContext, TextWriter outputWriter)
        {
            HashAlgorithm hashAlgorithm = null;
            FileSystemWalkerSettings fsWalkerSettings = null;
            
            dataTracker = new DataTracker();
            hashAlgorithm = Program.GetHashingEngine(operatingContext.HashingAlgorithm);
            
            //TODO: replace this ugly static cast with an implicit cast operator
            statsManager = new StatisticsManager (((HashStatistics)(operatingContext.ReportMode)),
                                                 new DataFormatter (outputWriter));
            hashWorker = new HashWorker(hashAlgorithm);
            fsWalkerSettings = new FileSystemWalkerSettings();
            fsWalkerSettings.FollowDirectorySymLinks = false;
            fsWalkerSettings.FollowFileSymLinks = false;
            fsWalkerSettings.RecurseDirectories = true;
            fsWalker = new FileSystemWalker(fsWalkerSettings);
            
            fsWalker.FileFound += this.FileSystemWalker_FileFound;
            fsWalker.DiagnosticDetected += this.FileSystemWalker_DiagnosticDetected;
            dataTracker.HashCalculated += this.DataTracker_HashCalculated;
            dataTracker.HashDuplicateDetected += this.DataTracker_HashDuplicateDetected;
            
            fsWalker.Run(operatingContext.InputPath);
            
            statsManager.DumpStats();
        }
  
        private void FileSystemWalker_FileFound(object sender, FileFoundEventArgs eventArgs)
        {
            FileProperties properties = hashWorker.ProcessFile(eventArgs.FileInfo);

            dataTracker.Add(properties.FileInfo.Length,
                            properties.HashValue,
                            properties.FileInfo.Name,
                            properties.FileInfo.DirectoryName,
                            properties.FileInfo.Attributes,
                            properties.FileInfo.CreationTime,
                            properties.FileInfo.LastWriteTime,
                            properties.FileInfo.LastAccessTime);
        }

        private void FileSystemWalker_DiagnosticDetected(object sender, FileSystemWalkerDiagnosticEventArgs eventArgs)
        {
            statsManager.FileSystemWalkerDiagnosticDetected(sender, eventArgs);
        }

        private void DataTracker_HashCalculated(object sender, HashCalculatedEventArgs eventArgs)
        {
            statsManager.HashCalculated(sender, eventArgs);
        }

        private void DataTracker_HashDuplicateDetected(object sender, HashDuplicateDetectedEventArgs eventArgs)
        {
            statsManager.HashDuplicateDetected(sender, eventArgs);
        }
        
        private static HashAlgorithm GetHashingEngine(HashingAlgorithm hashingAlgorithm)
        {
            HashAlgorithm engine = null;
            
            switch (hashingAlgorithm)
            {
                case HashingAlgorithm.MD5:
                    engine = MD5.Create();
//                   impl = new MD5Managed();
                    break;
                case HashingAlgorithm.SHA256:
                    engine = new SHA256Managed();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("hashingAlgorithm", "Unexpected value for this enum. Did you add a new value?");
            }
            
            return engine;
        }
    }
}
