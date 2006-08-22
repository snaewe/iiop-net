include ..\..\MakeVars

PRCURRENTTIME = ..\..\Utils\currenttime.exe

build: nunit-var
    cd SingleThreaded
    $(MAKE) build
    cd ..
    cd OmniOrbServer
    @if not defined OMNIORB_HOME echo OMNIORB_HOME not set, skipping OmniORB integration test
    @if defined OMNIORB_HOME $(MAKE) /C build
    cd ..
    cd JacOrbClient
    @if not defined JACORB_HOME echo JACORB_HOME not set, skipping JACORB integration test
    @if defined JACORB_HOME $(MAKE) /C build
    cd ..
    cd MultiThreaded
    $(MAKE) build
    cd ..

test: nunit-var
    @echo Bidirectional integration tests starting at:
    @$(PRCURRENTTIME)
    cd SingleThreaded
    $(MAKE) test
    cd ..
    cd OmniOrbServer
    @if not defined OMNIORB_HOME echo OMNIORB_HOME not set, skipping OmniORB integration test
    @if defined OMNIORB_HOME $(MAKE) /C test
    cd ..
    cd JacOrbClient
    @if not defined JACORB_HOME echo JACORB_HOME not set, skipping JACORB integration test
    @if defined JACORB_HOME $(MAKE) /C test
    cd ..
    cd MultiThreaded
    $(MAKE) test
    cd ..

   
clean:
    cd SingleThreaded
    $(MAKE) clean
    cd ..
    cd OmniOrbServer
    @if defined OMNIORB_HOME $(MAKE) /C clean
    cd ..
    cd JacOrbClient
    @if defined JACORB_HOME $(MAKE) /C clean
    cd ..
    cd MultiThreaded
    $(MAKE) clean
    cd ..
