/* BaseTypesImpl.cs
 * 
 * Project: IIOP.NET
 * Mapping-Plugin
 * 
 * WHEN      RESPONSIBLE
 * 17.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2003 Dominic Ullmann
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

namespace java.lang {


    [Serializable]
    public abstract class NumberImpl : Number {
     
        public NumberImpl() {
        }    

    }


    [Serializable]
    public class _BooleanImpl : _Boolean {

        public _BooleanImpl() : base() {
        }
        
        public _BooleanImpl(System.Boolean val) : base() {
            m_value = val;
        }

        #region unneeded methods (for mapper)
        
/*        public override bool equals(object arg) {
            return false;
        }
        
        public override int hashCode() {
            return 0;
        }
        
        public override string toString__() {
            return m_value.ToString();
        }
        
        public override string toString__boolean(System.Boolean arg0) {
            return "";
        }
        
        public override java.lang._Boolean valueOf__CORBA_WStringValue(string arg0) {
            return null;
        }
        
        public override java.lang._Boolean  valueOf__boolean(System.Boolean arg) {
            return null;
        }
        
        public override System.Boolean getBoolean(string arg) {
            return false;
        } 

*/
        
        #endregion unneeded methods (for mapper)
        
        public System.Boolean booleanValue() {
            return m_value;
        }


        
    }

    [Serializable]
    public class _ByteImpl : _Byte {

        public _ByteImpl() : base() {
        }
        
        public _ByteImpl(System.Byte val) : base() {
            m_value = val;
        }

        #region unneeded methods (for mapper)
/*
        public override System.Int32 compareTo(object arg) {
            return 0;
        }
        
        public override System.Int32 compareTo__java_lang_Byte(java.lang._Byte arg) {
            return 0;
        }
        
        public override java.lang._Byte decode(string arg) {
            return null;
        }
        
        public override bool equals(object arg) {
            return false;
        }
        
        public override int hashCode() {
            return 0;
        }
        
        public override System.Byte  parseByte__CORBA_WStringValue(string arg) {
            return 0;
        }
        
        public override System.Byte  parseByte__CORBA_WStringValue__long(string arg0, System.Int32 arg1) {
            return 0;
        }
        
        public override string toString__() {
            return m_value.ToString();
        }
        
        public override string toString__octet(System.Byte arg0) {
            return "";
        }
        
        public override java.lang._Byte valueOf__CORBA_WStringValue(string arg0) {
            return null;
        }
        
        public override java.lang._Byte  valueOf__CORBA_WStringValue__long(string arg0, System.Int32 arg1) {
            return null;
        }
                
        public override System.Double doubleValue() {
            return (System.Double)m_value;
        }
        
        public override System.Single floatValue() {
            return (System.Single)m_value;
        }
        
        public override System.Int16 shortValue() {
            return (System.Int16)m_value;
        }
        
        public override System.Int32 intValue() {
            return (System.Int32)m_value;
        }
        
        public override System.Int64 longValue() {
            return (System.Int64)m_value;
        }

*/
        
        #endregion unneeded methods (for mapper)
        
        public System.Byte byteValue() {
            return m_value;
        }
        
    }    

    [Serializable]
    public class _ShortImpl : _Short {

        public _ShortImpl() : base() {
        }
        
        public _ShortImpl(System.Int16 val) : base() {
            m_value = val;
        }

        #region unneeded methods (for mapper)
/*
        public override System.Int32 compareTo(object arg) {
            return 0;
        }
        
        public override System.Int32 compareTo__java_lang_Short(java.lang._Short arg) {
            return 0;
        }
        
        public override java.lang._Short decode(string arg) {
            return null;
        }
        
        public override bool equals(object arg) {
            return false;
        }
        
        public override int hashCode() {
            return 0;
        }
        
        public override System.Int16  parseShort__CORBA_WStringValue(string arg) {
            return 0;
        }
        
        public override System.Int16  parseShort__CORBA_WStringValue__long(string arg0, System.Int32 arg1) {
            return 0;
        }
        
        public override string toString__() {
            return m_value.ToString();
        }
        
        public override string toString__short(System.Int16 arg0) {
            return "";
        }
        
        public override java.lang._Short valueOf__CORBA_WStringValue(string arg0) {
            return null;
        }
        
        public override java.lang._Short  valueOf__CORBA_WStringValue__long(string arg0, System.Int32 arg1) {
            return null;
        }
        
        public override System.Byte byteValue() {
            return (System.Byte)m_value;
        }
        
        public override System.Double doubleValue() {
            return (System.Double)m_value;
        }
        
        public override System.Single floatValue() {
            return (System.Single)m_value;
        }
        
        public override System.Int32 intValue() {
            return (System.Int32)m_value;
        }
        
        public override System.Int64 longValue() {
            return (System.Int64)m_value;
        }        
*/

        #endregion unneeded methods (for mapper)
        
        public System.Int16 shortValue() {
            return m_value;
        }
        
    }
    
        
    [Serializable]
    public class _LongImpl : _Long {
        
        public _LongImpl() : base() {
        }
        
        public _LongImpl(System.Int64 val) : base() {
            m_value = val;
        }
        
        public System.Int64 longValue() {
            return m_value;
        }
        
    }
    
    
    [Serializable]
    public class _IntegerImpl : _Integer {

        public _IntegerImpl() : base() {
        }
        
        public _IntegerImpl(System.Int32 val) : base() {
            m_value = val;
        }

        #region unneeded methods (for mapper)

/*

        public override System.Int32 compareTo(object arg) {
            return 0;
        }
        
        public override System.Int32 compareTo__java_lang_Integer(java.lang.Integer arg) {
            return 0;
        }
        
        public override java.lang.Integer decode(string arg) {
            return null;
        }
        
        public override bool equals(object arg) {
            return false;
        }
        
        public override int hashCode() {
            return 0;
        }
        
        public override System.Int32  parseInt__CORBA_WStringValue(string arg) {
            return 0;
        }
        
        public override System.Int32  parseInt__CORBA_WStringValue__long(string arg0, System.Int32 arg1) {
            return 0;
        }
        
        public override string toBinaryString(System.Int32 arg) {
            return null;
        }
        
        public override string toOctalString(System.Int32 arg) {
            return null;
        }

        public override string toHexString(System.Int32 arg) {
            return null;
        }
        
        public override java.lang.Integer getInteger__CORBA_WStringValue(string arg0) {
            return null;
        }
        
        public override java.lang.Integer getInteger__CORBA_WStringValue__java_lang_Integer(string arg0, java.lang.Integer arg1) {
            return null;
        }
        
        public override java.lang.Integer getInteger__CORBA_WStringValue__long(string arg0, System.Int32 arg1) {
            return null;
        }
        
      // new in java 1.4.2?
//        public override void appendTo(System.Int32 arg0, java.lang.StringBuffer arg1) {
//        }

        public override string toString__() {
            return m_value.ToString();
        }
        
        public override string toString__long(System.Int32 arg0) {
            return "";
        }
        
        public override string toString__long__long(System.Int32 arg0, System.Int32 arg1) {
            return "";
        }
        
        public override java.lang.Integer valueOf__CORBA_WStringValue(string arg0) {
            return null;
        }
        
        public override java.lang.Integer  valueOf__CORBA_WStringValue__long(string arg0, System.Int32 arg1) {
            return null;
        }
        
        public override System.Byte byteValue() {
            return (System.Byte)m_value;
        }
        
        public override System.Double doubleValue() {
            return (System.Double)m_value;
        }
        
        public override System.Single floatValue() {
            return (System.Single)m_value;
        }
        
        public override System.Int16 shortValue() {
            return (System.Int16)m_value;
        }
                
        public override System.Int64 longValue() {
            return (System.Int64)m_value;
        }
*/
        
        #endregion unneeded methods (for mapper)
        
        public System.Int32 intValue() {
            return m_value;
        }

    }
    
    
    [Serializable]
    public class _DoubleImpl : _Double {

        public _DoubleImpl() : base() {
        }
        
        public _DoubleImpl(System.Double val) : base() {
            m_value = val;
        }

        #region unneeded properties (for mapper)
        
/*
        public override bool infinite {
            get {
                return false;
            }
        }
        
        public override bool naN {
            get {
                return false;
            }
        }

*/
        
        #endregion unneeded properties (for mapper)
        #region unneeded methods (for mapper)
        
/*
        public override System.Int32 compare(System.Double arg0, System.Double arg1) {
            return 0;
        }
        
        public override System.Int32 compareTo(object arg) {
            return 0;
        }
        
        public override System.Int32 compareTo__java_lang_Double(java.lang._Double arg) {
            return 0;
        }
        
        public override bool equals(object arg) {
            return false;
        }
        
        public override int hashCode() {
            return 0;
        }
        
        public override string toString__() {
            return m_value.ToString();
        }
        
        public override string toString__double(System.Double arg0) {
            return "";
        }
        
        public override java.lang._Double valueOf(string arg0) {
            return null;
        }
        
        public override bool isInfinite(System.Double arg0) {
            return false;
        }
        
        public override bool isNaN(System.Double arg0) {
            return false;
        }
        
        public override System.Double parseDouble(string arg0) {
            return 0;
        }
        
        public override System.Double longBitsToDouble(System.Int64 arg0) {
            return 0;
        }
        
        public override System.Int64 doubleToLongBits(System.Double arg0) {
            return 0;
        }
        
        public override System.Int64 doubleToRawLongBits(System.Double arg0) {
            return 0;
        }
        
        public override System.Byte byteValue() {
            return (System.Byte)m_value;
        }
        
        public override System.Single floatValue() {
            return (System.Single)m_value;
        }
        
        public override System.Int32 intValue() {
            return (System.Int32)m_value;
        }
        
        public override System.Int64 longValue() {
            return (System.Int64)m_value;
        }
        
        public override System.Int16 shortValue() {
            return (System.Int16)m_value;
        }
*/
        
        #endregion unneeded methods (for mapper)
        
        public System.Double doubleValue() {
            return m_value;
        }
        
    }
    
    [Serializable]
    public class _FloatImpl : _Float {

        public _FloatImpl() : base() {
        }
        
        public _FloatImpl(System.Single val) : base() {
            m_value = val;
        }

        #region unneeded properties (for mapper)
        
/*
        public override bool infinite {
            get {
                return false;
            }
        }
        
        public override bool naN {
            get {
                return false;
            }
        }
*/
        
        #endregion unneeded properties (for mapper)
        #region unneeded methods (for mapper)
        
/*
        public override System.Int32 compare(System.Single arg0, System.Single arg1) {
            return 0;
        }
        
        public override System.Int32 compareTo(object arg) {
            return 0;
        }
        
        public override System.Int32 compareTo__java_lang_Float(java.lang._Float arg) {
            return 0;
        }
        
        public override bool equals(object arg) {
            return false;
        }
        
        public override int hashCode() {
            return 0;
        }
        
        public override string toString__() {
            return m_value.ToString();
        }
        
        public override string toString__float(System.Single arg0) {
            return "";
        }
        
        public override java.lang._Float valueOf(string arg0) {
            return null;
        }
        
        public override bool isInfinite(System.Single arg0) {
            return false;
        }
        
        public override bool isNaN(System.Single arg0) {
            return false;
        }
        
        public override System.Single parseFloat(string arg0) {
            return 0;
        }
        
        public override System.Single intBitsToFloat(System.Int32 arg0) {
            return 0;
        }
        
        public override System.Int32 floatToIntBits(System.Single arg0) {
            return 0;
        }
        
        public override System.Int32 floatToRawIntBits(System.Single arg0) {
            return 0;
        }
        
        public override System.Byte byteValue() {
            return (System.Byte)m_value;
        }
        
        public override System.Double doubleValue() {
            return (System.Double)m_value;
        }
        
        public override System.Int32 intValue() {
            return (System.Int32)m_value;
        }
        
        public override System.Int64 longValue() {
            return (System.Int64)m_value;
        }
        
        public override System.Int16 shortValue() {
            return (System.Int16)m_value;
        }
*/
        
        #endregion unneeded methods (for mapper)
        
        public System.Single floatValue() {
            return m_value;
        }
        
    }


    [Serializable]
    public class CharacterImpl : Character {

        public CharacterImpl() : base() {
        }
        
        public CharacterImpl(System.Char val) : base() {
            m_value = val;
        }

        #region unneeded methods (for mapper)

/*

        public override System.Int32 compareTo(object arg) {
            return 0;
        }
        
        public override System.Int32 compareTo__java_lang_Character(java.lang.Character arg) {
            return 0;
        }
        
        public override bool equals(object arg) {
            return false;
        }
        
        public override int hashCode() {
            return 0;
        }
        
        public override string toString__() {
            return m_value.ToString();
        }
        
        public override string toString__wchar(System.Char arg0) {
            return "";
        }
        
        public override System.Byte getDirectionality(char arg0) {
            return 0;
        }
        
        public override System.Int32  getNumericValue(char arg0) {
            return 0;
        }
        
        public override bool isDefined(char arg0) {
            return true;
        }

        public override bool isDigit(char arg0) {
            return false;
        }
        
        public override bool isISOControl(char arg0) {
            return false;
        }

        public override bool isIdentifierIgnorable(char arg0) {
            return false;
        }

        public override bool isJavaIdentifierPart(char arg0) {
            return false;
        }

        public override bool isJavaIdentifierStart(char arg0) {
            return false;
        }

        public override bool isJavaLetter(char arg0) {
            return false;
        }

        public override bool isJavaLetterOrDigit(char arg0) {
            return false;
        }

        public override bool isLetter(char arg0) {
            return false;
        }

        public override bool isLetterOrDigit(char arg0) {
            return false;
        }

        public override bool isLowerCase(char arg0) {
            return false;
        }

        public override bool isMirrored(char arg0) {
            return false;
        }

        public override bool isSpace(char arg0) {
            return false;
        }

        public override bool isSpaceChar(char arg0) {
            return false;
        }

        public override bool isTitleCase(char arg0) {
            return false;
        }

        public override bool isUnicodeIdentifierPart(char arg0) {
            return false;
        }

        public override bool isUnicodeIdentifierStart(char arg0) {
            return false;
        }
        
        public override bool isUpperCase(char arg0) {
            return false;
        }
                
        public override bool isWhitespace(char arg0) {
            return false;
        }
        
        public override char toTitleCase(char arg0) {
            return ' ';
        }
        
        public override char toUpperCase(char arg0) {
            return ' ';
        }
        
        public override char toLowerCase(char arg0) {
            return ' ';
        }
        
        public override char[] toUpperCaseCharArray(char arg0) {
            return null;
        }
        
        public override char toUpperCaseEx(char arg0) {
            return ' ';
        }
        
        public override System.Int32 findInCharMap(char arg0) {
            return 0;
        }
        
        public override char forDigit(System.Int32 arg0, System.Int32 arg1) {
            return ' ';
        }
        
        public override System.Int32 digit(char arg0, System.Int32 arg1) {
            return 0;
        }
        
        public override System.Int32  _getType(char arg0) {
            return 0;
        }
*/
        
        #endregion unneeded methods (for mapper)
        
        public System.Char charValue() {
            return m_value;
        }

    }

}
