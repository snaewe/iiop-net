/* FragmentedMsgAssembler.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 18.05.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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


using System.Collections;
using System.IO;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop {

    /// <summary>
    /// this class manages and assembles fragmented 
    /// messages for one connection
    /// </summary>
    internal sealed class FragmentedMsgAssembler {
    
        #region Types
        
        private class FragmentedMsgDesc {
            
            #region IFields 
            
            private GiopHeader m_header;
            private Stream m_target = new MemoryStream();
            
            #endregion IFields
            #region IConstructors
            
            internal FragmentedMsgDesc(GiopHeader header) {
                m_header = header;    
            }
            
            #endregion IConstructors
            #region IProperties
            
            internal GiopHeader Header {
                get { 
                    return m_header;
                }
            }
            
            internal Stream Target {
                get {
                    return m_target;
                }
            }
            
            #endregion IProperties
            
        }
        
        #endregion Types
        #region Constants
    
        // GIOP 1.1 doen't support req-id in fragments -> use instead this key to retrieve
        // msg from Hashtable
        private object m_giop11Key = new object();
    
        #endregion Constants
        #region IFields
    
        private Hashtable m_fragmentedMsgs = new Hashtable();
    
        #endregion IFields
        #region IMethods
    
        private void AddFragmentInternal(object key, CdrInputStreamImpl source,
                                         uint contentLength) {
            lock(m_fragmentedMsgs.SyncRoot) {
                FragmentedMsgDesc fragmentDesc = (FragmentedMsgDesc) m_fragmentedMsgs[key];
                if (fragmentDesc == null) {
                    throw new IOException("illegal fragment");
                }
                // read payload of msg and copy into target stream
                Stream target = fragmentDesc.Target;
                IoUtil.StreamCopyExactly(source.BackingStream, target,
                                         (int)contentLength); // copy message body from transport stream to target stream
            }
        }
    
        /// <summary>
        /// completes the fragmented msg.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="source"></param>
        /// <param name="contentLength"></param>
        /// <param name="fullMsgHeader">returns the header of the complete msg as an out parameter</param>
        /// <returns>the full msg as a stream</returns>
        private Stream FinishFragmentedMsgInternal(object key,
                                                   CdrInputStreamImpl source,
                                                   uint contentLength, 
                                                   out GiopHeader fullMsgHeader) {
            lock(m_fragmentedMsgs.SyncRoot) {
                FragmentedMsgDesc fragmentDesc = (FragmentedMsgDesc) m_fragmentedMsgs[key];
                if (fragmentDesc == null) {
                    throw new IOException("illegal fragment");
                }
                // write read of msg-content
                Stream target = fragmentDesc.Target;
                IoUtil.StreamCopyExactly(source.BackingStream, target, 
                                         (int)contentLength); // copy message body from transport stream to target stream
                // write adapted header
                target.Seek(0, SeekOrigin.Begin);
                GiopHeader newHeader = new GiopHeader(fragmentDesc.Header.Version.Major, 
                                                      fragmentDesc.Header.Version.Minor,
                                                      (byte)(fragmentDesc.Header.GiopFlags ^ GiopHeader.FRAGMENT_MASK),
                                                      fragmentDesc.Header.GiopType);

                newHeader.WriteToStream(target, 
                                        ((uint)target.Length) - GiopHeader.HEADER_LENGTH);
                target.Seek(0, SeekOrigin.Begin);
                
                // remove for unfinished msg table
                m_fragmentedMsgs.Remove(fragmentDesc);
                
                // the header of the full msg
                fullMsgHeader = newHeader;
                // return the complete result msg
                return target;
            }
        }
    
        /// <summary>Start a new fragmented msg</summary>
        internal void StartFragment(CdrInputStreamImpl source, GiopHeader header) {
            lock(m_fragmentedMsgs.SyncRoot) {
                FragmentedMsgDesc fragmentDesc = new FragmentedMsgDesc(header);
                Stream target = fragmentDesc.Target;
                CdrOutputStream cdrOut = new CdrOutputStreamImpl(target, header.GiopFlags,
                                                                 header.Version);
                // write placeholder header (is replaced during finish)
                header.WriteToStream(cdrOut, header.ContentMsgLength);
                object descKey = null;
                int contentLength = (int) header.ContentMsgLength;
            
                if ((header.Version.Major == 1) && (header.Version.Minor == 1)) {
                    descKey = m_giop11Key;
                } else if (!((header.Version.Major == 1) && (header.Version.Minor == 0))) {
                    // GIOP 1.2 or newer
                    uint reqId = source.ReadULong();
                    cdrOut.WriteULong(reqId);
                    descKey = reqId;
                    contentLength -= 4;
                } else {
                    // no fragmentation allowed        
                    throw new IOException("fragmentation not allowed for GIOP 1.0");
                }
                
                // write content to target stream
                IoUtil.StreamCopyExactly(source.BackingStream, target,
                                         contentLength); // copy message body to target stream

                // add to unfinished message table
                m_fragmentedMsgs[descKey] = fragmentDesc;
            }
            
        }
    
        internal void AddFragment(CdrInputStreamImpl source, 
                                  GiopHeader header) {
            if ((header.Version.Major == 1) && (header.Version.Minor == 1)) {
                AddFragmentInternal(m_giop11Key, source, 
                                    header.ContentMsgLength);
            } else if (!((header.Version.Major == 1) && (header.Version.Minor == 0))) {
                // GIOP 1.2 or newer: read request id from fragment msg header
                uint reqId = source.ReadULong();
                AddFragmentInternal(reqId, source, header.ContentMsgLength - 4);
            } else {
                // no fragmentation allowed        
                throw new IOException("fragmentation not allowed for GIOP 1.0");
            }        
        }
        

        internal Stream FinishFragmentedMsg(CdrInputStreamImpl source, 
                                            GiopHeader header) {
            return FinishFragmentedMsg(source, ref header);
        }

        /// <param name="header">
        /// The fragment header of the last fragment. As an out parameter, the header of the complete msg is returned. 
        /// </param>
        internal Stream FinishFragmentedMsg(CdrInputStreamImpl source, 
                                            ref GiopHeader header) {
            Stream result;
            if ((header.Version.Major == 1) && (header.Version.Minor == 1)) {
                result = FinishFragmentedMsgInternal(m_giop11Key, source,
                                                     header.ContentMsgLength, out header);
            } else if (!((header.Version.Major == 1) && (header.Version.Minor == 0))) {
                // GIOP 1.2 or newer: read request id from fragment msg header
                uint reqId = source.ReadULong();
                result = FinishFragmentedMsgInternal(reqId, source,
                                                     header.ContentMsgLength - 4, out header);
            } else {
                // no fragmentation allowed        
                throw new IOException("fragmentation not allowed for GIOP 1.0");
            }
            return result;
        }
        
        internal bool IsLastFragment(GiopHeader header) {
            bool hasMore = ((header.GiopFlags & GiopHeader.FRAGMENT_MASK) > 0);
            return !hasMore;
        }
    
        #endregion IMethods
        
    }
    
}



#if UnitTest

namespace Ch.Elca.Iiop.Tests {
	
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Util;
    using Ch.Elca.Iiop.Cdr;


    /// <summary>
    /// Unit-tests for testing fragment assembling
    /// </summary>
    public class FragmentedMsgAsmTest : TestCase {
        
        
        /// <param name="fragmentContentBlocks">the nr of 4 byte blocks in the content; 
        /// must be even for GIOP 1.2</param>
        private CdrOutputStreamImpl AddStartMsg(Stream targetStream, GiopVersion version, 
                                                byte endianFlags, uint reqId,
                                                uint fragmentContentBlocks, out uint offsetInMsg) {
                                    
            byte giopFlags = (byte)(endianFlags | ((byte)2)); // more fragments
            
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(targetStream, endianFlags,
                                                                 version);
            GiopHeader startHeader = new GiopHeader(version.Major, version.Minor,
                                                    giopFlags, GiopMsgTypes.Request);
            
            uint contentLength = 0;
            if (!((version.Major == 1) && (version.Minor <= 1))) {
                // GIOP 1.2     
                contentLength = (uint)(4 + (fragmentContentBlocks * 4));
            } else {
                contentLength = (uint)(8 + (fragmentContentBlocks * 4));    
            }
            
            startHeader.WriteToStream(cdrOut, contentLength);
            if ((version.Major == 1) && (version.Minor == 1)) {
                // GIOP 1.1: add service context list here
                cdrOut.WriteULong(0); // no contexts
            } 
            cdrOut.WriteULong(reqId); // request id
            
            // more is not needed to write a correct GIOP-message for this test from here
            for (uint i = 0; i < fragmentContentBlocks * 4; i++) {
                cdrOut.WriteOctet((byte)(i % 255));
            }
            
            offsetInMsg = fragmentContentBlocks * 4;
            return cdrOut;                                     
        }
        
        /// <param name="fragmentContentBlocks">the nr of 4 byte blocks in the content; 
        /// must be even for GIOP 1.2</param>
        /// <return>the offset in the msg for more fragments</return>
        private uint AddFragmentInTheMiddle(CdrOutputStreamImpl targetStream, GiopVersion version,
                                            byte endianFlags, uint reqId, uint fragmentContentBlocks, 
                                            uint offsetInMsg) {
                                            
            byte giopFlags = (byte)(endianFlags | ((byte)2)); // more fragments
            
            GiopHeader fragmentHeader = new GiopHeader(version.Major, version.Minor,
                                                       endianFlags, GiopMsgTypes.Request);        
                                                
            uint contentLength = 0;
            if (!((version.Major == 1) && (version.Minor <= 1))) {
                // GIOP 1.2     
                contentLength = 4 + (fragmentContentBlocks * 4);
            } else {
                contentLength = (fragmentContentBlocks * 4);
            }
            
            fragmentHeader.WriteToStream(targetStream, contentLength);
            if (!((version.Major == 1) && (version.Minor <= 1))) {
                // GIOP 1.2     
                targetStream.WriteULong(reqId);
            }
                                                
            // more is not needed to write a correct GIOP-message for this test from here
            for (uint i = offsetInMsg; i < (offsetInMsg + (fragmentContentBlocks * 4)); i++) {
                targetStream.WriteOctet((byte)(i % 255));
            }
            
            return offsetInMsg + (fragmentContentBlocks * 4);
        }
        
        private uint AddFinishFragment(CdrOutputStreamImpl targetStream, GiopVersion version,
                                       byte endianFlags, uint reqId, uint reqContentLength, 
                                       uint offsetInMsg) {
                                            
            byte giopFlags = endianFlags; // no more fragments
            
            GiopHeader fragmentHeader = new GiopHeader(version.Major, version.Minor,
                                                       endianFlags, GiopMsgTypes.Request);
                                                
            uint contentLength = 0;
            if (!((version.Major == 1) && (version.Minor <= 1))) {
                // GIOP 1.2     
                contentLength = 4 + reqContentLength;
            } else {
                contentLength = reqContentLength;
            }
            
            fragmentHeader.WriteToStream(targetStream, contentLength);
            if (!((version.Major == 1) && (version.Minor <= 1))) {
                // GIOP 1.2     
                targetStream.WriteULong(reqId);
            }
                                                
            // more is not needed to write a correct GIOP-message for this test from here
            for (uint i = offsetInMsg; i < (offsetInMsg + reqContentLength); i++) {
                targetStream.WriteOctet((byte)(i % 255));
            }
            return offsetInMsg + reqContentLength;
        }        
        
        /// <param name="expectedContentLength">length in bytes after the request-id</param>
        private void CheckAssembledMessage(Stream msgStream, GiopVersion version,
                                           byte endianFlags, uint reqId, 
                                           uint expectedContentLength) {
                                           
            CdrInputStreamImpl inStream = new CdrInputStreamImpl(msgStream);
            GiopHeader header = new GiopHeader(inStream);
            
            Assertion.AssertEquals(GiopMsgTypes.Request, header.GiopType);
            Assertion.AssertEquals(version, header.Version);
            
            uint contentLength = 0;
            uint msgReqId = 0;
            if (!((version.Major == 1) && (version.Minor <= 1))) {
                // GIOP 1.2     
                // req-id
                contentLength = (uint)(4 + expectedContentLength);
                msgReqId = inStream.ReadULong();
            } else {
                // svc-cntx + req-id
                contentLength = (uint)(8 + expectedContentLength);    
                inStream.ReadULong(); // svc-cnxt
                msgReqId = inStream.ReadULong();
            }
            
            Assertion.AssertEquals(contentLength, header.ContentMsgLength);
            Assertion.AssertEquals(endianFlags, header.GiopFlags);

            for (int i = 0; i < expectedContentLength; i++) {
                Assertion.AssertEquals(i % 255, inStream.ReadOctet());
            }            
                                           
        }
        
        private void InternalTestTwoFramgents(GiopVersion version) {
            Stream msgStream = new MemoryStream();
            byte endianFlags = 0;
            uint reqId = 11;
            
            uint startMsgContentBlocks = 2;
            uint lastFragmentContentLength = 13;
            uint currentOffsetInMsg = 0; // the content offset for the next message
            CdrOutputStreamImpl cdrOut = AddStartMsg(msgStream, version, endianFlags,
                                                     reqId, startMsgContentBlocks,
                                                     out currentOffsetInMsg);
            uint endOffset = AddFinishFragment(cdrOut, version, endianFlags, reqId, 
                                               lastFragmentContentLength,
                                               currentOffsetInMsg);
                              
            msgStream.Seek(0, SeekOrigin.Begin);
            // start fragment
            CdrInputStreamImpl inStream = new CdrInputStreamImpl(msgStream);
            GiopHeader msgHeader = new GiopHeader(inStream);
            FragmentedMsgAssembler assembler = new FragmentedMsgAssembler();
            assembler.StartFragment(inStream, msgHeader);
            // finish fragment
            inStream = new CdrInputStreamImpl(msgStream);
            msgHeader = new GiopHeader(inStream);
            Stream resultStream = assembler.FinishFragmentedMsg(inStream, ref msgHeader);
            
            CheckAssembledMessage(resultStream, version, endianFlags, reqId,
                                  endOffset);

        }
        
        private void InternalTestThreeFramgents(GiopVersion version) {
            Stream msgStream = new MemoryStream();
            byte endianFlags = 0;
            uint reqId = 11;
            
            uint startMsgContentBlocks = 2;
            uint middleMsgContentBlocks = 4;
            uint lastFragmentContentLength = 13;
            uint currentOffsetInMsg = 0; // the content offset for the next message
            CdrOutputStreamImpl cdrOut = AddStartMsg(msgStream, version, endianFlags,
                                                     reqId, startMsgContentBlocks,
                                                     out currentOffsetInMsg);
            
            currentOffsetInMsg = AddFragmentInTheMiddle(cdrOut, version, endianFlags,
                                                        reqId, middleMsgContentBlocks,
                                                        currentOffsetInMsg);
            
            uint endOffset = AddFinishFragment(cdrOut, version, endianFlags, reqId,
                                               lastFragmentContentLength,
                                               currentOffsetInMsg);
                              
            msgStream.Seek(0, SeekOrigin.Begin);
            // start fragment
            CdrInputStreamImpl inStream = new CdrInputStreamImpl(msgStream);
            GiopHeader msgHeader = new GiopHeader(inStream);
            FragmentedMsgAssembler assembler = new FragmentedMsgAssembler();
            assembler.StartFragment(inStream, msgHeader);
            // middle fragment            
            inStream = new CdrInputStreamImpl(msgStream);
            msgHeader = new GiopHeader(inStream);
            assembler.AddFragment(inStream, msgHeader);
            // finish fragment
            inStream = new CdrInputStreamImpl(msgStream);
            msgHeader = new GiopHeader(inStream);
            Stream resultStream = assembler.FinishFragmentedMsg(inStream, ref msgHeader);
            
            CheckAssembledMessage(resultStream, version, endianFlags, reqId,
                                  endOffset);

        }        
        
        [Test]
        public void TestTwoFragmentsGiop1_2() {
            InternalTestTwoFramgents(new GiopVersion(1, 2));
        }
        
        [Test]
        public void TestThreeFragmentsGiop1_2() {
            InternalTestThreeFramgents(new GiopVersion(1, 2));
        }

        [Test]        
        public void TestTwoFragmentsGiop1_1() {
            InternalTestTwoFramgents(new GiopVersion(1, 1));            
        }
        
        [Test]
        public void TestThreeFragmentsGiop1_1() {
            InternalTestThreeFramgents(new GiopVersion(1, 1));
        }
        
        
        
    }

}


#endif
