NUNIT=D:\Programme\NUnitV2\bin\nunit.framework.dll 


build: copy-idl compile-idl build-client

build-client:
	if not exist bin\nul mkdir bin
	csc /t:library /r:$(NUNIT) /r:bin\IIOPChannel.dll /r:bin\Client.dll /out:bin\ClientTest.dll TestClient.cs


copy-idl:
    if not exist IDL\nul mkdir IDL
    cd IDL
    copy ..\..\..\..\IDLToCLSCompiler\IDL\*.idl .
    xcopy /s /q /y ..\..\TestServer\IDL .
    cd ..

compile-idl:
	cd IDL 
	..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o ..\bin client Ch\Elca\Iiop\IntegrationTests\TestService.idl
	cd ..

