/* IDLToCLS.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 30.04.06  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2006 Dominic Ullmann
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
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.IdlCompiler {


    /// <summary>
    /// The class responsible for handling the compiler command line.
    /// </summary>
    public class IDLToCLSCommandLine {
        
        #region IFields
        
        private string m_targetAssemblyName;
        private IList /* <string> */ m_inputFileNames = new ArrayList();
        private DirectoryInfo m_outputDirectory = new DirectoryInfo(".");
        private IList /* <FileInfo> */ m_customMappingFiles = new ArrayList();
        private FileInfo m_signKeyFile = null;
        private bool m_delaySign = false;
        private string m_asmVersion = null;
        private bool m_mapAnyToAnyContainer = false;
        private DirectoryInfo m_baseDirectory = null;
        private Type m_baseInterface = null;
        private bool m_generateVtSkeletons = false;
        private bool m_overwriteVtSkeletons = false;
        private DirectoryInfo m_vtSkeletonsTargetDir = new DirectoryInfo(".");
        private Type m_vtSkelcodeDomProviderType;
        private IList /* <DirectoryInfo> */ m_idlSourceDirs = new ArrayList();
        private IList /* <Assembly> */ m_refAssemblies = new ArrayList();
        private IList /* <string> */ m_preprocessorDefines = new ArrayList();
        private IList /* <DirectoryInfo> */ m_libDirectories = new ArrayList();
        
        private bool m_isInvalid = false;
        private string m_errorMessage = String.Empty;
        private bool m_isHelpRequested = false;
        
        #endregion IFields
        #region IConstructors
        
        public IDLToCLSCommandLine(string[] args) {
            ParseArgs(args);
        }
        
        #endregion IConstructors
        #region IProperties
        
        
        /// <summary>
        /// the name of the target assembly.
        /// </summary>
        public string TargetAssemblyName {
            get {
                return m_targetAssemblyName;
            }
        }
        
        /// <summary>
        /// the list of input file names; no fileinfo, because relative pathes are relative to base dir not current dir at the moment
        /// </summary>
        public IList /* <string> */ InputFileNames {
            get {
                return m_inputFileNames;
            }
        }
        
        /// <summary>the directory, the output will be written to.</summary>
        public DirectoryInfo OutputDirectory {
            get {
                return m_outputDirectory;
            }
        }

        /// <summary>the custom mapping files.</summary>
        public IList /* <FileInfo> */ CustomMappingFiles {
            get {
                return m_customMappingFiles;
            }
        }
        
        /// <summary>the key file used to sign the resulting assembly</summary>
        public FileInfo SignKeyFile {
            get {
                return m_signKeyFile;
            }
        }
        
        /// <summary>delay sign the assembly</summary>
        public bool DelaySign {
            get {
                return m_delaySign;
            }
        }
        
        /// <summary>the version of the target assembly.</summary>
        public string AssemblyVersion {
            get {
                return m_asmVersion;
            }
        }
        
        /// <summary>
        /// returns true, if any should be map to the any container type instead of object.
        /// </summary>
        public bool MapAnyToAnyContainer {
            get {
                return m_mapAnyToAnyContainer;
            }
        }
        
        /// <summary>
        /// the directory to change to, before doing any processing.
        /// </summary>
        public DirectoryInfo BaseDirectory {
            get {
                return m_baseDirectory;
            }
        }
        
        /// <summary>
        /// option to specify, that a generated concrete / abstract interface should inherit from
        /// a certain base interface.
        /// </summary>
        public Type BaseInterface {
            get {
                return m_baseInterface;                
            }                
        }
        
        /// <summary>
        /// Generate ValueType skeletons or not.
        /// </summary>
        public bool GenerateValueTypeSkeletons {
            get {
                return m_generateVtSkeletons;
            }
        }
        
        /// <summary>
        /// Overwrite already generated value type skeletons or not.
        /// </summary>
        public bool OverwriteValueTypeSkeletons {
            get {
                return m_overwriteVtSkeletons;
            }
        }        
        
        /// <summary>
        /// The target directory for the generated value type skeletons.
        /// </summary>
        public DirectoryInfo ValueTypeSkeletonsTargetDir {
            get {
                return m_vtSkeletonsTargetDir;
            }
        }
        
        /// <summary>
        /// the codedom provider to use for Valuetype skeleton generation.
        /// </summary>
        public Type ValueTypeSkeletonCodeDomProviderType {
            get {
                return m_vtSkelcodeDomProviderType;
            }
        }
        
        /// <summary>
        /// directories to search in for idl files.
        /// </summary>
        public IList /* <DirectoryInfo> */ IdlSourceDirectories {
            get {
                return m_idlSourceDirs;
            }
        }
        
        /// <summary>
        /// the referenced assemblies.
        /// </summary>
        public IList /* <Assembly> */ ReferencedAssemblies {
            get {
                return m_refAssemblies;
            }
        }
        
        /// <summary>
        /// the preprocessor defines.
        /// </summary>
        public IList /* <string> */ PreprocessorDefines {
            get {
                return m_preprocessorDefines;
            }
        }
        
        /// <summary>
        /// the lib directories to search for references.
        /// </summary>
        public IList /* DirectoryInfo> */ LibDirectories {
            get {
                return m_libDirectories;
            }
        }
        
        /// <summary>returns true, if an error has been detected.</summary>
        public bool IsInvalid {
            get {
                return m_isInvalid;
            }
        }
        
        /// <summary>
        /// if IsInvalid is true, contains the corresponding error message.
        /// </summary>
        public string ErrorMessage {
            get {
                return m_errorMessage;
            }
        }
        
        /// <summary>returns true, if help is requested.</summary>
        public bool IsHelpRequested {
            get {
                return m_isHelpRequested;
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        private void SetIsInvalid(string message) {
            m_isInvalid = true;
            m_errorMessage = message;
        }
        
        private bool ContainsFileInfoAlready(IList list, FileInfo info) {
            for (int i = 0; i < list.Count; i++) {
                if (((FileInfo)list[i]).FullName == info.FullName) {
                    return true;
                }
            }
            return false;
        }
        
        private void AddRefAssemblies(IList refAssemblies, IList libDirectories) {
            ArrayList allLibDirectories = new ArrayList();
            allLibDirectories.Add(new DirectoryInfo(".")); // current dir is the first to search
            allLibDirectories.AddRange(libDirectories);                        
            for (int j = 0; j < refAssemblies.Count; j++) {
                string errorMsg;
                if (!AddRefAssembly((string)refAssemblies[j], allLibDirectories, out errorMsg)) {
                    SetIsInvalid(errorMsg);
                    return;
                }
            }
        }
        
        private bool AddRefAssembly(string asmFileName, IList libDirectories,
                                    out string errorMsg) {
            errorMsg = String.Empty;
            Assembly loaded = null;
            if (!Path.IsPathRooted(asmFileName)) {
                foreach (DirectoryInfo libDir in libDirectories) {
                    string asmFullyQualifiedFile = Path.Combine(libDir.FullName, asmFileName);
                    if (File.Exists(asmFullyQualifiedFile)) {                    
                        loaded = LoadRefAssembly(asmFullyQualifiedFile, out errorMsg);
                        if (loaded != null) {
                            break;
                        }
                    }                
                }
            }
            if (loaded == null) {
                loaded = LoadRefAssembly(asmFileName, out errorMsg);
            }
            if (loaded != null) {
                m_refAssemblies.Add(loaded);
            }
            return (loaded != null);
        }
        
        private Assembly LoadRefAssembly(string asmName, out string errorMessage) {
            errorMessage = String.Empty;
            try {
                Assembly refAsm = Assembly.LoadFrom(asmName);
                return refAsm;
            } catch (Exception ex) {
                errorMessage = "can't load assembly: " + asmName + "\n" + ex;
                return null;
            }            
        }
        
        private void ParseArgs(string[] args) {
            int i = 0;
            ArrayList refAssemblies = new ArrayList();

            while ((i < args.Length) && (args[i].StartsWith("-"))) {
                if (args[i].Equals("-h") || args[i].Equals("-help")) {
                    m_isHelpRequested = true;
                    return;
                } else if (args[i].Equals("-o")) {
                    i++;
                    m_outputDirectory = new DirectoryInfo(args[i++]);
                } else if (args[i].StartsWith("-out:")) {                    
                    m_outputDirectory = new DirectoryInfo(args[i++].Substring(5));
                } else if (args[i].StartsWith("-fidl:")) {
                    try {
                        string idlFilesInFile = ReadAllTextFile(args[i++].Substring(6));            			
                        string[] idlFiles = idlFilesInFile.
                            Split(new char[] { ' ', '\t', '\n' });
                        foreach (string idlFile in idlFiles) {
                            if (idlFile != null && idlFile.Length > 0) {
                                this.m_inputFileNames.Add(idlFile.Trim());
                            }
                        }
                    } catch(Exception ex) {
                        SetIsInvalid("Failed to read file containing idl files: " + ex);
                    }
                } else if (args[i].StartsWith("-pidl:")) {
                    try {
                        string directoryWithIdlFiles = args[i++].Substring(6);
                        if (!Directory.Exists(directoryWithIdlFiles)) {
                            SetIsInvalid("Directory containing idl files to search does not exist: " + directoryWithIdlFiles);
                        }
                        string[] idlFiles = FindIdlFilesRecursively(directoryWithIdlFiles);
                        foreach (string idlFile in idlFiles) {
                            this.m_inputFileNames.Add(idlFile.Trim());
                        }
                    } catch (Exception ex) {
                        SetIsInvalid("Failed to retrieve all idl files recursively: " + ex);
                    }
                } else if (args[i].Equals("-r")) {
                    i++;
                    refAssemblies.Add(args[i++]);                    
                } else if (args[i].StartsWith("-r:")) {
                    refAssemblies.Add(args[i++].Substring(3));
                } else if (args[i].Equals("-c")) {
                    i++;
                    FileInfo customMappingFile = new System.IO.FileInfo(args[i++]);
                    if (!ContainsFileInfoAlready(m_customMappingFiles, customMappingFile)) {
                        m_customMappingFiles.Add(customMappingFile);
                    } else {
                        SetIsInvalid("tried to add a custom mapping file multiple times: " + customMappingFile.FullName);
                        return;
                    }
                    
                } else if (args[i].Equals("-snk")) {
                    i++;
                    m_signKeyFile = new FileInfo(args[i++]);
                } else if (args[i].Equals("-delaySign")) {
                    i++;
                    m_delaySign = true;
                } else if (args[i].Equals("-asmVersion")) {
                    i++;
                    m_asmVersion = args[i++];                    
                } else if (args[i].Equals("-mapAnyToCont")) {
                    i++;
                    m_mapAnyToAnyContainer = true;
                } else if (args[i].Equals("-basedir")) {
                    i++;
                    m_baseDirectory = new DirectoryInfo(args[i++]);
                    if (!Directory.Exists(m_baseDirectory.FullName)) {
                        SetIsInvalid(String.Format("Error: base directory {0} does not exist!", 
                                                   m_baseDirectory.FullName ) );
                        return;
                    }                    
                } else if (args[i].Equals("-idir")) {
                    i++;
                    m_idlSourceDirs.Add(new DirectoryInfo(args[i++]));
                } else if (args[i].Equals("-b")){                    
                    i++;
                    string baseInterfaceName = args[i++];
                    m_baseInterface = Type.GetType(baseInterfaceName, false);
                    if (m_baseInterface == null) {
                        SetIsInvalid(String.Format("Error: base interface {0} does not exist!", 
                                                   baseInterfaceName));
                        return;
                    }
                } else if (args[i].Equals("-d")) {
                    i++;
                    m_preprocessorDefines.Add(args[i++].Trim());
                } else if (args[i].Equals("-vtSkel")) {
                    i++;
                    m_generateVtSkeletons = true;
                } else if (args[i].Equals("-vtSkelProv")) {
                    i++;
                    string providerTypeName = args[i++].Trim();                    
                    m_vtSkelcodeDomProviderType = Type.GetType(providerTypeName, false);
                    if (m_vtSkelcodeDomProviderType == null) {
                        SetIsInvalid(String.Format("provider {0} not found!",
                                            providerTypeName));
                        return;
                    }
                } else if (args[i].Equals("-vtSkelTd")) {
                    i++;
                    m_vtSkeletonsTargetDir = new DirectoryInfo(args[i++]);
                } else if (args[i].Equals("-vtSkelO")) {
                    i++;
                    m_overwriteVtSkeletons = true;
                } else if (args[i].StartsWith("-lib:")) {
                    string libDirsString = args[i++].Substring(5);
                    string[] libDirs = libDirsString.Split(';');
                    for (int j = 0; j < libDirs.Length; j++) {
                        m_libDirectories.Add(new DirectoryInfo(libDirs[j]));
                    }
                } else {
                    SetIsInvalid(String.Format("Error: invalid option {0}", args[i]));
                    return;
                }                
            }

            if (i >= args.Length) { // target assembly name is next argument, which is not already parsed.
                SetIsInvalid("Error: target assembly name missing");
                return;
            }
            
            m_targetAssemblyName = args[i];
            i++;            
            
            for (int j = i; j < args.Length; j++) {
                m_inputFileNames.Add(args[j]);
            }

            if (m_inputFileNames.Count == 0) {
                SetIsInvalid("Error: idl-file(s) missing");
                return;
            }
            
            AddRefAssemblies(refAssemblies, m_libDirectories);
        }
        
        private string ReadAllTextFile(string path) {
            using (StreamReader sr = new StreamReader(path)) {
                return sr.ReadToEnd();
            }
        }
        
        private string[] FindIdlFilesRecursively(string directory) {
            ArrayList result = new ArrayList();
            string[] filesInCurrentDirectory = Directory.GetFiles(directory, "*.idl");
            result.AddRange(filesInCurrentDirectory);
            string[] subdirectories = Directory.GetDirectories(directory);
            foreach (string subdirectory in subdirectories) {
                result.AddRange(FindIdlFilesRecursively(subdirectory));
            }        	
            return (string[])result.ToArray(ReflectionHelper.StringType);
        }
        
        #endregion IMethods
        #region SMethods
        
        /// <summary>
        /// the command line howto for this command line.
        /// </summary>
        public static void HowTo(TextWriter target) {
            target.WriteLine("Compiler usage:");
            target.WriteLine("  IDLToCLSCompiler [options] target_assembly_name idl-files");
            target.WriteLine();
            target.WriteLine("creates a CLS assembly for the OMG IDL definition files.");
            target.WriteLine("target_assembly_name is the name of the target assembly without .dll");
            target.WriteLine("idl-files: one or more idl files containg OMG IDL definitions");
            target.WriteLine();
            target.WriteLine("options are:");
            target.WriteLine("-h or -help     help");
            target.WriteLine("-o directory    output directory (default is `-o .`)");
            target.WriteLine("-out:directory  the same as -o directory, but similar to the syntax of other .NET tools");
            target.WriteLine("-r assembly     assemblies to check for types in, instead of generating them");
            target.WriteLine("-r:assembly     the same as -r assembly, but similar to the syntax of other .NET tools");
            target.WriteLine("-lib:directory  additional directories to search for assemblies specified with -r (multiple -lib allowed)");
            target.WriteLine("-c xmlfile      specifies custom mappings");
            target.WriteLine("-d define       defines a preprocessor symbol");
            target.WriteLine("-b baseIF       the created Interfaces inherit from baseIF.");
            target.WriteLine("-basedir directory directory to change to before doing any processing.");
            target.WriteLine("-idir directory directory containing idl files (multiple -idir allowed)");
            target.WriteLine("-vtSkel         enable creation of value type implementation skeletons");
            target.WriteLine("-vtSkelProv     The fully qualified name of the codedomprovider to use for value type skeleton generation");
            target.WriteLine("-vtSkelTd       The targetDirectory for generated valuetype impl skeletons");
            target.WriteLine("-vtSkelO        Overwrite already present valuetype skeleton implementations");
            target.WriteLine("-snk            sign key file (used for generating strong named assemblies)");
            target.WriteLine("-delaySign      delay signing of assembly (snk file contains only a pk)");
            target.WriteLine("-asmVersion     the version of the generated assembly");
            target.WriteLine("-mapAnyToCont   maps idl any to the any container omg.org.CORBA.Any; if not specified, any is mapped to object");
            target.WriteLine("-fidl:listfile  a text file with the input idl files. the files can be seperated with whitespaces");
            target.WriteLine("-pidl:path      a path, where all (recursive) existinge idl files will be used for input.");
        }        
        
        #endregion SMethods
                                
    }
    
}


