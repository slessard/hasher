using System;
using System.Collections.Generic;
using System.IO;

namespace com.pigdawg.Hasher
{
    public class HashCalculatedEventArgs : EventArgs
    {
        private FileData m_data;
        
        /// <summary>
        /// Create and populate an instance of HashCalculatedEventArgs
        /// </summary>
        /// <param name="data">
        /// The <see cref="FileData"/> from the file with this hash value.
        /// </param>
        internal HashCalculatedEventArgs(FileData data)
        {
            if (null == data)
            {
                throw new ArgumentNullException("data");
            }
            
            m_data = data;
        }
        
        public FileData Data {
            get {
                return this.m_data;
            }
        }
    }

    public class HashDuplicateDetectedEventArgs : EventArgs
    {
        private FileData m_duplicateData;
        private FileData m_originalData;
        
        /// <summary>
        /// Create and populate an instance of HashDuplicateDetectedEventArgs
        /// </summary>
        /// <param name="duplicateData">
        /// The <see cref="FileData"/> from the file most recently detected with this hash value
        /// </param>
        /// <param name="originalData">
        /// The <see cref="FileData"/> from the first file detected with this hash value. Can be null.
        /// </param>
        internal HashDuplicateDetectedEventArgs(FileData duplicateData, FileData originalData)
        {
            if (null == duplicateData)
            {
                throw new ArgumentNullException("duplicateData");
            }
            
            m_duplicateData = duplicateData;
            m_originalData = originalData;
        }
        
        public FileData DuplicateData {
            get {
                return this.m_duplicateData;
            }
        }

        public FileData OriginalData {
            get {
                return this.m_originalData;
            }
        }
    }

    public class DataTracker
    {
        private Dictionary<string, FileData> m_hashes = new Dictionary<string, FileData>();
        
        public event EventHandler<HashCalculatedEventArgs> HashCalculated;
        public event EventHandler<HashDuplicateDetectedEventArgs> HashDuplicateDetected;
        
        public DataTracker ()
        {
        }

        public void Add (long size, byte[] hashValue, string fileName,
                         string directoryName, FileAttributes attributes,
                         DateTime created, DateTime modified, DateTime accessed)
        {
            FileData data = new FileData(size, hashValue, fileName, directoryName, attributes, created, modified, accessed);
            
            this.Add(data);
        }
        
        public void Add (FileData data)
        {
            string hashValue = Helper.HashValueToString(data.HashValue);
            FileData existingData = null;
            bool isDuplicate = false;
            
            isDuplicate = (m_hashes.TryGetValue(hashValue, out existingData));
                
            if (!isDuplicate)
            {
                m_hashes[hashValue] = data;
            }
            RaiseHashCalculatedEvent(data);

            if (isDuplicate)
            {
                if (null != existingData)
                {
                    // TODO: Comment on why the hash value is removed from the dictionary
                    // and what the significance is.
                    m_hashes[hashValue] = null;
                }
                
                RaiseHashDuplicateDetectedEvent(data, existingData);
            }
        }
        
        private void RaiseHashCalculatedEvent (FileData data)
        {
            EventHandler<HashCalculatedEventArgs> eventListeners = this.HashCalculated;
            if (null != eventListeners)
            {
                HashCalculatedEventArgs eventArgs = new HashCalculatedEventArgs(data);
                eventListeners(this, eventArgs);
            }
        }
        
        private void RaiseHashDuplicateDetectedEvent (FileData data, FileData existingData)
        {
            EventHandler<HashDuplicateDetectedEventArgs> eventListeners = this.HashDuplicateDetected;
            if (null != eventListeners)
            {
                HashDuplicateDetectedEventArgs eventArgs = new HashDuplicateDetectedEventArgs(data, existingData);
                eventListeners(this, eventArgs);
            }
        }
    }
}
