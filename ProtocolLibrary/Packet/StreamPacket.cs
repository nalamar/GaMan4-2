
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ProtocolLibrary.Cryptography;

namespace ProtocolLibrary.Packet
{
    /// <summary>
    /// Represents a network packet based on TCP (Stream) in the protocol. Every packet consists
    /// of a fixed header with a size of 608-bit and the data section with a variable length
    /// between 0 and 2^32 byte.
    /// 
    /// The first byte shows the protocol version. The 8-bit flags field reserves space
    /// for additional functions, like a compression or encoding. A 16-bit checksum
    /// is a fixed-size datum computed from the data section for the purpose of detecting
    /// corruptions that may have been introduced during its creation or transmission over udp.
    /// Every protocol packet has a type, which is 32-bit long. The source and destination
    /// address are on the top of the tcp-protocol and are only for internal purposes
    /// like routing or easy message dispatching. Following a 32-bit long field
    /// with the data length in number of bytes. At the end the data section
    /// holds all the transfered data.
    /// 
    /// <pre>
    /// 
    ///    PACKET:
    ///   ____________ _______________ _______________ _______________________________
    ///  | Bit offset |     0 - 7     |    8 - 15     |           15 - 31             |
    ///  |____________|_______________|_______________|_______________________________|
    ///  |          0 |    Version    |     Flags     |          Checksum             |
    ///  |____________|_______________|_______________|_______________________________|
    ///  |         32 |                             Type                              |
    ///  |____________|_______________________________________________________________|
    ///  |         64 |                        Source Address                         |
    ///  |____________|_______________________________________________________________| 
    ///  |        320 |                      Destination Address                      |
    ///  |____________|_______________________________________________________________| 
    ///  |        576 |                        Payload Length                         |
    ///  |____________|_______________________________________________________________|
    ///  |       +608 |                                                               |
    ///  |            |                                                               |
    ///  |            |                             Data                              |
    ///  |            |                                                               |
    ///  |____________|_______________________________________________________________|
    ///
    /// </pre>
    /// 
    /// Description of the header flags:
    /// 
    /// 10000000 => Data encoding with Unicode (UTF-8).
    /// 01000000 => Data encoding with ASCII.
    /// 00100000 => -
    /// 00010000 => -
    /// 00001000 => Data encryption.
    /// 00000100 => Data compression.
    /// 00000010 => -
    /// 00000001 => -
    /// </summary>
    public sealed class StreamPacket : IPacket
    {
        /// <summary>
        /// Creates a new protocol packet, with a header section and no
        /// data. Some fields will be automatically set, like the protocol version.
        /// </summary>
        public StreamPacket()
        {
            ProtocolVersion = Version.PROTOCOL_VERSION;
            _flags = 0;
            Type = 0;
            Source = null;
            Destination = null;
            Encoding = null;
            Data = new byte[0];
        }

        /// <summary>
        /// Serializes the packet and returns the packed information
        /// to the caller.
        /// </summary>
        /// <exception cref="System.ArgumentException">Thrown when the packet was corrupt</exception>
        /// <returns>The serialized packet</returns>
        public byte[] Serialize()
        {
            // First check if this packet is corrupt
            if (Corrupted)
            {
                throw new ArgumentException("StreamPacket: Trying to serialize a corrupt packet.");
            }

            // Calculate the necessary packet size
            byte[] buffer = new byte[HEADER_BIT_SIZE / 8 + Data.Length];

            // ----------------------------------------------------
            // 1. Serialize the version
            // ----------------------------------------------------
            buffer[0] = ProtocolVersion;

            // ----------------------------------------------------
            // 2. Serialize the flags
            // ----------------------------------------------------
            buffer[1] = _flags;

            // ----------------------------------------------------
            // 3. Serialize the checksum
            // ----------------------------------------------------
            Array.Copy(BitConverter.GetBytes(_checksum), 0, buffer, 2, 2);

            // ----------------------------------------------------
            // 4. Serialize the type
            // ----------------------------------------------------
            Array.Copy(BitConverter.GetBytes((int)Type), 0, buffer, 4, 4);

            // ----------------------------------------------------
            // 5. Serialize the source address
            // ----------------------------------------------------
            Array.Copy(AddrStructureConvert.GetBytes(Source), 0, buffer, 8, 32);

            // ----------------------------------------------------
            // 6. Serialize the destination address
            // ----------------------------------------------------
            Array.Copy(AddrStructureConvert.GetBytes(Destination), 0, buffer, 40, 32);

            // ----------------------------------------------------
            // 7. Serialize the size of the payload
            // ----------------------------------------------------
            Array.Copy(BitConverter.GetBytes(Data.Length), 0, buffer, 72, 4);

            // ----------------------------------------------------
            // 8. Serialize the data
            // ----------------------------------------------------
            Array.Copy(Data, 0, buffer, 76, Data.Length);

            return buffer;
        }

