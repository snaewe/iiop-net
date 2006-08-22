include ..\..\MakeVars

PRCURRENTTIME = ..\..\Utils\currenttime.exe

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
