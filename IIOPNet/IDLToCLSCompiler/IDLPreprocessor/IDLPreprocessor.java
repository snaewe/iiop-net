/* IDLPreprocessor.java
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

package Ch.Elca.Iiop.IdlPreprocessor;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.util.Hashtable;
import java.util.LinkedList;
import java.io.FileReader;
import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.ByteArrayInputStream;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.util.StringTokenizer;


/**
 * Summary description for Class1.
 */
public class IDLPreprocessor {
    
    #region IFields

    private BufferedReader m_fileStream;

    private InputStream m_processed = null;

    /** stores the preprocessor result */
    private PrintWriter m_outputStream;
    private ByteArrayOutputStream m_outData;

    private int m_ifOpen = 0;

    private Hashtable m_defined;

    #endregion IFields
    #region IConstructors
    
    /** @param resolveInclude should includefiles be included or not
     */
    public IDLPreprocessor(File toProcess) throws Exception, java.io.IOException {
        m_defined = new Hashtable();
        Init(toProcess);
    }

    /** internal constructor to resolve included files */
    private IDLPreprocessor(File toProcess, Hashtable symbols) throws Exception, java.io.IOException {
        m_defined = symbols;
        Init(toProcess);
    }

    #endregion IConstructors
    #region IMethods

    private void Init(File toProcess) throws Exception, java.io.IOException {
        m_fileStream = new BufferedReader(new FileReader(toProcess));
        m_outData = new ByteArrayOutputStream();
        m_outputStream = new PrintWriter(new OutputStreamWriter(m_outData), true);
    }

    /** processes the file */
    public void Process() throws Exception, java.io.IOException {
        String currentLine = m_fileStream.readLine();
        while (currentLine != null) {
            currentLine = currentLine.trim();
            if (currentLine.startsWith("#include"))    {
                ProcessInclude(currentLine);
            } else if (currentLine.startsWith("#define")) {
                ProcessDefine(currentLine);
            } else if (currentLine.startsWith("#ifndef")) {
                ProcessIfNDef(currentLine);
            } else if (currentLine.startsWith("#endif")) {
                ProcessEndIf(currentLine);
            } else {
                // write the current line to the output stream
                m_outputStream.println(currentLine);
            }                                       
            currentLine = m_fileStream.readLine();
        }
        m_fileStream.close();
        m_outputStream.println(""); // add a newline at the end, because parser needs at least one line
        m_processed = new ByteArrayInputStream(m_outData.toByteArray());
    }

    

    /** gets the preprocessed file for further processing */
    public InputStream GetProcessed() {
        return m_processed;
    }

    #region implementation of the preprocessing actions

    private void ProcessInclude(String currentLine) throws Exception {
        StringTokenizer tokenizer = new StringTokenizer(currentLine.trim());
        if (tokenizer.countTokens() <= 1) { 
            throw new Exception("include missing argument"); 
        }
        tokenizer.nextToken(); // ignore #include
        String fileToInclude = tokenizer.nextToken();
        if (fileToInclude.startsWith("\"")) { 
            fileToInclude = fileToInclude.substring(1); 
        }
        if (fileToInclude.endsWith("\"")) { 
            fileToInclude = fileToInclude.substring(0, fileToInclude.length() - 1); 
        }

        File toInclude = new File(fileToInclude);
        IDLPreprocessor includePreproc = new IDLPreprocessor(toInclude, m_defined);
        includePreproc.Process();
        InputStream result = includePreproc.GetProcessed();
        // copy result into current output stream
        CopyToOutputStream(result);
    }
    
    private void CopyToOutputStream(InputStream input) throws Exception {
        BufferedReader resultReader = new BufferedReader(new java.io.InputStreamReader(input));
        String currentLine = resultReader.readLine();
        while (currentLine != null) {
            m_outputStream.println(currentLine);
            currentLine = resultReader.readLine();
        }
        resultReader.close();
    }    
    
    private void ProcessDefine(String currentLine) throws Exception {

        StringTokenizer tokenizer = new StringTokenizer(currentLine.trim());
        if (tokenizer.countTokens() <= 1) { 
            throw new Exception("define missing argument"); 
        }
        if (tokenizer.countTokens() > 3) { 
            throw new Exception("too much tokens in define directive"); 
        }
        tokenizer.nextToken(); // ignore #ifndef token
        String define = tokenizer.nextToken();
                
        String value = "";
        if (tokenizer.hasMoreTokens()) {
            value = tokenizer.nextToken();
        }
        if (m_defined.containsKey(define)) {
            throw new Exception("redefinition of a variable: " + define);
        }
        m_defined.put(define, value);
        System.Diagnostics.Debug.WriteLine("defined symbol in preproc: " + define);
    }    
    
    private void ProcessIfNDef(String currentLine) throws Exception {
        m_ifOpen++;
        StringTokenizer tokenizer = new StringTokenizer(currentLine.trim());
        if (tokenizer.countTokens() <= 1) { 
            throw new Exception("ifndef missing argument"); 
        }
        if (tokenizer.countTokens() > 2) { 
            throw new Exception("too much tokens in ifndef directive"); 
        }
        tokenizer.nextToken(); // ignore #ifndef token
        String define = tokenizer.nextToken();
        if (m_defined.containsKey(define)) { // throw everything in block away
            SearchForEndif();
            m_ifOpen--;
        }
    }

    private void ProcessEndIf(String currentLine) throws Exception {
        m_ifOpen--;
        if (m_ifOpen < 0) { 
            throw new Exception("too much endif's encountered"); 
        }
    }

    /** search for a matching end-if for an if tag, throwing away everything in between */
    private void SearchForEndif() throws Exception {
        int moreIfs = 1; // more if's encountered than endif
        String currentLine = "";
        while ((moreIfs > 0) && (currentLine != null)) {
            if (currentLine.startsWith("#if")) {
                moreIfs++; 
            }
            if (currentLine.startsWith("#endif")) { 
                moreIfs--; 
            }
            if (moreIfs > 0) { 
                currentLine = m_fileStream.readLine().trim(); 
            }
        }
    }    

    #endregion implementation of the preprocessing actions

    #endregion IMethods

}

