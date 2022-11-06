using BntxLibrary.Common;
using BntxLibrary.Extensions;
using Syroot.BinaryData;
using System.Collections;

namespace BntxLibrary.Core
{
    public class TextureArray : IEnumerable
    {
        private Texture[] Textures = Array.Empty<Texture>();

        public int Length => Textures.Length;

        public Texture this[string name] {
            get {
                return Textures[0]; // use one of the StringDict/Pool things to load the texture from name
            }
            set { 
                // same for the setter
            }
        }

        public Texture this[int i] {
            get {
                return Textures[i];
            }
            set {
                Textures[i] = value;
            }
        }

        public void Add(string name, Texture texture)
        {

        }

        public void Replcae(string name, Texture texture)
        {

        }

        public void Remove(string name)
        {

        }

        public IEnumerator GetEnumerator()
        {
            foreach (var texrure in Textures) {
                yield return texrure;
            }
        }

        public TextureArray Read(BinaryStream stream, int count, long offset = 0)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            Textures = new Texture[count];
            for (int i = 0; i < count; i++) {
                Textures[i] = stream.ReadTemporary(() => new Texture().Read(stream));
            }
            return this;
        }

        public void Write(BinaryStream stream, BntxBase bntx)
        {
            foreach (var texture in Textures) {
                texture.Write(stream, bntx);
            }
        }
    }
}
