/* TypeFromTypeCodeGenerator.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 11.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Reflection.Emit;
using System.Globalization;
using omg.org.CORBA;


namespace Ch.Elca.Iiop.Idl {

    /// <summary>
    /// generates type from a type-code, if the type represented by the type-code is not known!
    /// </summary>
    internal class TypeFromTypeCodeRuntimeGenerator {
        
        #region SFields

        private static TypeFromTypeCodeRuntimeGenerator s_runtimeGen = new TypeFromTypeCodeRuntimeGenerator();

        #endregion SFields
        #region IFields
            
        private AssemblyBuilder m_asmBuilder;
        private ModuleBuilder m_modBuilder;

        #endregion IFields
        #region IConstructors
        
        private TypeFromTypeCodeRuntimeGenerator() {
            Initalize();
        }

        #endregion IConstructors
        #region SMethods
        
        public static TypeFromTypeCodeRuntimeGenerator GetSingleton() {
            return s_runtimeGen;
        }

        #endregion SMethods
        #region IMethods

        private void Initalize() {
            AssemblyName asmname = new AssemblyName();
            asmname.Name = "dynTypeCode";
            asmname.Version = new Version(0, 0, 0, 0);
            asmname.CultureInfo = CultureInfo.InvariantCulture;            
            asmname.SetPublicKeyToken(new byte[0]);
            m_asmBuilder = System.Threading.Thread.GetDomain().
                DefineDynamicAssembly(asmname, AssemblyBuilderAccess.Run);
            m_modBuilder = m_asmBuilder.DefineDynamicModule("typecodeTypes");            
        }

        /// <summar>
        /// check if the type with the name fullname is defined among the generated types. 
        /// If so, return the type
        /// </summary>
        internal Type RetrieveType(string fullname) {
            return m_modBuilder.GetType(fullname);
        }

        /// <summary>
        /// create the type with name fullname, described by forTypeCode!
        /// </summary>
        /// <param name="fullname"></param>
        /// <param name="forTypeCode"></param>
        /// <returns></returns>
        internal Type CreateOrGetType(string fullname, TypeCodeImpl forTypeCode) {
            lock(this) {
                Type result = RetrieveType(fullname);
                if (result == null) {
                    result = forTypeCode.CreateType(m_modBuilder, fullname);
                    Repository.RegisterDynamicallyCreatedType(result);
                }
                return result;
            }
        }

        #endregion IMethods

    }

}


#if UnitTest

namespace Ch.Elca.Iiop.Tests
{
    using System;
    using System.Reflection;
    using NUnit.Framework;
    using omg.org.CORBA;
    using Ch.Elca.Iiop.Idl;

    /// <summary>
    /// Unit-tests for testing type from typecode code generation for value types.
    /// </summary>
    [TestFixture]
    public class TypeFromTypeCodeGeneratorValueTypeTest
    {
        private TypeFromTypeCodeRuntimeGenerator m_gen;

        [SetUp]
        public void SetUp()
        {
            m_gen = TypeFromTypeCodeRuntimeGenerator.GetSingleton();
        }

        [Test]
        public void TestGenerate()
        {
            string name = "TestGenForTypeCodeType";
            string typeName = "Ch.Elca.Iiop.Tests." + name;
            string repId = "IDL:Ch/Elca/Iiop/Tests/TestGenForTypeCodeType:1.0";
            ValueMember m1 = new ValueMember("M1", new LongTC(), 0);
            ValueTypeTC vt = new ValueTypeTC(repId,
                                             name, new ValueMember[] { m1 },
                                             new NullTC(), 0);

            Type res = m_gen.CreateOrGetType(typeName, vt);
            Assert.NotNull(res);
            Assert.AreEqual(typeName, res.FullName, "type name");
            Assert.AreEqual(repId, Repository.GetRepositoryID(res), "rep id");
            Assert.NotNull(res.GetField(m1.name,
                                                 BindingFlags.Public | BindingFlags.Instance), "field M1");
            Assert.IsTrue(res.IsSerializable, "Serializable");
        }

        [Test]
        public void TestGenerateSpecialNameRepId()
        {
            string name = "TestGenForTypeCodeType3";
            string typeName = "Ch.Elca.Iiop.Tests." + name;
            string repId = "IDL:Ch/Elca/Iiop/Tests/Special_TestGenForTypeCodeType3:1.0";
            ValueTypeTC vt = new ValueTypeTC(repId,
                                             name, new ValueMember[0],
                                             new NullTC(), 0);

            Type res = m_gen.CreateOrGetType(typeName, vt);
            Assert.NotNull(res);
            Assert.AreEqual(typeName, res.FullName, "type name");
            Assert.AreEqual(repId, Repository.GetRepositoryID(res), "rep id");
        }

    }

    /// <summary>
    /// Unit-tests for testing type from typecode code generation for boxed value types.
    /// </summary>
    [TestFixture]
    public class TypeFromTypeCodeGeneratorBoxedValueTypeTest
    {
        private TypeFromTypeCodeRuntimeGenerator m_gen;

        [SetUp]
        public void SetUp()
        {
            m_gen = TypeFromTypeCodeRuntimeGenerator.GetSingleton();
        }

        [Test]
        public void TestGenerate()
        {
            string name = "TestBoxedGenForTypeCodeType";
            string typeName = "Ch.Elca.Iiop.Tests." + name;
            string repId = "IDL:Ch/Elca/Iiop/Tests/TestBoxedGenForTypeCodeType:1.0";
            LongTC boxedTC = new LongTC();
            ValueBoxTC vt = new ValueBoxTC(repId,
                                           name,
                                           boxedTC);

            Type res = m_gen.CreateOrGetType(typeName, vt);
            Assert.NotNull(res);
            Assert.AreEqual(typeName, res.FullName, "type name");
            Assert.AreEqual(repId, Repository.GetRepositoryID(res), "rep id");

            Assert.IsTrue(res.IsSerializable, "Serializable");
        }

        [Test]
        public void TestGenerateSpecialNameRepId()
        {
            string name = "TestBoxedGenForTypeCodeType3";
            string typeName = "Ch.Elca.Iiop.Tests." + name;
            string repId = "IDL:Ch/Elca/Iiop/Tests/Special_TestBoxedGenForTypeCodeType3:1.0";
            LongTC boxedTC = new LongTC();
            ValueBoxTC vt = new ValueBoxTC(repId,
                                           name,
                                           boxedTC);

            Type res = m_gen.CreateOrGetType(typeName, vt);
            Assert.NotNull(res);
            Assert.AreEqual(typeName, res.FullName, "type name");
            Assert.AreEqual(repId, Repository.GetRepositoryID(res), "rep id");
        }

    }

    /// <summary>
    /// Unit-tests for testing type from typecode code generation for boxed value types.
    /// </summary>
    [TestFixture]
    public class TypeFromTypeCodeGeneratorEnumTypeTest
    {
        private TypeFromTypeCodeRuntimeGenerator m_gen;

        [SetUp]
        public void SetUp()
        {
            m_gen = TypeFromTypeCodeRuntimeGenerator.GetSingleton();
        }

        [Test]
        public void TestGenerate()
        {
            string name = "TestEnumGenForTypeCodeType";
            string typeName = "Ch.Elca.Iiop.Tests." + name;
            string repId = "IDL:Ch/Elca/Iiop/Tests/TestEnumGenForTypeCodeType:1.0";
            string[] enumFields = new string[] { name + "_1", name + "_2" };
            EnumTC tc = new EnumTC(repId,
                                   name,
                                   enumFields);

            Type res = m_gen.CreateOrGetType(typeName, tc);
            Assert.NotNull(res);
            Assert.AreEqual(typeName, res.FullName, "type name");
            Assert.AreEqual(repId, Repository.GetRepositoryID(res), "rep id");
            string[] genEnumNames = Enum.GetNames(res);
            Assert.AreEqual(enumFields.Length, genEnumNames.Length, "nr of enum entries");
            Assert.AreEqual(enumFields[0], genEnumNames[0], "enum entry 1");
            Assert.AreEqual(enumFields[1], genEnumNames[1], "enum entry 2");
            Assert.IsTrue(res.IsSerializable, "Serializable");
        }

        [Test]
        public void TestGenerateSpecialNameRepId()
        {
            string name = "TestEnumGenForTypeCodeType3";
            string typeName = "Ch.Elca.Iiop.Tests." + name;
            string repId = "IDL:Ch/Elca/Iiop/Tests/Special_TestEnumGenForTypeCodeType3:1.0";
            EnumTC tc = new EnumTC(repId,
                                   name,
                                   new string[] { name + "_1", name + "_2" });

            Type res = m_gen.CreateOrGetType(typeName, tc);
            Assert.NotNull(res);
            Assert.AreEqual(typeName, res.FullName, "type name");
            Assert.AreEqual(repId, Repository.GetRepositoryID(res), "rep id");
        }

    }


    /// <summary>
    /// Unit-tests for testing type from typecode code generation for struct types.
    /// </summary>
    [TestFixture]
    public class TypeFromTypeCodeGeneratorStructTypeTest
    {
        private TypeFromTypeCodeRuntimeGenerator m_gen;

        [SetUp]
        public void SetUp()
        {
            m_gen = TypeFromTypeCodeRuntimeGenerator.GetSingleton();
        }

        [Test]
        public void TestGenerate()
        {
            string name = "TestStructGenForTypeCodeType";
            string typeName = "Ch.Elca.Iiop.Tests." + name;
            string repId = "IDL:Ch/Elca/Iiop/Tests/TestStructGenForTypeCodeType:1.0";
            StructMember m1 = new StructMember("M1", new LongTC());
            StructTC tc = new StructTC(repId,
                                       name, new StructMember[] {
                                       m1 });
            Type res = m_gen.CreateOrGetType(typeName, tc);
            Assert.NotNull(res);
            Assert.AreEqual(typeName, res.FullName, "type name");
            Assert.AreEqual(repId, Repository.GetRepositoryID(res), "rep id");
            Assert.NotNull(res.GetField(m1.name,
                                                 BindingFlags.Public | BindingFlags.Instance), "field M1");
            Assert.IsTrue(res.IsSerializable, "Serializable");
        }

        [Test]
        public void TestGenerateSpecialNameRepId()
        {
            string name = "TestStructGenForTypeCodeType3";
            string typeName = "Ch.Elca.Iiop.Tests." + name;
            string repId = "IDL:Ch/Elca/Iiop/Tests/Special_TestStructGenForTypeCodeType3:1.0";
            StructTC tc = new StructTC(repId,
                                       name, new StructMember[] {
                                       new StructMember("M1", new LongTC()) });
            Type res = m_gen.CreateOrGetType(typeName, tc);
            Assert.NotNull(res);
            Assert.AreEqual(typeName, res.FullName, "type name");
            Assert.AreEqual(repId, Repository.GetRepositoryID(res), "rep id");
        }

    }


    /// <summary>
    /// Unit-tests for testing type from typecode code generation for union types.
    /// </summary>
    [TestFixture]
    public class TypeFromTypeCodeGeneratorUnionTypeTest
    {
        private TypeFromTypeCodeRuntimeGenerator m_gen;

        [SetUp]
        public void SetUp()
        {
            m_gen = TypeFromTypeCodeRuntimeGenerator.GetSingleton();
        }

        [Test]
        public void TestGenerate()
        {
            string name = "TestUnionGenForTypeCodeType";
            string typeName = "Ch.Elca.Iiop.Tests." + name;
            string repId = "IDL:Ch/Elca/Iiop/Tests/TestUnionGenForTypeCodeType:1.0";

            UnionSwitchCase s1 = new UnionSwitchCase((int)0, "val_0", new LongTC());
            UnionSwitchCase s2 = new UnionSwitchCase((int)1, "val_1", new FloatTC());
            TypeCodeImpl discrTC = new LongTC();
            UnionTC tc = new UnionTC(repId, name, discrTC, 0,
                                     new UnionSwitchCase[] { s1, s2 });
            Type res = m_gen.CreateOrGetType(typeName, tc);
            Assert.NotNull(res);
            Assert.AreEqual(typeName, res.FullName, "type name");
            Assert.AreEqual(repId, Repository.GetRepositoryID(res), "rep id");

            MethodInfo getFieldForDiscrMethod =
                res.GetMethod(UnionGenerationHelper.GET_FIELD_FOR_DISCR_METHOD, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(getFieldForDiscrMethod, "get field for Discr method");
            FieldInfo fieldForDiscr1 = (FieldInfo)
                getFieldForDiscrMethod.Invoke(null, new object[] { s1.DiscriminatorValue });
            FieldInfo fieldForDiscr2 = (FieldInfo)
                getFieldForDiscrMethod.Invoke(null, new object[] { s2.DiscriminatorValue });
            Assert.NotNull(fieldForDiscr1, "fieldForDiscr1");
            Assert.NotNull(fieldForDiscr2, "fieldForDiscr2");
            Assert.AreEqual(((TypeCodeImpl)s1.ElementType).GetClsForTypeCode(),
                                   fieldForDiscr1.FieldType, "fieldForDiscr1 Type");
            Assert.AreEqual(
                                   ((TypeCodeImpl)s2.ElementType).GetClsForTypeCode(),
                                   fieldForDiscr2.FieldType, "fieldForDiscr2 Type");
            PropertyInfo discrProperty = res.GetProperty(UnionGenerationHelper.DISCR_PROPERTY_NAME,
                                                         BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(discrProperty, "discr property");
            Assert.AreEqual(discrTC.GetClsForTypeCode(),
                                   discrProperty.PropertyType, "discr property type");
            Assert.IsTrue(res.IsSerializable, "Serializable");
        }

        [Test]
        public void TestGenerateSpecialNameRepId()
        {
            string name = "TestUnionGenForTypeCodeType3";
            string typeName = "Ch.Elca.Iiop.Tests." + name;
            string repId = "IDL:Ch/Elca/Iiop/Tests/Special_TestUnionGenForTypeCodeType3:1.0";

            UnionSwitchCase s1 = new UnionSwitchCase((int)0, "val_0", new LongTC());
            UnionSwitchCase s2 = new UnionSwitchCase((int)1, "val_1", new FloatTC());
            TypeCodeImpl discrTC = new LongTC();
            UnionTC tc = new UnionTC(repId, name, discrTC, 0,
                                     new UnionSwitchCase[] { s1, s2 });
            Type res = m_gen.CreateOrGetType(typeName, tc);
            Assert.NotNull(res);
            Assert.AreEqual(typeName, res.FullName, "type name");
            Assert.AreEqual(repId, Repository.GetRepositoryID(res), "rep id");
        }

    }

}

#endif
