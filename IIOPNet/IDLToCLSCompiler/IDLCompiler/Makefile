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

$(Dest)IDLToCLSCompiler.exe: $(Dest)IIOPChannel.dll $(Dest)IDLPreprocessor.dll *.java *.jsl
	vjc $(CSFLAGS) /res:MappingPluginSchema.xsd /r:System.Xml.dll /r:$(Dest)IIOPChannel.dll /r:$(Dest)IDLPreprocessor.dll /t:exe /out:$@ *.java *.jsl 


clean:
	del /s /q $(Dest)*
	rmdir $(Dest)
