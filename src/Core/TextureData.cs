using BntxLibrary.Extensions;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BntxLibrary.Core
{
    public class TextureData
    {
        public static void Write(BinaryStream stream, BntxBase bntx)
        {
            stream.Write("BRTD");
            stream.WriteHeaderOffset(bntx);

            for (int i = 0; i < bntx.TextureArray.Length; i++) {

                long blockOffset = stream.Position;
                var tex = bntx.TextureArray[i];

                for (int m = 0; m < tex.MipOffsets.Length; m++) {
                    stream.WriteTemporary(() => stream.Write(blockOffset + tex.MipOffsets[m]), tex.SerdeMipOffsets[m]);
                }

                foreach (var mipdata in tex.TextureData) {
                    stream.Write(mipdata[0]);
                }
            }
        }
    }
}
