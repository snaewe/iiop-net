IIOPChannel = ..\..\IIOPChannel\bin\IIOPChannel.dll
IDLPreprocessor = ..\IDLPreprocessor\bin\IDLPreprocessor.dll
Dest = bin\ 

build: build-exe

build-exe: bin\IDLToCLSCompiler.exe

$(Dest)IIOPChannel.dll: $(IIOPChannel)
	if not exist $(Dest)nul mkdir $(Dest)
	copy $(IIOPChannel) $@

$(Dest)IDLPreprocessor.dll: $(IDLPreprocessor)
	if not exist $(Dest)nul mkdir $(Dest)
	copy $(IDLPreprocessor) $@

$(Dest)IDLToCLSCompiler.exe: $(Dest)IIOPChannel.dll $(Dest)IDLPreprocessor.dll *.cs
	csc $(CSFLAGS) /res:MappingPluginSchema.xsd /r:$(Dest)IIOPChannel.dll /r:$(Dest)IDLPreprocessor.dll /t:exe /out:$@ *.cs


clean:
	if exist $(Dest)* del /s /q $(Dest)*
	if exist $(Dest) rmdir $(Dest)
