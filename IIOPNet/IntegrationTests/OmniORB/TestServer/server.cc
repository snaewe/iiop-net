/*
 * IIOP.NET integration test
 */

#include "service.hh"
#include "unknownByClient.hh"
#include <stdio.h>


/*
 * TestService implementation inherits the POA skeleton class
 */

class TestService_impl : virtual public POA_TestService,
                         public PortableServer::RefCountServantBase
{

public:
  ::TestUnion EchoTestUnion(const ::TestUnion& arg);
  ::TestUnionE EchoTestUnionE(const ::TestUnionE& arg);
  CORBA::Any* RetrieveUnknownUnion();
  CORBA::Any* EchoAny(const CORBA::Any& arg);
  ::wstringSeq* RetrieveWstringSeq(const CORBA::WChar * val, CORBA::Long nrOrElems);
  ::wstringSeq* EchoWstringSeq(const ::wstringSeq& arg);
  ::seqOfWStringSeq* EchoSeqOfWStringSeq(const ::seqOfWStringSeq& arg);
};

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
TestService_impl::EchoAny (const CORBA::Any& arg) {
  return new CORBA::Any(arg);
}

::wstringSeq* 
TestService_impl::RetrieveWstringSeq(const CORBA::WChar * val, CORBA::Long nrOfElems) {
  CORBA::WChar** contentArr = new CORBA::WChar*[nrOfElems];
  for (int i = 0; i < nrOfElems; i++) {
      contentArr[i] = (CORBA::WChar*)val;
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

int
main (int argc, char *argv[])
{

  try {
  /*
   * Initialize the ORB
   */

  CORBA::ORB_var orb = CORBA::ORB_init (argc, argv);

  /*
   * Obtain a reference to the RootPOA and its Manager
   */

  CORBA::Object_var poaobj = orb->resolve_initial_references ("RootPOA");
  PortableServer::POA_var poa = PortableServer::POA::_narrow (poaobj);

  /*
   * Create a TestService object
   */

  TestService_impl * test = new TestService_impl;

  /*
   * Activate the Servant
   */

  PortableServer::ObjectId_var oid = poa->activate_object (test);
  PortableServer::POAManager_var mgr = poa->the_POAManager();


  /*
   * Write reference to file
   */

  CORBA::Object_var ref = poa->id_to_reference (oid.in());


  /*
   * Acquire a reference to the Naming Service
   */

  CORBA::Object_var nsobj =
    orb->resolve_initial_references ("NameService");


  CosNaming::NamingContext_var nc = 
    CosNaming::NamingContext::_narrow (nsobj);

  if (CORBA::is_nil (nc)) {
    fprintf(stderr, "oops, I cannot access the Naming Service!\n");
    return 1;
  }


  /*
   * Construct Naming Service name for our testservice
   */

  CosNaming::Name name;
  name.length (1);
  name[0].id = CORBA::string_dup ("test");
  name[0].kind = CORBA::string_dup ("");
  
  /*
   * Store a reference in the Naming Service. 
   */

  printf("Binding TestService in the Naming Service ... \n");
  nc->rebind (name, ref);
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
