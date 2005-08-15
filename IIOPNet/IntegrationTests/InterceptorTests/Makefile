PRCURRENTTIME = ..\Utils\currenttime.exe

build: nunit-var
    cd InvocationScenarios
    $(MAKE) build
    cd ..
    cd ServiceContextsAndTaggedComp
    $(MAKE) build
    cd ..
    cd PICurrent
    $(MAKE) build
    cd ..


nunit-var:        
    @if not defined NUNITV2_HOME echo Set the NUNITV2_HOME variable to the NUnit V2 home directory. Please check TestInstructions.txt.
    @if not defined NUNITV2_HOME exit 2

test: nunit-var
    @echo Interceptor integration tests starting at:
    @$(PRCURRENTTIME)    
    cd InvocationScenarios
    $(MAKE) test
    cd ..
    cd ServiceContextsAndTaggedComp
    $(MAKE) test
    cd ..
    cd PICurrent
    $(MAKE) test
    cd ..
   
clean:
    cd InvocationScenarios
    $(MAKE) clean
    cd ..
    cd ServiceContextsAndTaggedComp
    $(MAKE) clean
    cd ..
    cd PICurrent
    $(MAKE) clean
    cd ..
