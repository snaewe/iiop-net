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
    
    #endregion IFields
    #region IConstructors

    public IDLToCLS(String[] args) {
        ParseArgs(args);
    }

    #endregion IConstructors
    #region SMethods

    public static void Main(String[] args) {
        // enable trace
        // Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
        // Trace.AutoFlush = true;
        // Trace.Indent();
        
        try {
            IDLToCLS mapper = new IDLToCLS(args);
            mapper.MapIdl();
        } catch (System.Exception e) {
            Console.WriteLine("exception encountered: " + e);
            Environment.Exit(2);
        }
    }
    
    private static void HowTo() {
        Console.WriteLine("Compiler usage:");
        Console.WriteLine("  IDLToCLSCompiler [options] <target assembly name> idl-files");
        Console.WriteLine();
        Console.WriteLine("creates a CLS assembly for the OMG IDL definition files.");
        Console.WriteLine("target assembly name is the name of the target assembly without .dll");
        Console.WriteLine("idl-files: one or more idl files containg OMG IDL definitions");
        Console.WriteLine();
        Console.WriteLine("options are:");
        Console.WriteLine("-h              help");
        Console.WriteLine("-o directory    output directory (default is `-o .`)");
        Console.WriteLine("-r assembly     assemblies to check for types in, instead of generating them");
        Console.WriteLine("-c xmlfile      specifies custom mappings");
        Console.WriteLine("-d define       defines a preprocessor symbol");
        Console.WriteLine("-idir directory directory containing idl files (multiple -idir allowed)");
        Console.WriteLine("-vtSkel         enable creation of value type implementation skeletons");
        Console.WriteLine("-vtSkelProv     The fully qualified name of the codedomprovider to use for value type skeleton generation");
        Console.WriteLine("-vtSkelTd       The targetDirectory for generated valuetype impl skeletons");
        Console.WriteLine("-vtSkelO        Overwrite already present valuetype skeleton implementations");
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
                System.IO.FileInfo configFile = new System.IO.FileInfo(args[i++]);
                // add custom mappings from file
                CompilerMappingPlugin plugin = CompilerMappingPlugin.GetSingleton();
                plugin.AddMappingsFromFile(configFile);
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

    public void MapIdl() {
        MetaDataGenerator generator = new MetaDataGenerator(m_asmPrefix, m_destination, 
                                                            m_refAssemblies);
        if (m_vtSkelEnable) {
            generator.EnableValueTypeSkeletonGeneration(m_vtSkelcodeDomProvider,
                                                        m_vtSkelTd,
                                                        m_vtSkelOverwrite);
        }
        
        for (int i = 0; i < m_inputFileNames.Length; i++) {
            Console.WriteLine("processing file: " + m_inputFileNames[i]);
            Trace.WriteLine("");
            MemoryStream source = Preprocess(m_inputFileNames[i]); // preprocess the file
            IDLParser parser = new IDLParser(source);
            Trace.WriteLine("parsing file: " + m_inputFileNames[i]);
            ASTspecification spec = parser.specification();
            Trace.WriteLine(parser.getSymbolTable());
            // now parsed representation can be visited with the visitors    
            generator.InitalizeForSource(parser.getSymbolTable());
            spec.jjtAccept(generator, null);
            Trace.WriteLine("");
        }
        // save the result to disk
        generator.SaveAssembly();
    }
    
    #endregion IMethods

}

}
