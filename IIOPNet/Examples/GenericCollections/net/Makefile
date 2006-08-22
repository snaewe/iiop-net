include ..\..\..\MakeVars

CLS2IDL     = ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe
IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
bin         = bin\ 
IDL         = IDL\ 
IDLType     = Ch.Elca.Iiop.Demo.StorageSystem.Manager
IDLPath     = Ch\Elca\Iiop\Demo\StorageSystem\Manager

LAUNCH = ..\..\..\Utils\launch.exe
KILL = ..\..\..\Utils\kill.exe
DELAY = ..\..\..\Utils\delay.exe
     

build: build-exe build-idl


build-exe: $(bin) $(bin)StorageService.exe

$(bin) $(IDL):
	if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll

$(bin)StorageService.exe: $(bin)IIOPChannel.dll *.cs
	$(CSC) /t:exe /out:$(bin)StorageService.exe /r:$(bin)IIOPChannel.dll *.cs


build-idl: $(IDL) $(IDL)$(IDLPath).idl $(bin)StorageService.exe 

$(IDL)$(IDLPath).idl: $(bin)StorageService.exe 
	$(CLS2IDL) -o $(IDL) $(IDLType) $?

start-server: $(bin)StorageService.exe
	@$(LAUNCH) $(bin)StorageService.exe >> pid
  

stop-server:
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid


clean:
	if exist $(bin)nul rmdir /q /s $(bin)
	if exist $(IDL)nul rmdir /q /s $(IDL)
