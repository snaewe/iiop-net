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
using Ch.Elca.Iiop.Idl;

namespace Ch.Elca.Iiop.IntegrationTests.MappingPlugin {


    [Serializable]
    public class TestSerializableClassB1Impl : TestSerializableClassB1 {

    
        public override bool Equals(object arg) {
            if ((arg == null) || (!(arg is TestSerializableClassB1Impl))) {
                return false;
            }
            return Msg.Equals(((TestSerializableClassB1Impl)arg).Msg);
        }

        public override int GetHashCode() {
            return Msg.GetHashCode();
        }

    
    }
    
           
    /// <summary>implementation for the corba value type</summary>
    [Serializable]
    public class CustomMappedSerializableImpl : CustomMappedSerializable {        

        public CustomMappedSerializableImpl() {
        }
        
        public CustomMappedSerializableImpl(string msg) : this() {
            m_msg = msg;
        }

        public override String msg {
            get {
                return m_msg;
            }
            set { 
                m_msg = value;
            }
        }

    }

    
    /// <summary>instance mapper for CustomSerializable (java) <-> CustomSerializableCls (.NET)
    public class CustomMappedSerializableMapper : ICustomMapper {
                    
        public object CreateClsForIdlInstance(object idlInstance) {
            CustomMappedSerializable source = (CustomMappedSerializable)idlInstance;
            return new CustomMappedSerializableCls(source.msg);
        }
        
        public object CreateIdlForClsInstance(object clsInstance) {
            CustomMappedSerializableCls source = (CustomMappedSerializableCls)clsInstance;
            return new CustomMappedSerializableImpl(source.Message);
        }
        
        
    }


}
