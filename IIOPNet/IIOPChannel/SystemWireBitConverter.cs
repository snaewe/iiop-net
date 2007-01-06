/* SystemWireBitConverter.cs
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

namespace Ch.Elca.Iiop.Cdr {
	
	/// <summary>
	/// Convertes between bytes on the wire if endian is not equal to plattfrom endian (see BitConverter.IsLittleEndian)
	/// </summary>
	/// <remarks>This class is only intended for internal use.
	/// It does assume to be called correctly, to achieve better speed.</remarks>
	internal class NonNativeEndianSystemWireBitConverter {
		
		private static void Reverse2ForBC(byte[] wireVal) {
            byte tmp = wireVal[0];
            wireVal[0] = wireVal[1];
            wireVal[1] = tmp;			
        }
		
		private static void Reverse4ForBC(byte[] wireVal) {
           	byte tmp = wireVal[0];
           	wireVal[0] = wireVal[3];
           	wireVal[3] = tmp;
           	tmp = wireVal[1];
           	wireVal[1] = wireVal[2];
           	wireVal[2] = tmp;
        }
		
		private static void Reverse8ForBC(byte[] wireVal) {
           	byte tmp = wireVal[0];
           	wireVal[0] = wireVal[7];
           	wireVal[7] = tmp;
           	tmp = wireVal[1];
           	wireVal[1] = wireVal[6];
           	wireVal[6] = tmp;
           	tmp = wireVal[2];
           	wireVal[2] = wireVal[5];
           	wireVal[5] = tmp;
           	tmp = wireVal[3];
           	wireVal[3] = wireVal[4];
           	wireVal[4] = tmp;
        }		
		
		/// <summary>
		/// converts wireVal to a short considering wire and system endian difference.
		/// </summary>
		/// <remarks>Be careful, this method modifies wireVal</remarks>
		internal static Int16 ToInt16(byte[] wireVal) {
			Reverse2ForBC(wireVal);
			return BitConverter.ToInt16(wireVal, 0);
		}						
		
		/// <summary>
		/// converts wireVal to an int considering wire and system endian difference.
		/// </summary>
		/// <remarks>Be careful, this method modifies wireVal</remarks>
		internal static Int32 ToInt32(byte[] wireVal) {
			Reverse4ForBC(wireVal);
			return BitConverter.ToInt32(wireVal, 0);
		}								
		
		/// <summary>
		/// converts wireVal to a long considering wire and system endian difference.
		/// </summary>
		/// <remarks>Be careful, this method modifies wireVal</remarks>
		internal static Int64 ToInt64(byte[] wireVal) {
			Reverse8ForBC(wireVal);
			return BitConverter.ToInt64(wireVal, 0);
		}										

		/// <summary>
		/// converts wireVal to a ushort considering wire and system endian difference.
		/// </summary>
		/// <remarks>Be careful, this method modifies wireVal</remarks>
		internal static UInt16 ToUInt16(byte[] wireVal) {
			Reverse2ForBC(wireVal);
			return BitConverter.ToUInt16(wireVal, 0);
		}						
		
		/// <summary>
		/// converts wireVal to an uint considering wire and system endian difference.
		/// </summary>
		/// <remarks>Be careful, this method modifies wireVal</remarks>
		internal static UInt32 ToUInt32(byte[] wireVal) {
			Reverse4ForBC(wireVal);
			return BitConverter.ToUInt32(wireVal, 0);
		}								
		
		/// <summary>
		/// converts wireVal to a ulong considering wire and system endian difference.
		/// </summary>
		/// <remarks>Be careful, this method modifies wireVal</remarks>
		internal static UInt64 ToUInt64(byte[] wireVal) {
			Reverse8ForBC(wireVal);
			return BitConverter.ToUInt64(wireVal, 0);
		}			
		
		/// <summary>
		/// converts wireVal to a single considering wire and system endian difference.
		/// </summary>
		/// <remarks>Be careful, this method modifies wireVal</remarks>
		internal static Single ToSingle(byte[] wireVal) {
			Reverse4ForBC(wireVal);
			return BitConverter.ToSingle(wireVal, 0);
		}			
		
		/// <summary>
		/// converts wireVal to a single considering wire and system endian difference.
		/// </summary>
		/// <remarks>Be careful, this method modifies wireVal</remarks>
		internal static Double ToDouble(byte[] wireVal) {
			Reverse8ForBC(wireVal);
			return BitConverter.ToDouble(wireVal, 0);
		}		
		
		/// <summary>
		/// converts val to a wireval considering wire and system endian difference.
		/// </summary>
		internal static byte[] GetBytes(Int16 val) {
			byte[] wireVal = BitConverter.GetBytes(val);
			Reverse2ForBC(wireVal);
			return wireVal;
		}
		
		/// <summary>
		/// converts val to a wireval considering wire and system endian difference.
		/// </summary>
		internal static byte[] GetBytes(Int32 val) {
			byte[] wireVal = BitConverter.GetBytes(val);
			Reverse4ForBC(wireVal);
			return wireVal;
		}		
		
		/// <summary>
		/// converts val to a wireval considering wire and system endian difference.
		/// </summary>
		internal static byte[] GetBytes(Int64 val) {
			byte[] wireVal = BitConverter.GetBytes(val);
			Reverse8ForBC(wireVal);
			return wireVal;
		}		
		
		/// <summary>
		/// converts val to a wireval considering wire and system endian difference.
		/// </summary>
		internal static byte[] GetBytes(UInt16 val) {
			byte[] wireVal = BitConverter.GetBytes(val);
			Reverse2ForBC(wireVal);
			return wireVal;
		}
		
	    /// <summary>
		/// converts val to a wireval considering wire and system endian difference.
		/// </summary>
		internal static byte[] GetBytes(UInt32 val) {
			byte[] wireVal = BitConverter.GetBytes(val);
			Reverse4ForBC(wireVal);
			return wireVal;
		}
		
	    /// <summary>
		/// converts val to a wireval considering wire and system endian difference.
		/// </summary>
		internal static byte[] GetBytes(UInt64 val) {
			byte[] wireVal = BitConverter.GetBytes(val);
			Reverse8ForBC(wireVal);
			return wireVal;
		}
		
	    /// <summary>
		/// converts val to a wireval considering wire and system endian difference.
		/// </summary>
		internal static byte[] GetBytes(Single val) {
			byte[] wireVal = BitConverter.GetBytes(val);
			Reverse4ForBC(wireVal);
			return wireVal;
		}
		
	    /// <summary>
		/// converts val to a wireval considering wire and system endian difference.
		/// </summary>
		internal static byte[] GetBytes(Double val) {
			byte[] wireVal = BitConverter.GetBytes(val);
			Reverse8ForBC(wireVal);
			return wireVal;
		}
		
		
		
	}
	
}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using System;
    using System.Reflection;
    using Ch.Elca.Iiop.Cdr;
    using NUnit.Framework;
    
    /// <summary>
    /// Unit-tests for testing NonNativeEndianSystemWireBitConverter.
    /// </summary>
    [TestFixture]
    public class NonNativeEndianSystemWireBitConverterTest {

        private void AssertTestWToSInt16(Int16 numberToTest) {
            byte[] arg = BitConverter.GetBytes(numberToTest);
            Array.Reverse(arg);
            Int16 result =
    			NonNativeEndianSystemWireBitConverter.ToInt16(arg);
    		Assertion.AssertEquals("converted int 16" , numberToTest, result);
        }
        
    	[Test]
    	public void TestInt16WToS() {
    	    AssertTestWToSInt16((Int16)1);
    	    AssertTestWToSInt16((Int16)258);
    	    AssertTestWToSInt16(Int16.MaxValue);
    	    AssertTestWToSInt16(Int16.MinValue);
    	}
    	

    	private void AssertTestWToSInt32(Int32 numberToTest) {
            byte[] arg = BitConverter.GetBytes(numberToTest);
            Array.Reverse(arg);
            Int32 result =
    			NonNativeEndianSystemWireBitConverter.ToInt32(arg);
    		Assertion.AssertEquals("converted int 32" , numberToTest, result);
        }

    	
    	[Test]
    	public void TestInt32WToS() {
    	    AssertTestWToSInt32((Int32)1);
    	    AssertTestWToSInt32((Int32)258);
    	    AssertTestWToSInt32(Int32.MaxValue);
    	    AssertTestWToSInt32(Int32.MinValue);
    	}

    	private void AssertTestWToSInt64(Int64 numberToTest) {
            byte[] arg = BitConverter.GetBytes(numberToTest);
            Array.Reverse(arg);
            Int64 result =
    			NonNativeEndianSystemWireBitConverter.ToInt64(arg);
    		Assertion.AssertEquals("converted int 64" , numberToTest, result);
        }    	
    	
    	[Test]
    	public void TestInt64WToS() {
    	    AssertTestWToSInt64((Int64)1);
    	    AssertTestWToSInt64((Int64)258);
    	    AssertTestWToSInt64(Int64.MaxValue);
    	    AssertTestWToSInt64(Int64.MinValue);
    	}

    	private void AssertTestWToSUInt16(UInt16 numberToTest) {
            byte[] arg = BitConverter.GetBytes(numberToTest);
            Array.Reverse(arg);
            UInt16 result =
    			NonNativeEndianSystemWireBitConverter.ToUInt16(arg);
    		Assertion.AssertEquals("converted uint 16" , numberToTest, result);
        }    	    	
    	
    	[Test]
    	public void TestUInt16WBEWToS() {
    	    AssertTestWToSUInt16((UInt16)1);
    	    AssertTestWToSUInt16((UInt16)258);
    	    AssertTestWToSUInt16(UInt16.MaxValue);
    	    AssertTestWToSUInt16(UInt16.MinValue);    	    
    	}
    	
        private void AssertTestWToSUInt32(UInt32 numberToTest) {
            byte[] arg = BitConverter.GetBytes(numberToTest);
            Array.Reverse(arg);
            UInt32 result =
    			NonNativeEndianSystemWireBitConverter.ToUInt32(arg);
    		Assertion.AssertEquals("converted uint 32" , numberToTest, result);
        }    	    	    	
    	    	    	
    	[Test]
    	public void TestUInt32WToS() {
    	    AssertTestWToSUInt32((UInt32)1);
    	    AssertTestWToSUInt32((UInt32)258);
    	    AssertTestWToSUInt32(UInt32.MaxValue);
    	    AssertTestWToSUInt32(UInt32.MinValue);    	    
    	}
    	
        private void AssertTestWToSUInt64(UInt64 numberToTest) {
            byte[] arg = BitConverter.GetBytes(numberToTest);
            Array.Reverse(arg);
            UInt64 result =
    			NonNativeEndianSystemWireBitConverter.ToUInt64(arg);
    		Assertion.AssertEquals("converted uint 64" , numberToTest, result);
        }    	    	    	
    	    	    	
    	[Test]
    	public void TestUInt64WToS() {
    	    AssertTestWToSUInt64((UInt64)1);
    	    AssertTestWToSUInt64((UInt64)258);
    	    AssertTestWToSUInt64(UInt64.MaxValue);
    	    AssertTestWToSUInt64(UInt64.MinValue);    	    
    	}
    	
        private void AssertTestWToSSingle(Single numberToTest) {
            byte[] arg = BitConverter.GetBytes(numberToTest);
            Array.Reverse(arg);
            Single result =
    			NonNativeEndianSystemWireBitConverter.ToSingle(arg);
    		Assertion.AssertEquals("converted single" , numberToTest, result);
        }    	    	    	    	
    	
    	[Test]
    	public void TestSingleWToS() {
    	    AssertTestWToSSingle((Single)1.0f);
    	    AssertTestWToSSingle((Single)0.01f);
    	    AssertTestWToSSingle(Single.MaxValue);
    	    AssertTestWToSSingle(Single.MinValue);
    	}    	
    	
    	private void AssertTestWToSDouble(Double numberToTest) {
            byte[] arg = BitConverter.GetBytes(numberToTest);
            Array.Reverse(arg);
            Double result =
    			NonNativeEndianSystemWireBitConverter.ToDouble(arg);
    		Assertion.AssertEquals("converted double" , numberToTest, result);
        }    	    	    	    	    	
    	
    	[Test]
    	public void TestDoubleWToS() {
    	    AssertTestWToSDouble((Double)1.0f);
    	    AssertTestWToSDouble((Double)0.01f);
    	    AssertTestWToSDouble(Double.MaxValue);
    	    AssertTestWToSDouble(Double.MinValue);
    	}
    	
    	private void AssertTestSToWInt16(Int16 numberToTest) {
            byte[] expected = BitConverter.GetBytes(numberToTest);
            Array.Reverse(expected);
            byte[] result =
    			NonNativeEndianSystemWireBitConverter.GetBytes(numberToTest);
    		ArrayAssertion.AssertByteArrayEquals("converted int16" , expected, result);
        }    	    	    	    	    	
    	
    	[Test]
    	public void TestInt16SToW() {
    	    AssertTestSToWInt16((Int16)1);
    	    AssertTestSToWInt16((Int16)258);
    	    AssertTestSToWInt16(Int16.MaxValue);
    	    AssertTestSToWInt16(Int16.MinValue);
    	}
    	
    	private void AssertTestSToWInt32(Int32 numberToTest) {
            byte[] expected = BitConverter.GetBytes(numberToTest);
            Array.Reverse(expected);
            byte[] result =
    			NonNativeEndianSystemWireBitConverter.GetBytes(numberToTest);
    		ArrayAssertion.AssertByteArrayEquals("converted int32" , expected, result);
        }    	    	    	    	    	
    	
    	[Test]
    	public void TestInt32SToW() {
    	    AssertTestSToWInt32((Int32)1);
    	    AssertTestSToWInt32((Int32)258);
    	    AssertTestSToWInt32(Int32.MaxValue);
    	    AssertTestSToWInt32(Int32.MinValue);
    	}
    	
    	private void AssertTestSToWInt64(Int64 numberToTest) {
            byte[] expected = BitConverter.GetBytes(numberToTest);
            Array.Reverse(expected);
            byte[] result =
    			NonNativeEndianSystemWireBitConverter.GetBytes(numberToTest);
    		ArrayAssertion.AssertByteArrayEquals("converted int64" , expected, result);
        }    	    	    	    	    	
    	
    	[Test]
    	public void TestInt64SToW() {
    	    AssertTestSToWInt64((Int64)1);
    	    AssertTestSToWInt64((Int64)258);
    	    AssertTestSToWInt64(Int64.MaxValue);
    	    AssertTestSToWInt64(Int64.MinValue);
    	}
    	
    	private void AssertTestSToWUInt16(UInt16 numberToTest) {
            byte[] expected = BitConverter.GetBytes(numberToTest);
            Array.Reverse(expected);
            byte[] result =
    			NonNativeEndianSystemWireBitConverter.GetBytes(numberToTest);
    		ArrayAssertion.AssertByteArrayEquals("converted uint16" , expected, result);
        }    	    	    	    	    	
    	
    	[Test]
    	public void TestUInt16SToW() {
    	    AssertTestSToWUInt16((UInt16)1);
    	    AssertTestSToWUInt16((UInt16)258);
    	    AssertTestSToWUInt16(UInt16.MaxValue);
    	    AssertTestSToWUInt16(UInt16.MinValue);
    	}

    	private void AssertTestSToWUInt32(UInt32 numberToTest) {
            byte[] expected = BitConverter.GetBytes(numberToTest);
            Array.Reverse(expected);
            byte[] result =
    			NonNativeEndianSystemWireBitConverter.GetBytes(numberToTest);
    		ArrayAssertion.AssertByteArrayEquals("converted uint32" , expected, result);
        }    	    	    	    	    	
    	
    	[Test]
    	public void TestUInt32SToW() {
    	    AssertTestSToWUInt32((UInt16)1);
    	    AssertTestSToWUInt32((UInt16)258);
    	    AssertTestSToWUInt32(UInt16.MaxValue);
    	    AssertTestSToWUInt32(UInt16.MinValue);
    	}    	
    	
    	private void AssertTestSToWUInt64(UInt64 numberToTest) {
            byte[] expected = BitConverter.GetBytes(numberToTest);
            Array.Reverse(expected);
            byte[] result =
    			NonNativeEndianSystemWireBitConverter.GetBytes(numberToTest);
    		ArrayAssertion.AssertByteArrayEquals("converted uint64" , expected, result);
        }    	    	    	    	    	
    	
    	[Test]
    	public void TestUInt64SToW() {
    	    AssertTestSToWUInt64((UInt64)1);
    	    AssertTestSToWUInt64((UInt64)258);
    	    AssertTestSToWUInt64(UInt64.MaxValue);
    	    AssertTestSToWUInt64(UInt64.MinValue);
    	}
    	
    	private void AssertTestSToWSingle(Single numberToTest) {
            byte[] expected = BitConverter.GetBytes(numberToTest);
            Array.Reverse(expected);
            byte[] result =
    			NonNativeEndianSystemWireBitConverter.GetBytes(numberToTest);
    		ArrayAssertion.AssertByteArrayEquals("converted single" , expected, result);
        }    	    	    	    	    	
    	
    	[Test]
    	public void TestSingleSToW() {
    	    AssertTestSToWSingle((Single)1.0f);
    	    AssertTestSToWSingle((Single)0.01f);
    	    AssertTestSToWSingle(Single.MaxValue);
    	    AssertTestSToWSingle(Single.MinValue);
    	}
    	
    	private void AssertTestSToWDouble(Double numberToTest) {
            byte[] expected = BitConverter.GetBytes(numberToTest);
            Array.Reverse(expected);
            byte[] result =
    			NonNativeEndianSystemWireBitConverter.GetBytes(numberToTest);
    		ArrayAssertion.AssertByteArrayEquals("converted double" , expected, result);
        }    	    	    	    	    	
    	
    	[Test]
    	public void TestDoubleSToW() {
    	    AssertTestSToWDouble((Double)1.0f);
    	    AssertTestSToWDouble((Double)0.01f);
    	    AssertTestSToWDouble(Double.MaxValue);
    	    AssertTestSToWDouble(Double.MinValue);
    	}
    	    	
    }

}

#endif
    

