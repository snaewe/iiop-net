include ..\MakeVars

build: idl-files
	cd GenericCollections
	$(MAKE) build
	cd ..
	cd GettingStarted
	$(MAKE) build
	cd ..
	cd EJBChatroom
	cd Websphere_5
	@if not defined WAS_HOME echo WAS_HOME not set, skipping WAS5 example
	@if defined WAS_HOME $(MAKE) build
	cd ..
	cd Jboss3.2.1
	@if not defined JBOSS_HOME echo JBOSS_HOME not set, skipping JBOSS example
	@if defined JBOSS_HOME $(MAKE) build
	cd ..
	cd ..
	cd EJBCommandPattern
	cd Jboss3.2
	@if not defined JBOSS_HOME echo JBOSS_HOME not set, skipping JBOSS example
	@if defined JBOSS_HOME $(MAKE) build
	cd ..
	cd ..
	cd Callback
	cd ChatroomInterDN
	$(MAKE) build
	cd ..
	cd ..
	cd Exceptions
	cd JavaSideExceptions
	$(MAKE) build
	cd ..
	cd ..
	

idl-files:        
	if not exist ..\IDL\ir.idl echo You did not install IDL\ir.idl. Please check Readme.txt, Installation 
	if not exist ..\IDL\ir.idl exit 2

	if not exist ..\IDL\orb.idl echo You did not install IDL\orb.idl. Please check Readme.txt, Installation
	if not exist ..\IDL\orb.idl exit 2

nunit2_2-var:        
	@if not defined NUNITV2_2_HOME echo Set the NUNITV2_2_HOME variable to the NUnit V2_2 home directory. Please check TestInstructions.txt.
	@if not defined NUNITV2_2_HOME exit 2
nunitforms-var:
	@if not defined NUNITFORMS_HOME echo Set the NUNITFORMS_HOME variable to the NUnit Forms V1_3 home directory. Please check TestInstructions.txt.
	@if not defined NUNITFORMS_HOME exit 2

test: nunit2_2-var nunitforms-var
	cd Callback
	cd ChatroomInterDN
	$(MAKE) test
	cd ..
	cd ..

clean:
	cd GenericCollections
	$(MAKE) clean
	cd ..
	cd GettingStarted
	$(MAKE) clean
	cd ..
	cd EJBChatroom
	cd Websphere_5
        @if defined WAS_HOME $(MAKE) clean
	cd ..
	cd Jboss3.2.1
	@if defined JBOSS_HOME $(MAKE) clean
	cd ..
	cd ..
	cd EJBCommandPattern
	cd Jboss3.2
	@if defined JBOSS_HOME $(MAKE) clean
	cd ..
	cd ..
	cd Callback
	cd ChatroomInterDN
	$(MAKE) clean
	cd ..
	cd ..
	cd Exceptions
	cd JavaSideExceptions
	$(MAKE) clean
	cd ..
	cd ..
