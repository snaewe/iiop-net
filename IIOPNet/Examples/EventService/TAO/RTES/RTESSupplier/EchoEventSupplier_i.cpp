// EchoEventSupplier_i.cpp
// Implements a PushSupplier.

#include "EchoEventSupplier_i.h"

// Constructor duplicates the ORB reference.
EchoEventSupplier_i::EchoEventSupplier_i(CORBA::ORB_ptr orb)
  : orb_(CORBA::ORB::_duplicate(orb))
{
  // Nothing to do.
}

// Override the disconnect_push_Supplier() operation.
void EchoEventSupplier_i::disconnect_push_supplier(
        /* CORBA::Environment &ACE_TRY_ENV */
     )
     throw(CORBA::SystemException)
{
  // Deactivate this object.
  CORBA::Object_var obj = orb_->resolve_initial_references("POACurrent");
  PortableServer::Current_var current = PortableServer::Current::_narrow(obj.in());
  PortableServer::POA_var poa = current->get_POA();
  PortableServer::ObjectId_var objectId = current->get_object_id();
  poa->deactivate_object(objectId.in());
}
