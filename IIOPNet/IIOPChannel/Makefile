Key = ..\Key.snk
Dest = bin\ 
UnitTestDest = bin\UnitTest\ 

build: build-dll

build-dll: $(Dest)IIOPChannel.dll
	
$(Dest)IIOPChannel.dll: *.cs $(Key)
	if not exist $(Dest)nul mkdir $(Dest)
	csc $(CSFLAGS) /res:MappingPluginSchema.xsd /t:library /out:$@ *.cs
	
build-unit-tests: build nunit-var *.cs
	if not exist $(UnitTestDest)nul mkdir $(UnitTestDest)
	csc /nowarn:0618 $(CSFLAGS) /d:UnitTest /r:$(NUNITV2_HOME)\bin\nunit.framework.dll /t:library /out:$(UnitTestDest)IIOPChannel.dll *.cs

test: build-unit-tests
	%NUNITV2_HOME%\bin\nunit-console.exe /assembly:$(UnitTestDest)IIOPChannel.dll

nunit-var:
	if not defined NUNITV2_HOME echo Define NUNITV2_HOME as the home directory of your NUNIT installation
	if not defined NUNITV2_HOME exit 2

clean:
	if exist $(Dest)*.dll del /s /q $(Dest)*.dll
	if exist $(Dest)*.pdb del /s /q $(Dest)*.pdb
	if exist $(Dest)nul rmdir /s /q $(Dest) 
