Dest = bin\ 
UnitTestDest = bin\UnitTest\ 


build: build-dll

build-dll: $(Dest)IIOPChannel.dll
	
$(Dest)IIOPChannel.dll: *.cs
	if not exist $(Dest)nul mkdir $(Dest)
	csc $(CSFLAGS) /t:library /out:$@ *.cs
	

build-unit-tests: build nunit-var
	csc /nowarn:0618 /debug+ /d:UnitTest /r:$(NUNITV2_HOME)\bin\nunit.framework.dll /t:library /out:$(UnitTestDest)IIOPChannel.dll *.cs

nunit-var:
	if not defined NUNITV2_HOME echo Define NUNITV2_HOME as the home directory of your NUNIT installation
	if not defined NUNITV2_HOME exit 2

clean:
	del /s /q $(Dest)*.dll
	del /s /q $(Dest)*.pdb
	rmdir /s /q $(Dest) 
