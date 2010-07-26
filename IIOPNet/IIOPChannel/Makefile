include ..\MakeVars
Key = ..\Key.snk
Dest = bin\ 
UnitTestDest = bin\UnitTest\ 

build: build-dll

build-dll: $(Dest)IIOPChannel.dll
	
$(Dest)IIOPChannel.dll: *.cs $(Key)
	if not exist $(Dest)nul mkdir $(Dest)
	$(CSC) $(CSFLAGS) /res:MappingPluginSchema.xsd /t:library /out:$@ *.cs
	
build-unit-tests: build nunit-var *.cs
	if not exist $(UnitTestDest)nul mkdir $(UnitTestDest)
	$(CSC) /nowarn:0618 $(CSFLAGS) /d:UnitTest /r:$(NUNIT_FRAMEWORK_DLL) /res:MappingPluginSchema.xsd /t:library /out:$(UnitTestDest)IIOPChannel.dll *.cs
	copy $(NUNIT_FRAMEWORK_DLL) $(UnitTestDest)

test: build-unit-tests
	$(NUNIT_CONSOLE) $(NUNIT_ASSEMBLY_ARG_OPTION)$(UnitTestDest)IIOPChannel.dll

clean:
	if exist $(Dest)*.dll del /s /q $(Dest)*.dll
	if exist $(Dest)*.pdb del /s /q $(Dest)*.pdb
	if exist $(Dest)nul rmdir /s /q $(Dest) 
