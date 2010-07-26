/* CompilerTestsBase.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 01.08.06  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

#if UnitTest

namespace Ch.Elca.Iiop.IdlCompiler.Tests
{

    using System;
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.Text;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Idl;
    using parser;
    using Ch.Elca.Iiop.IdlCompiler.Action;
    using Ch.Elca.Iiop.IdlCompiler.Exceptions;
    using Ch.Elca.Iiop.Util;

    /// <summary>
    /// Base class for unit tests
    /// </summary>    
    public abstract class CompilerTestsBase
    {

        #region SFields

        private static Encoding s_latin1 = Encoding.GetEncoding("ISO-8859-1");

        #endregion
        #region IMethods


        /// <summary>
        /// Get an assembly name for a string name.
        /// </summary>
        protected AssemblyName GetAssemblyName(string name)
        {
            AssemblyName result = new AssemblyName();
            result.Name = name;
            return result;
        }

        /// <summary>
        /// Parses the idl and generates an assembly.
        /// </summary>
        protected Assembly CreateIdl(Stream source, AssemblyName targetName)
        {
            return CreateIdl(source, targetName, false, false);
        }

        /// <summary>
        /// Parses the idl and generates an assembly.
        /// </summary>
        protected Assembly CreateIdl(Stream source, AssemblyName targetName,
                                  bool anyToAnyContainerMapping,
                                  bool makeInterfaceDisposable)
        {
            return CreateIdl(source, targetName, anyToAnyContainerMapping,
                             makeInterfaceDisposable, new ArrayList());
        }

        protected Assembly CreateIdl(Stream source, AssemblyName targetName,
                                     bool anyToAnyContainerMapping, bool makeInterfaceDisposable,
                                     ArrayList refAssemblies)
        {
            IDLParser parser = new IDLParser(source);
            ASTspecification spec = parser.specification();
            // now parsed representation can be visited with the visitors
            MetaDataGenerator generator = new MetaDataGenerator(targetName, ".",
                                                                refAssemblies);
            generator.MapAnyToAnyContainer = anyToAnyContainerMapping;
            if (makeInterfaceDisposable)
            {
                generator.InheritedInterface = typeof(System.IDisposable);
            }
            generator.InitalizeForSource(parser.getSymbolTable());
            spec.jjtAccept(generator, null);
            Assembly result = generator.ResultAssembly;
            return result;
        }


        /// <summary>
        /// Creates a stream writer for a stream
        /// to place the source into.
        /// </summary>
        protected StreamWriter CreateSourceWriter(Stream stream)
        {
            StreamWriter writer = new StreamWriter(stream, s_latin1);
            return writer;
        }

        /// <summary>
        /// Check, that the repository id attribute is present with
        /// the given id.
        /// </summary>
        protected void CheckRepId(Type testType, string expected)
        {
            object[] repAttrs =
                testType.GetCustomAttributes(ReflectionHelper.RepositoryIDAttributeType,
                                             false);
            Assert.AreEqual(1, repAttrs.Length, "wrong number of RepIDAttrs");
            RepositoryIDAttribute repId = (RepositoryIDAttribute)repAttrs[0];
            Assert.AreEqual(expected, repId.Id, "wrong repId");
        }

        /// <summary>
        /// Check, that the correct interface type attribute has been placed.
        /// </summary>
        protected void CheckInterfaceAttr(Type testType, IdlTypeInterface expected)
        {
            object[] ifAttrs = testType.GetCustomAttributes(typeof(InterfaceTypeAttribute),
                                                            false);
            Assert.AreEqual(1, ifAttrs.Length, "wrong number of InterfaceTypeAttribute");
            InterfaceTypeAttribute ifAttr = (InterfaceTypeAttribute)ifAttrs[0];
            Assert.AreEqual(expected,
                                   ifAttr.IdlType, "wrong ifattr");
        }

        /// <summary>
        /// Check, that the impl class attribute has been placed specifying the correct class.
        /// </summary>
        protected void CheckImplClassAttr(Type toCheck, string implClassName)
        {
            object[] attrs = toCheck.GetCustomAttributes(ReflectionHelper.ImplClassAttributeType,
                                                         false);
            Assert.AreEqual(1, attrs.Length, "wrong number of ImplClassAttribute");
            ImplClassAttribute attr = (ImplClassAttribute)attrs[0];
            Assert.AreEqual(implClassName, attr.ImplClass, "wrong implclass attr");
        }

        /// <summary>
        /// Check, that an idl enum attribute has been placed.
        /// </summary>        
        protected void CheckIdlEnumAttributePresent(Type enumType)
        {
            object[] attrs = enumType.GetCustomAttributes(ReflectionHelper.IdlEnumAttributeType,
                                                          false);
            Assert.AreEqual(1, attrs.Length, "wrong number of IdlEnumAttribute");
        }

        /// <summary>
        /// Check, that the idl struct attribute has been placed.
        /// </summary>
        protected void CheckIdlStructAttributePresent(Type structType)
        {
            object[] attrs = structType.GetCustomAttributes(ReflectionHelper.IdlStructAttributeType,
                                                            false);
            Assert.AreEqual(1, attrs.Length, "wrong number of IdlStructAttribute");
        }

        /// <summary>
        /// Check, that the idl union attribute has been placed.
        /// </summary>
        protected void CheckIdlUnionAttributePresent(Type unionType)
        {
            object[] attrs = unionType.GetCustomAttributes(ReflectionHelper.IdlUnionAttributeType,
                                                           false);
            Assert.AreEqual(1, attrs.Length, "wrong number of IdlUnionAttribute");
        }

        /// <summary>
        /// Check, that the class is serializable.
        /// </summary>
        protected void CheckSerializableAttributePresent(Type toCheck)
        {
            Assert.AreEqual(true, toCheck.IsSerializable, "not serializable");
        }

        /// <summary>
        /// Check, that the ExplicitSerializationOrdered attribute has been placed.
        /// </summary>
        protected void CheckExplicitSerializationOrderedAttributePresent(Type type)
        {
            object[] attrs = type.GetCustomAttributes(ReflectionHelper.ExplicitSerializationOrderedType,
                                                      false);
            Assert.NotNull(attrs, "attrs null?");
            Assert.AreEqual(1, attrs.Length, "wrong number of ExplicitSerializationOrderedAttribute");
        }

        /// <summary>
        /// Check, that a given public method is present.
        /// </summary>
        protected void CheckPublicInstanceMethodPresent(Type testType, string methodName,
                                                      Type returnType, Type[] paramTypes)
        {
            CheckMethodPresent(testType, methodName, returnType, paramTypes,
                               BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        }

        /// <summary>
        /// Check, that a given method is present.
        /// </summary>
        protected void CheckMethodPresent(Type testType, string methodName,
                                        Type returnType, Type[] paramTypes, BindingFlags attrs)
        {
            MethodInfo testMethod = testType.GetMethod(methodName,
                                                       attrs,
                                                       null, paramTypes, null);
            Assert.NotNull(testMethod, String.Format("method {0} not found", methodName));

            Assert.AreEqual(returnType, testMethod.ReturnType, String.Format("wrong return type {0} in method {1}", testMethod.ReturnType, methodName));
        }

        /// <summary>
        /// Check, that a given property is present.
        /// </summary>
        protected void CheckPropertyPresent(Type testType, string propName,
                                        Type propType, BindingFlags attrs)
        {
            PropertyInfo testProp = testType.GetProperty(propName, attrs,
                                                       null, propType, Type.EmptyTypes,
                                                       null);
            Assert.NotNull(testProp, String.Format("property {0} not found", propName));

            Assert.AreEqual(propType, testProp.PropertyType, String.Format("wrong type {0} in property {1}", testProp.PropertyType, propName));
        }

        /// <summary>
        /// Check, that a given field is present.
        /// </summary>
        protected void CheckFieldPresent(Type testType, string fieldName,
                                       Type fieldType, BindingFlags flags)
        {
            FieldInfo testField = testType.GetField(fieldName, flags);
            Assert.NotNull(testField, String.Format("field {0} not found in type {1}", fieldName, testType.FullName));
            Assert.AreEqual(fieldType, testField.FieldType, String.Format("wrong field type {0} in field {1}",
                                                 testField.FieldType, testField.Name));
        }

        /// <summary>
        /// Check, that the given number of fields are present.
        /// </summary>
        protected void CheckNumberOfFields(Type testType, BindingFlags flags,
                                         System.Int32 expected)
        {
            FieldInfo[] fields = testType.GetFields(flags);
            Assert.AreEqual(expected, fields.Length, "wrong number of fields found in type: " + testType.FullName);
        }

        /// <summary>
        /// Check, that exactly one custom attribute is present in the given array.
        /// </summary>
        protected void CheckOnlySpecificCustomAttrInCollection(object[] testAttrs,
                                                             Type attrType)
        {
            Assert.AreEqual(1, testAttrs.Length, "wrong nr of custom attrs found");
            Assert.AreEqual(attrType,
                                   testAttrs[0].GetType(), "wrong custom attr found");
        }

        /// <summary>
        /// Check, that the type implements IIdlEntity.
        /// </summary>
        protected void CheckIIdlEntityInheritance(Type testType)
        {
            Type idlEntityIf = testType.GetInterface("IIdlEntity");
            Assert.NotNull(idlEntityIf, String.Format("type {0} doesn't inherit from IIdlEntity", testType.FullName));
        }

        /// <summary>
        /// Check an enum field.
        /// </summary>
        protected void CheckEnumField(FieldInfo field, string idlEnumValName)
        {
            Type enumType = field.DeclaringType;
            Assert.AreEqual(enumType, field.FieldType, "wrong enum val field type");
            Assert.AreEqual(idlEnumValName, field.Name, "wrong enum val field name");
        }

        #endregion

    }

}

#endif
