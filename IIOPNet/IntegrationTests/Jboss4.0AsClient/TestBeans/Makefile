include ..\..\..\MakeVars

Test=ch\elca\iiop\integrationTests\ 
BIN=bin\ 
IDL=IDL\ 
IDLExt=IDLExt\ 
TestPackage=Ch.Elca.Iiop.IntegrationTests.
JBOSS_CLP=%JBOSS_HOME_4%\server\all\lib\jboss-j2ee.jar;.
LAUNCH = ..\..\..\Utils\launch.exe
DELAY = ..\..\..\Utils\delay.exe
KILL = ..\..\..\Utils\kill.exe


build: copy-external-idl generate-java-for-idl copy-meta-data build-classes build-jar build-idl

copy-external-idl: $(IDLExt)
	if not exist $(IDLExt) mkdir $(IDLExt)
        cd $(IDLExt)
        copy ..\..\..\..\IDL\*.idl .
        xcopy /s /q /y ..\..\TestServer\IDL .
        cd ..

generate-java-for-idl:
        cd $(IDLExt)
	idlj -td .. Ch\Elca\Iiop\IntegrationTests\TestService.idl 
	idlj -td .. Ch\Elca\Iiop\IntegrationTests\ByteArrayContainer.idl
	idlj -td .. Predef.idl
	idlj -td .. Ch\Elca\Iiop\GenericUserException.idl
        cd ..

copy-meta-data: $(BIN)
	if not exist bin mkdir bin
        if not exist bin\META-INF mkdir bin\META-INF
	if exist *.xml copy *.xml bin\META-INF

build-classes: $(Test)TestFwd.java $(Test)TestHomeFwd.java $(Test)TestBeanFwd.java $(Test)OrbSingleton.java $(Test)TestService*.java $(Test)_TestService*.java $(Test)ByteArrayContainer*.java ch\elca\iiop\GenericUserException*.java
	javac -classpath $(JBOSS_CLP) -d bin $?

$(IDL):
	if not exist $@nul mkdir $@

$(BIN):
	if not exist $@nul mkdir $@

$(IDLExt):
	if not exist $@nul mkdir $@


build-jar:
	cd bin
        jar cv0f jboss4AsClient-integration_ejb.jar META-INF Ch
        cd ..

build-idl: $(IDL)
        cd bin
        rmic -idl -classpath %JBOSS_HOME_4%\server\all\lib\jboss-j2ee.jar;. -d ..\$(IDL) $(TestPackage)TestHomeFwd
        rmic -idl -classpath %JBOSS_HOME_4%\server\all\lib\jboss-j2ee.jar;. -d ..\$(IDL) $(TestPackage)TestFwd 
        cd ..


deploy-bean:
	@copy bin\jboss4AsClient-integration_ejb.jar %JBOSS_HOME_4%\server\all\deploy\

undeploy-bean:
	@if exist %JBOSS_HOME_4%\server\all\deploy\jboss4AsClient-integration_ejb.jar del /q %JBOSS_HOME_4%\server\all\deploy\jboss4AsClient-integration_ejb.jar

start-server: deploy-bean
        @echo Starting JBoss in configuration all
        @$(LAUNCH) -d $(JBOSS_HOME_4)\bin run.bat -c All >> pid

stop-server: stop undeploy-bean

stop:
	@echo stopping JBoss
        @$(LAUNCH) -d $(JBOSS_HOME_4)\bin shutdown.bat -S >> pid
        @$(DELAY) 30
	@for /F %%p in (pid) do @$(KILL) %%p
	@del /q pid


clean:
        if exist bin del /s /q bin
	if exist bin rmdir /s /q bin
	if exist IDL del /s /q IDL
	if exist IDL rmdir /s /q IDL
	if exist $(IDLExt) del /s /q $(IDLExt)
	if exist $(IDLExt) rmdir /s /q $(IDLExt)

