/*
 * IIOP.NET integration test
 */

#include "service.hh"
#include "unknownByClient.hh"
#include "internalif.hh"
#include <stdio.h>


/*
 * TestService implementation inherits the POA skeleton class
 */

class TestService_impl : virtual public POA_TestService,
                         public PortableServer::RefCountServantBase
{

private:
  boundedLongSeq* m_intSeq;
  PortableServer::POA_var m_rootPoa;
    
public:

  TestService_impl(PortableServer::POA_var rootPoa);

  CORBA::WChar EchoWChar(CORBA::WChar arg);
  CORBA::WChar* EchoWString(const CORBA::WChar* arg);
  char* EchoString(const char* arg);
  ::TestUnion EchoTestUnion(const ::TestUnion& arg);
  ::TestUnionE EchoTestUnionE(const ::TestUnionE& arg);
  CORBA::Any* RetrieveUnknownUnion();
  CORBA::Any* RetrieveWStringAsAny(const CORBA::WChar* arg);
  CORBA::Any* RetrieveStringAsAny(const char* arg);
  CORBA::Any* EchoAny(const CORBA::Any& arg);
  CORBA::Any* RetrieveStructWithTypedefMember(CORBA::Long elemVal);
  CORBA::Any* RetrieveTypedefedSeq(CORBA::Long nrOfElems, CORBA::Long memberVal);  
  CORBA::ULong ExtractFromULongAny(const CORBA::Any& arg);
  CORBA::Long ExtractFromLongTypeDef(const CORBA::Any& arg);
  CORBA::Any* RetrieveULongAsAny(CORBA::ULong arg);
  CORBA::Any* RetrieveLongTypeDefAsAny(CORBA::Long arg);

  CORBA::WChar* ExtractFromWStringAny(const CORBA::Any& arg);
  char* ExtractFromStringAny(const CORBA::Any& arg);
  seq_of_octect_seq* ExtractFromOctetOfOctetSeqAny(const CORBA::Any& arg);
  ::wstringSeq* RetrieveWstringSeq(const CORBA::WChar * val, CORBA::Long nrOrElems);
  ::wstringSeq* EchoWstringSeq(const ::wstringSeq& arg);
  ::seqOfWStringSeq* EchoSeqOfWStringSeq(const ::seqOfWStringSeq& arg);
  ::boundedLongSeq* EchoBoundedSeq(const ::boundedLongSeq& arg);
  ::Uuids* EchoUuids(const ::Uuids& arg);
  CORBA::Any *RetrieveUuidAsAny(CORBA::Long nrOfElementsOuter, CORBA::Long nrOfElementsInner, 
                                CORBA::Octet elemVal);
  TestService::InnerStruct EchoInnerStruct(const TestService::InnerStruct& arg);
  ::RecStruct* EchoRecStruct(const RecStruct& arg);

  ::SimpleStruct* EchoSimpleStruct(const SimpleStruct& arg);

    CORBA::Octet octet(CORBA::Octet arg);
  boundedLongSeq* sequence();
  void sequence(const boundedLongSeq& _v);  

  CORBA::Any* RetrieveInnerStructAsAny(const TestService::InnerStruct& arg);
  CORBA::Any* RetrieveEventAsAny(const TestService::Event& arg);


  BlobData EchoBlobData(const BlobData& data);
  intList_slice* EchoIntList5(const intList arg);
  int2Dim_slice* EchoInt2Dim2x2(const int2Dim arg);
  stringList_slice* EchoStringList5(const stringList arg);

  CCE::Assembly_ptr CreateAsm();
  

};


/*
 * TestSimpleServiceInternal implementation inherits the POA skeleton class
 */

class TestSimpleServiceInternal_impl : virtual public POA_Internal::TestSimpleServiceInternal,
                         public PortableServer::RefCountServantBase
{

  CORBA::Long EchoLong(CORBA::Long arg);

};

/*
 * Assembly implementation inherits the POA skeleton class
 */

