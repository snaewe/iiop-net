
build: nunit-var
	cd Utils
	$(MAKE) build
	cd ..
	cd Basic
	$(MAKE) build
	cd ..
        cd Direct
	$(MAKE) build
        cd ..
        cd JavaRMIIIOP
	$(MAKE) build
	cd ..

nunit-var:        
	@if not defined NUNITV2_HOME echo Set the NUNITV2_HOME variable to the NUnit V2 home directory. Please check TestInstructions.txt.
	@if not defined NUNITV2_HOME exit 2

clean:
	cd Utils
	$(MAKE) clean
	cd ..
	cd Basic
	$(MAKE) clean
	cd ..
        cd Direct
	$(MAKE) clean
        cd ..
	cd JavaRMIIIOP
	$(MAKE) clean
	cd ..
