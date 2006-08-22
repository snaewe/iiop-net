include ..\..\..\..\..\MakeVars

CLS2IDL     = ..\..\..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe
IIOPCHANNEL = ..\..\..\..\..\IIOPChannel\bin\IIOPChannel
bin         = bin\ 
IDL         = IDL\ 
IDLType     = Ch.Elca.Iiop.Tutorial.GettingStarted.Adder
IDLPath     = Ch\Elca\Iiop\Tutorial\GettingStarted\Adder
SERVEREXE   = $(bin)AdderServer.exe
COMMONDLL   = $(bin)Adder.dll
SSLPlugin = ..\..\..\..\..\SSLPLugin\bin\SSLPlugin.dll
SSLLib = ..\..\..\..\..\SSLPLugin\bin\org.mentalis.security.dll

LAUNCH = ..\..\..\..\..\Utils\launch.exe
KILL = ..\..\..\..\..\Utils\kill.exe
DELAY = ..\..\..\..\..\Utils\delay.exe


build: build-commondll build-serverexe build-idl

build-commondll: $(bin) $(COMMONDLL)
build-serverexe: $(bin) $(SERVEREXE)


$(bin) $(IDL):
    if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(IIOPCHANNEL).dll
    copy /y $(IIOPCHANNEL).dll $(bin)IIOPChannel.dll
    if EXIST $(IIOPCHANNEL).pdb copy /y $(IIOPCHANNEL).pdb $(bin)IIOPChannel.pdb
    
$(bin)SSLPlugin.dll: $(SSLPlugin)
	copy /y $(SSLPlugin) $(bin)SSLPLugin.dll

$(bin)org.mentalis.security.dll: $(SSLLib)
	copy /y $(SSLLib) $(bin)org.mentalis.security.dll

$(COMMONDLL): $(bin)IIOPChannel.dll NAdder.cs
    $(CSC) $(CSFLAGS) /t:library /out:$@ /r:$(bin)IIOPChannel.dll NAdder.cs

$(SERVEREXE): $(bin)IIOPChannel.dll $(COMMONDLL) $(bin)SSLPlugin.dll $(bin)org.mentalis.security.dll NAdderServer.cs
    $(CSC) $(CSFLAGS) /t:exe /out:$@ /r:$(bin)IIOPChannel.dll /r:$(COMMONDLL) /r:$(bin)SSLPlugin.dll /r:$(bin)org.mentalis.security.dll NAdderServer.cs


build-idl: $(IDL) $(IDL)$(IDLPath).idl $(COMMONDLL) 

$(IDL)$(IDLPath).idl: $(COMMONDLL) 
    $(CLS2IDL) -o $(IDL) $(IDLType) $?

start-server:
    @$(LAUNCH) $(SERVEREXE) >> pid


stop-server:
    @for /F %%p in (pid) do @$(KILL) %%p
    @del pid

clean:
    if exist $(bin)nul rmdir /q /s $(bin)
    if exist $(IDL)nul rmdir /q /s $(IDL)