class Assembly_impl : virtual public POA_CCE::Assembly,
                         public PortableServer::RefCountServantBase
{
};

TestService_impl::TestService_impl(PortableServer::POA_var rootPoa) {
    m_rootPoa = rootPoa;
}

CORBA::WChar 
TestService_impl::EchoWChar(CORBA::WChar arg) 
{
  return arg;
}

CORBA::WChar* 
TestService_impl::EchoWString(const CORBA::WChar* arg) 
{
    return CORBA::wstring_dup(arg);
}

char* 
TestService_impl::EchoString(const char* arg) 
{
    return CORBA::string_dup(arg);
}


::TestUnion
TestService_impl::EchoTestUnion (const ::TestUnion& arg)
{
  return arg;
}

::TestUnionE
TestService_impl::EchoTestUnionE (const ::TestUnionE& arg)
{
  return arg;
}

CORBA::Any*
TestService_impl::RetrieveUnknownUnion() {
    ::TestUnionE2 result;
    result.valE0(13);
    CORBA::Any* resultAny = new CORBA::Any;
    *resultAny <<= result;
    return resultAny;
}

CORBA::Any* 
TestService_impl::RetrieveWStringAsAny(const CORBA::WChar* arg) {
    CORBA::WChar* argVal = (CORBA::WChar*)arg;
    CORBA::Any* resultAny = new CORBA::Any;
    *resultAny <<= argVal;
    //printf("value-kind: %u\n", (*(*resultAny).type()).kind());
    return resultAny;
}

CORBA::Any* 
TestService_impl::RetrieveStringAsAny(const char* arg) {
    char* argVal = (char*)arg;
    CORBA::Any* resultAny = new CORBA::Any;
    *resultAny <<= argVal;
    //printf("value: %u\n", (*(*resultAny).type()).kind());
    return resultAny;
}


CORBA::ULong 
TestService_impl::ExtractFromULongAny(const CORBA::Any& arg) {
    CORBA::ULong result;
    arg >>= result;
    return result;    
}

CORBA::Long 
TestService_impl::ExtractFromLongTypeDef(const CORBA::Any& arg) {
    longTD extract;
    arg >>= extract;
    return (CORBA::Long)extract;
}


CORBA::Any*
TestService_impl::RetrieveULongAsAny(CORBA::ULong arg) {
    CORBA::Any* resultAny = new CORBA::Any;
    *resultAny <<= arg;
    return resultAny;    
}

CORBA::Any* 
TestService_impl::RetrieveLongTypeDefAsAny(CORBA::Long arg) {
    CORBA::Any* resultAny = new CORBA::Any;    
    longTD insert = (longTD)arg;
    *resultAny <<= insert;  
    resultAny->type(_tc_longTD);
    return resultAny;
}

CORBA::WChar* 
TestService_impl::ExtractFromWStringAny(const CORBA::Any& arg) {
    const CORBA::WChar* result;
    arg >>= result;
    return CORBA::wstring_dup(result);
}

char* 
TestService_impl::ExtractFromStringAny(const CORBA::Any& arg) {
    const char* result;
    arg >>= result;
    return CORBA::string_dup(result);
}


seq_of_octect_seq* 
TestService_impl::ExtractFromOctetOfOctetSeqAny(const CORBA::Any& arg) {
    seq_of_octect_seq* result;
    arg >>= result;
    return new seq_of_octect_seq(*result);
}



CORBA::Any*
TestService_impl::EchoAny (const CORBA::Any& arg) {
  return new CORBA::Any(arg);
}

CORBA::Any* 
TestService_impl::RetrieveStructWithTypedefMember(CORBA::Long elemVal) {
  
  StructWithTypedefMember result;
  result.longtdField = elemVal;

  CORBA::Any* resultAny = new CORBA::Any;
  *resultAny <<= result;
  return resultAny;    
}

