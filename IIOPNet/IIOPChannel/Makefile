Dest = bin\ 


build: build-dll

$(Dest):
	mkdir $(Dest)

build-dll: $(Dest) $(Dest)IIOPChannel.dll
	
$(Dest)IIOPChannel.dll: *.cs
	csc $(CSFLAGS) /t:library /out:$@ $? 
	
clean:
	del /s /q $(Dest)*.dll
	rmdir $(Dest) 
