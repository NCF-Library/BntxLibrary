using BntxLibrary.Core;
using Syroot.BinaryData;
using System.Text;

namespace BntxLibrary.Extensions
{
    public static class BinaryStreamExt
    {
        public static T ReadTemporary<T>(this BinaryStream stream, Func<T> callback, long? offset = null)
        {
            offset ??= stream.ReadInt64();
            if (offset == 0) {
                return default!;
            }

            using (stream.TemporarySeek(offset.Value, SeekOrigin.Begin)) {
                return callback.Invoke();
            }
        }

        public static Action? WriteTemporary(this BinaryStream stream, Action callback, long offset)
        {
            using (stream.TemporarySeek(offset, SeekOrigin.Begin)) {
                callback.Invoke();
            }
            return null;
        }

        public static string ReadStringFromOffset(this BinaryStream stream, long? offset = null, Encoding? encoding = null)
        {
            encoding ??= stream.Encoding;
            return stream.ReadTemporary(() => {

                stream.StringCoding = StringCoding.VariableByteCount;
                stream.Encoding = encoding;

                ushort size = stream.ReadUInt16();
                return stream.ReadString(size);
            }, offset);
        }

        public static void StashValue(this BinaryStream stream, BntxBase bntx, string key)
        {
            bntx.OffsetLog.Add(key, (sizeof(ushort), stream.Position));
        }

        public static void StashOffset(this BinaryStream stream, BntxBase bntx, string key, ushort dummyValue)
        {
            bntx.OffsetLog.Add(key, (sizeof(ushort), stream.Position));
            stream.WriteUInt16(dummyValue);
        }

        public static void StashOffset(this BinaryStream stream, BntxBase bntx, string key, uint dummyValue)
        {
            bntx.OffsetLog.Add(key, (sizeof(uint), stream.Position));
            stream.WriteUInt32(dummyValue);
        }

        public static void StashOffset(this BinaryStream stream, BntxBase bntx, string key, long dummyValue)
        {
            bntx.OffsetLog.Add(key, (sizeof(long), stream.Position));
            stream.WriteInt64(dummyValue);
        }

        public static void StashOffset(this BinaryStream stream, BntxBase bntx, string key, byte[] dummyValue)
        {
            bntx.OffsetLog.Add(key, (sizeof(byte), stream.Position));
            stream.WriteBytes(dummyValue);
        }

        public static void MoveStashOffset(this BinaryStream _, BntxBase bntx, string key, long offset)
        {
            bntx.OffsetLog[key] = (sizeof(byte), bntx.OffsetLog[key].Item2 + offset);
        }

        public static void WriteOffset(this BinaryStream stream, BntxBase bntx, string key)
        {
            long offset = bntx.OffsetLog[key].Item2;

            _ = bntx.OffsetLog[key].Item1 switch {
                sizeof(ushort) => stream.WriteTemporary(() => stream.WriteUInt16((ushort)stream.Position), offset),
                sizeof(uint) => stream.WriteTemporary(() => stream.WriteUInt32((uint)stream.Position), offset),
                sizeof(long) => stream.WriteTemporary(() => stream.WriteInt64(stream.Position), offset),
                sizeof(byte) => stream.WriteTemporary(() => {
                    stream.MoveStashOffset(bntx, key, sizeof(uint));
                    stream.WriteUInt32((uint)stream.Position);
                }, offset),
                _ => throw new NotImplementedException()
            };
        }

        public static void WriteHeaderOffset(this BinaryStream stream, BntxBase bntx)
        {
            if (bntx.OffsetLog.ContainsKey("__HEADEROFFSET__")) {

                // Write the last logged offset
                long offset = bntx.OffsetLog["__HEADEROFFSET__"].Item2;
                long size = stream.Position - offset;
                stream.WriteTemporary(() => {
                    stream.WriteUInt32((uint)size);
                    stream.WriteInt64(size);
                }, offset);
            }
            else {
                bntx.OffsetLog.Add("__HEADEROFFSET__", (0, 0L));
            }

            // Write the current offset
            bntx.OffsetLog["__HEADEROFFSET__"] = (0, stream.Position);
            stream.Write((uint)0);
        }
    }
}
