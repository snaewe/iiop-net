

build: idl-files
	cd GenericCollections
	$(MAKE) build
	cd ..
	cd GettingStarted
	$(MAKE) build
	cd ..

idl-files:        
	if not exist ..\IDL\ir.idl echo You did not install IDL\ir.idl. Please check Readme.txt, Installation 
	if not exist ..\IDL\ir.idl exit 2

	if not exist ..\IDL\orb.idl echo You did not install IDL\orb.idl. Please check Readme.txt, Installation
	if not exist ..\IDL\orb.idl exit 2

clean:
	cd GenericCollections
	$(MAKE) clean
	cd ..
	cd GettingStarted
	$(MAKE) clean
	cd ..
