include ..\..\..\MakeVars

LAUNCH = ..\..\..\Utils\launch.exe
KILL = ..\..\..\Utils\kill.exe
DELAY = ..\..\..\Utils\delay.exe

bin         = bin\ 
IDL         = IDL\ 
TestServiceDir = Ch\Elca\Iiop\IntegrationTests\ 

build: build-server build-stubs build-idl


build-server: $(bin)\TestServer.class $(bin)$(TestServiceDir)TestService.class $(bin)$(TestServiceDir)TestServiceImpl.class $(bin)$(TestServiceDir)Adder.class $(bin)$(TestServiceDir)AdderImpl.class $(bin)$(TestServiceDir)NamedValueImpl.class

$(bin)\TestServer.class $(bin)$(TestServiceDir)TestService.class $(bin)$(TestServiceDir)TestServiceImpl.class $(bin)$(TestServiceDir)Adder.class $(bin)$(TestServiceDir)AdderImpl.class $(bin)$(TestServiceDir)NamedValueImpl.class : TestServer.java $(TestServiceDir)TestService.java $(TestServiceDir)TestServiceImpl.java $(TestServiceDir)Adder.java $(TestServiceDir)AdderImpl.java $(TestServiceDir)NamedValueImpl.java
    if not exist $(bin)nul mkdir $(bin)
    javac -d bin Ch\Elca\Iiop\IntegrationTests\*.java
    javac -d bin TestServer.java    
    

build-stubs: $(bin)$(TestServiceDir)_TestServiceImpl_Tie.class $(bin)$(TestServiceDir)_TestService_Stub.class $(bin)$(TestServiceDir)_AdderImpl_Tie.class $(bin)$(TestServiceDir)_Adder_Stub.class

$(bin)$(TestServiceDir)_TestServiceImpl_Tie.class $(bin)$(TestServiceDir)_TestService_Stub.class $(bin)$(TestServiceDir)_AdderImpl_Tie.class $(bin)$(TestServiceDir)_Adder_Stub.class: $(bin)$(TestServiceDir)TestService.class $(bin)$(TestServiceDir)TestServiceImpl.class $(bin)$(TestServiceDir)Adder.class $(bin)$(TestServiceDir)AdderImpl.class
    cd bin
    rmic -iiop Ch.Elca.Iiop.IntegrationTests.TestServiceImpl
    rmic -iiop Ch.Elca.Iiop.IntegrationTests.AdderImpl
    rmic -iiop Ch.Elca.Iiop.IntegrationTests.TestSimpleIfImpl
    cd ..

build-idl: $(IDL)$(TestServiceDir)TestService.idl $(IDL)$(TestServiceDir)Adder.idl $(IDL)$(TestServiceDir)TestSimpleInterface1.idl $(IDL)$(TestServiceDir)TestSimpleInterface2.idl $(IDL)$(TestServiceDir)NamedValueImpl.idl

$(IDL)$(TestServiceDir)TestService.idl $(IDL)$(TestServiceDir)Adder.idl $(IDL)$(TestServiceDir)TestSimpleInterface1.idl $(IDL)$(TestServiceDir)TestSimpleInterface2.idl $(bin)$(TestServiceDir)NamedValueImpl.idl: $(bin)$(TestServiceDir)TestServiceImpl.class $(bin)$(TestServiceDir)AdderImpl.class $(bin)$(TestServiceDir)NamedValueImpl.class
    if not exist $(IDL) mkdir $(IDL)
    cd bin
    rmic -d ..\$(IDL) -idl Ch.Elca.Iiop.IntegrationTests.TestServiceImpl
    rmic -d ..\$(IDL) -idl Ch.Elca.Iiop.IntegrationTests.AdderImpl
    rmic -d ..\$(IDL) -idl Ch.Elca.Iiop.IntegrationTests.TestSimpleIfImpl
    rmic -d ..\$(IDL) -idl Ch.Elca.Iiop.IntegrationTests.NamedValueImpl
    cd ..

start-server:
	@$(LAUNCH) orbd.exe -ORBInitialPort 1050 >> pid
	@$(DELAY) 10
	@cd bin
	@..\$(LAUNCH) java.exe -Djava.naming.factory.initial=com.sun.jndi.cosnaming.CNCtxFactory -Djava.naming.provider.url=iiop://localhost:1050 -cp . TestServer >> ..\pid
	@cd ..


stop-server:
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid

clean:
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin







