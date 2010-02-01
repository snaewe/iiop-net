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
                Assert.IsNull(actual, message + " [actual not null]");
            }
            Assert.AreEqual(expected.Length, actual.Length, message + " [actual length not equal to expected]");
            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected.GetValue(i), actual.GetValue(i), message + " [actual element " + i + " differs from expected]");
            }
        }
        
    }
    
    
}

#endif
