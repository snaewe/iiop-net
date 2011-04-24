/* BitConverterUtils.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 21.06.06  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2006 Dominic Ullmann
 *
 * Copyright 2003 ELCA Informatique SA
 * Av. de la Harpe 22-24, 1000 Lausanne 13, Switzerland
 * www.elca.ch
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using Ch.Elca.Iiop;
using System.Runtime.InteropServices;

namespace Ch.Elca.Iiop.Cdr {

    /// <summary>
    /// Convertes between bytes on the wire with a specific endian and values.
    /// </summary>
    /// <remarks>This class is only intended for internal use.
    /// It does assume to be called correctly, to achieve better speed.</remarks>
    internal class BitConverterUtils {

        internal static byte[] ArrayToBytes<T>(T[] a) {
            byte[] bytes = new byte[a.Length * Marshal.SizeOf(typeof(T))];
            GCHandle gch = GCHandle.Alloc(a);

            try {
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(a, 0);
                Marshal.Copy(ptr, bytes, 0, bytes.Length);
            }
            finally {
                gch.Free();
            }

            return bytes;
        }

        internal static T[] BytesToArray<T>(int elemCount, byte[] bytes) {
            int elemSize = Marshal.SizeOf(typeof(T));
            int byteCount = elemSize * elemCount;
            if (byteCount > bytes.Length)
                throw new ArgumentException("element count * element size > buffer size");

            T[] a = new T[elemCount];

            GCHandle gch = GCHandle.Alloc(a);

            try {
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(a, 0);
                Marshal.Copy(bytes, 0, ptr, byteCount);
            }
            finally {
                gch.Free();
            }

            return a;
        }

        internal static byte[] ArrayToBytesReverse<T>(T[] a) {
            int elemSize = Marshal.SizeOf(typeof(T));

            if (elemSize == 1)                                       // no need to reverse byte order
                return ArrayToBytes(a);

            byte[] bytes = new byte[a.Length * elemSize];

            GCHandle gch = GCHandle.Alloc(a);
            GCHandle gch2 = GCHandle.Alloc(bytes);

            try {
                IntPtr src = Marshal.UnsafeAddrOfPinnedArrayElement(a, 0);
                IntPtr dst = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
                CopyReverseElems(src, dst, elemSize, a.Length);
            }
            finally {
                gch.Free();
                gch2.Free();
            }

            return bytes;
        }

        internal static T[] BytesToArrayReverse<T>(int elemCount, byte[] bytes) {
            int elemSize = Marshal.SizeOf(typeof(T));

            if (elemSize == 1)                                       // no need to reverse byte order
                return BytesToArray<T>(elemCount, bytes);

            int byteCount = elemSize * elemCount;
            if (byteCount > bytes.Length)
                throw new ArgumentException("element count * element size > buffer size");

            T[] a = new T[elemCount];

            GCHandle gch = GCHandle.Alloc(a);
            GCHandle gch2 = GCHandle.Alloc(bytes);

            try {
                IntPtr src = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
                IntPtr dst = Marshal.UnsafeAddrOfPinnedArrayElement(a, 0);
                CopyReverseElems(src, dst, elemSize, elemCount);
            }
            finally {
                gch.Free();
                gch2.Free();
            }

            return a;
        }

#if USE_UNSAFE_CODE

        internal static unsafe void CopyReverseElems(IntPtr src, IntPtr dst, int elSize, int elCount) {
            unchecked {
                switch (elSize) {
                    case 1: {           // implemented for completeness only. use Marshal.Copy instead, it's faster
                            byte* s = (byte*)src;
                            byte* d = (byte*)dst;
                            for (int i = elCount - 1; i >= 0; --i)
                                *d++ = *s++;
                        }
                        break;
                    case 2: {
                            ushort* s = (ushort*)src;
                            ushort* d = (ushort*)dst;
                            for (int i = elCount - 1; i >= 0; --i) {
                                ushort v = *s++;
                                *d++ = (ushort)((v << 8) | (v >> 8));
                            }
                        }
                        break;
                    case 4: {
                            uint* s = (uint*)src;
                            uint* d = (uint*)dst;
                            for (int i = elCount - 1; i >= 0; --i) {
                                uint v = *s++;
                                v = ((v << 8) & 0xff00ff00) | ((v >> 8) & 0x00ff00ff);
                                *d++ = (v << 16) | (v >> 16);
                            }
                        }
                        break;
                    case 8: {
                            ulong* s = (ulong*)src;
                            ulong* d = (ulong*)dst;
                            for (int i = elCount - 1; i >= 0; --i) {
                                ulong v = *s++;
                                v = ((v << 8) & 0xff00ff00ff00ff00L) | ((v >> 8) & 0x00ff00ff00ff00ffL);
                                v = ((v << 16) & 0xffff0000ffff0000L) | ((v >> 16) & 0x0000ffff0000ffffL);
                                *d++ = (v << 32) | (v >> 32);
                            }
                        }
                        break;
                    default: {
                            byte* s = (byte*)src;
                            byte* d = (byte*)dst;

                            for (int i = elCount - 1; i >= 0; --i) {
                                for (int j = elSize - 1; j >= 0; --j)
                                    d[j] = *s++;
                                d += elSize;
                            }
                        }
                        break;
                }
            }
        }

#else

        internal static void CopyReverseElems(IntPtr src, IntPtr dst, int elSize, int elCount) {
            unchecked {
                switch (elSize) {
                    // case 1: {           // implemented for completeness only. use Marshal.Copy instead, it's faster
                    //         for (int i = elCount - 1; i >= 0; --i) {
                    //             
                    //             Marshal.WriteByte(dst, Marshal.ReadByte(src));
                    //  
                    //             src = (IntPtr)((long)src + 1);
                    //             dst = (IntPtr)((long)dst + 1);
                    //         }
                    //     }
                    //     break;
                    case 2: {
                            for (int i = elCount - 1; i >= 0; --i) {
                                ushort v = (ushort)Marshal.ReadInt16(src);

                                v = (ushort)((v << 8) | (v >> 8));

                                Marshal.WriteInt16(dst, (short)v);

                                src = (IntPtr)((long)src + 2);
                                dst = (IntPtr)((long)dst + 2);
                            }
                        }
                        break;
                    case 4: {
                            for (int i = elCount - 1; i >= 0; --i) {
                                uint v = (uint)Marshal.ReadInt32(src);

                                v = ((v << 8) & 0xff00ff00) | ((v >> 8) & 0x00ff00ff);
                                v = (v << 16) | (v >> 16);

                                Marshal.WriteInt32(dst, (int)v);

                                src = (IntPtr)((long)src + 4);
                                dst = (IntPtr)((long)dst + 4);
                            }
                        }
                        break;
                    case 8: {
                            for (int i = elCount - 1; i >= 0; --i) {
                                ulong v = (ulong)Marshal.ReadInt64(src);

                                v = ((v << 8) & 0xff00ff00ff00ff00L) | ((v >> 8) & 0x00ff00ff00ff00ffL);
                                v = ((v << 16) & 0xffff0000ffff0000L) | ((v >> 16) & 0x0000ffff0000ffffL);
                                v = (v << 32) | (v >> 32);


                                Marshal.WriteInt64(dst, (long)v);

                                src = (IntPtr)((long)src + 8);
                                dst = (IntPtr)((long)dst + 8);
                            }
                        }
                        break;
                    default: {
                        throw new NotImplementedException();
                            // byte* s = (byte*)src;
                            // byte* d = (byte*)dst;
                            //  
                            // for (int i = elCount - 1; i >= 0; --i) {
                            //     for (int j = elSize - 1; j >= 0; --j)
                            //         d[j] = *s++;
                            //     d += elSize;
                            // }
                        }
                }
            }
        }
#endif

        internal static UInt16 Reverse(UInt16 v) {
            unchecked {
                v = (ushort)((v << 8) | (v >> 8));
                return v;
            }
        }
        
        internal static UInt32 Reverse(UInt32 v) {
            unchecked {
                v = (v << 16) | (v >> 16);
                v = ((v << 8) & 0xff00ff00) | ((v >> 8) & 0xff00ff);
                return v;
            }
        }

        internal static UInt64 Reverse(UInt64 v) {
            unchecked {
                v = (v << 32) | (v >> 32);
                v = ((v << 16) & 0xffff0000ffff0000L) | ((v >> 16) & 0x0000ffff0000ffffL);
                v = ((v << 8) & 0xff00ff00ff00ff00L) | ((v >> 8) & 0x00ff00ff00ff00ffL);
                return v;
            }
        }

        internal static Int16 Reverse(Int16 v) {
            return unchecked((Int16)Reverse((UInt16)v));
        }
        
        internal static Int32 Reverse(Int32 v) {
            return unchecked((Int32)Reverse((UInt32)v));
        }
        
        internal static Int64 Reverse(Int64 v) {
            return unchecked((Int64)Reverse((UInt64)v));
        }

#if USE_UNSAFE_CODE

        internal unsafe static Single Reverse(Single v) {
            UInt32 r = Reverse(*(UInt32*)&v);
            return *(Single*)&r;
        }

        internal unsafe static Double Reverse(Double v) {
            UInt64 r = Reverse(*(UInt64*)&v);
            return *(Double*)&r;
        }
#else
        internal static Single Reverse(Single v) {
            byte[] bytes = BitConverter.GetBytes(v);
            byte t = bytes[0];
            bytes[0] = bytes[3];
            bytes[3] = t;
            t = bytes[1];
            bytes[1] = bytes[2];
            bytes[2] = t;
            return BitConverter.ToSingle(bytes, 0);
        }

        internal static Double Reverse(Double v) {
            unchecked {
                UInt64 r = Reverse((UInt64)BitConverter.DoubleToInt64Bits(v));
                return BitConverter.Int64BitsToDouble((Int64)r);
            }
        }
#endif

    }
}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using System;
    using System.Reflection;
    using Ch.Elca.Iiop.Cdr;
    using NUnit.Framework;
    
    /// <summary>
    /// Unit-tests for testing BitConverterUtils.
    /// </summary>
    [TestFixture]
    public class BitConverterUtilsTest {

        
        [Test]
        public void TestInt16WBEWToS() {
            System.Int16 result = 
                BitConverterUtils.Reverse(BitConverter.ToInt16(new byte[] { 0, 1 }, 0));
            Assert.AreEqual(1, result, "converted wbe int 16");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToInt16(new byte[] { 1, 2 }, 0));
            Assert.AreEqual(258, result, "converted wbe int 16 (2)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToInt16(new byte[] { 0x7F, 0xFF }, 0));
            Assert.AreEqual(Int16.MaxValue, result, "converted wbe int 16 (3)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToInt16(new byte[] { 0x80, 0x00 }, 0));
            Assert.AreEqual(Int16.MinValue, result, "converted wbe int 16 (4)");
        }
        
        [Test]
        public void TestInt16WLEWToS() {
            System.Int16 result = 
                BitConverter.ToInt16(new byte[] { 1, 0 }, 0);
            Assert.AreEqual(1, result, "converted wbe int 16");
            
            result = 
                BitConverter.ToInt16(new byte[] { 2, 1 }, 0);
            Assert.AreEqual(258, result, "converted wbe int 16 (2)");
            
            result = 
                BitConverter.ToInt16(new byte[] { 0xFF, 0x7F }, 0);
            Assert.AreEqual(Int16.MaxValue, result, "converted wbe int 16 (3)");
            
            result = 
                BitConverter.ToInt16(new byte[] { 0x00, 0x80 }, 0);
            Assert.AreEqual(Int16.MinValue, result, "converted wbe int 16 (4)");
        }
        
        [Test]
        public void TestInt32WBEWToS() {
            System.Int32 result = 
                BitConverterUtils.Reverse(BitConverter.ToInt32(new byte[] { 0, 0, 0, 1 }, 0));
            Assert.AreEqual(1, result, "converted wbe int 32");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToInt32(new byte[] { 0, 0, 1, 2 }, 0));
            Assert.AreEqual(258, result, "converted wbe int 32 (2)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToInt32(new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, 0));
            Assert.AreEqual(Int32.MaxValue, result, "converted wbe int 32 (3)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToInt32(new byte[] { 0x80, 0x00, 0x00, 0x00 }, 0));
            Assert.AreEqual(Int32.MinValue, result, "converted wbe int 32 (4)");
        }
        
        [Test]
        public void TestInt32WLEWToS() {
            System.Int32 result = 
                BitConverter.ToInt32(new byte[] { 1, 0, 0, 0 }, 0);
            Assert.AreEqual(1, result, "converted wbe int 32");
            
            result = 
                BitConverter.ToInt32(new byte[] { 2, 1, 0, 0 }, 0);
            Assert.AreEqual(258, result, "converted wbe int 32 (2)");
            
            result = 
                BitConverter.ToInt32(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, 0);
            Assert.AreEqual(Int32.MaxValue, result, "converted wbe int 32 (3)");
            
            result = 
                BitConverter.ToInt32(new byte[] { 0x00, 0x00, 0x00, 0x80 }, 0);
            Assert.AreEqual(Int32.MinValue, result, "converted wbe int 32 (4)");
        }
        
        [Test]
        public void TestInt64WBEWToS() {
            System.Int64 result = 
                BitConverterUtils.Reverse(BitConverter.ToInt64(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, 0));
            Assert.AreEqual(1, result, "converted wbe int 64");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToInt64(new byte[] { 0, 0, 0, 0, 0, 0, 1, 2 }, 0));
            Assert.AreEqual(258, result, "converted wbe int 64 (2)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToInt64(new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0));
            Assert.AreEqual(Int64.MaxValue, result, "converted wbe int 64 (3)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToInt64(new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0));
            Assert.AreEqual(Int64.MinValue, result, "converted wbe int 64 (4)");
        }
        
        [Test]
        public void TestInt64WLEWToS() {
            System.Int64 result = 
                BitConverter.ToInt64(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 0);
            Assert.AreEqual(1, result, "converted wbe int 64");
            
            result = 
                BitConverter.ToInt64(new byte[] { 2, 1, 0, 0, 0, 0, 0, 0 }, 0);
            Assert.AreEqual(258, result, "converted wbe int 64 (2)");
            
            result = 
                BitConverter.ToInt64(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, 0);
            Assert.AreEqual(Int64.MaxValue, result, "converted wbe int 64 (3)");
            
            result = 
                BitConverter.ToInt64(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, 0);
            Assert.AreEqual(Int64.MinValue, result, "converted wbe int 64 (4)");
        }
        
        [Test]
        public void TestUInt16WBEWToS() {
            System.UInt16 result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt16(new byte[] { 0, 1 }, 0));
            Assert.AreEqual(1, result, "converted wbe uint 16");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt16(new byte[] { 1, 2 }, 0));
            Assert.AreEqual(258, result, "converted wbe uint 16 (2)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt16(new byte[] { 0xFF, 0xFF }, 0));
            Assert.AreEqual(UInt16.MaxValue, result, "converted wbe uint 16 (3)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt16(new byte[] { 0x00, 0x00 }, 0));
            Assert.AreEqual(UInt16.MinValue, result, "converted wbe uint 16 (4)");
        }
        
        [Test]
        public void TestUInt16WLEWToS() {
            System.UInt16 result = 
                BitConverter.ToUInt16(new byte[] { 1, 0 }, 0);
            Assert.AreEqual(1, result, "converted wbe uint 16");
            
            result = 
                BitConverter.ToUInt16(new byte[] { 2, 1 }, 0);
            Assert.AreEqual(258, result, "converted wbe uint 16 (2)");
            
            result = 
                BitConverter.ToUInt16(new byte[] { 0xFF, 0xFF }, 0);
            Assert.AreEqual(UInt16.MaxValue, result, "converted wbe uint 16 (3)");
            
            result = 
                BitConverter.ToUInt16(new byte[] { 0x00, 0x00 }, 0);
            Assert.AreEqual(UInt16.MinValue, result, "converted wbe uint 16 (4)");
        }
        
        [Test]
        public void TestUInt32WBEWToS() {
            System.UInt32 result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt32(new byte[] { 0, 0, 0, 1 }, 0));
            Assert.AreEqual(1, result, "converted wbe uint 32");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt32(new byte[] { 0, 0, 1, 2 }, 0));
            Assert.AreEqual(258, result, "converted wbe uint 32 (2)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt32(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0));
            Assert.AreEqual(UInt32.MaxValue, result, "converted wbe uint 32 (3)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt32(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0));
            Assert.AreEqual(UInt32.MinValue, result, "converted wbe uint 32 (4)");
        }
        
        [Test]
        public void TestUInt32WLEWToS() {
            System.UInt32 result = 
                BitConverter.ToUInt32(new byte[] { 1, 0, 0, 0 }, 0);
            Assert.AreEqual(1, result, "converted wbe uint 32");
            
            result = 
                BitConverter.ToUInt32(new byte[] { 2, 1, 0, 0 }, 0);
            Assert.AreEqual(258, result, "converted wbe uint 32 (2)");
            
            result = 
                BitConverter.ToUInt32(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0);
            Assert.AreEqual(UInt32.MaxValue, result, "converted wbe uint 32 (3)");
            
            result = 
                BitConverter.ToUInt32(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0);
            Assert.AreEqual(UInt32.MinValue, result, "converted wbe uint 32 (4)");
        }
        
        [Test]
        public void TestUInt64WBEWToS() {
            System.UInt64 result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt64(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, 0));
            Assert.AreEqual(1, result, "converted wbe uint 64");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt64(new byte[] { 0, 0, 0, 0, 0, 0, 1, 2 }, 0));
            Assert.AreEqual(258, result, "converted wbe uint 64 (2)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt64(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0));
            Assert.AreEqual(UInt64.MaxValue, result, "converted wbe uint 64 (3)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToUInt64(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0));
            Assert.AreEqual(UInt64.MinValue, result, "converted wbe uint 64 (4)");
        }
        
        [Test]
        public void TestUInt64WLEWToS() {
            System.UInt64 result = 
                BitConverter.ToUInt64(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 0);
            Assert.AreEqual(1, result, "converted wbe uint 64");
            
            result = 
                BitConverter.ToUInt64(new byte[] { 2, 1, 0, 0, 0, 0, 0, 0 }, 0);
            Assert.AreEqual(258, result, "converted wbe uint 64 (2)");
            
            result = 
                BitConverter.ToUInt64(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0);
            Assert.AreEqual(UInt64.MaxValue, result, "converted wbe uint 64 (3)");
            
            result = 
                BitConverter.ToUInt64(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0);
            Assert.AreEqual(UInt64.MinValue, result, "converted wbe uint 64 (4)");
        }


        [Test]
        public void TestInt16WBESToW() {
            byte[] result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((short)1));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 16", new byte[] { 0, 1 }, result);
            
            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((short)258));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 16 (2)", new byte[] { 1, 2 }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(Int16.MaxValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 16 (3)", new byte[] { 0x7F, 0xFF }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(Int16.MinValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 16 (4)", new byte[] { 0x80, 0x00 }, result);
        }
        
        [Test]
        public void TestInt16WLESToW() {
            byte[] result =
                BitConverter.GetBytes((short)1);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 16", new byte[] { 1, 0 }, result);
            
            result =
                BitConverter.GetBytes((short)258);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 16 (2)", new byte[] { 2, 1 }, result);

            result =
                BitConverter.GetBytes(Int16.MaxValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 16 (3)", new byte[] { 0xFF, 0x7F }, result);

            result =
                BitConverter.GetBytes(Int16.MinValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 16 (4)", new byte[] { 0x00, 0x80 }, result);
        }
        
        [Test]
        public void TestInt32WBESToW() {
            byte[] result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((int)1));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 32", new byte[] { 0, 0, 0, 1 }, result);
            
            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((int)258));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 32 (2)", new byte[] { 0, 0, 1, 2 }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(Int32.MaxValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 32 (3)", new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(Int32.MinValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 32 (4)", new byte[] { 0x80, 0x00, 0x00, 0x00 }, result);
        }
        
        [Test]
        public void TestInt32WLESToW() {
            byte[] result =
                BitConverter.GetBytes((int)1);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 32", new byte[] { 1, 0, 0, 0 }, result);
            
            result =
                BitConverter.GetBytes((int)258);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 32 (2)", new byte[] { 2, 1, 0, 0 }, result);

            result =
                BitConverter.GetBytes(Int32.MaxValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 32 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, result);

            result =
                BitConverter.GetBytes(Int32.MinValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 32 (4)", new byte[] { 0x00, 0x00, 0x00, 0x80 }, result);
        }
        
        [Test]
        public void TestInt64WBESToW() {
            byte[] result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((long)1));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 64", new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, result);
            
            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((long)258));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 64 (2)", new byte[] { 0, 0, 0, 0, 0, 0, 1, 2 }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(Int64.MaxValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 64 (3)", new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(Int64.MinValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 64 (4)", new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result);
        }
        
        [Test]
        public void TestInt64WLESToW() {
            byte[] result =
                BitConverter.GetBytes((long)1);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 64", new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, result);
            
            result =
                BitConverter.GetBytes((long)258);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 64 (2)", new byte[] { 2, 1, 0, 0, 0, 0, 0, 0 }, result);

            result =
                BitConverter.GetBytes(Int64.MaxValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 64 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, result);

            result =
                BitConverter.GetBytes(Int64.MinValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe int 64 (4)", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, result);
        }
        
        [Test]
        public void TestUInt16WBESToW() {
            byte[] result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((ushort)1));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16", new byte[] { 0, 1 }, result);
            
            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((ushort)258));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16 (2)", new byte[] { 1, 2 }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(UInt16.MaxValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16 (3)", new byte[] { 0xFF, 0xFF }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(UInt16.MinValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16 (4)", new byte[] { 0x00, 0x00 }, result);
        }
        
        [Test]
        public void TestUInt16WLESToW() {
            byte[] result =
                BitConverter.GetBytes((ushort)1);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16", new byte[] { 1, 0 }, result);
            
            result =
                BitConverter.GetBytes((ushort)258);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16 (2)", new byte[] { 2, 1 }, result);

            result =
                BitConverter.GetBytes(UInt16.MaxValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16 (3)", new byte[] { 0xFF, 0xFF }, result);

            result =
                BitConverter.GetBytes(UInt16.MinValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16 (4)", new byte[] { 0x00, 0x00 }, result);
        }
        
        [Test]
        public void TestUInt32WBESToW() {
            byte[] result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((uint)1));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32", new byte[] { 0, 0, 0, 1 }, result);
            
            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((uint)258));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32 (2)", new byte[] { 0, 0, 1, 2 }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(UInt32.MaxValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(UInt32.MinValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32 (4)", new byte[] { 0x00, 0x00, 0x00, 0x00 }, result);
        }
        
        [Test]
        public void TestUInt32WLESToW() {
            byte[] result =
                BitConverter.GetBytes((uint)1);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32", new byte[] { 1, 0, 0, 0 }, result);
            
            result =
                BitConverter.GetBytes((uint)258);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32 (2)", new byte[] { 2, 1, 0, 0 }, result);

            result =
                BitConverter.GetBytes(UInt32.MaxValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, result);

            result =
                BitConverter.GetBytes(UInt32.MinValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32 (4)", new byte[] { 0x00, 0x00, 0x00, 0x00 }, result);
        }
        
        [Test]
        public void TestUInt64WBESToW() {
            byte[] result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((ulong)1));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 64", new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, result);
            
            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((ulong)258));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 64 (2)", new byte[] { 0, 0, 0, 0, 0, 0, 1, 2 }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(UInt64.MaxValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 64 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(UInt64.MinValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 64 (4)", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result);
        }
        
        [Test]
        public void TestUInt64WLESToW() {
            byte[] result =
                BitConverter.GetBytes((ulong)1);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 64", new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, result);
            
            result =
                BitConverter.GetBytes((ulong)258);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 64 (2)", new byte[] { 2, 1, 0, 0, 0, 0, 0, 0 }, result);

            result =
                BitConverter.GetBytes(UInt64.MaxValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 64 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);

            result =
                BitConverter.GetBytes(UInt64.MinValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe uint 64 (4)", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result);
        }

        [Test]
        public void TestSingleWBEWToS() {
            System.Single result = 
                BitConverterUtils.Reverse(BitConverter.ToSingle(new byte[] { 0x3F, 0x80, 0x00, 0x00 }, 0));
            Assert.AreEqual((float)1.0f, result, "converted wbe single");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToSingle(new byte[] { 0x3C, 0x23, 0xD7, 0x0A }, 0));
            Assert.AreEqual((float)0.01f, result, "converted wbe single (2)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToSingle(new byte[] { 0x7F, 0x7F, 0xFF, 0xFF }, 0));
            Assert.AreEqual(Single.MaxValue, result, "converted wbe single (3)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToSingle(new byte[] { 0xFF, 0x7F, 0xFF, 0xFF }, 0));
            Assert.AreEqual(Single.MinValue, result, "converted wbe single (4)");
        }
        
        [Test]
        public void TestSingleWLEWToS() {
            System.Single result = 
                BitConverter.ToSingle(new byte[] { 0x00, 0x00, 0x80, 0x3F }, 0);
            Assert.AreEqual((float)1.0f, result, "converted wbe single");
            
            result = 
                BitConverter.ToSingle(new byte[] { 0x0A, 0xD7, 0x23, 0x3C }, 0);
            Assert.AreEqual((float)0.01f, result, "converted wbe single (2)");
            
            result = 
                BitConverter.ToSingle(new byte[] { 0xFF, 0xFF, 0x7F, 0x7F }, 0);
            Assert.AreEqual(Single.MaxValue, result, "converted wbe single (3)");
            
            result = 
                BitConverter.ToSingle(new byte[] { 0xFF, 0xFF, 0x7F, 0xFF }, 0);
            Assert.AreEqual(Single.MinValue, result, "converted wbe single (4)");
        }
        
        [Test]
        public void TestDoubleWBEWToS() {
            System.Double result = 
                BitConverterUtils.Reverse(BitConverter.ToDouble(new byte[] { 0x3F, 0xF0, 0, 0, 0, 0, 0, 0 }, 0));
            Assert.AreEqual(1.0, result, "converted wbe double");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToDouble(new byte[] { 0x3F, 0x84, 0x7A, 0xE1, 0x47, 0xAE, 0x14, 0x7B }, 0));
            Assert.AreEqual(0.01, result, "converted wbe double (2)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToDouble(new byte[] { 0x7F, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0));
            Assert.AreEqual(Double.MaxValue, result, "converted wbe double (3)");
            
            result = 
                BitConverterUtils.Reverse(BitConverter.ToDouble(new byte[] { 0xFF, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0));
            Assert.AreEqual(Double.MinValue, result, "converted wbe double (4)");
        }
        
        [Test]
        public void TestDoubleWLEWToS() {
            System.Double result = 
                BitConverter.ToDouble(new byte[] { 0, 0, 0, 0, 0, 0, 0xF0, 0x3F }, 0);
            Assert.AreEqual(1.0, result, "converted wbe double");
            
            result = 
                BitConverter.ToDouble(new byte[] { 0x7B, 0x14, 0xAE, 0x47, 0xE1, 0x7A, 0x84, 0x3F }, 0);
            Assert.AreEqual(0.01, result, "converted wbe double (2)");
            
            result = 
                BitConverter.ToDouble(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0x7F }, 0);
            Assert.AreEqual(Double.MaxValue, result, "converted wbe double (3)");
            
            result = 
                BitConverter.ToDouble(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0xFF }, 0);
            Assert.AreEqual(Double.MinValue, result, "converted wbe double (4)");
        }
        
        [Test]
        public void TestSingleWBESToW() {
            byte[] result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((float)1));
            ArrayAssertion.AssertByteArrayEquals("converted wbe single", new byte[] { 0x3F, 0x80, 0x00, 0x00 }, result);
            
            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((float)0.01));
            ArrayAssertion.AssertByteArrayEquals("converted wbe single (2)", new byte[] { 0x3C, 0x23, 0xD7, 0x0A }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(Single.MaxValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe single (3)", new byte[] { 0x7F, 0x7F, 0xFF, 0xFF }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(Single.MinValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe single (4)", new byte[] { 0xFF, 0x7F, 0xFF, 0xFF }, result);
        }
        
        [Test]
        public void TestSingleWLESToW() {
            byte[] result =
                BitConverter.GetBytes((float)1);
            ArrayAssertion.AssertByteArrayEquals("converted wbe single", new byte[] { 0x00, 0x00, 0x80, 0x3F }, result);
            
            result =
                BitConverter.GetBytes((float)0.01);
            ArrayAssertion.AssertByteArrayEquals("converted wbe single (2)", new byte[] { 0x0A, 0xD7, 0x23, 0x3C }, result);

            result =
                BitConverter.GetBytes(Single.MaxValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe single (3)", new byte[] { 0xFF, 0xFF, 0x7F, 0x7F }, result);

            result =
                BitConverter.GetBytes(Single.MinValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe single (4)", new byte[] { 0xFF, 0xFF, 0x7F, 0xFF }, result);
        }
        
        [Test]
        public void TestDoubleWBESToW() {
            byte[] result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((double)1));
            ArrayAssertion.AssertByteArrayEquals("converted wbe double", new byte[] { 0x3F, 0xF0, 0, 0, 0, 0, 0, 0 }, result);
            
            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse((double)0.01));
            ArrayAssertion.AssertByteArrayEquals("converted wbe double (2)", new byte[] { 0x3F, 0x84, 0x7A, 0xE1, 0x47, 0xAE, 0x14, 0x7B }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(Double.MaxValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe double (3)", new byte[] { 0x7F, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);

            result =
                BitConverter.GetBytes(BitConverterUtils.Reverse(Double.MinValue));
            ArrayAssertion.AssertByteArrayEquals("converted wbe double (4)", new byte[] { 0xFF, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);
        }
        
        [Test]
        public void TestDoubleWLESToW() {
            byte[] result =
                BitConverter.GetBytes((double)1);
            ArrayAssertion.AssertByteArrayEquals("converted wbe double", new byte[] { 0, 0, 0, 0, 0, 0, 0xF0, 0x3F }, result);
            
            result =
                BitConverter.GetBytes((double)0.01);
            ArrayAssertion.AssertByteArrayEquals("converted wbe double (2)", new byte[] { 0x7B, 0x14, 0xAE, 0x47, 0xE1, 0x7A, 0x84, 0x3F }, result);

            result =
                BitConverter.GetBytes(Double.MaxValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe double (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0x7F }, result);

            result =
                BitConverter.GetBytes(Double.MinValue);
            ArrayAssertion.AssertByteArrayEquals("converted wbe double (4)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0xFF }, result);
        }
    }

}

#endif

