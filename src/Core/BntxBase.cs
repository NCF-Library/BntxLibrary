#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using BntxLibrary.Common;
using BntxLibrary.Extensions;
using Syroot.BinaryData;
using Syroot.BinaryData.Core;

namespace BntxLibrary.Core
{
    public class BntxBase
    {
        //
        // Offsets

        internal ushort StringPoolOffset;
        internal uint FileNameOffset;
        internal uint RelocationTableOffset;
        internal long TextureArrayOffset;
        internal long StringDictionaryOffset;

        //
        // Meta data

        public string Magic = "BNTX";
        public uint FileSize;
        public uint Padding;
        public int TextureCount;
        public long TextureDataOffset;

        // 
        // Storage

        public StringPool StringPool { get; set; } = new();
        public RelocationTable RelocationTable { get; set; } = new();
        public TextureArray TextureArray { get; set; } = new();
        public StringDictionary StringDictionary { get; set; } = new();

        //
        // Public properties

        public Endian Endian { get; set; }
        public string Name { get; set; }
        public string Platform { get; set; }
        public string Version { get; set; }
        public uint Alignment { get; set; }
        public uint TargetAddressSize { get; set; }
        public ushort Flag { get; set; }

        public Dictionary<string, Texture> Textures { get; set; } = new();

        //
        // Serializer offset log

        internal Dictionary<string, (int, long)> OffsetLog = new();

        public BntxBase() { }
        public BntxBase(string name, Endian endian = Endian.Little)
        {
            Name = name;
            Endian = endian;
            Platform = "NX";
            Version = "0.4.0.0";
            // Textures : Load textures into a managed dictionary (or custom type), handle in/out in type functions, etc
        }

        public BntxBase Read(BinaryStream stream)
        {
            Magic = stream.ReadString(4);
            Padding = stream.ReadUInt32();
            Version = string.Join(".", stream.ReadBytes(4));
            Endian = stream.ReadEnum<Endian>();
            Alignment = stream.Read1Byte();
            TargetAddressSize = stream.Read1Byte();
            FileNameOffset = stream.ReadUInt32();
            Flag = stream.ReadUInt16();
            StringPoolOffset = stream.ReadUInt16();
            RelocationTableOffset = stream.ReadUInt32();
            FileSize = stream.ReadUInt32();
            Platform = stream.ReadString(4).Trim();
            TextureCount = stream.ReadInt32();
            TextureArrayOffset = stream.ReadInt64();
            TextureDataOffset = stream.ReadInt64();
            StringDictionaryOffset = stream.ReadInt64();

            StringPool.Read(stream, StringPoolOffset);
            RelocationTable.Read(stream, RelocationTableOffset);
            TextureArray.Read(stream, TextureCount, TextureArrayOffset);
            StringDictionary.Read(stream, StringDictionaryOffset);

            Name = StringPool[FileNameOffset];

            return this;
        }

        public byte[] Write()
        {
            using MemoryStream data = new();
            using BinaryStream stream = new(data);

            stream.Write(Magic);
            stream.Write(Padding);
            stream.Write(Version.Split('.').Select(x => (byte)char.Parse(x)));
            stream.Write(TargetAddressSize);
            stream.StashOffset(this, "FileNameOffset", (uint)0);
            stream.Write(Flag);
            stream.StashOffset(this, "StringPoolOffset", 0);
            stream.StashOffset(this, "RelocationTableOffset", 0L);
            stream.StashOffset(this, "FileSize", 0L);
            stream.Write(Platform.PadRight(4, ' '));
            stream.Write(Textures.Count); // May want to use single managed dict instead of TextureArray
            stream.StashOffset(this, "TextureArrayOffset", 0L); // Add a new RLT entry to section 1
            stream.StashOffset(this, "TextureDataOffset", 0L); // Add a new RLT entry to section 2
            stream.StashOffset(this, "StringDictionaryOffset", 0L);
            // Add a new RLT entry to section 1 (Memory Pool)
            stream.WriteUInt64(0x58);
            stream.Write(new ulong[2]); // Padding?
            // Orig:
            // stream.Write(0L);
            // stream.Write(0L);
            stream.Write(new byte[320]); // Space for memory pool
            // Add a new RLT entry to section 1 (Texture Array)

            // Write empty texture reference block
            stream.StashOffset(this, "TextureReferenceBlock", new byte[Textures.Count*8]); // May want to use single managed texture dict instead of TextureArray
            stream.Align(8);

            // Write StringPool
            stream.WriteOffset(this, "StringPoolOffset");
            StringPool.Write(stream, this);
            stream.Align(8);

            // Write String Dictionary
            stream.WriteOffset(this, "StringDictionaryOffset");
            StringDictionary.Write(stream, this);
            stream.Align(8);

            stream.WriteOffset(this, "TextureArrayOffset");
            TextureArray.Write(stream, this);

            stream.StashValue(this, "SectionOneSize");
            stream.Seek(16, SeekOrigin.Current);

            int alignment = MathExt.RoundUp((int)stream.Position, 1 << (int)Alignment) - (int)stream.Position;
            stream.Position += alignment != 0 ? alignment - 16 : 0;
            OffsetLog.Add("DataBlockPosition", (8, stream.Position));

            TextureData.Write(stream, this);

            stream.WriteOffset(this, "RelocationTableOffset");
            RelocationTable.Write(stream, this);

            return new byte[0];
        }
    }
}
