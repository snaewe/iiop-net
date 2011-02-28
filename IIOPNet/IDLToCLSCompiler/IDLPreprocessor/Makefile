include ..\..\MakeVars

Key = ..\..\Key.snk
Dest = bin\ 

build: build-dll

build-dll: bin\IDLPreprocessor.dll

$(Dest)IDLPreprocessor.dll: *.cs $(Key)
	if not exist $(Dest)nul mkdir $(Dest)
	$(CSC) $(CSFLAGS) /t:library /out:$@ *.cs


clean:
	if exist $(Dest)* del /s /q $(Dest)*
	if exist $(Dest) rmdir /s /q $(Dest)
