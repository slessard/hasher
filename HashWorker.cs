using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace com.pigdawg.Hasher
{
    public enum HashingAlgorithm
    {
        None = 0,
        SHA256,
        MD5,
    }

    public class HashWorker
    {
        private readonly HashAlgorithm m_hashAlgorithm = new SHA256Managed ();

        public HashWorker (HashAlgorithm hashAlgorithm)
        {
            m_hashAlgorithm = hashAlgorithm;
        }

        public FileProperties ProcessFile (FileInfo fileInfo)
        {
            Debug.Assert((null != fileInfo), "fileInfo should not be null");
            Debug.Assert(fileInfo.Exists, "File does not exist");
            
            byte[] hashValue = null;
            
            using (FileStream inStream = fileInfo.OpenRead())
            {
                hashValue = m_hashAlgorithm.ComputeHash(inStream);
            }
            
            return new FileProperties(fileInfo, hashValue);
        }
    }
}
