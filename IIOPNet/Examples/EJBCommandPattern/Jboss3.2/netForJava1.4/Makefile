include ..\..\..\..\MakeVars

build: build-cls build-client


copy-idl:
	if not exist IDL mkdir IDL
	xcopy /s /q /y ..\java\IDL IDL\ 
	cd IDL
	copy ..\..\..\..\..\IDLToCLSCompiler\IDL\orb.idl .
    cd ..

build-cls: copy-idl
	if not exist bin mkdir bin	
	cd IDL	
	..\..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o ..\bin commands ch\elca\iiop\demo\ejbCommand\CommandTarget.idl ch\elca\iiop\demo\ejbCommand\CommandTargetHome.idl ch\elca\iiop\demo\ejbCommand\AddOp.idl ch\elca\iiop\demo\ejbCommand\SubOp.idl
	cd ..	


build-client:
	copy ..\..\..\..\IIOPChannel\bin\IIOPChannel.dll bin\ 
	$(CSC) $(CSFLAGS) /r:bin\IIOPChannel.dll /r:bin\commands.dll /t:exe /out:bin\Client.exe *.cs	
	
build-valuetype-skeleton-impl: copy-idl
    if not exist bin mkdir bin	
    cd IDL	
	..\..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -vtSkel -vtSkelTd .. -o ..\bin commandsVal ch\elca\iiop\demo\ejbCommand\AddOp.idl ch\elca\iiop\demo\ejbCommand\SubOp.idl
	cd ..
	cd bin
	del commandsVal.dll
	cd ..

start-client:
    bin\Client.exe

clean:
	if exist bin del /s /q bin\ 
	if exist bin rmdir /s /q bin\ 
	if exist IDL del /s /q IDL\ 
	if exist IDL rmdir /s /q IDL\ 

	