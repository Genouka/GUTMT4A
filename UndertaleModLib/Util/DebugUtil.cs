using System;
using System.Diagnostics.CodeAnalysis;

namespace UndertaleModLib.Util
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods| DynamicallyAccessedMemberTypes.PublicProperties| DynamicallyAccessedMemberTypes.PublicEvents| DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class DebugUtil
    {
        
        /// <summary>
        /// Asserts that a specified condition is true.
        /// </summary>
        /// <param name="expr">The condition to assert for.</param>
        /// <param name="msg">The message to use if the assertion fails.</param>
        /// <exception cref="Exception">Gets thrown if the assertion fails.</exception>
        public static void Assert(bool expr, string msg = "Unknown error.")
        {
            if (expr)
                return;

            throw new Exception($"Assertion failed! {msg}");
        }
    }
}
