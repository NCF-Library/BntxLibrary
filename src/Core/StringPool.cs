using BntxLibrary.Extensions;
using Syroot.BinaryData;
using System.Collections;
using System.Diagnostics;

namespace BntxLibrary.Core
{
    public class StringPool : IEnumerable
    {
        private readonly Dictionary<long, string> Strings = new();
        private long NullOffset = -1;

        public int Count => Strings.Count;

        public string this[long offset] {
            get => Strings[offset];
            set => Strings[offset] = value;
        }

        public long this[string str] {
            get => Strings.Where(x => x.Value == str).First().Key;
        }
        
        public void Add(string str) => Strings.Add(NullOffset--, str);

        public void Remove(string str) => Strings.Remove(this[str]);

        public IEnumerator GetEnumerator()
        {
            foreach ((var _, var str) in Strings) {
                yield return str;
            }
        }

        public StringPool Read(BinaryStream stream, long offset = 0)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            // Magic (4:string)
            // Next block offset (4:uint32)
            // Current block size (8:int64)
            stream.Seek(16);

            // Init array with string count (length)
            uint length = stream.ReadUInt32();

            // Padding
            stream.Seek(4);

            for (int i = 0; i < length; i++) {
                ushort size = stream.ReadUInt16();
                Strings.Add(stream.Position, stream.ReadString(size));

                // Padding
                stream.Seek(size % 2 == 0 ? 2 : 1);
            }

            return this;
        }

        public void Write(BinaryStream stream, BntxBase bntx)
        {
            stream.Write("_STR");
            stream.WriteHeaderOffset(bntx);
            stream.Write(Strings.Count);
            stream.Align(4); // This does padding I think 😅

            foreach (string str in Strings.Values) {
                stream.Write((ushort)str.Length);
                stream.Write(str);
                stream.Align(2);
            }
        }
    }
}