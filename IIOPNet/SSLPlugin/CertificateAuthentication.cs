/* CertificateAuthentication.cs
 * 
 * Project: IIOP.NET
 * SslPlugin for IIOPChannel using mentalis security library
 * 
 * WHEN      RESPONSIBLE
 * 15.08.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2004 Dominic Ullmann
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
using System.IO;
using System.Net;
using Org.Mentalis.Security.Certificates; 

namespace Ch.Elca.Iiop.Security.Ssl {
 

    /// <summary>
    /// interace to implement for an ssl client.
    /// </summary>
    public interface IClientSideAuthentication {

        /// <summary>
        /// is called by the transport factory with non-default options passed to the channel constructor
        /// </summary>
        /// <param name="options">potential options; must ignore unknown options</param>
        void SetupClientOptions(IDictionary options);
        
        /// <summary>the client certificate to use, when server request client authentication</summary>
        /// <returns>the certificate to pass, or null if not available</returns>
        Certificate GetClientCertificate(DistinguishedNameList acceptable);
        
        /// <summary>
        /// checks, if the server certificate is valid
        /// </summary>
        /// <returns>true, if the certificate is valid, otherwise false</returns>
        bool IsValidServerCertificate(Certificate serverCert, CertificateChain allServerCertsReceived, IPAddress serverAddress);
            
    }
    
    
    /// <summary>
    /// interface to implement for an ssl server.
    /// </summary>
    public interface IServerSideAuthentication {
        
        /// <summary>
        /// is called by the transport factory with non-default options passed to the channel constructor
        /// </summary>
        /// <param name="options">potential options; must ignore unknown options</param>
        void SetupServerOptions(IDictionary options);
        
        /// <summary>
        /// returns the server certificate to use for sending to the client; must be != null!
        /// </summary>
        Certificate GetServerCertificate();
        
        /// <summary>
        /// checks, if the client certificate is vali
        /// </summary>        
        /// <returns>true if valid, otherwise false</returns>
        bool IsValidClientCertificate(Certificate clientCert, CertificateChain allClientCertsReceived, IPAddress clientAddress);
        
    }
    
    public class CertificateAuthenticationBase {
                
        /// <summary>
        /// verifies the certificate chain against the certificate store
        /// </summary>
        /// <param name="allCertsReceived">the chain to verify</param>
        /// <param name="expectedCNName">the expected CN; may be null</param>
        /// <param name="authType">the authtype: is the certificate to verify a client or server certificate; 
        /// i.e when verifying a client cert, pass AuthType.Client; when verifying a server cert: pass AuthType.Server</param>ram>
        /// <returns></returns>
        protected bool IsValidCertificate(CertificateChain allCertsReceived, string expectedCNName, AuthType authType) {
            VerificationFlags verificationFlags = VerificationFlags.None;
            if (expectedCNName == null) {
                verificationFlags = VerificationFlags.IgnoreInvalidName;
            }
            CertificateStatus status = allCertsReceived.VerifyChain(expectedCNName, authType, verificationFlags);
            return status == CertificateStatus.ValidCertificate;
        }
        
        protected byte[] GetKeyHashForKeyHashString(string hashString) {            
            
            if ((hashString == null) || (!(hashString.Length % 2 == 0))) {
                throw new ArgumentException("not a valid key hash string: " + hashString);
            }
            byte[] result = new byte[hashString.Length / 2];
            for (int i = 0; i < result.Length; i++) {
                string hexNumber = String.Concat((char)hashString[i*2], (char)hashString[(i*2)+1]);
                result[i] = Byte.Parse(hexNumber, System.Globalization.NumberStyles.AllowHexSpecifier);                
            }
            return result;
        }
        
        protected Certificate LoadCertificateFromStore(StoreLocation storeLocation, string storeName, string certHashString) {
            CertificateStore store = new CertificateStore(storeLocation, storeName);            
            byte[] certHash = GetKeyHashForKeyHashString(certHashString);
            return store.FindCertificateByHash(certHash);            
        }
        
    }
    
        
    /// <summary>
    /// does verfiy the received certificates against windows keystore; doesn't have an own client side certificate
    /// </summary>
    public class DefaultClientAuthenticationImpl : CertificateAuthenticationBase, IClientSideAuthentication {
        
        /// <summary>
        /// the name of the server (CN in certificate), which should send it's certificate. The default is null and name is not considered
        /// </summary>
        public const string EXPECTED_SERVER_CERTIFICATE_CName = "ServerCertificateCNameKey";


        /// <summary>
        /// if != null, the name of the server-certificate will be checked
        /// </summary>
        protected string m_expectedServerCName = null;
        

        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.Security.Ssl.IClientSideAuthentication.SetupClientOptions"/>
        /// </summary>        
        public virtual void SetupClientOptions(IDictionary options) {
            foreach (DictionaryEntry entry in options) {
                switch ((string)entry.Key) {            
                    case EXPECTED_SERVER_CERTIFICATE_CName:
                        m_expectedServerCName = (string)entry.Value;
                        break;
                    default:
                        // ignore
                        break;                    
                }
            }            
        }
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.Security.Ssl.IClientSideAuthentication.GetClientCertificate"/>
        /// </summary>
        public virtual Certificate GetClientCertificate(DistinguishedNameList acceptable) {
            return null;
        }
        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.Security.Ssl.IClientSideAuthentication.IsValidServerCertificate"/>
        /// </summary>        
        public virtual bool IsValidServerCertificate(Certificate serverCert, CertificateChain allServerCertsReceived, IPAddress serverAddress) {            
            return IsValidCertificate(allServerCertsReceived, m_expectedServerCName, AuthType.Server);
        }
        
    }

    /// <summary>
    /// loads a client side certificate from the windows keystore, checks server certificate against keystore
    /// </summary>
    public class ClientAuthenticationLoadFromPersonalStore : DefaultClientAuthenticationImpl {
        
        /// <summary>
        /// store name for store containing personal certificates (including private keys)
        /// </summary>
        private const string MY_STORE_NAME = "MY";                
        
        public const string CLIENT_CERTIFICATE = "ClientCertificateHashKey";
        /// <summary>
        /// the location of the store, from which the client certificate should be taken from
        /// (one of CurrentService, CurrentUser, CurrentUserGroupPolicy, LocalMachine,
        /// LocalMachineEnterprise, LocalMachineGroupPolicy, Services, Unknown, Users
        /// The default is Unknown
        /// </summary>
        public const string STORE_LOCATION = "StoreLocationKey";
                
        private Certificate m_clientCertificate = null;        

        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.Security.Ssl.IClientSideAuthentication.SetupClientOptions"/>
        /// </summary>        
        public override void SetupClientOptions(IDictionary options) {
            base.SetupClientOptions(options);
            
            StoreLocation storeLocation = StoreLocation.Unknown;
            string storeName = MY_STORE_NAME;
            string certificateHash = null;
            
            foreach (DictionaryEntry entry in options) {
                switch ((string)entry.Key) {
                    case CLIENT_CERTIFICATE:
                        certificateHash = (string)entry.Value;
                        break;
                    case STORE_LOCATION:
                        if (!Enum.IsDefined(typeof(StoreLocation), (string)entry.Value)) {
                            throw new ArgumentException("invalid store location: " + entry.Value);
                        }
                        storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), (string)entry.Value);
                        break;
                    default:
                        // ignore
                        break;
                }
            }    
            
            if (certificateHash != null) {
                // load the certificate, if specified
                m_clientCertificate = LoadCertificateFromStore(storeLocation, storeName, certificateHash);
                if (m_clientCertificate == null) {
                    throw new ArgumentException("certificate not found for hash: " + certificateHash);
                }
            }
        }
                        
        /// <summary>
        /// see <see cref="Ch.Elca.Iiop.Security.Ssl.IClientSideAuthentication.GetClientCertificate"/>
        /// </summary>
        public override Certificate GetClientCertificate(DistinguishedNameList acceptable) {
            return m_clientCertificate;
        }                
        
    }


    /// <summary>
    /// loads the server certificate from personal store; verifies the client certificate against the store
    /// </summary>
    public class DefaultServerAuthenticationImpl : CertificateAuthenticationBase, IServerSideAuthentication {
                
        /// <summary>
        /// store name for store containing personal certificates (including private keys)
        /// </summary>
        private const string MY_STORE_NAME = "MY";                
        
        public const string SERVER_CERTIFICATE = "ServerCertificateHashKey";
        /// <summary>
        /// the location of the store, from which the client certificate should be taken from
        /// (one of CurrentService, CurrentUser, CurrentUserGroupPolicy, LocalMachine,
        /// LocalMachineEnterprise, LocalMachineGroupPolicy, Services, Unknown, Users
        /// The default is Unknown
        /// </summary>
        public const string STORE_LOCATION = "StoreLocationKey";
                
        private Certificate m_serverCertificate = null;        
        
        
        public virtual void SetupServerOptions(IDictionary options) {
            StoreLocation storeLocation = StoreLocation.Unknown;
            string storeName = MY_STORE_NAME;
            string certificateHash = null;
            
            foreach (DictionaryEntry entry in options) {
                switch ((string)entry.Key) {
                    case SERVER_CERTIFICATE:
                        certificateHash = (string)entry.Value;
                        break;
                    case STORE_LOCATION:
                        if (!Enum.IsDefined(typeof(StoreLocation), (string)entry.Value)) {
                            throw new ArgumentException("invalid store location: " + entry.Value);
                        }
                        storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), (string)entry.Value);
                        break;
                    default:
                        // ignore
                        break;
                }
            }    
            
            if (certificateHash != null) {
                // load the certificate, if specified
                m_serverCertificate = LoadCertificateFromStore(storeLocation, storeName, certificateHash);
                if (m_serverCertificate == null) {
                    throw new ArgumentException("certificate not found for hash: " + certificateHash);
                }
            } else {
                throw new ArgumentException("need a server certificate; Please pass certificate hash using DefaultServerAuthenticationImpl.SERVER_CERTIFICATE option");
            }
        
        }
        
        public virtual Certificate GetServerCertificate() {
            return m_serverCertificate;                
        }
        
        public virtual bool IsValidClientCertificate(Certificate clientCert, CertificateChain allClientCertsReceived, 
                                                     IPAddress clientAddress) {
            return IsValidCertificate(allClientCertsReceived, null, AuthType.Client);
        }
        
        
    }

}
