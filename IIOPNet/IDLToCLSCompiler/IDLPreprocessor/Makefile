Dest = bin\ 

build: build-dll

build-dll: bin\IDLPreprocessor.dll

$(Dest)IDLPreprocessor.dll: *.java *.jsl
	if not exist $(Dest)nul mkdir $(Dest)
	vjc $(CSFLAGS) /t:library /out:$@ *.java *.jsl 


clean:
	del /s /q $(Dest)*
	rmdir $(Dest)
