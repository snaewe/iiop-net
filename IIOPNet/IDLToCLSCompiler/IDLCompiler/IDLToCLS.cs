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
    
    private String[] m_inputFileNames = null;
    private String m_asmPrefix = null;
    private String m_destination = ".";

    private ArrayList m_refAssemblies = new ArrayList();
    
    private CodeDomProvider m_vtSkelcodeDomProvider = new CSharpCodeProvider();
    private DirectoryInfo m_vtSkelTd = new DirectoryInfo(".");
    private bool m_vtSkelOverwrite = false;
    private bool m_vtSkelEnable = false;
    
    private string m_keyFile;
    private bool m_delaySign = false;
    private string m_asmVersion = null;
    private bool m_mapAnyToAnyCont = false;
    
    #endregion IFields
    #region IConstructors

    public IDLToCLS(String[] args) {
        ParseArgs(args);
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
        Console.WriteLine("Compiler usage:");
        Console.WriteLine("  IDLToCLSCompiler [options] target_assembly_name idl-files");
        Console.WriteLine();
        Console.WriteLine("creates a CLS assembly for the OMG IDL definition files.");
        Console.WriteLine("target_assembly_name is the name of the target assembly without .dll");
        Console.WriteLine("idl-files: one or more idl files containg OMG IDL definitions");
        Console.WriteLine();
        Console.WriteLine("options are:");
        Console.WriteLine("-h              help");
        Console.WriteLine("-o directory    output directory (default is `-o .`)");
        Console.WriteLine("-r assembly     assemblies to check for types in, instead of generating them");
        Console.WriteLine("-c xmlfile      specifies custom mappings");
        Console.WriteLine("-d define       defines a preprocessor symbol");
        Console.WriteLine("-basedir directory directory to change to before doing any processing.");
        Console.WriteLine("-idir directory directory containing idl files (multiple -idir allowed)");
        Console.WriteLine("-vtSkel         enable creation of value type implementation skeletons");
        Console.WriteLine("-vtSkelProv     The fully qualified name of the codedomprovider to use for value type skeleton generation");
        Console.WriteLine("-vtSkelTd       The targetDirectory for generated valuetype impl skeletons");
        Console.WriteLine("-vtSkelO        Overwrite already present valuetype skeleton implementations");
        Console.WriteLine("-snk            sign key file (used for generating strong named assemblies)");
        Console.WriteLine("-delaySign      delay signing of assembly (snk file contains only a pk)");
        Console.WriteLine("-asmVersion     the version of the generated assembly");
        Console.WriteLine("-mapAnyToCont   maps idl any to the any container omg.org.CORBA.Any; if not specified, any is mapped to object");
    }
    
    public static void Error(String message) {
        Console.WriteLine(message);
        Console.WriteLine();
        HowTo();
        Environment.Exit(1);
    }

    #endregion SMethods
    #region IMethods
    
    private void ParseArgs(String[] args) {
        int i = 0;
        ArrayList customMappingFiles = new ArrayList();

        while ((i < args.Length) && (args[i].StartsWith("-"))) {
            if (args[i].Equals("-h")) {
                HowTo();
                i++;
                Environment.Exit(0);
            } else if (args[i].Equals("-o")) {
                i++;
                m_destination = args[i++];
            } else if (args[i].Equals("-r")) {
                i++;
                try {                    
                    Assembly refAsm = Assembly.LoadFrom(args[i++]);
                    m_refAssemblies.Add(refAsm);
                } catch (Exception ex) {
                    Console.WriteLine("can't load assembly: " + args[i] + "\n" + ex);
                    Environment.Exit(3);
                }                
            } else if (args[i].Equals("-c")) {
                i++;
                FileInfo customMappingFile = new System.IO.FileInfo(args[i++]);
                if (!customMappingFiles.Contains(customMappingFile)) {
                    customMappingFiles.Add(customMappingFile);
                } else {
                    Error("tried to add a custom mapping file multiple times: " + customMappingFile.FullName);
                }
            } else if (args[i].Equals("-d")) {
                i++;
                IDLPreprocessor.AddDefine(args[i++].Trim());
            } else if (args[i].Equals("-idir")) {
                i++;
                DirectoryInfo dir = new DirectoryInfo(args[i++]);
                IDLPreprocessor.SetIdlDir(dir);
            } else if (args[i].Equals("-vtSkel")) {
                i++;
                m_vtSkelEnable = true;
            } else if (args[i].Equals("-vtSkelProv")) {
                i++;
                string providerTypeName = args[i++].Trim();
                Type codeDomProvType = Type.GetType(providerTypeName, false);
                if (codeDomProvType == null) {
                    Error(String.Format("provider {0} not found!",
                                        providerTypeName));
                }
                m_vtSkelcodeDomProvider = 
                    (CodeDomProvider) Activator.CreateInstance(codeDomProvType);
                                           
                
            } else if (args[i].Equals("-vtSkelTd")) {
                i++;
                m_vtSkelTd = new DirectoryInfo(args[i++]);
            } else if (args[i].Equals("-vtSkelO")) {
                i++;
                m_vtSkelOverwrite = true;
            } else if (args[i].Equals("-snk")) {
                i++;
                m_keyFile = args[i++];
            } else if (args[i].Equals("-delaySign")) {
                i++;
                m_delaySign = true;
            } else if (args[i].Equals("-asmVersion")) {
                i++;
                m_asmVersion = args[i++];
            } else if (args[i].Equals("-basedir")) {
                i++;
                string base_dir = args[i++];
                if (!Directory.Exists(base_dir))
                {
                    Error( String.Format("Error: base directory {0} does not exist!", base_dir ) );
                    Environment.Exit(3);
                }
                Environment.CurrentDirectory = (base_dir);
            } else if (args[i].Equals("-mapAnyToCont")) {
                i++;
                m_mapAnyToAnyCont = true;
            } else {
                Error(String.Format("Error: invalid option {0}", args[i]));
            }
        }

        if (!Directory.Exists(m_destination)) {
            Directory.CreateDirectory(m_destination);
        }
        
        if ((i + 2) > args.Length) {
            Error("Error: target assembly name or idl-file missing");
        }
        
        m_asmPrefix = args[i];
        i++;
        
        m_inputFileNames = new String[args.Length - i];
        Array.Copy(args, i, m_inputFileNames, 0, m_inputFileNames.Length);                
        
        SetupAssemblyResolver();
        AddCustomMappings(customMappingFiles);
        
    }
    
    /// <summary>setup assembly resolution: consider all directories of /r files 
    /// and current directory as assembly containing directories</summary>
    private void SetupAssemblyResolver() {
        ArrayList searchDirectoryList = new ArrayList();
        searchDirectoryList.Add(new DirectoryInfo(".").FullName);
        foreach (Assembly refAsm in m_refAssemblies) {
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
        result.Name = m_asmPrefix;
        if (m_keyFile != null) {
            if (!m_delaySign) {
                // load keypair
                StrongNameKeyPair snP = new StrongNameKeyPair(File.Open(m_keyFile, 
                                                                        FileMode.Open, 
                                                                        FileAccess.Read));
                result.KeyPair = snP;
            } else {
                // deleay signing, load only pk
                FileStream publicKeyStream = File.Open(m_keyFile, FileMode.Open);
                byte[] publicKey = new byte[publicKeyStream.Length];
                publicKeyStream.Read(publicKey, 0, (int)publicKeyStream.Length);
                // Provide the assembly with a public key.
                result.SetPublicKey(publicKey);
                publicKeyStream.Close();
            }
        }
        if (m_asmVersion != null) {
            result.Version = new Version(m_asmVersion);
        }
        return result;
    }
    

    public void MapIdl() {
        MetaDataGenerator generator = new MetaDataGenerator(GetAssemblyName(), 
                                                            m_destination,
                                                            m_refAssemblies);
        if (m_vtSkelEnable) {
            generator.EnableValueTypeSkeletonGeneration(m_vtSkelcodeDomProvider,
                                                        m_vtSkelTd,
                                                        m_vtSkelOverwrite);
        }       
        generator.MapAnyToAnyContainer = m_mapAnyToAnyCont;
        
        string currentDir = Directory.GetCurrentDirectory();
        for (int i = 0; i < m_inputFileNames.Length; i++) {
            Debug.WriteLine("checking file: " + m_inputFileNames[i] );

            string rootedPath = m_inputFileNames[i];
            if (!Path.IsPathRooted(m_inputFileNames[i])) {
                rootedPath = Path.Combine(currentDir, m_inputFileNames[i]);
            }
            string searchDirectory = Path.GetDirectoryName(rootedPath);
            string fileName = Path.GetFileName(rootedPath);

            string[] expandedFiles = Directory.GetFileSystemEntries(searchDirectory, fileName);
            if (expandedFiles.Length > 0) {
                foreach (string file in expandedFiles) {
                    processFile(generator, file);
                }
            } else {
                Error("file(s) not found: " + m_inputFileNames[i]);                
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
