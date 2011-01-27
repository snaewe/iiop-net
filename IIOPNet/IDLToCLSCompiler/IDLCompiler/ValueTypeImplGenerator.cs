/* MetadataGenerator.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 11.01.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2004 Dominic Ullmann
 *
 * Copyright 2004 ELCA Informatique SA
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
 using System.CodeDom;
 using System.CodeDom.Compiler;
 using System.Reflection;
 using Ch.Elca.Iiop.Idl;
 
 
 namespace Ch.Elca.Iiop.IdlCompiler.Action {
     
     
     /// <summary>
     /// Generates a skeleton implementation for Corba valuetypes.
     /// </summary>
     public class ValueTypeImplGenerator {
         
         
         #region IFields
         
         private CodeDomProvider m_codeDomProvider;
         private DirectoryInfo m_targetDir = null;
         
         private bool m_overwriteWhenExist = false;
         
         #endregion IFields
         #region IConstructors
         
         /// <summary>
         /// Default constructor
         /// </summary>
         /// <param name="codeGen">
         /// The codedom-provider for the target language
         /// </param>
         /// <param name="targetDir">
         /// The target directory for the generated source files.
         /// </param>
         public ValueTypeImplGenerator(CodeDomProvider provider,
                                       DirectoryInfo targetDir,
                                       bool overwriteWhenExist) {
             m_codeDomProvider = provider;
             m_targetDir = targetDir;
             m_overwriteWhenExist = overwriteWhenExist;
         }
         
         #endregion IConstructors         
         #region IMethods
         
         /// <summary>
         /// Generates a value type implementation for forValueType.
         /// </summary>
         /// <returns>true if generated, false if skipped</returns>
         public bool GenerateValueTypeImpl(Type forValueType) {
             // create the fileName from targetDirectory and 
             // full type name
             string fileName = forValueType.FullName;
             fileName = fileName.Replace(".", "_") + "." + m_codeDomProvider.FileExtension;
             fileName = Path.Combine(m_targetDir.FullName, fileName);
             
             if (File.Exists(fileName) && !m_overwriteWhenExist) {
                 Console.WriteLine("skip generation for " + 
                                   forValueType.FullName + 
                                   " because implementation file " +
                                   fileName + " already exists ");
                 return false;                                   
             }
             
             StreamWriter targetStream = new StreamWriter(fileName, false);
             IndentedTextWriter writer = new IndentedTextWriter(targetStream);
                          
             GenerateValueTypeImplToFile(forValueType, writer);
             
             writer.Flush();
             writer.Close();
             
             return true;
         }
         
         
         /// <summary>
         /// Generates a value type implementation for forValueType in
         /// targetWriter
         /// </summary>
         private void GenerateValueTypeImplToFile(Type forValueType,
                                                  TextWriter targetWriter) {
             CodeNamespace targetNamespace = 
                 new CodeNamespace(forValueType.Namespace);
             CodeTypeDeclaration valTypeImpl =
                 new CodeTypeDeclaration(forValueType.Name + "Impl");             
             targetNamespace.Types.Add(valTypeImpl);                                                      
             valTypeImpl.TypeAttributes = TypeAttributes.Class |
                                          TypeAttributes.Public;
             
             valTypeImpl.BaseTypes.Add(forValueType.Name);
             valTypeImpl.CustomAttributes.Add(
                 new CodeAttributeDeclaration("Serializable"));
             AddNamespaceImport(targetNamespace, "System");
             
             AddMembers(forValueType, valTypeImpl, targetNamespace);

             m_codeDomProvider.GenerateCodeFromNamespace(targetNamespace,
                                                       targetWriter, 
                                                       new CodeGeneratorOptions());
         }
         
         private void AddMembers(Type forValueType, 
                                 CodeTypeDeclaration valTypeImpl,
                                 CodeNamespace targetNamespace) {

             ConstructorInfo[] valTypeConstrs =
                 forValueType.GetConstructors(BindingFlags.Public |
                                              BindingFlags.NonPublic | 
                                              BindingFlags.Instance |
                                              BindingFlags.DeclaredOnly);
             foreach (ConstructorInfo constr in valTypeConstrs) {
                 if (((constr.Attributes & MethodAttributes.Public) > 0) ||
                     ((constr.Attributes & MethodAttributes.Family) > 0)) {
                     AddConstructor(valTypeImpl, targetNamespace, constr);
                 }
             }
             
             
             Type investigatedType = forValueType;
             while (typeof(IIdlEntity).IsAssignableFrom(investigatedType)) {
                 AddDeclaredMethods(investigatedType, valTypeImpl, targetNamespace);
                 AddDeclaredProperties(investigatedType, valTypeImpl, targetNamespace);
                 investigatedType = investigatedType.BaseType;
             }
                                 
         }
         
         /// <summary>adds skleton implementation for all the methods directly declared by the given type</summary>
         private void AddDeclaredMethods(Type forValueType,
                                         CodeTypeDeclaration valTypeImpl,
                                         CodeNamespace targetNamespace) {

             MethodInfo[] valTypeMethods =
                 forValueType.GetMethods(BindingFlags.Public |
                                         BindingFlags.Instance |
                                         BindingFlags.DeclaredOnly);
             foreach (MethodInfo method in valTypeMethods) {
                 // do not add property accessor methods here
                 if (!method.IsSpecialName) {
                     AddMethod(valTypeImpl, targetNamespace, method);
                 }
             }                                                          
                                             
         }
         
         /// <summary>adds skleton implementation for all the properties directly declared by the given type</summary>
         private void AddDeclaredProperties(Type forValueType,
                                            CodeTypeDeclaration valTypeImpl,
                                            CodeNamespace targetNamespace) {
             PropertyInfo[] valTypeProperties =
                 forValueType.GetProperties(BindingFlags.Public |
                                            BindingFlags.Instance |
                                            BindingFlags.DeclaredOnly);
             foreach (PropertyInfo prop in valTypeProperties) {
                 AddProperty(valTypeImpl, targetNamespace, prop);
             }
         }                                                              
         
         private void AddConstructor(CodeTypeDeclaration valTypeImpl,
                                     CodeNamespace valTypeNamespace,
                                     ConstructorInfo forConstructor) {             
             CodeConstructor constructor = new CodeConstructor();
             constructor.Attributes = MemberAttributes.Public;
             
             foreach (ParameterInfo param in forConstructor.GetParameters()) {
                 constructor.Parameters.Add(
                     new CodeParameterDeclarationExpression(param.ParameterType,
                                                            param.Name));
                 constructor.BaseConstructorArgs.Add(
                     new CodeVariableReferenceExpression(param.Name));
                 
                 AddNamespaceImport(valTypeNamespace,
                                    param.ParameterType.Namespace);
             }
                                    
             valTypeImpl.Members.Add(constructor);

         }
         
         private void AddMethod(CodeTypeDeclaration valTypeImpl,
                                CodeNamespace valTypeNamespace,
                                MethodInfo forMethod) {
             CodeMemberMethod method = new CodeMemberMethod();
             method.Name = forMethod.Name;
             method.Attributes = MemberAttributes.Public | 
                                 MemberAttributes.Override;
             method.ReturnType = new CodeTypeReference(forMethod.ReturnType);
             AddNamespaceImport(valTypeNamespace,
                                forMethod.ReturnType.Namespace);
             
             foreach (ParameterInfo paramInfo in forMethod.GetParameters()) {
                 method.Parameters.Add( 
                     new CodeParameterDeclarationExpression(paramInfo.ParameterType,
                                                            paramInfo.Name));
                 AddNamespaceImport(valTypeNamespace, 
                                    paramInfo.ParameterType.Namespace);
             }             

             method.Statements.Add(CreateNotImplementedStatement());             
             
             valTypeImpl.Members.Add(method);
             
         }
         
         private void AddProperty(CodeTypeDeclaration valTypeImpl,
                                  CodeNamespace valTypeNamespace,
                                  PropertyInfo forProperty) {                          

             CodeMemberProperty prop = new CodeMemberProperty();
             prop.Name = forProperty.Name;
             prop.Type = new CodeTypeReference(forProperty.PropertyType);
             prop.Attributes = MemberAttributes.Public | MemberAttributes.Override;
             prop.GetStatements.Add(CreateNotImplementedStatement());
             if (forProperty.CanWrite) {
                 prop.SetStatements.Add(CreateNotImplementedStatement());
             }                                       
                                                                
             AddNamespaceImport(valTypeNamespace,
                                forProperty.PropertyType.Namespace);
                                      
             valTypeImpl.Members.Add(prop);             
         }
         
         private CodeThrowExceptionStatement CreateNotImplementedStatement() {
             CodeThrowExceptionStatement result = 
                 new CodeThrowExceptionStatement(
                     new CodeObjectCreateExpression(
                        new CodeTypeReference(typeof(System.NotImplementedException)),    
                        new CodeExpression[] {} ) );
             return result;
         }
         
         private void AddNamespaceImport(CodeNamespace valTypeNamespace, 
                                         string import) {
             if (valTypeNamespace.Name.Equals(import)) {
                 return;
             }
             
             foreach (CodeNamespaceImport alreadyImported in
                      valTypeNamespace.Imports) {
                 if (alreadyImported.Namespace.Equals(import)) {
                     return;
                 }
             }
             valTypeNamespace.Imports.Add(
                     new CodeNamespaceImport(import));
         }
         
         #endregion IMethods
         
     }
     
 }
