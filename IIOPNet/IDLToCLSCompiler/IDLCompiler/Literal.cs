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
        
        /*    Literal Or(Literal toOrWith);
        Literal Xor(Literal toXorWith);
        Literal And(Literal toAndWith);
        Literal ShiftBy(Literal shift);
        Literal MultBy(Literal toMultWith);
        Literal Add(Literal toAddTo);
        Literal Sub(Literal toSubTo); */
        // void Negate();
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
        void EmitLoadValue(ILGenerator gen, Type targetType);

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

        public void EmitLoadValue(ILGenerator gen, Type targetType) {
            if (targetType.Equals(typeof(Single))) {
                Single val = Convert.ToSingle(m_value);
                gen.Emit(OpCodes.Ldc_R4, val);
            } else if (targetType.Equals(typeof(Double))) {
                gen.Emit(OpCodes.Ldc_R8, m_value);
            } else {
                throw new LiteralTypeMismatchException("floating point", targetType);
            }            
        }

        #endregion IMethods
        
    }

    public struct IntegerLiteral : Literal {

        #region IFields
        
        private Int64 m_value;

        #endregion IFields
        #region IConsturctors

        public IntegerLiteral(Int64 val) {
            m_value = val;
        }
    
        #endregion IConstructors
        #region IMethods
        
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
            return m_value;
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

        public void EmitLoadValue(ILGenerator gen, Type targetType) {
            if (targetType.Equals(typeof(Int16))) {
                if ((m_value < Int16.MinValue) || (m_value > Int16.MaxValue)) {
                    throw new LiteralTypeMismatchException("integer literal need more than 16bit, not assignable to short");
                }
                Int32 val = Convert.ToInt32(m_value);
                gen.Emit(OpCodes.Ldc_I4, val);
            } else if (targetType.Equals(typeof(Int32))) {
                if ((m_value < Int32.MinValue) || (m_value > Int32.MaxValue)) {
                    throw new LiteralTypeMismatchException("integer literal need more than 32bit, not assignable to long");
                }
                Int32 val = Convert.ToInt32(m_value);
                gen.Emit(OpCodes.Ldc_I4, val);
            } else if (targetType.Equals(typeof(Int64))) {
                gen.Emit(OpCodes.Ldc_I8, m_value);
            } else if (targetType.Equals(typeof(Byte))) {
                Int32 val;
                if ((m_value < 0) && (m_value >= SByte.MinValue)) {                    
                    Console.WriteLine("illegal const value found for octet " + m_value + "; please change");
                    // compensate illegal java idl generated for some octet vals
                    val = Convert.ToInt32(((Byte)m_value));
                } else if ((m_value < Byte.MinValue) || (m_value > Byte.MaxValue)) {                    
                    throw new LiteralTypeMismatchException("integer literal need more than 8bit, not assignable to octet");
                } else {
                    val = Convert.ToInt32(m_value);
                }
                gen.Emit(OpCodes.Ldc_I4, val);
            } else {
                throw new LiteralTypeMismatchException("integer", targetType);
            }
        }

        #endregion IMethods
        
    }

    public struct CharLiteral : Literal {

        #region IFields
        
        private Char m_value;

        #endregion IFields
        #region IConstructors
    
        public CharLiteral(Char val) {
            m_value = val;
        }

        #endregion IConstructors
        #region IMethods

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

        public void EmitLoadValue(ILGenerator gen, Type targetType) {
            if (!targetType.Equals(typeof(Char))) {
                throw new LiteralTypeMismatchException("char", targetType);
            }
            Int32 asInt32 = Convert.ToInt32(m_value);
            gen.Emit(OpCodes.Ldc_I4, asInt32);
        }

        #endregion IMethods
        
    }

    public struct StringLiteral : Literal {

        #region IFields
        
        private String m_value;

        #endregion IFields
        #region IConstructors
    
        public StringLiteral(String val) {
            m_value = val;
        }

        #endregion IConstructors
        #region IMethods

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

        public void EmitLoadValue(ILGenerator gen, Type targetType) {
            if (!targetType.Equals(typeof(String))) {
                throw new LiteralTypeMismatchException("string", targetType);
            }
            gen.Emit(OpCodes.Ldstr, m_value);
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

        public void EmitLoadValue(ILGenerator gen, Type targetType) {
            if (!targetType.Equals(typeof(Boolean))) {
                throw new LiteralTypeMismatchException("boolean", targetType);
            }
            if (m_value == false) {
                gen.Emit(OpCodes.Ldc_I4_0);
            } else {
                gen.Emit(OpCodes.Ldc_I4_1);
            }
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

        public void EmitLoadValue(ILGenerator gen, Type targetType) {
            if (!targetType.IsEnum) {
                throw new LiteralTypeMismatchException("enum", targetType);
            }
            // get the int value for the idl enum
            gen.Emit(OpCodes.Ldc_I4, (System.Int32)m_value);
        }

        #endregion IMethods
        
    }

    
}

