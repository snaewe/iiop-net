
build: nunit-var
    cd SingleThreaded
    $(MAKE) build
    cd ..
    cd OmniOrbServer
    $(MAKE) build
    cd ..
    cd JacOrbClient
    $(MAKE) build
    cd ..
    cd MultiThreaded
    $(MAKE) build
    cd ..


nunit-var:        
    @if not defined NUNITV2_HOME echo Set the NUNITV2_HOME variable to the NUnit V2 home directory. Please check TestInstructions.txt.
    @if not defined NUNITV2_HOME exit 2

test: nunit-var
    cd SingleThreaded
    $(MAKE) test
    cd ..
    cd OmniOrbServer
    $(MAKE) test
    cd ..
    cd JacOrbClient
    $(MAKE) test
    cd ..
    cd MultiThreaded
    $(MAKE) test
    cd ..

   
clean:
    cd SingleThreaded
    $(MAKE) clean
    cd ..
    cd OmniOrbServer
    $(MAKE) clean
    cd ..
    cd JacOrbClient
    $(MAKE) clean
    cd ..
    cd MultiThreaded
    $(MAKE) clean
    cd ..
