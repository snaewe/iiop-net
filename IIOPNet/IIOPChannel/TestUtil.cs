#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using System;
    using NUnit.Framework;    

    public sealed class ArrayAssertion {
        
        private ArrayAssertion() {            
        }
        
        /// <summary>
        /// make sure, that the two arrays are equal.
        /// </summary>
        public static void AssertByteArrayEquals(string message, byte[] expected, byte[] actual) {
            AssertEquals(message, (Array)expected, (Array)actual);
        }
        
        public static void AssertEquals(string message, Array expected, Array actual) {
            if (expected == null) {
                Assertion.AssertNull(message + " [actual not null]", actual);
            }
            Assertion.AssertEquals(message + " [actual length not equal to expected]", expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++) {
                Assertion.AssertEquals(message + " [actual element " + i + " differs from expected]", 
                                       expected.GetValue(i), actual.GetValue(i));
            }
        }
        
    }
    
    
}

#endif
