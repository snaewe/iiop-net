/*
 * IIOP.NET integration test
 */

#include "service.h"
#include <coss/CosNaming.h>

using namespace std;

/*
 * TestService implementation inherits the POA skeleton class
 */

class TestService_impl : virtual public POA_TestService
{

public:
  ::TestUnion EchoTestUnion(const ::TestUnion& arg);
  ::TestUnionE EchoTestUnionE(const ::TestUnionE& arg);
  CORBA::Any* RetrieveUnknownUnion();
  CORBA::Any* EchoAny(const CORBA::Any& arg);

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
    CORBA::Any* resultAny = new CORBA::Any();
    *resultAny <<= &result;
    return resultAny;
}

CORBA::Any*
TestService_impl::EchoAny (const CORBA::Any& arg) {
  return new CORBA::Any(arg);
}

int
main (int argc, char *argv[])
{
  /*
   * Initialize the ORB
   */

  CORBA::ORB_var orb = CORBA::ORB_init (argc, argv);

  /*
   * Obtain a reference to the RootPOA and its Manager
   */

  CORBA::Object_var poaobj = orb->resolve_initial_references ("RootPOA");
  PortableServer::POA_var poa = PortableServer::POA::_narrow (poaobj);
  PortableServer::POAManager_var mgr = poa->the_POAManager();

  /*
   * Create a TestService object
   */

  TestService_impl * test = new TestService_impl;

  /*
   * Activate the Servant
   */

  PortableServer::ObjectId_var oid = poa->activate_object (test);

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
    cerr << "oops, I cannot access the Naming Service!" << endl;
    exit (1);
  }

  cout << "Nameservice contacted ..." << flush;

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

  cout << "Binding TestService in the Naming Service ... " << flush;
  nc->rebind (name, ref);
  cout << "done." << endl;


  /*
   * Activate the POA and start serving requests
   */

  cout << "Running." << endl;

  mgr->activate ();
  orb->run();

  /*
   * Shutdown (never reached)
   */

  poa->destroy (TRUE, TRUE);
  delete test;

  return 0;
}
