/* MapTypeInfo.cs
 * 
 * Project: IIOP.NET
 * CLSToIDLGenerator
 * 
 * WHEN      RESPONSIBLE
 * 07.02.03  Dominic Ullmann (DUL), dul@elca.ch
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

        public Type m_type;
        public AttributeExtCollection m_attributes;

        #endregion IFields
        #region IConstructors
        
        public MapTypeInfo(Type type, Util.AttributeExtCollection attributes) {
            m_type = type;
            m_attributes = attributes;
        }
        
        #endregion IConstructors
        #region IMethods

        public override bool Equals(object obj) {
            if (obj == null) { 
                return false; 
            }
            if (!(obj.GetType().Equals(typeof(MapTypeInfo)))) { 
                return false; 
            }
            
            if (!m_type.Equals(((MapTypeInfo)obj).m_type)) { 
                return false; 
            }
            if (!m_attributes.Equals(((MapTypeInfo)obj).m_attributes)) { 
                return false; 
            }
            return true;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        #endregion IMethods

    }




}
