CLS2IDL     = ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe
IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
bin         = bin\ 
IDL         = IDL\ 
IDLType     = Ch.Elca.Iiop.Demo.StorageSystem.Manager
IDLPath     = Ch\Elca\Iiop\Demo\StorageSystem\Manager
     

build: build-exe build-idl


build-exe: $(bin) $(bin)StorageService.exe

$(bin) $(IDL):
	if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll

$(bin)StorageService.exe: $(bin)IIOPChannel.dll *.cs
	csc /t:exe /out:$(bin)StorageService.exe /r:$(bin)IIOPChannel.dll *.cs


build-idl: $(IDL) $(IDL)$(IDLPath).idl $(bin)StorageService.exe 

$(IDL)$(IDLPath).idl: $(bin)StorageService.exe 
	$(CLS2IDL) -o $(IDL) $(IDLType) $?

clean:
	if exist $(bin)nul rmdir /q /s $(bin)
	if exist $(IDL)nul rmdir /q /s $(IDL)
