/* StandardCorbaOps.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 23.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;

namespace Ch.Elca.Iiop {

    /// <summary>
    /// this class is a handler for the standard corba-ops like _is_a
    /// </summary>
    public class StandardCorbaOps : MarshalByRefObject, IObject {

        #region Constants

        internal const string WELLKNOWN_URI = "wellKnown/CORBAHandler";

        #endregion Constants
        #region SFields
        
        private static MarshalByRefObject s_standardOpsH;
        private static ArrayList s_standardOpList = new ArrayList();
        private static Hashtable s_opToCallTable = new Hashtable();
        private static object s_lock = new object();
        
        internal static Type s_type = typeof(StandardCorbaOps);

        #endregion SFields
        #region SConstructor

        static StandardCorbaOps() {
            string isAMethodName = "_is_a";
            s_standardOpList.Add(isAMethodName);
            s_opToCallTable.Add(isAMethodName, 
                                s_type.GetMethod(MapMethodName(isAMethodName), 
                                                 BindingFlags.Public | BindingFlags.Instance));
            
            string nonExistMethodName = "_non_existent";
            s_standardOpList.Add(nonExistMethodName);
            s_opToCallTable.Add(nonExistMethodName, 
                                s_type.GetMethod(MapMethodName(nonExistMethodName), 
                                                 BindingFlags.Public | BindingFlags.Instance));

            
            // TODO: other standard ops
        }

        #endregion SConstructor
        #region IConstructors
        
        private StandardCorbaOps() {
        }

        #endregion IConstructors
        #region SMethods

        internal static void SetUpHandler() {
            lock(s_lock) {
                if (s_standardOpsH == null) {
                    s_standardOpsH = new StandardCorbaOps();
                    // publish this
                    RemotingServices.Marshal(s_standardOpsH, WELLKNOWN_URI);
                }
            }
        }

        /// <summary>
        /// get the method name of the method responsible for handling a call to the corba-method with name idlName
        /// </summary>
        /// <param name="idlName"></param>
        /// <returns></returns>
         internal static string MapMethodName(string idlName) {
            return idlName.Substring(1);
        }

        internal static bool CheckIfStandardOp(string idlMethodName) {
            if (s_standardOpList.BinarySearch(idlMethodName, new CaseInsensitiveComparer()) >= 0) {
                return true;
            } else {
                return false;
            }
        }
        
        /// <summary>gets the method, which should be called on this object,
        /// when methodName is received</summary>
        internal static MethodInfo GetMethodToCallForStandardMethod(string methodName) {
            return (MethodInfo)s_opToCallTable[methodName];
        }

        #endregion SMethods
        #region IMethods

        public override Object InitializeLifetimeService() {
            // this object should live forever
            return null;
        }

        /// <summary>
        /// this is the implementation for the is_a operation
        /// </summary>
        /// <param name="objectUri">the uri of the object, for which this request is performed</param>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        public bool is_a(string objectUri, string repositoryId) {
            Type serverType = RemotingServices.GetServerTypeForUri(objectUri);
            if (serverType == null) {
                throw new OBJECT_NOT_EXIST(0, CompletionStatus.Completed_No); 
            }           
            
            Debug.WriteLine("test if : " + serverType + " _is_a " + repositoryId);
            Type toCheck = Repository.GetTypeForId(repositoryId);
            return toCheck != null && toCheck.IsAssignableFrom(serverType);
        }

        /// <summary>this method has no implementation, is does only specify the interface of the _is_a method</summary>
        [CLSCompliant(false)]
        public bool _is_a([WideCharAttribute(false)][StringValueAttribute]string repositoryId) {
            throw new NotSupportedException("this method should not be called");
        }
        
        /// <summary>
        /// this is the implementation for the non_existent operation
        /// </summary>
        /// <param name="objectUri">the uri of the object, for which this request is performed</param>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        public bool non_existent(string objectUri) {
            Type serverType = RemotingServices.GetServerTypeForUri(objectUri);
            return (serverType == null);
        }

        /// <summary>this method has no implementation, is does only specify the interface of the _non_existent method</summary>
        [CLSCompliant(false)]
        public bool _non_existent() {
            throw new NotSupportedException("this method should not be called");
        }

        #endregion IMethods

    }
}


namespace omg.org.CORBA {


    /// <summary>mapping of the CORBA Object interface</summary>
    [CLSCompliant(false)]
    [InterfaceType(IdlTypeInterface.ConcreteInterface)]
    public interface IObject : IIdlEntity {
                
        [FromIdlName("_is_a")]
        bool _is_a([WideCharAttribute(false)][StringValueAttribute]string
                   repositoryId);
                
        [FromIdlName("_non_existent")]
        bool _non_existent();
        
    }

}
