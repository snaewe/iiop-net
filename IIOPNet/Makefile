projects = IIOPChannel CLSToIDLGenerator


build: build-base build-examples

build-examples:
	cd Examples
	$(MAKE) build
	cd ..

build-base:
	cd IIOPChannel
	$(MAKE) build
	cd ..
	cd CLSToIDLGenerator
	$(MAKE) build
	cd ..
	cd IDLToCLSCompiler
	$(MAKE) build
	cd ..


build-tests: build-base
	cd IIOPChannel
	$(MAKE) build-unit-tests
	cd ..
        cd IntegrationTests
        $(MAKE) build
        cd ..

clean:
	cd IIOPChannel
	$(MAKE) clean
	cd ..
	cd CLSToIDLGenerator
	$(MAKE) clean
	cd ..
	cd IDLToCLSCompiler
	$(MAKE) clean
	cd ..
	cd Examples
	$(MAKE) clean
	cd ..