CORBA::Any* 
TestService_impl::RetrieveTypedefedSeq(CORBA::Long nrOfElems, CORBA::Long memberVal) {
    CORBA::Any* resultAny = new CORBA::Any;
    
    CORBA::Long* contentArr = new CORBA::Long[nrOfElems];
    for (int i = 0; i < nrOfElems; i++) {
        contentArr[i] = memberVal;
    }
    boundedLongSeq* resultSeq = new boundedLongSeq((CORBA::ULong)nrOfElems, contentArr);    
   
    *resultAny <<= resultSeq;
    return resultAny;
}

::wstringSeq* 
TestService_impl::RetrieveWstringSeq(const CORBA::WChar * val, CORBA::Long nrOfElems) {
  CORBA::WChar** contentArr = new CORBA::WChar*[nrOfElems];
  for (int i = 0; i < nrOfElems; i++) {
      contentArr[i] = CORBA::wstring_dup(val);
  }
  wstringSeq* result = new wstringSeq((CORBA::ULong)nrOfElems, (CORBA::ULong)nrOfElems, contentArr);
  return result;
}

::wstringSeq*
TestService_impl::EchoWstringSeq(const ::wstringSeq& arg) {
  return new ::wstringSeq(arg);
}

::seqOfWStringSeq* 
TestService_impl::EchoSeqOfWStringSeq(const ::seqOfWStringSeq& arg) {
  return new ::seqOfWStringSeq(arg);
}

::boundedLongSeq*
TestService_impl::EchoBoundedSeq(const ::boundedLongSeq& arg) {
  return new ::boundedLongSeq(arg);
}

::Uuids*
TestService_impl::EchoUuids(const ::Uuids& arg) {
  return new ::Uuids(arg);
}

CORBA::Any*
TestService_impl::RetrieveUuidAsAny(CORBA::Long nrOfElementsOuter, CORBA::Long nrOfElementsInner, 
                                    CORBA::Octet elemVal) {
    CORBA::Any* resultAny = new CORBA::Any;
    
    Uuid* contentArr = new Uuid[nrOfElementsOuter];
    for (int i = 0; i < nrOfElementsOuter; i++) {
        CORBA::Octet* innerContent = new CORBA::Octet[nrOfElementsInner];
        for (int j = 0; j < nrOfElementsInner; j++) {
            innerContent[j] = elemVal;
        }
        Uuid* innerArrElem = new Uuid((CORBA::ULong)nrOfElementsInner, innerContent);
        contentArr[i] = *innerArrElem;
    }
        
    Uuids* resultSeq = new Uuids((CORBA::ULong)nrOfElementsOuter, (CORBA::ULong)nrOfElementsOuter,
                                 contentArr);
   
    *resultAny <<= resultSeq;
    return resultAny;

  
}

TestService::InnerStruct 
TestService_impl::EchoInnerStruct(const TestService::InnerStruct& arg) {
  return arg;
}

::RecStruct *
TestService_impl::EchoRecStruct(const RecStruct& arg) {
  return new ::RecStruct(arg);
}

::SimpleStruct* 
TestService_impl::EchoSimpleStruct(const SimpleStruct& arg) {
  return new ::SimpleStruct(arg);
}

CORBA::Octet 
TestService_impl::octet(CORBA::Octet arg) {
  return arg;
}

boundedLongSeq* 
TestService_impl::sequence() {
    return m_intSeq;
}
void
TestService_impl::sequence(const boundedLongSeq& _v) {
    m_intSeq = new ::boundedLongSeq(_v);
}


CORBA::Any* 
TestService_impl::RetrieveInnerStructAsAny(const TestService::InnerStruct& arg) {
  TestService::InnerStruct* resultContent = new TestService::InnerStruct(arg);
  CORBA::Any* result = new CORBA::Any;
  *result <<= resultContent;
  return result;
}

CORBA::Any* 
TestService_impl::RetrieveEventAsAny(const TestService::Event& arg) {
  TestService::Event* resultContent = new TestService::Event(arg);
  CORBA::Any* result = new CORBA::Any;
  *result <<= resultContent;
  return result;
}

