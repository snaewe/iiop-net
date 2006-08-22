/* MetadataGenerator.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 06.10.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Reflection;
using System.Reflection.Emit;

using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;

namespace parser {


    /// <summary>
    /// thrown if a wrong operend is used in an expression
    /// </summary>
    public class InvalidOperandInExpressionException : Exception {
        
        public InvalidOperandInExpressionException(string message) : base(message) {
        }
        
    }

    /// <summary>
    /// this exception is thrown, if a literal cannot be assigned to a constant of the specified type.
    /// </summary>
    public class LiteralTypeMismatchException : Exception {
        public LiteralTypeMismatchException(string literalType, Type fieldType) : 
            base(String.Format("The {0}-literal is not assignable to a constant with type {1}", literalType, fieldType)) {
        }

        public LiteralTypeMismatchException(string message) : base(message) {
        }
    }

    // TODO: add fixed point
    /// <summary>
    /// represents a literal in IDL
    /// </summary>
    public interface Literal {

        #region IMethods
        
        Literal Or(Literal toOrWith);
        Literal Xor(Literal toXorWith);
        Literal And(Literal toAndWith);
        Literal ShiftLeftBy(Literal shift);
        Literal ShiftRightBy(Literal shift);
        Literal MultBy(Literal toMultWith);
        Literal DivBy(Literal toDivBy);
        Literal ModBy(Literal toModBy);
        Literal Add(Literal toAddTo);
        Literal Sub(Literal toSubTo);
        void Negate();
        void InvertSign();
          
        object GetValue();
        
        Double GetFloatValue();
        Int64 GetIntValue();
        Char GetCharValue();
        String GetStringValue();
        Boolean GetBooleanValue();

        /// <summary>
        /// checks, if literal value is loadable as targetType and if yes, emits a load instruction
        /// </summary>
        /// <param name="gen">the generator to use for generating the load code</param>
        /// <param name="targetType">the type, the literal value should be assigned to</param>
        /// <param name="assignFromType">the type, the literal should be of; if not equal to targetType, the
        /// value should be casted from assignFromType to targetType, before loading</param>
        void EmitLoadValue(ILGenerator gen, Type targetType, Type assignFromType);

        /// <summary>
        /// gets the value usable to assign to targetType.
        /// </summary>
        /// <param name="targetType">the type, the literal value should be assigned to</param>
        /// <param name="assignFromType">the type, the literal should be of; if not equal to targetType, the
        /// value should be casted from assignFromType to targetType, before loading</param>
        /// <returns>the value of the suitable type</returns>
        object GetValueToAssign(Type targetType, Type assignFromType);

        /// <summary>
        /// checks, if this literal can be assigned to the given type.
        /// </summary>
        bool IsAssignableTo(TypeContainer type);

        #endregion IMethods

    }


    public struct FloatLiteral : Literal {

        #region IFields
        
        private double m_value;

        #endregion IFields
        #region IConsturctors

        public FloatLiteral(Double val) {
            m_value = val;
        }
    
        #endregion IConstructors
        #region IMethods
        
        public Literal Or(Literal toOrWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise or on float");
        }
        
        public Literal Xor(Literal toXorWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise xor on float");
        }
        
        public Literal And(Literal toAndWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise and on float");
        }
        
        public Literal ShiftRightBy(Literal shift) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise shift on float");
        }
        
        public Literal ShiftLeftBy(Literal shift) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise shift on float");
        }
        
        public Literal MultBy(Literal toMultWith) {
            return new FloatLiteral(m_value * toMultWith.GetFloatValue());
        }
        
        public Literal DivBy(Literal toDivBy) {
            return new FloatLiteral(m_value / toDivBy.GetFloatValue());
        }
        
        public Literal ModBy(Literal toModBy) {
            return new FloatLiteral(m_value % toModBy.GetFloatValue());
        }        
        
        public Literal Add(Literal toAddTo) {
             return new FloatLiteral(m_value + toAddTo.GetFloatValue());
        }
        
        public Literal Sub(Literal toSubTo) {
             return new FloatLiteral(m_value - toSubTo.GetFloatValue());
        }
        
        public void Negate() {
            throw new InvalidOperandInExpressionException("Cannot negate a float");
        }        
        
        public void InvertSign() {
            m_value = m_value * (-1.0);
        }
        
        public object GetValue() {
            return m_value;
        }

        public Double GetFloatValue() {
            return m_value;
        }
    
        public Int64 GetIntValue() {
            throw new InvalidOperandInExpressionException("require an integer operand, but found a float operand: " + m_value);
        }
        
        public Char GetCharValue() {
            throw new InvalidOperandInExpressionException("require a char operand, but found a float operand: " + m_value);
        }
        
        public String GetStringValue() {
            throw new InvalidOperandInExpressionException("require a string operand, but found a float operand: " + m_value);
        }
        public Boolean GetBooleanValue() {
            throw new InvalidOperandInExpressionException("require a boolean operand, but found a float operand: " + m_value);
        }

        public void EmitLoadValue(ILGenerator gen, Type targetType, Type assignFromType) {
            if (targetType.Equals(ReflectionHelper.SingleType)) {
                Single val = Convert.ToSingle(m_value);
                gen.Emit(OpCodes.Ldc_R4, val);
//                gen.Emit(OpCodes.Ldc_R4, (float)m_value);
            } else if (targetType.Equals(ReflectionHelper.DoubleType)) {
                gen.Emit(OpCodes.Ldc_R8, m_value);
            } else {
                throw new LiteralTypeMismatchException("floating point", targetType);
            }            
        }

        public object GetValueToAssign(Type targetType, Type assignFromType) {
            if (targetType.Equals(ReflectionHelper.SingleType)) {
                return Convert.ToSingle(m_value);
            } else if (targetType.Equals(ReflectionHelper.DoubleType)) {
                return m_value;
            } else {
                throw new LiteralTypeMismatchException("floating point", targetType);
            }
        }

        public bool IsAssignableTo(TypeContainer type) {
            return type.GetCompactClsType().Equals(ReflectionHelper.SingleType) ||
                   type.GetCompactClsType().Equals(ReflectionHelper.DoubleType);
        }

        #endregion IMethods
        
    }

    public struct IntegerLiteral : Literal {

        #region IFields
        
        private Decimal m_value;

        #endregion IFields
        #region IConsturctors

        /// <summary>
        /// use decimal, because no bigint class in .NET ... (decimal has bigger range than Int64 and UInt64)
        /// </summary>
        /// <param name="val"></param>
        public IntegerLiteral(Decimal val) {
            m_value = val;
        }
    
        #endregion IConstructors
        #region IMethods
        
        public Literal Or(Literal toOrWith) {
            return new IntegerLiteral(GetIntValue() | toOrWith.GetIntValue());
        }        
        
        public Literal Xor(Literal toXorWith) {
            return new IntegerLiteral(GetIntValue() ^ toXorWith.GetIntValue());
        }
        
        public Literal And(Literal toAndWith) {            
            return new IntegerLiteral(GetIntValue() & toAndWith.GetIntValue());
        }
        
        public Literal ShiftRightBy(Literal shift) {
            if ((shift.GetIntValue() >= 64) || (shift.GetIntValue() < 0)) {
                throw new InvalidOperandInExpressionException("Invalid shift by argument " + 
                    shift.GetIntValue() + "; shift is only possible with values in the range 0 <= shift < 63.");
            }
            return new IntegerLiteral(GetIntValue() >> (int)shift.GetIntValue());
        }
        
        public Literal ShiftLeftBy(Literal shift) {
            if ((shift.GetIntValue() >= 64) || (shift.GetIntValue() < 0)) {
                throw new InvalidOperandInExpressionException("Invalid shift by argument " + 
                    shift.GetIntValue() + "; shift is only possible with values in the range 0 <= shift < 63.");
            }
            return new IntegerLiteral(GetIntValue() << (int)shift.GetIntValue());
        }
        
        public Literal MultBy(Literal toMultWith) {
            return new IntegerLiteral(m_value * toMultWith.GetIntValue());
        }
        
        public Literal DivBy(Literal toDivBy) {
            return new IntegerLiteral(m_value / toDivBy.GetIntValue());
        }
        
        public Literal ModBy(Literal toModBy) {
            return new IntegerLiteral(m_value % toModBy.GetIntValue());
        }        
        
        public Literal Add(Literal toAddTo) {
            return new IntegerLiteral(m_value + toAddTo.GetIntValue());
        }
        
        public Literal Sub(Literal toSubTo) {
            return new IntegerLiteral(m_value - toSubTo.GetIntValue());
        }
        
        public void Negate() {
            m_value = ~ GetIntValue();
        }        
        
        public void InvertSign() {
            m_value = m_value * (-1);
        }
        
        public object GetValue() {
            return m_value;
        }
        
        public Double GetFloatValue() {
            throw new InvalidOperandInExpressionException("require an float operand, but found a integer operand: " + m_value);
        }
    
        public Int64 GetIntValue() {
            if (m_value >= Int64.MinValue && m_value <= Int64.MaxValue) {
                return Convert.ToInt64(m_value);
            } else if (m_value >= UInt64.MinValue && m_value <= UInt64.MaxValue) {
                UInt64 asUint64 = Convert.ToUInt64(m_value);
                return unchecked((Int64)asUint64); // do an unchecked cast, overflow no issue here
            } else {
                throw new LiteralTypeMismatchException("integer literal need more than 64bit, not convertible to int64 value");                
            }
        }
        
        public Char GetCharValue() {
            throw new InvalidOperandInExpressionException("require a char operand, but found an integer operand: " + m_value);
        }
        
        public String GetStringValue() {
            throw new InvalidOperandInExpressionException("require a string operand, but found an integer operand: " + m_value);
        }

        public Boolean GetBooleanValue() {
            throw new InvalidOperandInExpressionException("require a boolean operand, but found an integer operand: " + m_value);
        }

        private object ConvertToInt16(Type targetType, Type assignFromType) {
            Int16 val;
            if (assignFromType.Equals(typeof(System.UInt16))) {
                // handling to uint -> int conversion of the mapping
                if ((m_value < UInt16.MinValue) || (m_value > UInt16.MaxValue)) {
                    throw new LiteralTypeMismatchException("integer literal need more than 16bit, not assignable to short");
                }
                System.UInt16 asUint16 = Convert.ToUInt16(m_value);
                val = unchecked((System.Int16)asUint16); // cast to int16; do an unchecked cast, overflow no issue here
            } else {
                if ((m_value < Int16.MinValue) || (m_value > Int16.MaxValue)) {
                    throw new LiteralTypeMismatchException("integer literal need more than 16bit, not assignable to short");
                }
                val = Convert.ToInt16(m_value);
            }
            return val;
        }

        private object ConvertToInt32(Type targetType, Type assignFromType) {
            System.Int32 val;
            if (assignFromType.Equals(typeof(System.UInt32))) {
                // handling to uint -> int conversion of the mapping
                if ((m_value < UInt32.MinValue) || (m_value > UInt32.MaxValue)) {
                    throw new LiteralTypeMismatchException("integer literal need more than 16bit, not assignable to short");
                }
                UInt32 asUint32 = Convert.ToUInt32(m_value);
                val = unchecked((System.Int32)asUint32); // cast to int32; do an unchecked cast, overflow no issue here
            } else {
                if ((m_value < Int32.MinValue) || (m_value > Int32.MaxValue)) {
                    throw new LiteralTypeMismatchException("integer literal need more than 32bit, not assignable to long");
                }
                val = Convert.ToInt32(m_value);
            }
            return val;
        }

        private object ConvertToInt64(Type targetType, Type assignFromType) {
            Int64 val;
            if (assignFromType.Equals(typeof(System.UInt64))) {
                if ((m_value < UInt64.MinValue) || (m_value > UInt64.MaxValue)) {
                    throw new LiteralTypeMismatchException("integer literal need more than 64bit, not assignable to ulong long");
                }
                UInt64 asUint64 = Convert.ToUInt64(m_value);
                val = unchecked((Int64)asUint64); // cast to int64; do an unchecked cast, overflow no issue here
            } else {
                if ((m_value < Int64.MinValue) || (m_value > Int64.MaxValue)) {
                    throw new LiteralTypeMismatchException("integer literal need more than 64bit, not assignable to long long");
                }
                val = Convert.ToInt64(m_value);
            }
            return val;
        }

        private object ConvertToByte(Type targetType, Type assignFromType) {
            Byte val;
            if ((m_value < 0) && (m_value >= SByte.MinValue)) {
                Console.WriteLine("illegal const value found for octet " + m_value + "; please change");
                // compensate illegal java idl generated for some octet vals
                SByte asSByte = Convert.ToSByte(m_value);
                val = unchecked((Byte)asSByte); // cast to byte; do an unchecked cast, overflow no issue here
            } else if ((m_value < Byte.MinValue) || (m_value > Byte.MaxValue)) {
                throw new LiteralTypeMismatchException("integer literal need more than 8bit, not assignable to octet");
            } else {
                val = (Byte)m_value;
            }
            return val;
        }

        private void EmitLoadInt16(ILGenerator gen, Type targetType, Type assignFromType) {
            System.Int32 val = Convert.ToInt32(ConvertToInt16(targetType, assignFromType));
            gen.Emit(OpCodes.Ldc_I4, val);
        }
        
        private void EmitLoadInt32(ILGenerator gen, Type targetType, Type assignFromType) {
            System.Int32 val = Convert.ToInt32(ConvertToInt32(targetType, assignFromType));
            gen.Emit(OpCodes.Ldc_I4, val);            
        }
        
        private void EmitLoadInt64(ILGenerator gen, Type targetType, Type assignFromType) {
            Int64 val = Convert.ToInt64(ConvertToInt64(targetType, assignFromType));            
            gen.Emit(OpCodes.Ldc_I8, val);
        }
        
        private void EmitLoadByte(ILGenerator gen, Type targetType, Type assignFromType) {
            Int32 val = Convert.ToInt32(ConvertToByte(targetType, assignFromType));
            gen.Emit(OpCodes.Ldc_I4, val);
        }

        public void EmitLoadValue(ILGenerator gen, Type targetType, Type assignFromType) {
            if (targetType.Equals(typeof(Int16))) {
                EmitLoadInt16(gen, targetType, assignFromType);
            } else if (targetType.Equals(typeof(Int32))) {
                EmitLoadInt32(gen, targetType, assignFromType);
            } else if (targetType.Equals(typeof(Int64))) {
                EmitLoadInt64(gen, targetType, assignFromType);                
            } else if (targetType.Equals(typeof(Byte))) {
                EmitLoadByte(gen, targetType, assignFromType);
            } else {
                throw new LiteralTypeMismatchException("integer", targetType);
            }
        }

        public object GetValueToAssign(Type targetType, Type assignFromType) {
            if (targetType.Equals(typeof(Int16))) {
                return ConvertToInt16(targetType, assignFromType);
            } else if (targetType.Equals(typeof(Int32))) {
                return ConvertToInt32(targetType, assignFromType);
            } else if (targetType.Equals(typeof(Int64))) {
                return ConvertToInt64(targetType, assignFromType);
            } else if (targetType.Equals(typeof(Byte))) {
                return ConvertToByte(targetType, assignFromType);
            } else {
                throw new LiteralTypeMismatchException("integer", targetType);
            }
        }

        public bool IsAssignableTo(TypeContainer type) {
            if (type.GetCompactClsType().Equals(ReflectionHelper.ByteType)) {
                return m_value >= SByte.MinValue && m_value <= Byte.MaxValue;    
            } else if (type.GetCompactClsType().Equals(ReflectionHelper.Int16Type)) {
                if (type.GetAssignableFromType().Equals(typeof(UInt16))) {
                    return m_value >= UInt16.MinValue && m_value <= UInt16.MaxValue;
                } else {
                    return m_value >= Int16.MinValue && m_value <= Int16.MaxValue;
                }
            } else if (type.GetCompactClsType().Equals(ReflectionHelper.Int32Type)) {
                if (type.GetAssignableFromType().Equals(typeof(UInt32))) {
                    return m_value >= UInt32.MinValue && m_value <= UInt32.MaxValue;
                } else {
                    return m_value >= Int32.MinValue && m_value <= Int32.MaxValue;
                }                
            } else if (type.GetCompactClsType().Equals(ReflectionHelper.Int64Type)) {
                if (type.GetAssignableFromType().Equals(typeof(UInt64))) {
                    return m_value >= UInt64.MinValue && m_value <= UInt64.MaxValue;
                } else {
                    return m_value >= Int64.MinValue && m_value <= Int64.MaxValue;
                }                
            } else {
                return false;
            }
        }        

        #endregion IMethods
        
    }

    public struct CharLiteral : Literal {

        #region IFields
        
        private Char m_value;
        
        /// <summary>constructed from a wide char literal?</summary>
        private bool m_isWide;

        #endregion IFields
        #region IConstructors
    
        public CharLiteral(Char val, bool isWide) {
            m_value = val;
            m_isWide = isWide;
        }

        #endregion IConstructors
        #region IMethods
        
        public Literal Or(Literal toOrWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise or on char");
        }
        
        public Literal Xor(Literal toXorWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise xor on char");
        }
        
        public Literal And(Literal toAndWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise and on char");
        }
        
        public Literal ShiftRightBy(Literal shift) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise shift on char");
        }
        
        public Literal ShiftLeftBy(Literal shift) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise shift on char");
        }
        
        public Literal MultBy(Literal toMultWith) {
            throw new InvalidOperandInExpressionException("Cannot use mult with char");            
        }
        
        public Literal DivBy(Literal toDivBy) {
            throw new InvalidOperandInExpressionException("Cannot use div with char");
        }
        
        public Literal ModBy(Literal toModBy) {
            throw new InvalidOperandInExpressionException("Cannot use mod with char");
        }        
        
        public Literal Add(Literal toAddTo) {
            throw new InvalidOperandInExpressionException("Cannot use add on char");
        }        
        
        public Literal Sub(Literal toSubTo) {
            throw new InvalidOperandInExpressionException("Cannot use sub on char");
        }
        
        public void Negate() {
            throw new InvalidOperandInExpressionException("Cannot negate a char");
        }        

        public void InvertSign() {
            throw new InvalidOperationException("unary operator - not allowed for characters");
        }

        public object GetValue() {
            return m_value;
        }
        
        public Double GetFloatValue() {
            throw new InvalidOperandInExpressionException("require a float operand, but found a char operand: " + m_value);
        }
    
        public Int64 GetIntValue() {
            throw new InvalidOperandInExpressionException("require an integer operand, but found a char operand: " + m_value);
        }
        
        public Char GetCharValue() {
            return m_value;
        }
        
        public String GetStringValue() {
            throw new InvalidOperandInExpressionException("require a string operand, but found a char operand: " + m_value);
        }
        public Boolean GetBooleanValue() {
            throw new InvalidOperandInExpressionException("require a boolean operand, but found a char operand: " + m_value);
        }

        public object GetValueToAssign(Type targetType, Type assignFromType) {
            if (!targetType.Equals(ReflectionHelper.CharType)) {
                throw new LiteralTypeMismatchException("char", targetType);
            }
            return m_value;
        }

        public void EmitLoadValue(ILGenerator gen, Type targetType, Type assignFromType) {
            Int32 asInt32 = Convert.ToInt32(GetValueToAssign(targetType, assignFromType));
            gen.Emit(OpCodes.Ldc_I4, asInt32);
        }
        
        public bool IsAssignableTo(TypeContainer type) {
            return type.GetCompactClsType().Equals(ReflectionHelper.CharType);                   
        }        

        #endregion IMethods
        
    }

    public struct StringLiteral : Literal {

        #region IFields
        
        private String m_value;
        
        /// <summary>constructed from a wide string literal?</summary>
        private bool m_isWide;

        #endregion IFields
        #region IConstructors
    
        public StringLiteral(String val, bool isWide) {
            m_value = val;
            m_isWide = isWide;
        }

        #endregion IConstructors
        #region IMethods
        
        public Literal Or(Literal toOrWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise or on string");
        }        
        
        public Literal Xor(Literal toXorWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise xor on string");
        }
        
        public Literal And(Literal toAndWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise and on string");
        }
        
        public Literal ShiftRightBy(Literal shift) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise shift on string");
        }
        
        public Literal ShiftLeftBy(Literal shift) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise shift on string");
        }
        
        public Literal MultBy(Literal toMultWith) {
            throw new InvalidOperandInExpressionException("Cannot use mult with string");            
        }
        
        public Literal DivBy(Literal toDivBy) {
            throw new InvalidOperandInExpressionException("Cannot use div with string");
        }
        
        public Literal ModBy(Literal toModBy) {
            throw new InvalidOperandInExpressionException("Cannot use mod with string");
        }        
        
        public Literal Add(Literal toAddTo) {
            throw new InvalidOperandInExpressionException("Cannot use add on string");
        }        
        
        public Literal Sub(Literal toSubTo) {
            throw new InvalidOperandInExpressionException("Cannot use sub on string");
        }
        
        public void Negate() {
            throw new InvalidOperandInExpressionException("Cannot negate a string");
        }        

        public void InvertSign() {
            throw new InvalidOperationException("unary operator - not allowed for strings");
        }

        public object GetValue() {
            return m_value;
        }
        
        public Double GetFloatValue() {
            throw new InvalidOperandInExpressionException("require a float operand, but found a string operand: " + m_value);
        }
    
        public Int64 GetIntValue() {
            throw new InvalidOperandInExpressionException("require an integer operand, but found a string operand: " + m_value);
        }
        
        public Char GetCharValue() {
            throw new InvalidOperandInExpressionException("require a char operand, but found a string operand: " + m_value);
        }
        
        public String GetStringValue() {
            return m_value;
        }
        public Boolean GetBooleanValue() {
            throw new InvalidOperandInExpressionException("require a boolean operand, but found a string operand: " + m_value);
        }

        public object GetValueToAssign(Type targetType, Type assignFromType) {
            if (!targetType.Equals(ReflectionHelper.StringType)) {
                throw new LiteralTypeMismatchException("string", targetType);
            }
            return m_value;
        }

        public void EmitLoadValue(ILGenerator gen, Type targetType, Type assignFromType) {
            gen.Emit(OpCodes.Ldstr, Convert.ToString(GetValueToAssign(targetType, assignFromType)));
        }
        
        public bool IsAssignableTo(TypeContainer type) {
            return type.GetCompactClsType().Equals(ReflectionHelper.StringType);                   
        }                

        #endregion IMethods
        
    }

    public struct BooleanLiteral : Literal {

        #region IFields
        
        private Boolean m_value;

        #endregion IFields
        #region IConstructors
    
        public BooleanLiteral(Boolean val) {
            m_value = val;
        }

        #endregion IConstructors
        #region IMethods
        
        public Literal Or(Literal toOrWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise or on bool");
        }        
        
        public Literal Xor(Literal toXorWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise xor on bool");
        }
        
        public Literal And(Literal toAndWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise and on bool");
        }
        
        public Literal ShiftRightBy(Literal shift) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise shift on bool");
        }
        
        public Literal ShiftLeftBy(Literal shift) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise shift on bool");
        }
        
        public Literal MultBy(Literal toMultWith) {
            throw new InvalidOperandInExpressionException("Cannot use mult with bool");            
        }
        
        public Literal DivBy(Literal toDivBy) {
            throw new InvalidOperandInExpressionException("Cannot use div with bool");
        }
        
        public Literal ModBy(Literal toModBy) {
            throw new InvalidOperandInExpressionException("Cannot use mod with bool");
        }        
        
        public Literal Add(Literal toAddTo) {
            throw new InvalidOperandInExpressionException("Cannot use add on bool");
        }

        public Literal Sub(Literal toSubTo) {
            throw new InvalidOperandInExpressionException("Cannot use sub on bool");
        }        

        public void Negate() {
            m_value = !m_value;
        }        
        
        public void InvertSign() {
            throw new InvalidOperationException("unary operator - not allowed for boolean");
        }
        
        public object GetValue() {
            return m_value;
        }
        
        public Double GetFloatValue() {
            throw new InvalidOperandInExpressionException("require a float operand, but found a boolean operand: " + m_value);
        }
    
        public Int64 GetIntValue() {
            throw new InvalidOperandInExpressionException("require an integer operand, but found a boolean operand: " + m_value);
        }
        
        public Char GetCharValue() {
            throw new InvalidOperandInExpressionException("require a char operand, but found a boolean operand: " + m_value);
        }
        
        public String GetStringValue() {
            throw new InvalidOperandInExpressionException("require a string operand, but found a boolean operand: " + m_value);
        }
        public Boolean GetBooleanValue() {
            return m_value;
        }

        public object GetValueToAssign(Type targetType, Type assignFromType) {
            if (!targetType.Equals(ReflectionHelper.BooleanType)) {
                throw new LiteralTypeMismatchException("boolean", targetType);
            }
            return m_value;
        }

        public void EmitLoadValue(ILGenerator gen, Type targetType, Type assignFromType) {
            if (!targetType.Equals(ReflectionHelper.BooleanType)) {
                throw new LiteralTypeMismatchException("boolean", targetType);
            }
            if (m_value == false) {
                gen.Emit(OpCodes.Ldc_I4_0);
            } else {
                gen.Emit(OpCodes.Ldc_I4_1);
            }
        }
        
        public bool IsAssignableTo(TypeContainer type) {
            return type.GetCompactClsType().Equals(ReflectionHelper.BooleanType);                   
        }                        

        #endregion IMethods
        
    }

    public struct EnumValLiteral : Literal {

        #region IFields
        
        private object m_value;

        #endregion IFields
        #region IConsturctors

        public EnumValLiteral(object val) {
            if (!val.GetType().IsEnum) {
                throw new LiteralTypeMismatchException("enum literal can't be created from a non-enum");
            }
            m_value = val;
        }
    
        #endregion IConstructors
        #region IMethods
        
        public Literal Or(Literal toOrWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise or on enum");
        }
        
        public Literal Xor(Literal toXorWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise xor on enum");
        }
        
        public Literal And(Literal toAndWith) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise and on enum");
        }
        
        public Literal ShiftRightBy(Literal shift) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise shift on enum");
        }        
        
        public Literal ShiftLeftBy(Literal shift) {
            throw new InvalidOperandInExpressionException("Cannot use bitwise shift on enum");
        }
        
        public Literal MultBy(Literal toMultWith) {
            throw new InvalidOperandInExpressionException("Cannot use mult with enum");            
        }
        
        public Literal DivBy(Literal toDivBy) {
            throw new InvalidOperandInExpressionException("Cannot use div with enum");
        }
        
        public Literal ModBy(Literal toModBy) {
            throw new InvalidOperandInExpressionException("Cannot use mod with enum");
        }
        
        public Literal Add(Literal toAddTo) {
            throw new InvalidOperandInExpressionException("Cannot use add on enum");
        }        
        
        public Literal Sub(Literal toSubTo) {
            throw new InvalidOperandInExpressionException("Cannot use sub on enum");
        }
        
        public void Negate() {
            throw new InvalidOperandInExpressionException("Cannot negate an enum");
        }        
        
        public void InvertSign() {
            throw new InvalidOperationException("unary operator - not allowed for enum value");
        }
        
        public object GetValue() {
            return m_value;
        }
        
        public Double GetFloatValue() {
            throw new InvalidOperandInExpressionException("require an double operand, but found an enum val operand: " + m_value);
        }
    
        public Int64 GetIntValue() {
            throw new InvalidOperandInExpressionException("require an integer operand, but found an enum val operand: " + m_value);
        }
        
        public Char GetCharValue() {
            throw new InvalidOperandInExpressionException("require a char operand, but found an enum operand: " + m_value);
        }
        
        public String GetStringValue() {
            throw new InvalidOperandInExpressionException("require a string operand, but found an enum operand: " + m_value);
        }
        public Boolean GetBooleanValue() {
            throw new InvalidOperandInExpressionException("require a boolean operand, but found an enum operand: " + m_value);
        }

        public object GetValueToAssign(Type targetType, Type assignFromType) {
            if (!targetType.IsEnum) {
                throw new LiteralTypeMismatchException("enum", targetType);
            }
            return m_value;
        }

        public void EmitLoadValue(ILGenerator gen, Type targetType, Type assignFromType) {
            // get the int value for the idl enum
            gen.Emit(OpCodes.Ldc_I4, (System.Int32)GetValueToAssign(targetType, assignFromType));
        }
        
        public bool IsAssignableTo(TypeContainer type) {
            return type.GetCompactClsType().IsEnum;
        }                        

        #endregion IMethods
        
    }

    
}

