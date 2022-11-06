namespace BntxLibrary.Extensions
{
    public static class MathExt
    {
        public static int RoundUp(int x, int y) => ((x - 1) | (y - 1)) + 1;
    }
}
