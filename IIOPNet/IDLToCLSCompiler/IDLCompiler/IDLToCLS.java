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
import parser.IDLParser;
import parser.ASTspecification;

import Ch.Elca.Iiop.IdlPreprocessor.IDLPreprocessor;

import System.Diagnostics.*;
import System.IO.Directory;
import System.Reflection.Assembly;
import java.util.LinkedList;

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
        } catch (Exception e) {
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
        File file = new File(fileName);
        // all Preprocessor instances share the same symbol definitions
        IDLPreprocessor preproc = new IDLPreprocessor(file);
        preproc.Process();
        InputStream result = preproc.GetProcessed();
        BufferedReader reader = new BufferedReader(new InputStreamReader(result));
        String line = "";
        while (line != null) {
            Debug.WriteLine(line);
            line = reader.readLine();
        }
        result.reset();
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
