using BntxLibrary.Core;
using Syroot.BinaryData;
using Syroot.BinaryData.Core;

namespace BntxLibrary
{
    /// <summary>
    /// Public functions for basic interfacing
    /// </summary>
    public class BntxFile : BntxBase
    {
        public BntxFile() { }
        public BntxFile(Stream stream) => Read(new BinaryStream(stream));
        public BntxFile(string file) => Read(new BinaryStream(File.OpenRead(file)));
        public BntxFile(byte[] data) => Read(new BinaryStream(new MemoryStream(data)));
        public BntxFile(string name, Endian endianess) : base(name, endianess) { }

        public static BntxBase FromBinary(string file) => new BntxFile(new BinaryStream(File.OpenRead(file)));
        public static BntxBase FromBinary(Stream stream) => new BntxFile(new BinaryStream(stream));
        public static BntxBase FromBinary(byte[] data) => new BntxFile(new BinaryStream(new MemoryStream(data)));

        public byte[] ToBinary() => Write();
    }
}
