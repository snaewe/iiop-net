/* ValueTypeImpls.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 09.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace Ch.Elca.Iiop.IntegrationTests {


    [Serializable]
    public class TestSerializableClassB1Impl : TestSerializableClassB1 {
    
    }

    [Serializable]
    public class TestSerializableClassB2Impl : TestSerializableClassB2 {
    
    }

    [Serializable]
    public class TestSerializableClassCImpl : TestSerializableClassC {    
    
        public override System.String Format() {
            return Msg;
        }
    }
    
    [Serializable]
    public class TestSerializableClassDImpl : TestSerializableClassD {    
    
    }
    
    [Serializable]
    public class TestSerializableClassEImpl : TestSerializableClassE {    
    
    }

    [Serializable]
    public class TestSerWithInnerImpl : TestSerWithInner {

        public TestSerWithInnerImpl() {
        }    
    
    }

    [Serializable]
    public class TestSerWithInner__AnInnerClassImpl : TestSerWithInner__AnInnerClass {    

        public TestSerWithInner__AnInnerClassImpl() {
        }    

        public TestSerWithInner__AnInnerClassImpl(TestSerWithInner arg) {
            m_thisU00240 = arg;
        }    

    
    }


    [Serializable]
    public class TestSerializableMixedValAndBaseImpl : TestSerializableMixedValAndBase {
    

    }
    
    [Serializable]
    public class TestRecursiveValTypeImpl : TestRecursiveValType {
    
    }

    [Serializable]
    public class J_TestStartByUnderscoreImpl : J_TestStartByUnderscore {
    
    }

    [Serializable]
    public class _InImpl : _In {
    
    }    
    
    
    [Serializable()]
    public class NamedValueImplImpl : NamedValueImpl {
        
        public NamedValueImplImpl() {
        }
        
        public NamedValueImplImpl(string arg0, int arg1) : 
                base(arg0, arg1) {
            m_name = arg0;
            m_value = arg1;
        }
        
        public override void setValue(int arg0) {
            m_value = arg0;
        }
        
        public override void setName(string arg0) {
            m_name = arg0;
        }
        
        public override int getValue() {
            return m_value;
        }
        
        public override string getName() {
            return m_name;
        }

        public override bool Equals(object other) {
            if (!(other is NamedValueImplImpl)) {
                return false;
            }
            NamedValueImplImpl otherCasted = (NamedValueImplImpl)other;
            return (m_value == otherCasted .m_value) && (m_name == otherCasted.m_name);            
        }

        public override int GetHashCode() {
            int result = m_value.GetHashCode();
            if (m_name != null) {
                result = result ^ m_name.GetHashCode();
            }
            return result;
        }

        
    }



}