        /// <summary>
        /// Deserialize a new message from a given stream. Rethrows an IOException or 
        /// ArgumentException to the caller.
        /// </summary>
        /// <exception cref="System.ArgumentException">Thrown when the argument was invalid</exception>
        /// <exception cref="System.IO.IOException">Thrown when the I/O failed</exception>
        /// <param name="stream"></param>
        public void Deserialize(Stream stream)
        {
            try
            {

                byte[] buffer = new byte[32];

                // ----------------------------------------------------
                // 1. Read the version
                // ----------------------------------------------------
                int bytes = stream.Read(buffer, 0, 1);
                if (bytes != 1)
                {
                    return;
                }

                ProtocolVersion = buffer[0];

                // ----------------------------------------------------
                // 2. Read the flags
                // ----------------------------------------------------
                bytes = stream.Read(buffer, 0, 1);
                if (bytes != 1)
                {
                    return;
                }

                _flags = buffer[0];

                // ----------------------------------------------------
                // 3. Read the checksum
                // ----------------------------------------------------
                bytes = stream.Read(buffer, 0, 2);
                if (bytes != 2)
                {
                    return;
                }

                _checksum = BitConverter.ToUInt16(buffer, 0);

                // ----------------------------------------------------
                // 4. Read the type
                // ----------------------------------------------------
                bytes = stream.Read(buffer, 0, 4);
                if (bytes != 4)
                {
                    return;
                }

                Type = (PacketType)Enum.ToObject(typeof(PacketType), BitConverter.ToUInt32(buffer, 0));

                bool blnIsExist = Enum.IsDefined(typeof(PacketType), Type);

                if (!blnIsExist)
                    return;

                // ----------------------------------------------------
                // 5. Read the source address
                // ----------------------------------------------------
                bytes = stream.Read(buffer, 0, 32);
                if (bytes != 32)
                {
                    return;
                }

                Source = AddrStructureConvert.GetEndPoint(buffer);

                // ----------------------------------------------------
                // 6. Read the destination address
                // ----------------------------------------------------
                bytes = stream.Read(buffer, 0, 32);
                if (bytes != 32)
                {
                    return;
                }

                Destination = AddrStructureConvert.GetEndPoint(buffer);

                // ----------------------------------------------------
                // 7. Read the size of the payload
                // ----------------------------------------------------
                bytes = stream.Read(buffer, 0, 4);
                if (bytes != 4)
                {
                    return;
                }

                int size = BitConverter.ToInt32(buffer, 0);

                // ----------------------------------------------------
                // 8. Read the data
                // ----------------------------------------------------
                _data = new byte[size];
                if (size != 0)
                {
                    bytes = stream.Read(_data, 0, size);
                    if (bytes != size)
                    {
                        return;
                    }
                }

            }
            catch
            {
                throw;  // Rethrow any exception
            }
        }

        /// <summary>
        /// Creates a formatted packet and returns it as a string
        /// representation.
        /// </summary>
        /// <returns>The packet as a string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(200 + Data.Length);

            // Create the header
            sb.AppendFormat("\nProtocol Version (8 bit) as Integer: {0}\n", ProtocolVersion);
            sb.AppendFormat("Flags (8 bit) as Bit Sequence: {0}\n", Convert.ToString(_flags, 2));
            sb.AppendFormat("Checksum (16 bit) as Integer: {0}\n", _checksum);
            sb.AppendFormat("Type (32 bit) as ProtocolType: {0}\n", Type.ToString("G"));
            sb.AppendFormat("Source Address (256 bit) as IPEndPoint: {0}\n", Source);
            sb.AppendFormat("Destination Address (256 bit) as IPEndPoint: {0}\n", Destination);
            sb.AppendFormat("Payload Length (32 bit) as Integer: {0}\n", Data.Length);
            sb.AppendFormat("Data ({0} byte) as Hex Sequence: {1}\n\n", Data.Length,    // Append data
                BitConverter.ToString(Data).Replace("-", ""));

            return sb.ToString();
        }

        /// <summary>
        /// This is a simple method for computing the 16-bit one's complement
        /// checksum of a byte buffer. The byte buffer will be padded with
        /// a zero byte if an uneven number.
        /// </summary>
        /// <returns>Computeted checksum</returns>
        private ushort ComputeChecksum()
        {
            uint xsum = 0;
            ushort shortval = 0, hiword = 0, loword = 0;

            // Sum up the 16-bits
            for (int i = 0; i < _data.Length / 2; i++)
            {
                hiword = (ushort)(((ushort)_data[i * 2]) << 8);
                loword = (ushort)_data[(i * 2) + 1];
                shortval = (ushort)(hiword | loword);
                xsum = xsum + (uint)shortval;
            }

            // Pad if necessary
            if ((_data.Length % 2) != 0)
            {
                xsum += (uint)_data[_data.Length - 1];
            }

            xsum = ((xsum >> 16) + (xsum & 0xFFFF));
            xsum = (xsum + (xsum >> 16));
            shortval = (ushort)(~xsum);
            return shortval;
        }

        #region Getter/Setter

