/* FragmentedMsgAssembler.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 18.05.03  Dominic Ullmann (DUL), dul@elca.ch
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
		private object GIOP_1_1_KEY = new object();
	
		#endregion Constants
		#region IFields
	
		private Hashtable m_fragmentedMsgs = new Hashtable();
	
		#endregion IFields
		#region IMethods
	
		private void AddFragmentInternal(object key, CdrInputStream source,
		                                 uint contentLength) {
			lock(m_fragmentedMsgs.SyncRoot) {
				FragmentedMsgDesc fragmentDesc = (FragmentedMsgDesc) m_fragmentedMsgs[key];
				if (fragmentDesc == null) {
					throw new IOException("illegal fragment");
				}
				// read payload of msg and copy into target stream
				Stream target = fragmentDesc.Target;
				byte[] content = source.ReadOpaque((int) contentLength);
				target.Write(content, 0, content.Length);
			}
		}
	
		private Stream FinishFragmentedMsgInternal(object key,
		                                           CdrInputStream source,
		                                           uint contentLength) {
			lock(m_fragmentedMsgs.SyncRoot) {
				FragmentedMsgDesc fragmentDesc = (FragmentedMsgDesc) m_fragmentedMsgs[key];
				if (fragmentDesc == null) {
					throw new IOException("illegal fragment");
				}
				// write read of msg-content
				Stream target = fragmentDesc.Target;
				byte[] content = source.ReadOpaque((int) contentLength);
				target.Write(content, 0, content.Length);
				// write adapted header
				target.Seek(0, SeekOrigin.Begin);
				fragmentDesc.Header.WriteToStream(target, 
				                                  ((uint)target.Length) - GiopHeader.HEADER_LENGTH);
				target.Seek(0, SeekOrigin.Begin);
				// return the complete result msg
				return target;
			}
		}
	
		/// <summary>Start a new fragmented msg</summary>
		internal void StartFragment(CdrInputStream source, GiopHeader header) {
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
					descKey = GIOP_1_1_KEY;
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
				byte[] content = source.ReadOpaque(contentLength);
				cdrOut.WriteOpaque(content);
			}
			
		}
	
		internal void AddFragment(CdrInputStream source, 
		                          GiopHeader header) {
			if ((header.Version.Major == 1) && (header.Version.Minor == 1)) {
				AddFragmentInternal(GIOP_1_1_KEY, source, 
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
		
		internal Stream FinishFragmentedMsg(CdrInputStream source, 
		                                    GiopHeader header) {
			Stream result;
			if ((header.Version.Major == 1) && (header.Version.Minor == 1)) {
				result = FinishFragmentedMsgInternal(GIOP_1_1_KEY, source,
				                                     header.ContentMsgLength);
			} else if (!((header.Version.Major == 1) && (header.Version.Minor == 0))) {
				// GIOP 1.2 or newer: read request id from fragment msg header
				uint reqId = source.ReadULong();
				result = FinishFragmentedMsgInternal(reqId, source,
				                                     header.ContentMsgLength - 4);
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
