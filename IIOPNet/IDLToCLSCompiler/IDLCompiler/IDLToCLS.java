/* IDLToCLS.java
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 14.02.03  Dominic Ullmann (DUL), dul@elca.ch
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


import java.io.*;

import action.MetaDataGenerator;

import parser.IDLParser;
import parser.ASTspecification;

import System.Diagnostics.*;

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

    #region SMethods

    public static void main(String[] args) {
        // enable trace
        // Trace.get_Listeners().Add(new TextWriterTraceListener(System.Console.get_Out()));
        // Trace.set_AutoFlush(true);
        // Trace.Indent();
        
        try {
            String asmPrefix = GetAsmPrefix(args);
            MetaDataGenerator generator = new MetaDataGenerator(asmPrefix);
            String[] inputFileNames = GetFileArgs(args);
            for (int i = 0; i < inputFileNames.length; i++) {
                System.out.println("processing file: " + inputFileNames[i]);
                Trace.WriteLine("");
                InputStream source = Preprocess(inputFileNames[i]); // preprocess the file
                IDLParser parser = new IDLParser(source);
                Trace.WriteLine("parsing file: " + inputFileNames[i]);
                ASTspecification spec = parser.specification();
                Trace.WriteLine(parser.getSymbolTable());
                // now parsed representation can be visited with the visitors    
                generator.InitalizeForSource(parser.getSymbolTable());
                spec.jjtAccept(generator, null);
                Trace.WriteLine("");
            }
            // save the result to disk
            generator.SaveAssembly();
        } catch (Exception e) {
            System.err.println("exception encountered: " + e);
        }
        
        
    }
    
    private static String GetAsmPrefix(String[] args) throws Exception {
        if ((args.length < 1) || (args[0].endsWith(".idl"))) {
            PrintUsage();
            throw new Exception("target assembly name missing"); 
        }
        return args[0];
    }

    private static String[] GetFileArgs(String[] args) throws Exception {
        if (args.length < 2) {
            PrintUsage();
            throw new Exception("file argument missing"); 
        }
        String[] result = new String[args.length-1];
        System.arraycopy(args, 1, result, 0, result.length);
        return result;
    }

    private static InputStream Preprocess(String fileName) throws Exception {
        File file = new File(fileName);
        // all Preprocessor instances share the same symbol definitions
        IDLPreprocessor.IDLPreprocessor preproc = new IDLPreprocessor.IDLPreprocessor(file);
        preproc.process();
        InputStream result = preproc.getProcessed();
        BufferedReader reader = new BufferedReader(new InputStreamReader(result));
        String line = "";
        while (line != null) {
            Debug.WriteLine(line);
            line = reader.readLine();
        }
        result.reset();
        return result;
    }
    
    private static void PrintUsage() {
        System.out.println("first argument: target assembly name");
        System.out.println("other arguments: idl-files");
    }

    #endregion SMethods

}
