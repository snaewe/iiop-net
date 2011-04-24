/* IDLGenerator.cs
 * 
 * Project: IIOP.NET
 * CLSToIDLGenerator
 * 
 * WHEN      RESPONSIBLE
 * 31.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.IO;
using System.Diagnostics;

using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.Idl {

    /// <summary>
    /// Generates IDL for the specified CLS class
    /// </summary>
    public class IdlGenerator {

        #region Types

        private class CustomAssemblyResolver {

            private string m_baseDir;

            public CustomAssemblyResolver(string baseDir) {
                m_baseDir = baseDir;
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
                return null;
            }

            private Assembly LoadByRelativePath(string asmFileName) {
                string candidate = Path.Combine(m_baseDir, asmFileName);
                try {
                    return Assembly.LoadFrom(candidate);
                } catch (Exception) {
                    return null;
                }                
            }
        }

        #endregion Types
        #region SFields

        /// <summary>
        /// destination directory for the generated IDL files. Set with "-o dir"
        /// </summary>
        public static String s_destination = ".";

        #endregion SFields
        #region SMethods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args) {
            try {
                Type typeToMap = ParseArgs(args);

                Console.WriteLine("emitting Predef.idl");
                EmitPredefIdl();
                Console.WriteLine("generating IDL for type: " + typeToMap.FullName);
                Console.WriteLine("destination: " + s_destination);
                ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
                mapper.MapClsType(typeToMap, 
                    AttributeExtCollection.EmptyCollection, new GenerationActionDefineTypes(s_destination));
            } catch (Exception e) {
                Console.WriteLine("error while running generator: " + e);
            }
        }

        public static void HowTo() {
            Console.WriteLine("Compiler usage:");
            Console.WriteLine("  CLSIDLGenerator [options] typename assembly");
            Console.WriteLine();
            Console.WriteLine("creates the OMG IDL definition file for a CLS type.");
            Console.WriteLine("typename is the full type name, including namespaces");
            Console.WriteLine("assembly is the assembly file (dll or exe) with the type definition");
            Console.WriteLine();
            Console.WriteLine("options are:");
            Console.WriteLine("-h              help");
            Console.WriteLine("-o directory    output directory (default is `-o .`)");
            Console.WriteLine("-c xmlfile      specifies custom mappings");
        }

        public static void Error(String message) {
            Console.WriteLine(message);
            Console.WriteLine();
            HowTo();
            Environment.Exit(1);                        
        }

        /// <summary>returns the type to be mapped or terminate</summary>
        private static Type ParseArgs(string[] args) {
            int i = 0;

            System.IO.FileInfo configFile = null;
            while ((i < args.Length) && (args[i][0] == '-')) {
                switch (args[i]) {
                    case "-h":
                        HowTo();
                        i++;
                        break;
                    case "-o":
                        i++;
                        s_destination = args[i++];
                        break;
                    case "-c":
                        i++;
                        configFile = new System.IO.FileInfo(args[i++]);
                        break;
                    default:
                        Error(String.Format("Error: invalid option {0}", args[i]));
                        break;
                }
            }
            if (!Directory.Exists(s_destination)) {
                Directory.CreateDirectory(s_destination);
            }

            if (i < args.Length-2) {
                Error("Error: typename or assembly missing");
            } else if (i > args.Length + 2) {
                Error("Error: too many parameters");
            }

            Assembly assembly = null;
            Type     type     = null;

            try {
                assembly = Assembly.LoadFrom(args[i+1]);
            } catch (Exception e) {
                Error(String.Format("Error while loading assembly: {0}", e.Message));
            }

            // append private assembly search path: directory of assembly containing type to map -> find assemblies it depends on
            SetupAppDomain(assembly);

            try {
                type = assembly.GetType(args[i], true);
            } catch (Exception e) {
                Error(String.Format("Error while loading type: {0}", e.Message));
            }

            if (configFile != null) {
                // add custom mappings from file
                GeneratorMappingPlugin plugin = GeneratorMappingPlugin.GetSingleton();
                plugin.AddMappingsFromFile(configFile);
            }

            return type;
        }

        /// <summary>
        /// set up the AppDomain for the type to map in the containing assembly assemblyToMap
        /// </summary>
        /// <param name="assemblyToMap">the assembly containing the type to map</param>
        private static void SetupAppDomain(Assembly assemblyToMap) {
            AppDomain curDomain = AppDomain.CurrentDomain;

            curDomain.AppendPrivatePath(curDomain.BaseDirectory);
            // directory of the assembly containing the typ to map
            // make sure to probe in directory containing type to map, if not found itself ...
            string directoryName = new FileInfo(assemblyToMap.Location).DirectoryName;
            CustomAssemblyResolver resolver = new CustomAssemblyResolver(directoryName);
            ResolveEventHandler hndlr = new ResolveEventHandler(resolver.AssemblyResolve);
            curDomain.AssemblyResolve += hndlr;
        }

        private static void EmitPredefIdl() {
            string predefFile = Path.Combine(s_destination, "Predef.idl");

            TextWriter writer = new StreamWriter(predefFile);
            writer.WriteLine("// auto-generated IDL file by CLS to IDL mapper");
            writer.WriteLine("");
            writer.WriteLine("// " + predefFile);
            writer.WriteLine("");
            
            
            writer.WriteLine("#include \"orb.idl\"");

            writer.Flush();
            writer.Close();
        }

        #endregion SMethods

    }
}
