/* IDLToCLS.java
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

package Ch.Elca.Iiop.IdlCompiler;


import java.io.*;

import Ch.Elca.Iiop.IdlCompiler.Action.MetaDataGenerator;
import Ch.Elca.Iiop.IdlCompiler.Action.CompilerMappingPlugin;
import parser.IDLParser;
import parser.ASTspecification;

import Ch.Elca.Iiop.IdlPreprocessor.IDLPreprocessor;

import System.Diagnostics.*;
import System.IO.Directory;
import System.IO.FileInfo;
import System.IO.DirectoryInfo;
import System.IO.MemoryStream;
import System.IO.SeekOrigin;
import System.Reflection.Assembly;
import java.util.LinkedList;

import System.Convert;
import System.IO.StreamReader;
import System.IO.FileStream;
import System.IO.FileMode;
import System.Text.Encoding;

/**
 * 
 * The main compiler class
 * 
 * @version 
 * @author dul
 * 
 *
 */
public class IDLToCLS {

    #region IFields
    
    private String[] m_inputFileNames = null;
    private String m_asmPrefix = null;
    private String m_destination = ".";

    private LinkedList m_refAssemblies = new LinkedList();
    
    #endregion IFields
    #region IConstructors

    public IDLToCLS(String[] args) {
        ParseArgs(args);
    }

    #endregion IConstructors
    #region SMethods

    public static void main(String[] args) {
        // enable trace
        // Trace.get_Listeners().Add(new TextWriterTraceListener(System.Console.get_Out()));
        // Trace.set_AutoFlush(true);
        // Trace.Indent();
        
        try {
            IDLToCLS mapper = new IDLToCLS(args);
            mapper.MapIdl();
        } catch (System.Exception e) {
            System.err.println("exception encountered: " + e);
            System.exit(2);
        }
    }
    
    private static void HowTo() {
        System.out.println("Compiler usage:");
        System.out.println("  IDLToCLSCompiler [options] <target assembly name> idl-files");
        System.out.println();
        System.out.println("creates a CLS assembly for the OMG IDL definition files.");
        System.out.println("target assembly name is the name of the target assembly without .dll");
        System.out.println("idl-files: one or more idl files containg OMG IDL definitions");
        System.out.println();
        System.out.println("options are:");
        System.out.println("-h              help");
        System.out.println("-o directory    output directory (default is `-o .`)");
        System.out.println("-r assembly     assemblys to check for types in, instead of generating them");
        System.out.println("-c xmlfile      specifies custom mappings");
        System.out.println("-d define       defines a preprocessor symbol");
        System.out.println("-idir directory specifies the directory, containing default idl files");
    }
    
    public static void Error(String message) {
        System.out.println(message);
        System.out.println();
        HowTo();
        System.exit(1);
    }

    #endregion SMethods
    #region IMethods
    
    private void ParseArgs(String[] args) {
        int i = 0;

        while ((i < args.length) && (args[i].startsWith("-"))) {
            if (args[i].equals("-h")) {
                HowTo();
                i++;
                System.exit(0);
            } else if (args[i].equals("-o")) {
                i++;
                m_destination = args[i++];
            } else if (args[i].equals("-r")) {
                i++;
                try {                    
                    Assembly refAsm = Assembly.LoadFrom(args[i++]);
                    m_refAssemblies.add(refAsm);
                } catch (Exception ex) {
                    System.out.println("can't load assembly: " + args[i]);
                    System.exit(3);
                }                
            } else if (args[i].equals("-c")) {
                i++;
                System.IO.FileInfo configFile = new System.IO.FileInfo(args[i++]);
                // add custom mappings from file
                CompilerMappingPlugin plugin = CompilerMappingPlugin.GetSingleton();
                plugin.AddMappingsFromFile(configFile);
            } else if (args[i].equals("-d")) {
                i++;
                IDLPreprocessor.AddDefine(args[i++].Trim());
            } else if (args[i].equals("-idir")) {
                i++;
                DirectoryInfo dir = new DirectoryInfo(args[i++]);
                IDLPreprocessor.SetIdlDir(dir);
            } else {
                Error(String.Format("Error: invalid option {0}", args[i]));
            }
        }

        if (!Directory.Exists(m_destination)) {
            Directory.CreateDirectory(m_destination);
        }
        
        if ((i + 2) > args.length) {
            Error("Error: target assembly name or idl-file missing");
        }
        
        m_asmPrefix = args[i];
        i++;
        
        m_inputFileNames = new String[args.length - i];
        System.arraycopy(args, i, m_inputFileNames, 0, m_inputFileNames.length);
    }
    
    private InputStream Preprocess(String fileName) throws Exception {
        FileInfo file = new FileInfo(fileName);
        // all Preprocessor instances share the same symbol definitions
        IDLPreprocessor preproc = new IDLPreprocessor(file);
        preproc.Process();
        MemoryStream resultProc = preproc.GetProcessed();
        Encoding latin1 = Encoding.GetEncoding("ISO-8859-1");
        StreamReader stReader = new StreamReader(resultProc, latin1);
        String line = "";
        while (line != null) {
            Debug.WriteLine(line);
            line = stReader.ReadLine();
        }
        resultProc.Seek(0, SeekOrigin.Begin);
        ByteArrayInputStream result = new ByteArrayInputStream(
                                              ConvertUByteArray(resultProc.ToArray()));
        return result;
    }
    
    /**
     * converts a byte[] to an sbyte[]
     **/
    private byte[] ConvertUByteArray(ubyte[] arg) {
        if (arg == null) {
            return null;
        }
        byte[] result = new byte[arg.get_Length()];
        for (int i = 0; i < arg.get_Length(); i++) {
            result[i] = (byte)(arg[i]);
        }
        return result;
    }
    
    public void MapIdl() throws Exception {
        MetaDataGenerator generator = new MetaDataGenerator(m_asmPrefix, m_destination, m_refAssemblies);
        for (int i = 0; i < m_inputFileNames.length; i++) {
            System.out.println("processing file: " + m_inputFileNames[i]);
            Trace.WriteLine("");
            InputStream source = Preprocess(m_inputFileNames[i]); // preprocess the file
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
