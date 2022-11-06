using BntxLibrary.Enums;
using BntxLibrary.Exceptions;
using BntxLibrary.Extensions;
using Syroot.BinaryData;
using System.Text;

namespace BntxLibrary.Common
{
    public class UserData
    {
        public object? Value;
        public string? Name;
        public long DataOffset;
        public int Count;
        public UserDataType Type;

        public byte[]? Bytes => Type == UserDataType.String ? (byte[]?)Value : throw new InvalidUserDataType(Type, UserDataType.String);
        public int[]? Ints => Type == UserDataType.String ? (int[]?)Value : throw new InvalidUserDataType(Type, UserDataType.String);
        public float[]? Floats => Type == UserDataType.String ? (float[]?)Value : throw new InvalidUserDataType(Type, UserDataType.String);
        public string[]? Strings => Type == UserDataType.String || Type == UserDataType.WString ? (string[]?)Value : throw new InvalidUserDataType(Type, UserDataType.String);

        public UserData(BinaryStream stream)
        {
            Name = stream.ReadStringFromOffset();
            DataOffset = stream.ReadInt64();
            Count = stream.ReadInt32();
            Type = stream.ReadEnum<UserDataType>(strict: true);

            // Reserved
            stream.Seek(43);

            Value = Type switch {
                UserDataType.Byte => stream.ReadTemporary(() => stream.ReadSBytes(Count), DataOffset),
                UserDataType.Int32 => stream.ReadTemporary(() => stream.ReadInt32s(Count), DataOffset),
                UserDataType.Single => stream.ReadTemporary(() => stream.ReadSingles(Count), DataOffset),
                UserDataType.String => stream.ReadTemporary(() => stream.ReadStringFromOffset(Count, Encoding.UTF8), DataOffset),
                UserDataType.WString => stream.ReadTemporary(() => stream.ReadStringFromOffset(Count, Encoding.Unicode), DataOffset),
                _ => throw new InvalidDataException($"Could not load UserDataType '{Type}'.")
            };
        }

        public static List<UserData> ReadList(BinaryStream stream, int count, long? offset = null)
        {
            List<UserData> values = new();

            offset ??= stream.ReadInt64();
            if (offset == 0) {
                return values;
            }

            using (stream.TemporarySeek(offset.Value, SeekOrigin.Begin)) {
                for (int i = 0; i < count; i++) {
                    values.Add(new UserData(stream));
                }
            }

            return values;
        }
    }
}
