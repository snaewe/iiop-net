include ..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll 
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb 
bin         = bin\ 
BEANPATH    = ch\elca\iiop\integrationTests\ 

build: copy-idl compile-idl build-client

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(bin) $(IIOPCHANNEL)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
        if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

build-client: $(bin) $(bin)IIOPChannel.dll *.cs
	$(CSC) $(CSFLAGS) /t:library /r:$(NUNIT_FRAMEWORK_DLL) /r:bin\IIOPChannel.dll /r:bin\Client.dll /out:bin\ClientTest.dll TestClient.cs ValueTypeImpls.cs ExceptionImpl.cs

copy-idl:
    if not exist IDL\nul mkdir IDL
    cd IDL
    copy ..\..\..\..\IDLToCLSCompiler\IDL\*.idl .
    xcopy /s /q /y ..\..\TestBeans\IDL .
    REM copy correct idl for jagged arrays
    copy ..\CorrectIDL\org\omg\boxedRMI\seq2*.idl org\omg\boxedRMI\ 
    copy ..\CorrectIDL\org\omg\boxedRMI\seq3*.idl org\omg\boxedRMI\ 
    copy ..\CorrectIDL\org\omg\boxedRMI\CORBA\seq2*.idl org\omg\boxedRMI\CORBA\ 
    cd ..

compile-idl:
	cd IDL 
	..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o ..\bin client $(BEANPATH)Test.idl $(BEANPATH)TestHome.idl $(BEANPATH)IntAdder.idl $(BEANPATH)IntAdderHome.idl org\omg\boxedRMI\seq2_long.idl org\omg\boxedRMI\seq2_octet.idl org\omg\boxedRMI\seq3_octet.idl org\omg\boxedRMI\CORBA\seq2_WStringValue.idl
	cd ..

clean:
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin
	