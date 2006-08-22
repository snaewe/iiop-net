include ..\..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\..\IIOPChannel\bin\IIOPChannel.dll 
IIOPCHANNELPDB = ..\..\..\..\IIOPChannel\bin\IIOPChannel.pdb 
bin         = bin\ 

build: build-client

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
        if not exist $(bin)nul mkdir $(bin)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
        if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

IDL\Ch\Elca\Iiop\Demo\Chatroom\IChatroom.idl: ..\Server\IDL\Ch\Elca\Iiop\Demo\Chatroom\IChatroom.idl
    if not exist IDL\nul mkdir IDL
    cd IDL
    copy ..\..\..\..\..\IDLToCLSCompiler\IDL\*.idl .
    xcopy /s /q /y ..\..\Server\IDL .
    cd ..

$(bin)chatroom.dll: IDL\Ch\Elca\Iiop\Demo\Chatroom\IChatroom.idl
    cd IDL 
    ..\..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o ..\$(bin) chatroom Ch\Elca\Iiop\Demo\Chatroom\IChatroom.idl
    cd ..

$(bin)NUnitForms.dll:
	if not exist $(bin)nul mkdir $(bin)
	copy /y $(NUNITFORMS) $(bin)NUnitForms.dll


build-client: $(bin)IIOPChannel.dll $(bin)chatroom.dll Client.cs ClientForm.cs MessageImpl.cs MessageListenerImpl.cs
	$(CSC) $(CSFLAGS) /r:$(bin)IIOPChannel.dll /r:$(bin)chatroom.dll /t:exe /out:$(bin)Client.exe Client.cs ClientForm.cs MessageImpl.cs MessageListenerImpl.cs

$(bin)ClientTest.dll: $(bin)NUnitForms.dll $(bin)IIOPChannel.dll $(bin)chatroom.dll *.cs
	$(CSC) $(CSFLAGS) /t:library /r:$(NUNIT_FRAMEWORK_DLL) /r:$(bin)NUnitForms.dll /r:$(bin)IIOPChannel.dll /r:$(bin)chatroom.dll /out:$(bin)ClientTest.dll *.cs 

start-client: $(bin)ClientTest.dll
	$(NUNIT_CONSOLE) $(NUNIT_ASSEMBLY_ARG_OPTION)bin\ClientTest.dll

clean:
	if exist bin del /s /q bin\ 
	if exist bin rmdir /s /q bin\ 
	if exist IDL del /s /q IDL\ 
	if exist IDL rmdir /s /q IDL\ 

	
