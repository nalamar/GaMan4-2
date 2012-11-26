using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolLibrary.Cryptography
{
    /// <summary>
    /// A simple compression class in the protocol to reduce packet sizes
    /// and network traffic. Do not use this on small packets, it will
    /// not be efficient.
    /// </summary>
    public class GZip
    {
        /// <summary>
        /// Compresses a raw byte array.
        /// </summary>
        /// <param name="raw">The raw data, which should be compressed</param>
        /// <returns>Returns the compressed byte array</returns>
        public static byte[] Compress(byte[] raw)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gzipStream.Write(raw, 0, raw.Length);
                }
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Decompresses a compressed byte array in the protocol.
        /// </summary>
        /// <param name="gzip">A zipped byte array</param>
        /// <returns>Returns the decompressed byte array</returns>
        public static byte[] Decompress(byte[] gzip)
        {
            const int size = 1024;
            // Create a buffer and write into while reading from the GZIP stream.
            byte[] buffer = new byte[size];
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
                {
                    int count = 0;
                    do
                    {
                        count = gzipStream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memoryStream.Write(buffer, 0, count);
                        }
                    } while (count > 0);
                }
                return memoryStream.ToArray();
            }
        }
    }
}
