// ----------------------------------------------------------------------
// ExceptionUtil.h
//
// Copyright 1998, Object Computing, Inc.
// 
// Some simple utilities to ease the pain of dealing with
// CORBA::Exceptions.
// ----------------------------------------------------------------------

#ifndef _ExceptionUtil_h_
#define _ExceptionUtil_h_

#include <tao/corba.h>

#include <ace/streams.h> 

static ostream& operator<<(ostream& os, CORBA::Exception& exc)
{
  // save the id of the exception
  const char* id = exc._id();

  // determine if it is a SystemException or UserException
  CORBA::SystemException* sysexc = CORBA::SystemException::_downcast(&exc);
  if (sysexc != (CORBA::SystemException*)0) {
    os << "CORBA::SystemException: ID " << id << ", "
       << "minor code = 0x" << hex << sysexc->minor() << ", "
       << "completed = ";
    switch(sysexc->completed()) {
      case CORBA::COMPLETED_YES  : os << "YES"    ; break;
      case CORBA::COMPLETED_NO   : os << "NO"     ; break;
      case CORBA::COMPLETED_MAYBE: os << "MAYBE"  ; break;
      default                    : os << "UNKNOWN"; break;
    }
    os << ends;
  }
  else {
    os << "CORBA::Exception: ID " << id << ends;
  }

  return os;
}

#endif // _ExceptionUtil_h_
