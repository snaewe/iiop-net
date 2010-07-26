include ..\..\MakeVars

Key = ..\..\Key.snk
IIOPChannel = ..\..\IIOPChannel\bin\IIOPChannel.dll
IDLPreprocessor = ..\IDLPreprocessor\bin\IDLPreprocessor.dll
Dest = bin\ 
DestUt = bin\UnitTest\ 

build: build-exe

build-exe: $(Dest)IDLToCLSCompiler.exe

$(Dest)IIOPChannel.dll: $(IIOPChannel)
	if not exist $(Dest)nul mkdir $(Dest)
	copy $(IIOPChannel) $@

$(Dest)IDLPreprocessor.dll: $(IDLPreprocessor)
	if not exist $(Dest)nul mkdir $(Dest)
	copy $(IDLPreprocessor) $@

$(Dest)IDLToCLSCompiler.exe: $(Dest)IIOPChannel.dll $(Dest)IDLPreprocessor.dll *.cs $(Key)
	$(CSC) /nowarn:0162,0164,0168,0169,0219 $(CSFLAGS) /res:MappingPluginSchema.xsd /r:$(Dest)IIOPChannel.dll /r:$(Dest)IDLPreprocessor.dll /t:exe /out:$@ *.cs


build-tests: nunit-var $(DestUt)IDLToCLSCompiler.exe

$(DestUt)IIOPChannel.dll: $(IIOPChannel)
	if not exist $(DestUt)nul mkdir $(DestUt)
	copy $(IIOPChannel) $@

$(DestUt)IDLPreprocessor.dll: $(IDLPreprocessor)
	if not exist $(DestUt)nul mkdir $(DestUt)
	copy $(IDLPreprocessor) $@


$(DestUt)IDLToCLSCompiler.exe: $(DestUt)IIOPChannel.dll $(DestUt)IDLPreprocessor.dll *.cs $(Key)
	$(CSC) $(CSFLAGS) /d:UnitTest /res:MappingPluginSchema.xsd /r:$(NUNIT_FRAMEWORK_DLL) /r:$(DestUt)IIOPChannel.dll /r:$(DestUt)IDLPreprocessor.dll /t:exe /out:$@ *.cs
	copy $(NUNIT_FRAMEWORK_DLL) $(DestUt)


test: build-tests
	$(NUNIT_CONSOLE) $(NUNIT_ASSEMBLY_ARG_OPTION)$(DestUt)IDLToCLSCompiler.exe

	
clean:
	if exist $(Dest)* del /s /q $(Dest)*
	if exist $(Dest) rmdir /s /q $(Dest)
