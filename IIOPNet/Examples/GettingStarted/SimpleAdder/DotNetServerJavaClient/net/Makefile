CLS2IDL     = ..\..\..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe
IIOPCHANNEL = ..\..\..\..\..\IIOPChannel\bin\IIOPChannel.dll
bin         = bin\ 
IDL         = IDL\ 
IDLType     = Ch.Elca.Iiop.Tutorial.GettingStarted.AdderImpl
IDLPath     = Ch\Elca\Iiop\Tutorial\GettingStarted\AdderImpl

build: build-exe build-idl

build-exe: $(bin) $(bin)AdderServer.exe

$(bin) $(IDL):
	if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll

$(bin)AdderServer.exe: $(bin)IIOPChannel.dll *.cs
	csc /t:exe /out:$@ /r:$(bin)IIOPChannel.dll *.cs


build-idl: $(IDL) $(IDL)$(IDLPath).idl $(bin)AdderServer.exe 

$(IDL)$(IDLPath).idl: $(bin)AdderServer.exe 
	$(CLS2IDL) -o $(IDL) $(IDLType) $?

clean:
	if exist $(bin)nul rmdir /q /s $(bin)
	if exist $(IDL)nul rmdir /q /s $(IDL)