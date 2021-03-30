using System;
using System.Diagnostics;
using com.pigdawg.utils.FileSystem;

namespace com.pigdawg.Hasher
{
    public enum HashStatistics
    {
        None = 0,
        DuplicateHashes,
        AllHashes,
    }
    
    public class StatisticsManager
    {
        private HashStatistics m_hashStatistics;
        private DataFormatter m_formatter;

        private DateTime m_processingStarted;
        private TimeSpan m_runningTime;
        
        private ulong m_counterFilesProcessed;
        private ulong m_counterFileReparsePoints;
        private ulong m_counterFilesFailed;
        private ulong m_counterFilesRead;
        private ulong m_counterDirectoriesProcessed;
        private ulong m_counterDirectoryReparsePoints;
        private ulong m_counterDirectoriesFailed;
        private ulong m_counterDirectoriesRead;
        private ulong m_counterUnknownObjects;
        private ulong m_counterHashDuplicates;
        
        public StatisticsManager(HashStatistics hashStatistics, DataFormatter formatter)
        {
            m_hashStatistics = hashStatistics;
            m_formatter = formatter;
        }
        
        public ulong FilesProcessed {
            get {
                return this.m_counterFilesProcessed;
            }
        }

        public ulong FileReparsePoints {
            get {
                return this.m_counterFileReparsePoints;
            }
        }

        public ulong FilesFailed {
            get {
                return this.m_counterFilesFailed;
            }
        }

        public ulong FilesRead {
            get {
                return this.m_counterFilesRead;
            }
        }

        public ulong DirectoriesProcessed {
            get {
                return this.m_counterDirectoriesProcessed;
            }
        }

        public ulong DirectoryReparsePoints {
            get {
                return this.m_counterDirectoryReparsePoints;
            }
        }

        public ulong DirectoriesFailed {
            get {
                return this.m_counterDirectoriesFailed;
            }
        }

        public ulong DirectoriesRead {
            get {
                return this.m_counterDirectoriesRead;
            }
        }

        public ulong UnknownObjects {
            get {
                return this.m_counterUnknownObjects;
            }
        }
        
        public ulong HashDuplicates {
            get {
                return this.m_counterHashDuplicates;
            }
        }
        
        public TimeSpan RunningTime {
            get {
                return m_runningTime;
            }
        }
        
        public void DumpStats()
        {
            System.Console.Out.WriteLine("Running time:          {0}", this.RunningTime.ToString());
            System.Console.Out.WriteLine("Duplicate Hashes:      {0}", this.HashDuplicates);
            System.Console.Out.WriteLine("Unknown Objects:       {0}", this.UnknownObjects);
            System.Console.Out.WriteLine("Directories Processed: {0}", this.DirectoriesProcessed);
            System.Console.Out.WriteLine("    Read:         {0}", this.DirectoriesRead);
            System.Console.Out.WriteLine("    Failed:       {0}", this.DirectoriesFailed);
            System.Console.Out.WriteLine("    ReparsePoint: {0}", this.DirectoryReparsePoints);
            System.Console.Out.WriteLine("Files Processed:       {0}", this.FilesProcessed);
            System.Console.Out.WriteLine("    Read:         {0}", this.FilesRead);
            System.Console.Out.WriteLine("    Failed:       {0}", this.FilesFailed);
            System.Console.Out.WriteLine("    ReparsePoint: {0}", this.FileReparsePoints);
        }
        
        internal void HashCalculated(object sender, HashCalculatedEventArgs eventArgs)
        {
            if (HashStatistics.AllHashes == m_hashStatistics)
            {
                m_formatter.WriteData(eventArgs.Data);
            }
        }
        
        internal void HashDuplicateDetected(object sender, HashDuplicateDetectedEventArgs eventArgs)
        {
            m_counterHashDuplicates++;
            
            if (HashStatistics.DuplicateHashes == m_hashStatistics)
            {
                if (null != eventArgs.OriginalData)
                {
                    m_formatter.WriteData(eventArgs.OriginalData);
                }            
                
                m_formatter.WriteData(eventArgs.DuplicateData);
            }
        }
        
        internal void FileSystemWalkerDiagnosticDetected(object sender, FileSystemWalkerDiagnosticEventArgs eventArgs)
        {
            switch (eventArgs.Diagnostic)
            {
                case FileSystemWalkerDiagnostic.ProcessingStarted:
                    m_processingStarted = DateTime.UtcNow;
                    m_formatter.WriteHeader();
                    break;
                case FileSystemWalkerDiagnostic.FileProcessing:
                    m_counterFilesProcessed++;
                    break;
                case FileSystemWalkerDiagnostic.FileReparsePoint:
                    m_counterFileReparsePoints++;
                    break;
                case FileSystemWalkerDiagnostic.FileFailed:
                    m_counterFilesFailed++;
                    break;
                case FileSystemWalkerDiagnostic.FileRead:
                    m_counterFilesRead++;
                    break;
                case FileSystemWalkerDiagnostic.DirectoryProcessing:
                    m_counterDirectoriesProcessed++;
                    break;
                case FileSystemWalkerDiagnostic.DirectoryReparsePoint:
                    m_counterDirectoryReparsePoints++;
                    break;
                case FileSystemWalkerDiagnostic.DirectoryFailed:
                    m_counterDirectoriesFailed++;
                    break;
                case FileSystemWalkerDiagnostic.DirectoryRead:
                    m_counterDirectoriesRead++;
                    break;
                case FileSystemWalkerDiagnostic.UnknownObject:
                    m_counterUnknownObjects++;
                    break;
                case FileSystemWalkerDiagnostic.ProcessingCompleted:
                    m_runningTime = DateTime.UtcNow.Subtract(m_processingStarted);
                    break;
                default:
                    Debug.Fail("Unexpected value for eventArgs.Diagnostic: " + eventArgs.Diagnostic.ToString());
                    break;
            }
        }
    }
}
