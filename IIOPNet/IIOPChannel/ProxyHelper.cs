/* ProxyHelper.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 05.03.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Runtime.Remoting;
using Ch.Elca.Iiop.CorbaObjRef;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop {


    public sealed class ProxyHelper {

        #region IConstructors        

        private ProxyHelper() {
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// returns a stringified ior for the proxy
        /// </summary>
        public static string GetUriForProxy(object proxy) {
            MarshalByRefObject mbrProxy = proxy as MarshalByRefObject;
            if ((mbrProxy == null) || (!RemotingServices.IsTransparentProxy(proxy))) {
                throw new ArgumentException("argument is not a proxy");
            }
            string uri = RemotingServices.GetObjectUri(mbrProxy);
            if (!IiopUrlUtil.IsUrl(uri)) {
                throw new ArgumentException("unknown url type for proxy: " + uri);
            }
            if (IiopUrlUtil.IsIorString(uri)) {
                return uri;
            } else {
                // create an IOR assuming type is CORBA::Object
                return IiopUrlUtil.CreateIorForUrl(uri, "").ToString();
            }
        }

        #endregion IMethods

    }

}