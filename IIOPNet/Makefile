projects = IIOPChannel CLSToIDLGenerator


build:
	cd IIOPChannel
	$(MAKE) build
	cd ..
	cd CLSToIDLGenerator
	$(MAKE) build
	cd ..
	cd IDLToCLSCompiler
	$(MAKE) build
	cd ..
	cd Examples
	$(MAKE) build
	cd ..

build-tests: build
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
	$(MAKE) build
	cd ..
	cd Examples
	$(MAKE) build
	cd ..

