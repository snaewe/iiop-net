/*
 * IIOP.NET integration test
 */

#include "service.hh"
#include <stdio.h>


/*
 * TestService implementation inherits the POA skeleton class
 */

class TestService_impl : virtual public POA_TestService,
                         public PortableServer::RefCountServantBase
{

private:
CallbackIntIncrementer_var m_callBackIncr;
    
public:

    CORBA::Octet TestIncByte(CORBA::Octet arg);

    void RegisterCallbackIntIncrementer(CallbackIntIncrementer_ptr incrementer);
    CORBA::Long IncrementWithCallbackIncrementer(CORBA::Long arg);

    void string_callback(CallBack_ptr cb, const char * mesg);


};

CORBA::Octet 
TestService_impl::TestIncByte(CORBA::Octet arg) 
{
    return arg+1;
}

void 
TestService_impl::RegisterCallbackIntIncrementer(CallbackIntIncrementer_ptr incrementer) 
{
  printf("register incrementer callback!\n");    
  m_callBackIncr = CallbackIntIncrementer::_duplicate(incrementer);
  printf("registered incrementer callback!\n");  
}


CORBA::Long
TestService_impl::IncrementWithCallbackIncrementer(CORBA::Long arg)
{
    printf("callback registered incrementer callback!\n");  
    return m_callBackIncr->TestIncInt32(arg);
}

void 
TestService_impl::string_callback(CallBack_ptr cb, const char * mesg) 
{
  if( CORBA::is_nil(cb) ) {
    printf("Received a nil callback.\n");
    return;
  }  

  printf("perform callback!\n");
  cb->call_back(mesg);
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
  PortableServer::POA_var rootPoa = PortableServer::POA::_narrow (poaobj);
  PortableServer::POAManager_var mgr = rootPoa->the_POAManager();
  mgr->activate ();

  // Create a POA with the Bidirectional policy
  CORBA::PolicyList pl;
  pl.length(1);
  CORBA::Any a;
  a <<= BiDirPolicy::BOTH;
  pl[0] = orb->create_policy(BiDirPolicy::BIDIRECTIONAL_POLICY_TYPE, a);

  PortableServer::POA_var poa = rootPoa->create_POA("bidir", mgr, pl);

  // create the servant and activate
  TestService_impl * test = new TestService_impl;
  PortableServer::ObjectId_var oid = poa->activate_object (test);

  // get reference
  CORBA::Object_var ref = poa->id_to_reference (oid.in());

  // naming service
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
   * start serving requests
   */

  printf("Running.\n");
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
