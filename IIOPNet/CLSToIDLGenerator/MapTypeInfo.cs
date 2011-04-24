/* MapTypeInfo.cs
 * 
 * Project: IIOP.NET
 * CLSToIDLGenerator
 * 
 * WHEN      RESPONSIBLE
 * 07.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Collections;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.Idl {

    /// <summary>helper class, containing Type and Attriutes on Param/Field/...</summary>
    internal class MapTypeInfo {

        #region IFields

        private Type m_type;
        private AttributeExtCollection m_attributes;
        private bool m_isFwdDeclPossible;

        #endregion IFields
        #region IConstructors
        
        public MapTypeInfo(Type type, Util.AttributeExtCollection attributes, bool isFwdDeclPossible) {
            if ((type == null) || (attributes == null)) {
                throw new ArgumentException("type and attributes must be != null");
            }
            m_type = type;
            m_attributes = attributes;            
            m_isFwdDeclPossible = isFwdDeclPossible;
        }
        
        #endregion IConstructors
        #region IProperties
        
        public Type Type {
            get {
                return m_type;
            }
        }
        
        public AttributeExtCollection Attributes {
            get {
                return m_attributes;
            }
        }
        
        public bool IsForwardDeclPossible {
            get {
                return m_isFwdDeclPossible;
            }
        }
        
        #endregion IProperties
        #region IMethods
               
        public void RemoveAttributeOfType(Type attrType) {
            Attribute found;
            m_attributes = m_attributes.RemoveAttributeOfType(attrType, out found);
        }

        public override bool Equals(object obj) {
            if (!(obj is MapTypeInfo)) { 
                return false; 
            }            
            if (!Type.Equals(((MapTypeInfo)obj).Type)) { 
                return false; 
            }
            if (!Attributes.Equals(((MapTypeInfo)obj).Attributes)) { 
                return false; 
            }
            return true;
        }

        public override int GetHashCode() {
            return Type.GetHashCode() ^ Attributes.GetHashCode();
        }
        
        public override string ToString() {
            return "Type: " + Type + "; nr of attributes: " + Attributes.Count;
        }

        #endregion IMethods

    }

}
