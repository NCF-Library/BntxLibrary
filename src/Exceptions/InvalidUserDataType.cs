using BntxLibrary.Enums;

namespace BntxLibrary.Exceptions
{
    internal class InvalidUserDataType : Exception
    {
        public InvalidUserDataType(UserDataType dataType, UserDataType expDataType) : base($"Could not cast UserDataType '{dataType}' to '{expDataType}'.") { }
    }
}
