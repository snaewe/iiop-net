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


using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop {
 
 
 
    /// <summary>
    /// this class manages and assembles fragmented messages
    /// for one connection.
    /// </summary>
    internal sealed class FragmentedMessageAssembler {
 

        #region Types
 
        private class FragmentedMsgDesc {
 
            #region IFields
 
            private Stream m_target;
            /// <summary>
            /// the header of the first msg fragment
            /// </summary>
            private GiopHeader m_header;
            /// <summary>
            /// the combined header for the whole message
            /// </summary>
            private GiopHeader m_newHeader;
 
            #endregion IFields
            #region IConstructors
 
            internal FragmentedMsgDesc(CdrInputStreamImpl firstFragment, int nrOfBytesFromCurrentPos,
                                       GiopHeader header, uint reqId) {
                m_target = new MemoryStream();
                m_header = header;
                CdrOutputStream outputStream =
                    header.WriteToStream(m_target, (uint)nrOfBytesFromCurrentPos); // place-holder header
                outputStream.WriteULong(reqId); // add req-id, because already read
                AddFragment(firstFragment, nrOfBytesFromCurrentPos);
 
            }
 
            #endregion IConstructors
            #region IProperties
 
            private GiopHeader Header {
                get {
                    return m_header;
                }
            }
 
            /// <summary>
            /// the header for the whole message
            /// </summary>
            internal GiopHeader CombinedHeader {
                get {
                    return m_newHeader;
                }
            }
 
            internal Stream Target {
                get {
                    return m_target;
                }
            }
 
            #endregion IProperties
            #region IMethods
 
            internal void AddLastFragment(CdrInputStreamImpl fragmentToAdd, int nrOfBytesFromCurrentPos) {
                AddFragment(fragmentToAdd, nrOfBytesFromCurrentPos);
                // write adapted header
                m_target.Seek(0, SeekOrigin.Begin);
                m_newHeader = new GiopHeader(Header.Version.Major,
                                             Header.Version.Minor,
                                             (byte)(Header.GiopFlags ^ GiopHeader.FRAGMENT_MASK),
                                             Header.GiopType);
                m_newHeader.WriteToStream(m_target,
                                          ((uint)m_target.Length) - GiopHeader.HEADER_LENGTH);
                m_target.Seek(0, SeekOrigin.Begin);
            }
 
            internal void AddFragment(CdrInputStreamImpl fragmentToAdd, int nrOfBytesFromCurrentPos) {
 
                IoUtil.StreamCopyExactly(fragmentToAdd.BackingStream, m_target,
                                         nrOfBytesFromCurrentPos); // copy message body to target stream
            }
 
            #endregion IMethods
 
        }
 
        #endregion Types
        #region IFields
 
        private Hashtable /* requestid, fragmentedMsgDesc */ m_fragmentedMsgs = new Hashtable();
 
        #endregion IFields
        #region IMethods
 
        private void CheckGiop1_2OrLater(GiopHeader header) {
            if (header.Version.IsBeforeGiop1_2()) {
                // for giop 1.0 fragmentation is not allowed
                // for giop 1.1 fragmentation is not supported by IIOP.NET, because defragmentation
                // is not possible without complete demarshalling
                Debug.WriteLine("fragment using giop version " + header.Version + " not supported by IIOP.NET / not allowed");
                throw new IOException("fragmentation not supported for 1.0/1.1");
            }
        }
 
        /// <summary>Start a new fragmented msg</summary>
        internal void StartFragment(Stream fragment) {
            CdrInputStreamImpl cdrInput = new CdrInputStreamImpl(fragment);
            GiopHeader header = new GiopHeader(cdrInput);
            CheckGiop1_2OrLater(header);
 
            // GIOP 1.2 or newer: read request id from msg; for giop 1.2, the requestId follows just
            // after the header for request, reply, locateRequest and locateReply; only those messages
            // can be fragmented
            uint reqId = cdrInput.ReadULong();
            int payLoadLength = (int)(header.ContentMsgLength - 4);
            lock(m_fragmentedMsgs.SyncRoot) {
                FragmentedMsgDesc fragmentDesc = new FragmentedMsgDesc(cdrInput, payLoadLength, header, reqId);
                m_fragmentedMsgs[reqId] = fragmentDesc;
            }
        }
 
        internal void AddFragment(Stream fragment) {
            AddFragmentInternal(fragment, false);
        }
 
        internal Stream FinishFragmentedMsg(Stream fragment, out GiopHeader combinedHeader) {
            FragmentedMsgDesc result = AddFragmentInternal(fragment, true);
            combinedHeader = result.CombinedHeader;
            return result.Target;
        }
 
        /// <summary>
        /// adds a fragment to the combined message.
        /// </summary>
        /// <returns>the fragment description for the message</returns>
        private FragmentedMsgDesc AddFragmentInternal(Stream fragment, bool isLastFragment) {
            CdrInputStreamImpl cdrInput = new CdrInputStreamImpl(fragment);
            GiopHeader header = new GiopHeader(cdrInput);
            CheckGiop1_2OrLater(header);
 
            // GIOP 1.2 or newer: read request id from fragment msg header
            uint reqId = cdrInput.ReadULong();
            int payLoadLength = (int)(header.ContentMsgLength - 4);
            lock(m_fragmentedMsgs.SyncRoot) {
                FragmentedMsgDesc fragmentDesc = (FragmentedMsgDesc) m_fragmentedMsgs[reqId];
                if (fragmentDesc == null) {
                    throw new IOException("illegal fragment; not found previous fragment for request-id: " + reqId);
                }
                if (!isLastFragment) {
                    fragmentDesc.AddFragment(cdrInput, payLoadLength);
                } else {
                    fragmentDesc.AddLastFragment(cdrInput, payLoadLength);
                    // remove the desc for unfinished msg from table
                    m_fragmentedMsgs.Remove(reqId);
                }
                return fragmentDesc;
            }
        }
 
        internal void CancelFragmentsIfInProgress(uint requestId) {
            lock(m_fragmentedMsgs.SyncRoot) {
                m_fragmentedMsgs.Remove(requestId); // remove, if available; otherwise do nothing
            }
        }
 
        #endregion IMethods
        #region SMethods
 
        /// <summary>checks, if the message is only a fragment; non-legal fragmentted messages are not discovered here.
        /// Only the fragmented or not fact is considered</summary>
        internal static bool IsFragmentedMessage(GiopHeader header) {
            return ((header.GiopType == GiopMsgTypes.Fragment) ||
                    header.IsFragmentedBitSet());
 
        }
 
        internal static bool IsStartFragment(GiopHeader header) {
            return header.GiopType != GiopMsgTypes.Fragment;
        }
 
        internal static bool IsLastFragment(GiopHeader header) {
            return (header.GiopType == GiopMsgTypes.Fragment) &&
                   (!header.IsFragmentedBitSet());
        }
 
        #endregion SMethods

 
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
    [TestFixture]
    public class FragmentedMsgAsmTest {
 
 
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
 
            Assert.AreEqual(GiopMsgTypes.Request, header.GiopType);
            Assert.AreEqual(version, header.Version);
 
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
 
            Assert.AreEqual(contentLength, header.ContentMsgLength);
            Assert.AreEqual(endianFlags, header.GiopFlags);

            for (int i = 0; i < expectedContentLength; i++) {
                Assert.AreEqual(i % 255, inStream.ReadOctet());
            }
 
        }
 
        private void InternalTestTwoFramgents(GiopVersion version) {
            Stream msgStream = new MemoryStream();
            byte endianFlags = 0;
            uint reqId = 11;
 
            uint startMsgContentBlocks = 2; // make sure, that start fragment length is a multiple of 8; giop 1.2
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
            FragmentedMessageAssembler assembler = new FragmentedMessageAssembler();
            assembler.StartFragment(msgStream);
            // finish fragment
            GiopHeader combinedHeader;
            Stream resultStream = assembler.FinishFragmentedMsg(msgStream, out combinedHeader);
 
            CheckAssembledMessage(resultStream, version, endianFlags, reqId,
                                  endOffset);

        }
 
        private void InternalTestThreeFramgents(GiopVersion version) {
            Stream msgStream = new MemoryStream();
            byte endianFlags = 0;
            uint reqId = 11;
 
            uint startMsgContentBlocks = 2; // make sure, that start fragment length is a multiple of 8; giop 1.2
            uint middleMsgContentBlocks = 4; // make sure, that middle fragment length is a multiple of 8; giop 1.2
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
            FragmentedMessageAssembler assembler = new FragmentedMessageAssembler();
            assembler.StartFragment(msgStream);
            // middle fragment
            assembler.AddFragment(msgStream);
            // finish fragment
            GiopHeader combinedHeader;
            Stream resultStream = assembler.FinishFragmentedMsg(msgStream, out combinedHeader);
 
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
            try {
                InternalTestTwoFramgents(new GiopVersion(1, 1));
                Assert.Fail("giop 1.1 fragment not detected as unsupported");
            } catch (Exception) {
                // ok, not supported
            }
        }
 
        [Test]
        public void TestThreeFragmentsGiop1_1() {
            try {
                InternalTestThreeFramgents(new GiopVersion(1, 1));
                Assert.Fail("giop 1.1 fragment not detected as unsupported");
            } catch (Exception) {
                // ok, not supported
            }
        }
 
        [Test]
        public void TestCantAddFragmentIfNotStarted() {
            Stream msgStream = new MemoryStream();
            byte endianFlags = 0;
            uint reqId = 15;
            uint lastFragmentContentLength = 13;
            uint currentOffsetInMsg = 0; // the content offset for the next message
            GiopVersion version = new GiopVersion(1,2);
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(msgStream, endianFlags, version);
            AddFinishFragment(cdrOut, version, endianFlags, reqId,
                              lastFragmentContentLength,
                              currentOffsetInMsg);
            msgStream.Seek(0, SeekOrigin.Begin);
            try {
                FragmentedMessageAssembler assembler = new FragmentedMessageAssembler();
                // finish fragment
                GiopHeader combinedHeader;
                assembler.FinishFragmentedMsg(msgStream, out combinedHeader);
                Assert.Fail("accepted finish fragment, although no start fragment seen");
            } catch (IOException) {
                // ok, no start fragment found
            }
        }

        [Test]
        public void TestCantAddFragmentAfterMessageFinished() {
            Stream msgStream = new MemoryStream();
            byte endianFlags = 0;
            uint reqId = 16;
 
            uint startMsgContentBlocks = 2; // make sure, that start fragment length is a multiple of 8; giop 1.2
            uint lastFragmentContentLength = 13;
            uint currentOffsetInMsg = 0; // the content offset for the next message
            GiopVersion version = new GiopVersion(1,2);
            CdrOutputStreamImpl cdrOut = AddStartMsg(msgStream, version, endianFlags,
                                                     reqId, startMsgContentBlocks,
                                                     out currentOffsetInMsg);
            uint endOffset = AddFinishFragment(cdrOut, version, endianFlags, reqId,
                                               lastFragmentContentLength,
                                               currentOffsetInMsg);
 
            msgStream.Seek(0, SeekOrigin.Begin);
            // start fragment
            FragmentedMessageAssembler assembler = new FragmentedMessageAssembler();
            assembler.StartFragment(msgStream);
            // finish fragment
            GiopHeader combinedHeader;
            assembler.FinishFragmentedMsg(msgStream, out combinedHeader);

            // now check, that no additional finish fragment is supported
            msgStream = new MemoryStream();
            currentOffsetInMsg = 0; // the content offset for the next message
            cdrOut = new CdrOutputStreamImpl(msgStream, endianFlags, version);
            AddFinishFragment(cdrOut, version, endianFlags, reqId,
                              lastFragmentContentLength,
                              currentOffsetInMsg);
            msgStream.Seek(0, SeekOrigin.Begin);
            try {
                // finish fragment
                assembler.FinishFragmentedMsg(msgStream, out combinedHeader);
                Assert.Fail("accepted finish fragment, although no start fragment seen");
            } catch (IOException) {
                // ok, no start fragment found
            }
        }
 
    }

}

#endif