#if UnitTest


namespace Ch.Elca.Iiop.IdlCompiler.Tests {
    
    using System;
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.Text;
    using NUnit.Framework;
    using Ch.Elca.Iiop.IdlCompiler;

    /// <summary>
    /// Unit-tests for the IDLToCLS CommandLine handling.
    /// </summary>
    [TestFixture]
    public class IDLToCLSCommandLineTest {
                        
        
        [Test]
        public void TestDefaultOutputDir() {
            DirectoryInfo testDir = new DirectoryInfo(".");
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "testAsm", "test.idl" });
            Assertion.AssertEquals("OutputDirectory", testDir.FullName,
                                   commandLine.OutputDirectory.FullName);            
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestOutDirSpaceSeparator() {            
            DirectoryInfo testDir = new DirectoryInfo(Path.Combine(".", "testOut"));
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-o", testDir.FullName, "testAsm", "test.idl" });
            Assertion.AssertEquals("OutputDirectory", testDir.FullName,
                                   commandLine.OutputDirectory.FullName);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestOutDirColonSeparator() {
            DirectoryInfo testDir = new DirectoryInfo(Path.Combine(".", "testOut"));
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-out:" + testDir.FullName, "testAsm", "test.idl" });
            Assertion.AssertEquals("OutputDirectory", testDir.FullName,
                                   commandLine.OutputDirectory.FullName);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestWrongArgument() {
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-InvalidArg"} );
            Assertion.Assert("Invalid Arg detection",
                             commandLine.IsInvalid);
            Assertion.AssertEquals("invalid arguments message",
                                   "Error: invalid option -InvalidArg",
                                   commandLine.ErrorMessage);
        }
        
        [Test]
        public void TestMissingTargetAssemblyName() {
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[0] );
            Assertion.Assert("Invalid commandLine detection",
                             commandLine.IsInvalid);
            Assertion.AssertEquals("invalid commandLine message",
                                   "Error: target assembly name missing",
                                   commandLine.ErrorMessage);
        }
        
        [Test]
        public void TestMissingIdlFileName() {
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "testAsm" } );
            Assertion.Assert("Invalid commandLine detection",
                             commandLine.IsInvalid);
            Assertion.AssertEquals("invalid commandLine message",
                                   "Error: idl-file(s) missing",
                                   commandLine.ErrorMessage);
        }
        
        [Test]
        public void TestIsHelpRequested() {
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-h"} );
            Assertion.Assert("Help requested",
                             commandLine.IsHelpRequested);            
            commandLine = new IDLToCLSCommandLine(
                new string[] { "-help"} );
            Assertion.Assert("Help requested",
                             commandLine.IsHelpRequested);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestTargetAssemblyName() {
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "testAsm", "test.idl" } );
            Assertion.AssertEquals("targetAssemblyName", "testAsm",
                                   commandLine.TargetAssemblyName);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        

        [Test]
        public void TestSingleIdlFile() {
            string file1 = "test1.idl";
            
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "testAsm", file1 } );
            Assertion.AssertEquals("idl files", 1,
                                   commandLine.InputFileNames.Count);
            Assertion.AssertEquals("idl file1", 
                                   file1,
                                   commandLine.InputFileNames[0]);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }        
        
        [Test]
        public void TestIdlFiles() {            
            string file1 = "test1.idl";
            string file2 = "test2.idl";
            string file3 = "test3.idl";
            
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "testAsm", file1, file2, file3 } );
            Assertion.AssertEquals("idl files", 3,
                                   commandLine.InputFileNames.Count);
            Assertion.AssertEquals("idl file1", 
                                   file1,
                                   commandLine.InputFileNames[0]);
            Assertion.AssertEquals("idl file2", 
                                   file2,
                                   commandLine.InputFileNames[1]);
            Assertion.AssertEquals("idl file3", 
                                   file3,
                                   commandLine.InputFileNames[2]);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }           
        
        [Test]
        public void TestCustomMappingFiles() {
            string customMappingFile1 = "customMapping1.xml";
            string customMappingFile2 = "customMapping2.xml";
            
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-c", customMappingFile1, "-c", customMappingFile2,
                               "testAsm", "test.idl" });
            Assertion.AssertEquals("CustomMappingFiles", 2,
                                   commandLine.CustomMappingFiles.Count);
            Assertion.AssertEquals("CustomMappingFile 1", customMappingFile1,
                                   ((FileInfo)commandLine.CustomMappingFiles[0]).Name);
            Assertion.AssertEquals("CustomMappingFile 2", customMappingFile2,
                                   ((FileInfo)commandLine.CustomMappingFiles[1]).Name);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestCustomMappingFilesMultipleTheSame() {
            string customMappingFile1 = "customMapping1.xml";
            string customMappingFile2 = "customMapping1.xml";
            
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-c", customMappingFile1, "-c", customMappingFile2,
                               "testAsm", "test.idl" });
            Assertion.Assert("Invalid commandLine detection",
                             commandLine.IsInvalid);
            Assertion.Assert("invalid commandLine message",
                             commandLine.ErrorMessage.StartsWith(
                                "tried to add a custom mapping file multiple times: "));
        }
        
        [Test]
        public void TestSnkFile() {
            string snkFile = "test.snk";
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-snk", snkFile, "testAsm", "test.idl" });
            Assertion.AssertEquals("Key file", snkFile,
                                   commandLine.SignKeyFile.Name);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestDelaySign() {            
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-delaySign", "testAsm", "test.idl" });
            Assertion.Assert("DelaySign", commandLine.DelaySign);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }        

        [Test]
        public void TestAsmVersion() {
            string asmVersion = "1.0.0.0";
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-asmVersion", asmVersion, "testAsm", "test.idl" });
            Assertion.AssertEquals("Target Assembly Version", 
                                   asmVersion,
                                   commandLine.AssemblyVersion);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }        
        
        [Test]
        public void TestMapToAnyContainer() {
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-mapAnyToCont", "testAsm", "test.idl" });
            Assertion.Assert("Map any to any container", commandLine.MapAnyToAnyContainer);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestBaseDirectory() {            
            DirectoryInfo testDir = new DirectoryInfo(".");
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-basedir", testDir.FullName, "testAsm", "test.idl" });
            Assertion.AssertEquals("BaseDirectory", testDir.FullName,
                                   commandLine.BaseDirectory.FullName);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }        
        
        [Test]
        public void TestBaseDirectoryNonExisting() {            
            DirectoryInfo testDir = new DirectoryInfo(Path.Combine(".", "NonExistantBaseDir"));
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-basedir", testDir.FullName, "testAsm", "test.idl" });
            Assertion.Assert("Invalid Base directory",
                             commandLine.IsInvalid);
            Assertion.AssertEquals("invalid arguments message",
                                   String.Format(
                                       "Error: base directory {0} does not exist!", testDir.FullName),
                                   commandLine.ErrorMessage);
        }        
        
        [Test]
        public void TestInheritBaseInterface() {
            Type type = typeof(IDisposable);
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-b", type.FullName, "testAsm", "test.idl" });
            Assertion.AssertEquals("BaseInterface", type.FullName,
                                   commandLine.BaseInterface.FullName);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestBaseInterfaceNonExisting() {                        
            string baseInterfaceName = "System.IDisposableNonExisting";
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-b", baseInterfaceName, "testAsm", "test.idl" });
            Assertion.Assert("Invalid base interface",
                             commandLine.IsInvalid);
            Assertion.AssertEquals("invalid arguments message",
                                   String.Format(
                                       "Error: base interface {0} does not exist!", baseInterfaceName),
                                   commandLine.ErrorMessage);
        }
                
        [Test]
        public void TestVtSkel() {
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-vtSkel", "testAsm", "test.idl" });
            Assertion.Assert("Value Type Skeleton generation", 
                             commandLine.GenerateValueTypeSkeletons);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestVtSkelOverwrite() {
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-vtSkelO", "testAsm", "test.idl" });
            Assertion.Assert("Value Type Skeleton overwrite", 
                             commandLine.OverwriteValueTypeSkeletons);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestVtTargetDir() {
            DirectoryInfo testDir = new DirectoryInfo(Path.Combine(".", "testGenVtDir"));
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-vtSkelTd", testDir.FullName, "testAsm", "test.idl" });
            Assertion.AssertEquals("Valuetype Skeletons Target Directory", testDir.FullName,
                                   commandLine.ValueTypeSkeletonsTargetDir.FullName);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }                
        
        [Test]
        public void TestVtGenerationProvider() {
            Type provider = typeof(Microsoft.CSharp.CSharpCodeProvider);
            string providerName = provider.AssemblyQualifiedName;
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-vtSkelProv", providerName, "testAsm", "test.idl" });
            Assertion.Assert("Command Line Validity", !commandLine.IsInvalid);
            Assertion.AssertEquals("Valuetype Skeletons Generation Provider", provider,
                                   commandLine.ValueTypeSkeletonCodeDomProviderType);
        }                        
        
        [Test]
        public void TestVtGenerationProviderInvalid() {                        
            string providerName = "System.NonExistingProvider";
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-vtSkelProv", providerName, "testAsm", "test.idl" });
            Assertion.Assert("Invalid codedom provider",
                             commandLine.IsInvalid);
            Assertion.AssertEquals("invalid arguments message",
                                   String.Format(
                                       "provider {0} not found!", providerName),
                                   commandLine.ErrorMessage);
        }
        
        [Test]
        public void TestIdlSourceDirectories() {
            DirectoryInfo dir1 = new DirectoryInfo(".");
            DirectoryInfo dir2 = new DirectoryInfo(Path.Combine(".", "testIdlDir"));
                        
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-idir", dir1.FullName, "-idir", dir2.FullName, "testAsm", "test.idl" } );
            Assertion.AssertEquals("idl source dirs", 2,
                                   commandLine.IdlSourceDirectories.Count);
            Assertion.AssertEquals("idl source dir 1", 
                                   dir1.FullName,
                                   ((DirectoryInfo)commandLine.IdlSourceDirectories[0]).FullName);
            Assertion.AssertEquals("idl source dir 2", 
                                   dir2.FullName,
                                   ((DirectoryInfo)commandLine.IdlSourceDirectories[1]).FullName);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestRefAssembliesSpaceSeparator() {
            Assembly asm1 = this.GetType().Assembly;
            Assembly asm2 = typeof(TestAttribute).Assembly;
                        
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-r", asm1.CodeBase, "-r", asm2.CodeBase, "testAsm", "test.idl" } );
            Assertion.AssertEquals("referenced assemblies", 2,
                                   commandLine.ReferencedAssemblies.Count);
            Assertion.AssertEquals("ref assembly 1", 
                                   asm1.FullName,
                                   ((Assembly)commandLine.ReferencedAssemblies[0]).FullName);
            Assertion.AssertEquals("ref assembly 2", 
                                   asm2.FullName,
                                   ((Assembly)commandLine.ReferencedAssemblies[1]).FullName);            
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestRefAssembliesColonSeparator() {
            Assembly asm1 = this.GetType().Assembly;
            Assembly asm2 = typeof(TestAttribute).Assembly;
                        
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-r:" + asm1.CodeBase, "-r:" + asm2.CodeBase, "testAsm", "test.idl" } );
            Assertion.AssertEquals("referenced assemblies", 2,
                                   commandLine.ReferencedAssemblies.Count);
            Assertion.AssertEquals("ref assembly 1", 
                                   asm1.FullName,
                                   ((Assembly)commandLine.ReferencedAssemblies[0]).FullName);
            Assertion.AssertEquals("ref assembly 2", 
                                   asm2.FullName,
                                   ((Assembly)commandLine.ReferencedAssemblies[1]).FullName);            
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);
        }
        
        [Test]
        public void TestRefAssembliesInvalid() {                                    
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-r", "inexistientAssembly.dll", "testAsm", "test.idl" } );
                        
            Assertion.Assert("Command line validity", commandLine.IsInvalid);
            Assertion.Assert("invalid arguments message",
                             commandLine.ErrorMessage.StartsWith("can't load assembly: inexistientAssembly.dll"));
        }        
        
        [Test]
        public void TestPreprocessorDefines() {
            string def1 = "def1";
            string def2 = "def2";            
            
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-d", def1, "-d", def2, "testAsm", "test.idl" } );
            Assertion.AssertEquals("defines", 2,
                                   commandLine.PreprocessorDefines.Count);
            Assertion.AssertEquals("define 1", 
                                   def1,
                                   commandLine.PreprocessorDefines[0]);
            Assertion.AssertEquals("define 2", 
                                   def2,
                                   commandLine.PreprocessorDefines[1]);
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);            
        }
        
        [Test]
        public void TestLibDirs() {
            DirectoryInfo dir1 = new DirectoryInfo(Path.Combine(".", "lib1"));
            DirectoryInfo dir2 = new DirectoryInfo(Path.Combine(".", "lib2"));
            DirectoryInfo dir3 = new DirectoryInfo(Path.Combine(".", "lib3"));
            
            IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                new string[] { "-lib:" + dir1.FullName + ";" + dir2.FullName, 
                               "-lib:" + dir3.FullName, "testAsm", "test.idl" } );
            Assertion.AssertEquals("libs", 3,
                                   commandLine.LibDirectories.Count);
            Assertion.AssertEquals("lib dir 1", 
                                   dir1.FullName,
                                   ((DirectoryInfo)commandLine.LibDirectories[0]).FullName);
            Assertion.AssertEquals("lib dir 2", 
                                   dir2.FullName,
                                   ((DirectoryInfo)commandLine.LibDirectories[1]).FullName);
            Assertion.AssertEquals("lib dir 3", 
                                   dir3.FullName,
                                   ((DirectoryInfo)commandLine.LibDirectories[2]).FullName);            
            
            Assertion.Assert("Command line validity", !commandLine.IsInvalid);            
        }
        
        [Test]
        public void TestIdlFilesReadFromFile() {
            string fileWithIdlFiles = Path.GetTempFileName();
            try {
                string file1 = "test1.idl";
                string file2 = "test2.idl";
                using (StreamWriter sw = new StreamWriter(fileWithIdlFiles)) {
                    sw.WriteLine(file1);
                    sw.WriteLine(file2);
                }
                
                IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                    new string[] { "-fidl:" + fileWithIdlFiles, "testAsm" } );
                Assertion.AssertEquals("idl files", 2,
                                   commandLine.InputFileNames.Count);
                Assertion.AssertEquals("idl file1", 
                                       file1,
                                       commandLine.InputFileNames[0]);
                Assertion.AssertEquals("idl file2", 
                                       file2,
                                       commandLine.InputFileNames[1]);
            
                Assertion.Assert("Command line validity", !commandLine.IsInvalid);
                
            } finally {
                if (File.Exists(fileWithIdlFiles)) {
                    File.Delete(fileWithIdlFiles);
                }
            }
        }
        
        [Test]
        public void TestIdlFilesRecursivelyFromDirectory() {
            string tempPath = Path.Combine(Path.GetTempPath(), "IDLCommandLineRecursiveFileTest");
            if (Directory.Exists(tempPath)) {
                Directory.Delete(tempPath, true);
            }        	
            string subDir1 = Path.Combine(tempPath, "subDir1");
            string subDir2 = Path.Combine(tempPath, "subDir2");
            
            try {        		
                Directory.CreateDirectory(tempPath);
                Directory.CreateDirectory(subDir1);
                Directory.CreateDirectory(subDir2);
            
                string file1 = Path.Combine(tempPath, "test1.idl");
                string file2 = Path.Combine(tempPath, "test2.idl");
                string file3 = Path.Combine(subDir1, "test3.idl");
                string file4 = Path.Combine(subDir2, "test4.idl");
                
                
                using (FileStream fs = File.Create(file1)) {
                    fs.Close();
                }
                using (FileStream fs = File.Create(file2)) {
                    fs.Close();
                }
                using (FileStream fs = File.Create(file3)) {
                    fs.Close();
                }
                using (FileStream fs = File.Create(file4)) {
                    fs.Close();
                }
                
                IDLToCLSCommandLine commandLine = new IDLToCLSCommandLine(
                    new string[] { "-pidl:" + tempPath, "testAsm" } );
                Assertion.AssertEquals("idl files", 4,
                                   commandLine.InputFileNames.Count);	        	
                Assertion.AssertEquals("idl file1", 
                                       file1,
                                       commandLine.InputFileNames[0]);
                Assertion.AssertEquals("idl file2", 
                                       file2,
                                       commandLine.InputFileNames[1]);
                Assertion.AssertEquals("idl file3", 
                                       file3,
                                       commandLine.InputFileNames[2]);
                Assertion.AssertEquals("idl file4", 
                                       file4,
                                       commandLine.InputFileNames[3]);
            
                Assertion.Assert("Command line validity", !commandLine.IsInvalid);
                
            } finally {
                if (Directory.Exists(tempPath)) {
                    Directory.Delete(tempPath, true);
                }
            }
        }        
        
    }
}

#endif
 

