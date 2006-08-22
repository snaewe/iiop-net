include ..\..\..\..\..\MakeVars
!include .\MakeVars
IDLSource=..\netServer\IDL 
IDLTarget=IDL\ 

Predef=..\net\IDL\Predef.idl

build: omniorb_home_var client.exe

client.exe: client.obj AdderSK.obj GenericUserExceptionSK.obj
	$(LD) $(LDFLAGS) /OUT:$@ client.obj AdderSK.obj GenericUserExceptionSK.obj $(LDLIBS)

client.cc : Adder.hh GenericUserException.hh

$(IDLTarget)Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.idl $(IDLTarget)Ch\Elca\Iiop\GenericUserException.idl:
	if not exist $(IDLTarget)nul mkdir $(IDLTarget)
	copy $(OMNIORB_HOME)\IDL\orb.idl $(IDLTarget)
	xcopy /s /y $(IDLSource) $(IDLTarget)

Adder.hh : $(IDLTarget)Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.idl
	$(IDL) -I$(IDLTarget) $(IDLTarget)Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.idl

GenericUserException.hh : $(IDLTarget)Ch\Elca\Iiop\GenericUserException.idl
	$(IDL) -I$(IDLTarget) $(IDLTarget)Ch\Elca\Iiop\GenericUserException.idl

clean:
	if exist IDL\nul rmdir /q /s IDL
	if exist Adder.hh del /q Adder.hh
	if exist AdderSk.cc del /q AdderSk.cc
	if exist GenericUserException.hh del /q GenericUserException.hh
	if exist GenericUserExceptionSk.cc del /q GenericUserExceptionSk.cc

