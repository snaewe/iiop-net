/* IDLToCLS.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 14.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Globalization;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using parser;
using Ch.Elca.Iiop.IdlCompiler.Action;
using Ch.Elca.Iiop.IdlPreprocessor;

namespace Ch.Elca.Iiop.IdlCompiler {


/// <summary>
/// the main compiler class
/// </summary>
public class IDLToCLS {
    
    #region Types
    
        private class CustomAssemblyResolver {

            private IList m_candidateDirectories;

            public CustomAssemblyResolver(IList candidateDirectories) {
                m_candidateDirectories = candidateDirectories;
            }

            public Assembly AssemblyResolve(object sender, ResolveEventArgs args) {
                Debug.WriteLine("custom resolve");
                Debug.WriteLine("assembly: " + args.Name);
                string asmSimpleName = args.Name;
                if (asmSimpleName.IndexOf(",") > 0) {
                    asmSimpleName = asmSimpleName.Substring(0, asmSimpleName.IndexOf(",")).Trim();
                }
                
                Assembly found = LoadByRelativePath(asmSimpleName + ".dll");
                if (found != null) {
                    return found;
                }
                found = LoadByRelativePath(asmSimpleName + ".exe");
                if (found != null) {
                    return found;
                }
                found = LoadByRelativePath(asmSimpleName + Path.PathSeparator + asmSimpleName + ".dll");
                if (found != null) {
                    return found;
                }
                found = LoadByRelativePath(asmSimpleName + Path.PathSeparator + asmSimpleName + ".exe");
                if (found != null) {
                    return found;
                }
                // nothing found
                Debug.WriteLine("assembly not found: " + args.Name);
                return null;
            }

            private Assembly LoadByRelativePath(string asmFileName) {
                Assembly result = null;
                foreach (string candidateDir in m_candidateDirectories) {
                    string candidate = Path.Combine(candidateDir, asmFileName);
                    try {
                        result = Assembly.LoadFrom(candidate);
                    } catch (Exception) { }
                    if (result != null) {
                        break;
                    }
                }
                return result;
            }
        }

    
    #endregion Types
    #region Constants    
    #endregion Constants
    #region IFields
    
    private IDLToCLSCommandLine m_commandLine;
      
    private CodeDomProvider m_vtSkelcodeDomProvider = new CSharpCodeProvider();

    #endregion IFields
    #region IConstructors

    public IDLToCLS(String[] args) {
        Setup(args);
    }

    #endregion IConstructors
    #region SMethods

    public static void Main(String[] args) {
#if TRACE
        // enable trace
        Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
        Trace.AutoFlush = true;
        Trace.Indent();                
#endif

        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            IDLToCLS mapper = new IDLToCLS(args);
            mapper.MapIdl();
        } catch (System.Exception e) {
            Console.WriteLine("exception encountered: " + e);
            Environment.Exit(2);
        }
    }
    
    private static void HowTo() {
        IDLToCLSCommandLine.HowTo(Console.Out);
    }
    
    public static void Error(String message) {
        Console.WriteLine(message);
        Console.WriteLine();
        HowTo();
        Environment.Exit(1);
    }

    #endregion SMethods
    #region IMethods
    
    private void Setup(String[] args) {
        m_commandLine = new IDLToCLSCommandLine(args);
        if (m_commandLine.IsHelpRequested) {
            HowTo();
            Environment.Exit(0);
        }
        
        if (m_commandLine.IsInvalid) {
            Error(m_commandLine.ErrorMessage);
        }
        
        // create output directory
        if (!Directory.Exists(m_commandLine.OutputDirectory.FullName)) {
            Directory.CreateDirectory(m_commandLine.OutputDirectory.FullName);
        }
        
        // process include dirs
        for (int i = 0; i < m_commandLine.IdlSourceDirectories.Count; i++) {
            IDLPreprocessor.SetIdlDir((DirectoryInfo)m_commandLine.IdlSourceDirectories[i]);
        }
        
        if (m_commandLine.BaseDirectory != null) {
            Environment.CurrentDirectory = m_commandLine.BaseDirectory.FullName;
        }

        // preprocessor defines
        for (int i = 0; i < m_commandLine.PreprocessorDefines.Count; i++) {
            IDLPreprocessor.AddDefine((string)m_commandLine.PreprocessorDefines[i]);
        }
        
        // vt-skeleton generation setup
        if (m_commandLine.GenerateValueTypeSkeletons &&
            m_commandLine.ValueTypeSkeletonCodeDomProviderType != null) {
            m_vtSkelcodeDomProvider = 
                    (CodeDomProvider) Activator.CreateInstance(
                        m_commandLine.ValueTypeSkeletonCodeDomProviderType);
        }
        
        SetupAssemblyResolver();
        AddCustomMappings(m_commandLine.CustomMappingFiles);
        
    }
    
    /// <summary>setup assembly resolution: consider all directories of /r files 
    /// and current directory as assembly containing directories</summary>
    private void SetupAssemblyResolver() {
        ArrayList searchDirectoryList = new ArrayList();
        searchDirectoryList.Add(new DirectoryInfo(".").FullName);
        foreach (Assembly refAsm in m_commandLine.ReferencedAssemblies) {
            string asmDir = new FileInfo(refAsm.Location).Directory.FullName;
            if (!searchDirectoryList.Contains(asmDir)) {
                searchDirectoryList.Add(asmDir);
            }
        }
        
        AppDomain curDomain = AppDomain.CurrentDomain;
        curDomain.AppendPrivatePath(curDomain.BaseDirectory);
        
        CustomAssemblyResolver resolver = new CustomAssemblyResolver(searchDirectoryList);
        ResolveEventHandler hndlr = new ResolveEventHandler(resolver.AssemblyResolve);
        curDomain.AssemblyResolve += hndlr;
    }
    
    private void AddCustomMappings(IList /*<FileInfo>*/ mappingFiles) {    
        // add custom mappings from files
        CompilerMappingPlugin plugin = CompilerMappingPlugin.GetSingleton();
        foreach (FileInfo mappingFile in mappingFiles) {            
            plugin.AddMappingsFromFile(mappingFile);
        }
    }
    
    private MemoryStream Preprocess(String fileName) {
        FileInfo file = new FileInfo(fileName);
        // all Preprocessor instances share the same symbol definitions
        IDLPreprocessor preproc = new IDLPreprocessor(file);
        preproc.Process();
        MemoryStream resultProc = preproc.GetProcessed();

        // debug print, create a new memory stream to protect resultProc from beeing manipulated...
        MemoryStream forRead = new MemoryStream();
        resultProc.WriteTo(forRead);
        forRead.Seek(0, SeekOrigin.Begin); 
        Encoding latin1 = Encoding.GetEncoding("ISO-8859-1");
        StreamReader stReader = new StreamReader(forRead, latin1);
        String line = "";
        while (line != null) {
            Debug.WriteLine(line);
            line = stReader.ReadLine();
        }
        stReader.Close();

        // make sure, resultStream is at the beginning
        resultProc.Seek(0, SeekOrigin.Begin);
        return resultProc;
    }
    
    
    private AssemblyName GetAssemblyName() {
        AssemblyName result = new AssemblyName();
        result.Name = m_commandLine.TargetAssemblyName;
        if (m_commandLine.SignKeyFile != null) {
            if (!m_commandLine.DelaySign) {
                // load keypair
                StrongNameKeyPair snP = new StrongNameKeyPair(File.Open(m_commandLine.SignKeyFile.FullName, 
                                                                        FileMode.Open, 
                                                                        FileAccess.Read));
                result.KeyPair = snP;
            } else {
                // deleay signing, load only pk
                FileStream publicKeyStream = File.Open(m_commandLine.SignKeyFile.FullName, FileMode.Open);
                byte[] publicKey = new byte[publicKeyStream.Length];
                publicKeyStream.Read(publicKey, 0, (int)publicKeyStream.Length);
                // Provide the assembly with a public key.
                result.SetPublicKey(publicKey);
                publicKeyStream.Close();
            }
        }
        if (m_commandLine.AssemblyVersion != null) {
            result.Version = new Version(m_commandLine.AssemblyVersion);
        }
        return result;
    }
    

    public void MapIdl() {
        MetaDataGenerator generator = new MetaDataGenerator(GetAssemblyName(), 
                                                            m_commandLine.OutputDirectory.FullName,
                                                            m_commandLine.ReferencedAssemblies);
        if (m_commandLine.GenerateValueTypeSkeletons) {
            generator.EnableValueTypeSkeletonGeneration(m_vtSkelcodeDomProvider,
                                                        m_commandLine.ValueTypeSkeletonsTargetDir,
                                                        m_commandLine.OverwriteValueTypeSkeletons);
        }       
        generator.MapAnyToAnyContainer = m_commandLine.MapAnyToAnyContainer;
        if (m_commandLine.BaseInterface != null) {
            generator.InheritedInterface = m_commandLine.BaseInterface;
        }

        string currentDir = Directory.GetCurrentDirectory();
        for (int i = 0; i < m_commandLine.InputFileNames.Count; i++) {
            Debug.WriteLine("checking file: " + m_commandLine.InputFileNames[i] );

            string rootedPath = (string)m_commandLine.InputFileNames[i];
            if (!Path.IsPathRooted((string)m_commandLine.InputFileNames[i])) {
                rootedPath = Path.Combine(currentDir, (string)m_commandLine.InputFileNames[i]);
            }
            string searchDirectory = Path.GetDirectoryName(rootedPath);
            string fileName = Path.GetFileName(rootedPath);

            string[] expandedFiles = Directory.GetFileSystemEntries(searchDirectory, fileName);
            if (expandedFiles.Length > 0) {
                foreach (string file in expandedFiles) {
                    processFile(generator, file);
                }
            } else {
                Error("file(s) not found: " + m_commandLine.InputFileNames[i]);                
            }
        }
        // save the result to disk
        generator.SaveAssembly();
    }
        
    private void processFile( MetaDataGenerator generator, string file ) {
        Console.WriteLine("processing file: " + file);
        Trace.WriteLine("");
        MemoryStream source = Preprocess( file ); // preprocess the file
        IDLParser parser = new IDLParser(source);
        Trace.WriteLine("parsing file: " + file );
        ASTspecification spec = parser.specification();
        Trace.WriteLine(parser.getSymbolTable());
        // now parsed representation can be visited with the visitors    
        generator.InitalizeForSource(parser.getSymbolTable());
        spec.jjtAccept(generator, null);
        Trace.WriteLine("");
    }
    
    #endregion IMethods

}

}
