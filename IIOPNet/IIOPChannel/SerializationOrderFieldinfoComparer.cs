/* IDLAttributes.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Reflection;
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Util {

    /// <summary>
    /// Compares two FieldInfos based on their ExplicitSerializationOrderType
    /// </summary>
    public sealed class ExplicitOrderFieldInfoComparer : IComparer {

        #region SFields
 
        private static ExplicitOrderFieldInfoComparer s_instance =
            new ExplicitOrderFieldInfoComparer();
 
        #endregion SFields
        #region IConstructor
 
        private ExplicitOrderFieldInfoComparer() {
        }
 
        #endregion IConstructor
        #region SProperties
 
        /// <summary>
        /// the singleton instance of the comparer.
        /// </summary>
        public static ExplicitOrderFieldInfoComparer Instance {
            get {
                return s_instance;
            }
        }
 
        #endregion SProperties
        #region IMethods
 
        /// <summary>
        /// Compare the two given FieldInfos concerning
        /// their serializationOrder
        /// </summary>
        public int Compare(object x, object y) {
            FieldInfo xF = (FieldInfo)x;
            FieldInfo yF = (FieldInfo)y;
 
            object[] xAttrs =
                xF.GetCustomAttributes(ReflectionHelper.ExplicitSerializationOrderNrType, false);
            object[] yAttrs =
                yF.GetCustomAttributes(ReflectionHelper.ExplicitSerializationOrderNrType, false);
 
            if (xAttrs.Length <= 0 || yAttrs.Length <= 0) {
                throw new BAD_PARAM(945, CompletionStatus.Completed_MayBe);
            }
            int xOrder = ((ExplicitSerializationOrderNr)xAttrs[0]).OrderNr;
            int yOrder = ((ExplicitSerializationOrderNr)yAttrs[0]).OrderNr;
 
            return xOrder - yOrder; // result of compare should be positive, if x > y, else negative
        }
 
        #endregion IMethods
    }
 
    /// <summary>
    /// Compares two FieldInfos based on their Name
    /// </summary>
    public sealed class ImplicitOrderFieldInfoComparer : IComparer {

        #region SFields
 
        private static ImplicitOrderFieldInfoComparer s_instance =
            new ImplicitOrderFieldInfoComparer();
 
        #endregion SFields
        #region IConstructor
 
        private ImplicitOrderFieldInfoComparer() {
        }
 
        #endregion IConstructor
        #region SProperties
 
        /// <summary>
        /// the singleton instance of the comparer.
        /// </summary>
        public static ImplicitOrderFieldInfoComparer Instance {
            get {
                return s_instance;
            }
        }
 
        #endregion SProperties
        #region IMethods
 
        /// <summary>
        /// Compare the two given FieldInfos concerning
        /// their field name
        /// </summary>
        public int Compare(object x, object y) {
            FieldInfo xF = (FieldInfo)x;
            FieldInfo yF = (FieldInfo)y;
 
            return String.CompareOrdinal(xF.Name, yF.Name);
        }
 
        #endregion IMethods
    }
 
}
