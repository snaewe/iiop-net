/* StringToIntConverter.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 07.03.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2004 Dominic Ullmann
 *
 * Copyright 2004 ELCA Informatique SA
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

namespace parser
{


    /// <summary>calculates a integer value from it's string representation</summary>
    internal sealed class StringToIntConverter
    {

        #region Constants

        private const int SUPPORTED_MIN_BASIS = 2;
        private const int SUPPORTED_MAX_BASIS = 36; // Z represents 35 (basis 10)

        #endregion Constants
        #region IConstructors

        private StringToIntConverter()
        {
        }

        #endregion IConstructors
        #region SMethods

        /// <summary>
        /// convert the string representation of a number to the corresponding Int64.
        /// </summary>
        public static Decimal Parse(string val, int basis)
        {
            string valueToParse = val;
            if ((basis < SUPPORTED_MIN_BASIS) ||
                (basis > SUPPORTED_MAX_BASIS))
            {
                throw new ArgumentException("basis not supported: " + basis);
            }

            valueToParse = valueToParse.Trim(); // ignore leading and trailing spaces

            bool negative = false;
            if (valueToParse.StartsWith("-"))
            {
                negative = true;
                valueToParse = valueToParse.Substring(1);
                if (valueToParse.Length == 0)
                {
                    throw new ArgumentException("invalid number: " + val);
                }
            }

            Decimal result = 0;
            Decimal currentDigitBaseVal = 1;
            for (int i = valueToParse.Length - 1; i >= 0; i--)
            {
                // check here, because for large numbers, an overflow of currentDigitBaseVal 
                // may be possible at parse end, without a problem for parsed number itself
                if (currentDigitBaseVal < 0)
                {
                    throw CreateOutOfRangeException(val, negative);
                }

                Decimal currentDigit = ParseChar(valueToParse[i], basis);
                if (!negative)
                {
                    result += currentDigit * currentDigitBaseVal;
                    if (result < 0)
                    {
                        throw CreateOutOfRangeException(val, negative);
                    }
                }
                else
                {
                    // make sure, that we don't get into trouble for Int64.MinValue
                    result -= currentDigit * currentDigitBaseVal;
                    if (result > 0)
                    {
                        throw CreateOutOfRangeException(val, negative);
                    }
                }
                // calculate value of next digit
                currentDigitBaseVal = currentDigitBaseVal * basis;
            }

            return result;
        }

        private static Exception CreateOutOfRangeException(string val, bool isNeg)
        {
            if (!isNeg)
            {
                return new ArgumentException("val is too big to parse: " + val);
            }
            else
            {
                return new ArgumentException("val is too small to parse: " + val);
            }
        }

        private static Int64 ParseChar(char ch, int basis)
        {
            if (!Char.IsLetterOrDigit(ch))
            {
                throw new ArgumentException("invalid character in value: " + ch);
            }
            if (Char.IsDigit(ch))
            {
                return ((int)ch) - ((int)'0');
            }
            else
            {
                // calculate digit value for letter A - Z / a - z:
                Char asUpper = Char.ToUpper(ch);
                return 10 + (((int)asUpper) - ((int)'A'));
            }

        }

        #endregion SMethods




    }


}


#if UnitTest


namespace Ch.Elca.Iiop.IdlCompiler.Tests
{

    using System;
    using NUnit.Framework;
    using parser;

    /// <summary>
    /// Unit-tests for testing assembly generation
    /// for IDL
    /// </summary>
    [TestFixture]
    public class StringToIntConverterTest
    {

        [SetUp]
        public void SetupEnvironment()
        {

        }

        [TearDown]
        public void TearDownEnvironment()
        {

        }

        [Test]
        public void TestBasis10()
        {
            Assert.AreEqual(0, StringToIntConverter.Parse("0", 10), "parse error");

            Assert.AreEqual(1, StringToIntConverter.Parse("1", 10), "parse error");
            Assert.AreEqual(11, StringToIntConverter.Parse("11", 10), "parse error");
            Assert.AreEqual(10001, StringToIntConverter.Parse("10001", 10), "parse error");
            Assert.AreEqual(Int64.MaxValue, StringToIntConverter.Parse(Int64.MaxValue.ToString(), 10), "parse error");
            Assert.AreEqual(UInt64.MaxValue, StringToIntConverter.Parse(UInt64.MaxValue.ToString(), 10), "parse error");

            Assert.AreEqual(-1, StringToIntConverter.Parse("-1", 10), "parse error");
            Assert.AreEqual(-11, StringToIntConverter.Parse("-11", 10), "parse error");
            Assert.AreEqual(-10001, StringToIntConverter.Parse("-10001", 10), "parse error");
            Assert.AreEqual(Int64.MinValue, StringToIntConverter.Parse(Int64.MinValue.ToString(), 10), "parse error");
            Assert.AreEqual(UInt64.MinValue, StringToIntConverter.Parse(UInt64.MinValue.ToString(), 10), "parse error");
        }

        [Test]
        public void TestBasis16()
        {
            Assert.AreEqual(0, StringToIntConverter.Parse("0", 16), "parse error");

            Assert.AreEqual(1, StringToIntConverter.Parse("1", 16), "parse error");
            Assert.AreEqual(241, StringToIntConverter.Parse("F1", 16), "parse error");
            Assert.AreEqual(983041, StringToIntConverter.Parse("F0001", 16), "parse error");
            Assert.AreEqual(Int64.MaxValue, StringToIntConverter.Parse("7FFFFFFFFFFFFFFF", 16), "parse error");

            Assert.AreEqual(-1, StringToIntConverter.Parse("-1", 16), "parse error");
            Assert.AreEqual(-241, StringToIntConverter.Parse("-F1", 16), "parse error");
            Assert.AreEqual(-983041, StringToIntConverter.Parse("-F0001", 16), "parse error");
            Assert.AreEqual(Int64.MinValue, StringToIntConverter.Parse("-8000000000000000", 16), "parse error");
        }

        [Test]
        public void TestBasis8()
        {
            Assert.AreEqual(0, StringToIntConverter.Parse("0", 8), "parse error");

            Assert.AreEqual(1, StringToIntConverter.Parse("1", 8), "parse error");
            Assert.AreEqual(57, StringToIntConverter.Parse("71", 8), "parse error");

            Assert.AreEqual(-1, StringToIntConverter.Parse("-1", 8), "parse error");
            Assert.AreEqual(-57, StringToIntConverter.Parse("-71", 8), "parse error");
        }

    }

}


#endif
