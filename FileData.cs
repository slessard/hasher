using System;
using System.IO;

namespace com.pigdawg.Hasher
{
    public class FileProperties
    {
        private FileInfo m_fileInfo;
        private byte[] m_hashValue;

        public FileProperties (FileInfo fileInfo, byte[] hashValue)
        {
            m_fileInfo = fileInfo;
            m_hashValue = hashValue;
        }

        public FileInfo FileInfo
        {
            get
            {
                return m_fileInfo;
            }
        }

        public byte[] HashValue
        {
            get
            {
                return m_hashValue;
            }
        }
    }

    public class FileData
    {
        long m_size;
        byte[] m_hashValue;
        string m_fileName;
        string m_directoryName;
        FileAttributes m_attributes;
        DateTime m_created;
        DateTime m_modified;
        DateTime m_accessed;
            
        public FileData(long size, byte[] hashValue, string fileName,
                        string directoryName, FileAttributes attributes,
                        DateTime created, DateTime modified, DateTime accessed)
        {
            m_size = size;
            m_hashValue = hashValue;
            m_fileName = fileName;
            m_directoryName = directoryName;
            m_attributes = attributes;
            m_created = created;
            m_modified = modified;
            m_accessed = accessed;
        }
        
        public long Length {
            get {
                return this.m_size;
            }
        }

        public byte[] HashValue {
            get {
                return this.m_hashValue;
            }
        }

        public string Name {
            get {
                return this.m_fileName;
            }
        }

        public string DirectoryName {
            get {
                return this.m_directoryName;
            }
        }

        public FileAttributes Attributes {
            get {
                return this.m_attributes;
            }
        }

        public DateTime CreationTime {
            get {
                return this.m_created;
            }
        }

        public DateTime LastWriteTime {
            get {
                return this.m_modified;
            }
        }

        public DateTime LastAccessTime {
            get {
                return this.m_accessed;
            }
        }

        public override string ToString ()
        {
//            directoryName = string.Concat("\"", fileInfo.DirectoryName.ToString(), "\"");
//            fileName = string.Concat("\"", fileInfo.Name.ToString(), "\"");
//            attributes = string.Concat("\"", fileInfo.Attributes.ToString(), "\"");
//            length = fileInfo.Length.ToString();
//            created = fileInfo.CreationTime.ToString();
//            modified = fileInfo.LastWriteTime.ToString();
//            accessed = fileInfo.LastAccessTime.ToString();

            return string.Format("[FileData: Size={0}, HashValue={1}, FileName={2}, DirectoryName={3}, Attributes={4}, Created={5}, Modified={6}, Accessed={7}]",
                                 Length, HashValue, Name, DirectoryName, Attributes, CreationTime, LastWriteTime, LastAccessTime);
        }

    }
}
