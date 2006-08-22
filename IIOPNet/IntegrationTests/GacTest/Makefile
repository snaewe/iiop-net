include ..\..\MakeVars

PRCURRENTTIME = ..\..\Utils\currenttime.exe

build: nunit-var
    cd OnlyInGac
    $(MAKE) build
    cd ..
    cd LocalAndInGac
    $(MAKE) build
    cd ..

test: nunit-var
    @echo Gac integration tests starting at:
    @$(PRCURRENTTIME)    
    cd OnlyInGac
    $(MAKE) test
    cd ..
    cd LocalAndInGac
    $(MAKE) test
    cd ..
   
clean:
    cd OnlyInGac
    $(MAKE) clean
    cd ..
    cd LocalAndInGac
    $(MAKE) clean
    cd ..
