using BntxLibrary.Common;
using BntxLibrary.Extensions;
using Syroot.BinaryData;
using System.Collections;

namespace BntxLibrary.Core
{
    public class StringDictionary : IEnumerable
    {
        private readonly Dictionary<string, StringDictionaryEntry> Table = new();
        private StringDictionaryEntry RootNode;

        public int Count => Table.Count;

        public StringDictionaryEntry this[string key] {
            get => Table[key];
            set => Table[key] = value;
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var item in Table) {
                yield return item.Value;
            }
        }

        public StringDictionary Read(BinaryStream stream, long position = 0)
        {
            stream.Seek(position, SeekOrigin.Begin);

            // Skip signature
            stream.Seek(4);

            int length = stream.ReadInt32();

            // Read root node
            RootNode.Reference = stream.ReadInt32();
            RootNode.LeftIndex = stream.ReadUInt16();
            RootNode.RightIndex = stream.ReadUInt16();

            for (int i = 0; i < length; i++) {

                int reference = stream.ReadInt32();
                ushort idxLeft = stream.ReadUInt16();
                ushort idxRight = stream.ReadUInt16();
                string name = stream.ReadStringFromOffset();

                Table.Add(name, new StringDictionaryEntry(reference, idxLeft, idxRight));
            }

            return this;
        }

        public void Write(BinaryStream stream, BntxBase bntx)
        {
            stream.Write("_DIC");
            stream.Write(Count);

            // Save root node in RLT section 1
            stream.Write(RootNode.Reference);
            stream.Write(RootNode.LeftIndex);
            stream.Write(RootNode.RightIndex);

            foreach (var entry in Table) {
                stream.Write(entry.Value.Reference);
                stream.Write(entry.Value.LeftIndex);
                stream.Write(entry.Value.RightIndex);
                stream.Write(entry.Key);
            }
        }
    }
}
