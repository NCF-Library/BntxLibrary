using Syroot.BinaryData;

namespace BntxLibrary.Core
{
    /// <summary>
    /// Unused class (?)
    /// </summary>
    public class RelocationTable
    {
        public RelocationTable Read(BinaryStream stream, long offset = 0)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            stream.Seek(20);
            return this;
        }

        public void Write(BinaryStream stream, BntxBase bntx)
        {
            stream.Align(1 << (int)bntx.Alignment);

            stream.WriteString("_RLT");
            stream.Write(stream.Position-4); // Subtract 4 to account for the signature
            // Save the section count (typically 2 in BNTX files?)
            // Write 4 bytes of padding

            // iterate the sections
            // write {
            //  section.Pos, section.Size, section.EntryIndex, section.EntryCount
            // }
            // 
            // iterate the section entries (after iterating all sections previously)
            // write {
            //  entry.Pos, (ushort)entry.StructCount, (byte)entry.OffsetCount, (byte)entry.PaddingCount
            // }
        }
    }
}
