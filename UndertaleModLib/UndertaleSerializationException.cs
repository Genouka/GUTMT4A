using System;
using System.Diagnostics.CodeAnalysis;

namespace UndertaleModLib
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods| DynamicallyAccessedMemberTypes.PublicProperties| DynamicallyAccessedMemberTypes.PublicEvents| DynamicallyAccessedMemberTypes.PublicConstructors)]
    [Serializable]
    internal class UndertaleSerializationException : Exception
    {
        public UndertaleSerializationException()
        {
        }

        public UndertaleSerializationException(string message) : base(message)
        {
        }

        public UndertaleSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}