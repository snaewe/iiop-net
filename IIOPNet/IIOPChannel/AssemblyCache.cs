/* Repository.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 24.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Reflection;
using System.Collections;
using System.IO;
using System.Diagnostics;

namespace Ch.Elca.Iiop.Idl {

    /// <summary>loads all accessible assemblies for the Channel</summary>    
    internal class AssemblyCache {
    
        #region SFields
        
        private static AssemblyCache s_asmCache = new AssemblyCache();
        
        #endregion SFields
        #region IFields                
        
        /// <summary>the cached assemblies</summary>
        private ArrayList m_asmCache = new ArrayList();
        /// <summary>the cached assemblies as an array</summary>
        private Assembly[] m_cachedAssemblies = new Assembly[0];
        
        #endregion IFields
        #region IConsturctors
        
        private AssemblyCache() {
            CreateAssemblyCache();
        }
        
        #endregion IConstructors        
        #region IProperties

        /// <summary>the assemblies cached by this cache</summary>
        public Assembly[] CachedAssemblies {
            get {
                return m_cachedAssemblies;
            }
        }
        
        #endregion IProperties
        #region SMethods
        
        public static AssemblyCache GetSingleton() {
            return s_asmCache;
        }
        
        #endregion SMethods        
        #region IMethods
        
        /// <summary>
        /// loads the reachable assemblies into memory for fast access, otherwise the solution would be much too slow
        /// </summary>
        /// <remarks>
        /// loading an assembly from disk takes very long --> load them to the beginning into memory
        /// loading a type from an assembly with asm.GetType is a fast operation
        /// </remarks>
        private void CreateAssemblyCache() {
            MethodInfo curMethod = (MethodInfo)MethodBase.GetCurrentMethod();
            Assembly curAsm = curMethod.DeclaringType.Assembly; // the channel assembly
            m_asmCache.Add(curAsm); // add channel assembly to the asm cache
            
            // search for other assemblies
            AppDomain curAppDomain = AppDomain.CurrentDomain;

            DirectoryInfo[] asmDirs = DetermineAssemblyDirs(curAppDomain);
            foreach (DirectoryInfo asmDir in asmDirs) {
                CacheAssembliesFromDir(asmDir);
            }
                                            
            // create array from arraylist for safe access without Enumerator
            m_cachedAssemblies = (Assembly[])m_asmCache.ToArray(typeof(Assembly));
        }
        
        /// <summary>
        /// determines the directories to search for assemblies
        /// </summary>
        private DirectoryInfo[] DetermineAssemblyDirs(AppDomain forAppDomain) {
            ArrayList dirs = new ArrayList();
            
            DirectoryInfo baseDir = new DirectoryInfo(forAppDomain.BaseDirectory); // search appdomain directory for assemblies
            dirs.Add(baseDir);
            
            // cache assemblies in private bin path
            string relativePath = forAppDomain.RelativeSearchPath;
            string[] pathes = new string[0];
            if (relativePath != null) {
                pathes = relativePath.Split(Path.PathSeparator);
            }
            foreach (string path in pathes) {
                string fullPath = Path.Combine(baseDir.FullName, path);
                DirectoryInfo dir = new DirectoryInfo(fullPath);
                if (!dirs.Contains(dir)) {
                    dirs.Add(dir);
                }
            }
            
            DirectoryInfo[] asmDirs = (DirectoryInfo[])dirs.ToArray(typeof(DirectoryInfo));
            // check subdirs of every directory in asmDirs for module directories (contains .netmodule + dll)
            // cause: VS.NET creates for multimodule assemblies an own directory
            foreach (DirectoryInfo candidateParent in asmDirs) {
                DirectoryInfo[] subdirs = candidateParent.GetDirectories();
                foreach (DirectoryInfo candidate in subdirs) {
                    FileInfo[] moduleFiles = (candidate.GetFiles("*.netmodule"));
                    if ((moduleFiles != null) && (moduleFiles.Length > 0)) {
                        dirs.Add(candidate);
                    }
                }
            }
            // return directories
            return (DirectoryInfo[])dirs.ToArray(typeof(DirectoryInfo));
        }

        /// <summary>searches for assemblies in the specified directory, loads them and adds them to the cache</summary>
        private void CacheAssembliesFromDir(DirectoryInfo dir) {
            FileInfo[] potAsmDll = dir.GetFiles("*.dll");
            CacheAssemblies(potAsmDll);
            FileInfo[] potAsmExe = dir.GetFiles("*.exe");
            CacheAssemblies(potAsmExe);
        }
        /// <summary>loads the assemblies from the files and adds them to the cache</summary>
        /// <param name="asms"></param>
        private void CacheAssemblies(FileInfo[] asms) {
            for (int i = 0; i < asms.Length; i++) {
                try {
                    Assembly asm = Assembly.LoadFrom(asms[i].FullName);
                    if (!m_asmCache.Contains(asm)) {
                        m_asmCache.Add(asm); // add assembly to cache
                    }
                } catch (Exception e) {
                    Debug.WriteLine("invalid asm found, exception: " + e);
                }
            }
        }
        
        #endregion IMethods
        
    
    }

    
}
