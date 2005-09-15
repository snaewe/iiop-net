#include "Adder.hh"
#include "GenericUserException.hh"
#include <stdio.h>



int
main (int argc, char *argv[])
{

  try {
  /*
   * Initialize the ORB
   */

  CORBA::ORB_var orb = CORBA::ORB_init (argc, argv);

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

  CosNaming::Name name;
  name.length (1);
  name[0].id = CORBA::string_dup ("adder");
  name[0].kind = CORBA::string_dup (""); 
  printf("Getting Adder from the Naming Service ... \n");
  CORBA::Object_var adder_obj = nc->resolve (name);
  Ch::Elca::Iiop::Tutorial::GettingStarted::Adder_var adder =
  Ch::Elca::Iiop::Tutorial::GettingStarted::Adder::_narrow(adder_obj);

  float arg1 = 1.0;
  float arg2 = 2.0;

  printf("arg1: ");
  scanf ("%f", &arg1);

  printf("arg2: ");
  scanf ("%f", &arg2);

  double result = adder->Add(arg1, arg2);
  printf("result %f + %f = %f", arg1, arg2, result);

  } catch(...) {
    fprintf(stderr, "Caught unknown exception.\n");
  }

  return 0;
}