BlobData
TestService_impl::EchoBlobData(const BlobData& data) {
   return data;
}

intList_slice* 
TestService_impl::EchoIntList5(const intList arg) {
    return intList_dup(arg);
}

int2Dim_slice* 
TestService_impl::EchoInt2Dim2x2(const int2Dim arg) {
    return int2Dim_dup(arg);
}

stringList_slice*
TestService_impl::EchoStringList5(const stringList arg) {
    return stringList_dup(arg);
}

CCE::Assembly_ptr 
TestService_impl::CreateAsm() {

    Assembly_impl * asmImpl = new Assembly_impl;

    /*
     * Activate the Servant
     */

    PortableServer::ObjectId_var oid = m_rootPoa->activate_object (asmImpl);
    CORBA::Object_var ref = m_rootPoa->id_to_reference (oid.in());
    return CCE::Assembly::_narrow(ref);
}


CORBA::Long 
TestSimpleServiceInternal_impl::EchoLong(CORBA::Long arg) {
    return arg;
}


int
main (int argc, char *argv[])
{
  printf("starting...\n");
  try {
  /*
   * Initialize the ORB
   */

  CORBA::ORB_var orb = CORBA::ORB_init (argc, argv);
  printf("orb initialized.\n");

  /*
   * Obtain a reference to the RootPOA and its Manager
   */

  CORBA::Object_var poaobj = orb->resolve_initial_references ("RootPOA");
  PortableServer::POA_var poa = PortableServer::POA::_narrow (poaobj);
  printf("root poa resolved.\n");

  /*
   * Create service objects
   */

  TestService_impl * test = new TestService_impl(poa);
  TestSimpleServiceInternal_impl * testInternalIf =
                            new TestSimpleServiceInternal_impl();
  printf("service object created.\n");

  /*
   * Activate the Servants
   */

  PortableServer::ObjectId_var oidTest = poa->activate_object (test);
  PortableServer::ObjectId_var oidTestInternal = poa->activate_object (testInternalIf);
  printf("service object activated.\n");
  PortableServer::POAManager_var mgr = poa->the_POAManager();


  CORBA::Object_var refTest = poa->id_to_reference (oidTest.in());
  CORBA::Object_var refTestInternalIf = poa->id_to_reference (oidTestInternal.in());

  printf("object references aquired.\n");

  /*
   * Acquire a reference to the Naming Service
   */

  CORBA::Object_var nsobj =
    orb->resolve_initial_references ("NameService");

  printf("naming service reference found.\n");

  CosNaming::NamingContext_var nc = 
    CosNaming::NamingContext::_narrow (nsobj);

  printf("root namingcontext resolved.\n");

  if (CORBA::is_nil (nc)) {
    fprintf(stderr, "oops, I cannot access the Naming Service!\n");
    return 1;
  }


  /*
   * Construct Naming Service name for our services
   */

  CosNaming::Name nameTest;
  nameTest.length (1);
  nameTest[0].id = CORBA::string_dup ("test");
  nameTest[0].kind = CORBA::string_dup ("");

  CosNaming::Name nameTestInternalIf;
  nameTestInternalIf.length (1);
  nameTestInternalIf[0].id = CORBA::string_dup ("testInternal");
  nameTestInternalIf[0].kind = CORBA::string_dup ("");
  
  /*
   * Store a reference in the Naming Service. 
   */

  printf("Binding TestService in the Naming Service ... \n");
  nc->rebind (nameTest, refTest);
  printf("Binding TestServiceInternal in the Naming Service ... \n");
  nc->rebind (nameTestInternalIf, refTestInternalIf);
  printf("done.\n");

  /*
   * Activate the POA and start serving requests
   */

  printf("Running.\n");

  mgr->activate ();
  orb->run();

  /*
   * Shutdown (never reached)
   */

  poa->destroy (TRUE, TRUE);
  delete test;
  } catch(...) {
    fprintf(stderr, "Caught unknown exception.\n");
  }

  return 0;
}