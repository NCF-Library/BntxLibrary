namespace BntxLibrary.Enums
{
    /// <summary>
    /// Represents the possible data types of values stored in <see cref="UserData"/> instances.
    /// </summary>
    public enum UserDataType : byte
    {
        /// <summary>
        /// The values is an <see cref="Int32"/> array.
        /// </summary>
        Int32,

        /// <summary>
        /// The values is a <see cref="Single"/> array.
        /// </summary>
        Single,

        /// <summary>
        /// The values is a <see cref="String"/> array encoded in ASCII.
        /// </summary>
        String,

        /// <summary>
        /// The values is a <see cref="Byte"/> array.
        /// </summary>
        Byte,

        /// <summary>
        /// The values is a <see cref="String"/> array encoded in UTF-16.
        /// </summary>
        WString,
    }
}
