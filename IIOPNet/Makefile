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

build-base-debug:
    set CSFLAGS=/debug+ /d:DEBUG /d:TRACE /d:DEBUG_LOGFILE
    $(MAKE) build-base
    
build-ssl: build-base
    cd thirdparty\SSL\seclib
    $(MAKE) build
    cd ..\..\..
    cd SSLPlugin
    $(MAKE) build
    cd ..


build-tests: build-base
    cd IIOPChannel
    $(MAKE) build-unit-tests
    cd ..
    cd IDLToCLSCompiler
    $(MAKE) build-unit-tests
    cd ..
    cd IntegrationTests
    $(MAKE) build
    cd ..

build-tests-ssl: build-tests
    cd IntegrationTests
    $(MAKE) build-ssl
    cd ..

test:
    cd IIOPChannel
    $(MAKE) test
    cd ..
    cd IDLToCLSCompiler
    $(MAKE) test
    cd ..
    cd IntegrationTests
    $(MAKE) test
    cd ..   

test-ssl : test
    cd IntegrationTests
    $(MAKE) test-ssl
    cd ..

clean-base:
    cd IIOPChannel
    $(MAKE) clean
    cd ..
    cd CLSToIDLGenerator
    $(MAKE) clean
    cd ..
    cd IDLToCLSCompiler
    $(MAKE) clean
    cd ..

clean: clean-base
    cd SslPlugin
    $(MAKE) clean
    cd ..
    cd Examples
    $(MAKE) clean
    cd ..
    cd IntegrationTests
    $(MAKE) clean
    cd ..

rebuild-base: clean-base build-base

rebuild-base-debug: clean-base build-base-debug

