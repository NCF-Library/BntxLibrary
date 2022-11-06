using BntxLibrary.Core;
using BntxLibrary.Enums;
using BntxLibrary.Extensions;
using Syroot.BinaryData;

namespace BntxLibrary.Common
{
    public class Texture
    {
        //
        // Serializer storage
        public long[] SerdeMipOffsets = Array.Empty<long>();

        //
        // Fields

        public StringDictionary StringDictionary = new();
        public int ReadTextureLayout;
        public int SparseBinding;
        public int SparseResidency;
        public List<List<byte[]>> TextureData = new();
        public long DescSlotDataOffset;
        public long ParentOffset;
        public long PtrOffset;
        public long BlockSize;
        public long TexPtr;
        public long TexView;
        public long UserDataOffset;
        public long[] MipOffsets = Array.Empty<long>();
        public uint ArrayLength;
        public uint BlockHeightLog2;
        public uint ChannelType;
        public uint NextBlockOffset;
        public uint TextureLayout;
        public uint TextureLayout2;

        public AccessFlags AccessFlags { get; set; }
        public bool UseSRGB { get; set; }
        public string? Name { get; set; }
        public SurfaceFormat Format { get; set; }
        public uint Height { get; set; }
        public uint MipCount { get; set; }
        public uint Width { get; set; }

        public byte Flags { get; set; }
        public Dim Dim { get; set; }
        public int Alignment { get; set; }
        public SurfaceDim SurfaceDim { get; set; }
        public TileMode TileMode { get; set; }
        public uint Depth { get; set; }
        public uint ImageSize { get; set; }
        public uint SampleCount { get; set; }
        public uint Swizzle { get; set; }

        public ChannelType ChannelAlpha { get; set; }
        public ChannelType ChannelBlue { get; set; }
        public ChannelType ChannelGreen { get; set; }
        public ChannelType ChannelRed { get; set; }

        public List<UserData> UserData { get; set; } = new();

        public Texture Read(BinaryStream stream)
        {
            stream.Seek(4); // Signature
            NextBlockOffset = stream.ReadUInt32();
            BlockSize = stream.ReadInt64();

            Flags = stream.Read1Byte();
            Dim = stream.ReadEnum<Dim>(strict: true);
            TileMode = stream.ReadEnum<TileMode>(true);
            Swizzle = stream.ReadUInt16();
            MipCount = stream.ReadUInt16();
            SampleCount = stream.ReadUInt32();
            Format = stream.ReadEnum<SurfaceFormat>(true);

            AccessFlags = stream.ReadEnum<AccessFlags>(false);
            Width = stream.ReadUInt32();
            Height = stream.ReadUInt32();
            Depth = stream.ReadUInt32();
            ArrayLength = stream.ReadUInt32();
            TextureLayout = stream.ReadUInt32();
            TextureLayout2 = stream.ReadUInt32();
            stream.Seek(20); // Reserved
            ImageSize = stream.ReadUInt32();

            if (ImageSize == 0) {
                throw new Exception("Image size was '0'.");
            }

            Alignment = stream.ReadInt32();
            ChannelType = stream.ReadUInt32();
            SurfaceDim = stream.ReadEnum<SurfaceDim>(true);
            Name = stream.ReadStringFromOffset();

            ParentOffset = stream.ReadInt64();
            PtrOffset = stream.ReadInt64();
            UserDataOffset = stream.ReadInt64();
            TexPtr = stream.ReadInt64();
            TexView = stream.ReadInt64();
            DescSlotDataOffset = stream.ReadInt64();
            StringDictionary = stream.ReadTemporary(() => StringDictionary.Read(stream));

            UserData = stream.ReadTemporary(() => Common.UserData.ReadList(stream, StringDictionary.Count), UserDataOffset);
            MipOffsets = stream.ReadTemporary(() => stream.ReadInt64s((int)MipCount), PtrOffset);

            ChannelRed = (ChannelType)((ChannelType >> 0) & 0xff);
            ChannelGreen = (ChannelType)((ChannelType >> 8) & 0xff);
            ChannelBlue = (ChannelType)((ChannelType >> 16) & 0xff);
            ChannelAlpha = (ChannelType)((ChannelType >> 24) & 0xff);
            TextureData = new List<List<byte[]>>();

            ReadTextureLayout = Flags & 1;
            SparseBinding = Flags >> 1;
            SparseResidency = Flags >> 2;
            BlockHeightLog2 = TextureLayout & 7;

            int arrayOffset = 0;
            for (int array = 0; array < ArrayLength; array++) {

                List<byte[]> mips = new();
                for (int level = 0; level < MipCount; level++) {
                    int size = (int)((MipOffsets[0] + ImageSize - MipOffsets[level]) / ArrayLength);
                    using (stream.TemporarySeek(arrayOffset + MipOffsets[level], SeekOrigin.Begin)) {
                        mips.Add(stream.ReadBytes(size));
                    }

                    if (mips[level].Length == 0) {
                        throw new InvalidDataException(
                            $"Empty mip map at level '{level}'.\n" +
                            $"  Texture: {Name}\n" +
                            $"  Image Size: {ImageSize}\n" +
                            $"  Size: {size}\n" +
                            $"  Array Length: {ArrayLength}\n"
                        );
                    }
                }

                TextureData.Add(mips);
                arrayOffset += mips[0].Length;
            }

            int mip = 0;
            long StartMip = MipOffsets[0];
            foreach (long offset in MipOffsets) {
                MipOffsets[mip++] = offset - StartMip;
            }

            return this;
        }

        public void Write(BinaryStream stream, BntxBase bntx)
        {
            stream.Write("BRTI");
            stream.WriteHeaderOffset(bntx);
        }
    }
}