        /// <summary>
        /// Sets or gets the compression of the data. If compression
        /// is set to true, the data section will be automatically compressed.
        /// This is done within the packet class.
        /// </summary>
        public bool Compressed
        {
            get
            {
                return (_flags & COMPRESSION_ON_MASK) == COMPRESSION_ON_MASK ? true : false;
            }

            set
            {
                if (value)
                {
                    // Compress data if not already compressed
                    if (!Compressed && Data != null)
                    {
                        Data = GZip.Compress(Data);
                        _flags |= COMPRESSION_ON_MASK;
                    }
                }
                else
                {
                    // Check if data was compressed, de-compress if necessary
                    if (Compressed && Data != null && !Encrypted)
                    {
                        Data = GZip.Decompress(Data);
                        _flags &= COMPRESSION_OFF_MASK;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the encryption of the data. If encryption
        /// is set to true and supported by the packet class, the data 
        /// section will be automatically encrypted. For the symmetric-key
        /// encryption a non-zero length password must be set.
        /// </summary>
        public bool Encrypted
        {
            get
            {
                return (_flags & ENCRYPTION_ON_MASK) == ENCRYPTION_ON_MASK ? true : false;
            }

            set
            {
                if (value)
                {
                    // Encrypt data if not already encrypted
                    if (!Encrypted && Data != null && Password != null && Password.Length > 0)
                    {
                        AES aes = new AES(AES.KeySize.Bits128, Password);
                        Data = aes.Encrypt(Data);
                        _flags |= ENCRYPTION_ON_MASK;
                    }
                }
                else
                {
                    // Check if data was encrypted, decrypt if necessary
                    if (Encrypted && Data != null && Password != null && Password.Length > 0)
                    {
                        AES aes = new AES(AES.KeySize.Bits128, Password);
                        Data = aes.Decrypt(Data);
                        _flags &= ENCRYPTION_OFF_MASK;
                    }
                }
            }
        }

        /// <summary>
        /// Sets or gets the encoding of the payload (data). Returns null
        /// if no character encoding was set. The caller is
        /// responsible for the right encoding.
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                if ((_flags & ENCODING_UNICODE_ON_MASK) == ENCODING_UNICODE_ON_MASK)
                    return Encoding.Unicode;
                else if ((_flags & ENCODING_ASCII_ON_MASK) == ENCODING_ASCII_ON_MASK)
                    return Encoding.ASCII;
                else
                    return null;
            }

            set
            {
                if (value == Encoding.Unicode)
                {
                    _flags |= ENCODING_UNICODE_ON_MASK;
                }
                else if (value == Encoding.ASCII)
                {
                    _flags |= ENCODING_ASCII_ON_MASK;
                }
                else
                {
                    _flags &= ENCODING_OFF_MASK;    // No encoding
                }
            }
        }

        /// <summary>
        /// Gets or sets the packet data.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
                if (value != null)
                {
                    _checksum = ComputeChecksum();
                }
            }
        }

        /// <summary>
        /// Indicates, if this packet is corrupted.
        /// </summary>
        public bool Corrupted
        {
            get
            {
                if (Data == null || Type == 0 || Source == null || Destination == null ||
                    _checksum != ComputeChecksum())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the packet data delimiter.
        /// </summary>
        public string Delimiter
        {
            get
            {
                return DELIMITER;
            }
        }

        /// <summary>
        /// Gets or sets a password for the encryption of the packet 
        /// data, if encryption is supported.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets the protocol version.
        /// </summary>
        public byte ProtocolVersion { get; private set; }

        /// <summary>
        /// Gets or sets the mesage type.
        /// </summary>
        public PacketType Type { get; set; }

        /// <summary>
        /// Gets or sets the source address.
        /// </summary>
        public IPEndPoint Source { get; set; }

        /// <summary>
        /// Gets or sets the destination address.
        /// </summary>
        public IPEndPoint Destination { get; set; }

        #endregion

        #region Fields

        /// <summary>
        /// The data array. Holds all packet data.
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Gets the header flags.
        /// </summary>
        private byte _flags;

        /// <summary>
        /// The checksum of the data.
        /// </summary>
        private ushort _checksum;

        /// <summary>
        /// Size of a header in bits.
        /// </summary>
        private const ushort HEADER_BIT_SIZE = 608;

        /// <summary>
        /// The protocol delimiter
        /// </summary>
        private const string DELIMITER = "~###~";

        #endregion

        #region Bit Masks

        private const byte ENCODING_UNICODE_ON_MASK = 0x80;    // 1000 0000
        private const byte ENCODING_ASCII_ON_MASK = 0x40;  // 0100 0000
        private const byte ENCODING_OFF_MASK = 0x3f;    // 0011 1111
        private const byte COMPRESSION_ON_MASK = 0x4;   // 0000 0100
        private const byte COMPRESSION_OFF_MASK = 0xfb; // 1111 1011
        private const byte ENCRYPTION_ON_MASK = 0x8;    // 0000 1000
        private const byte ENCRYPTION_OFF_MASK = 0xf7;  // 1111 0111

        #endregion
    }
}
