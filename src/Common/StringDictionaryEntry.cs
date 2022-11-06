namespace BntxLibrary.Common
{
    public struct StringDictionaryEntry
    {
        public int Reference;
        public ushort LeftIndex;
        public ushort RightIndex;

        public StringDictionaryEntry(int reference, ushort leftIndex, ushort rightIndex)
        {
            Reference = reference;
            LeftIndex = leftIndex;
            RightIndex = rightIndex;
        }
    }
}
