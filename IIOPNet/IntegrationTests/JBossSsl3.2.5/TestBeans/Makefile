include ..\..\..\MakeVars

Test=ch\elca\iiop\integrationTests\ 
BIN=bin\ 
IDL=IDL\ 
TestPackage=ch.elca.iiop.integrationTests.
LAUNCH = ..\..\..\Utils\launch.exe
DELAY = ..\..\..\Utils\delay.exe
KILL = ..\..\..\Utils\kill.exe

JBOSS_SSL_CONF = allSsl
JBOSS_SSL_CONF_DIR = $(JBOSS_HOME_3_2_5)\server\$(JBOSS_SSL_CONF)\ 
JBOSS_SSL_CONF_CER_DIR = $(JBOSS_SSL_CONF_DIR)Cert\ 
KEYSTORE = $(JBOSS_SSL_CONF_CER_DIR)iiop-net-integrationtest.keystore
KEYSTOREPASS = iiop-over-ssl-test
EXPORTED_CER = $(JBOSS_SSL_CONF_CER_DIR)export.cer
CERT_ALIAS = JBOSS_IIOPNET_IT_TEST

JBOSS_CLP=$(JBOSS_HOME_3_2_5)\server\$(JBOSS_SSL_CONF)\lib\jboss-j2ee.jar;.
BEAN_JAR=jboss3-2_5-integration_ejb_SSL.jar

build: jboss-vars copy-meta-data build-classes build-jar build-idl

jboss-vars:        
	@if not defined JBOSS_HOME_3_2_5 echo Set the JBOSS_HOME_3_2_5 variable to the Jboss home directory. 
	@if not defined JBOSS_HOME_3_2_5 exit 2
	@if not defined JAVA_HOME echo Set the JAVA_HOME variable to the jdk home directory. 
	@if not defined JAVA_HOME exit 2

check-setup:
	@if NOT EXIST $(JBOSS_SSL_CONF_DIR) $(MAKE) /c setup

setup: jboss-vars
# jboss-ssl deployment
	@if not EXIST $(JBOSS_HOME_3_2_5)\server\all echo template all configuration doesn't exist
	@if not EXIST $(JBOSS_HOME_3_2_5)\server\all exit 3

	@if EXIST $(JBOSS_SSL_CONF_DIR) rmdir /s $(JBOSS_SSL_CONF_DIR)
	
	xcopy /s $(JBOSS_HOME_3_2_5)\server\all $(JBOSS_SSL_CONF_DIR) 
	@if EXIST (JBOSS_SSL_CONF_DIR)deploy\jboss*integration_ejb.jar del /Q $(JBOSS_SSL_CONF_DIR)deploy\jboss*integration_ejb.jar
	copy /Y JbossSslConf\conf\jacorb.properties $(JBOSS_SSL_CONF_DIR)conf\jacorb.properties
 	copy /Y JbossSslConf\deploy\iiop-service.xml $(JBOSS_SSL_CONF_DIR)deploy\iiop-service.xml
	if not EXIST $(JBOSS_SSL_CONF_CER_DIR) mkdir $(JBOSS_SSL_CONF_CER_DIR)

	$(JAVA_HOME)\bin\keytool -genkey -alias $(CERT_ALIAS) -keyalg RSA -validity 25000 -keystore $(KEYSTORE) -storepass $(KEYSTOREPASS) -keypass $(KEYSTOREPASS) -dname "CN=JBoss Server IIOP.NET it-test"
	$(JAVA_HOME)\bin\keytool -export -alias $(CERT_ALIAS) -keystore $(KEYSTORE) -storepass $(KEYSTOREPASS) -file $(EXPORTED_CER)

#need to import into java cacerts, otherwise jboss client can't connect to nameservice for deployment
#cacerts pwd is changeit by default
	@echo importing certificate into java cacerts root certificate store, answer yes to complete
	@if not defined CACERTS_PWD $(JAVA_HOME)\bin\keytool -import -alias $(CERT_ALIAS) -file $(EXPORTED_CER) -keystore $(JAVA_HOME)\jre\lib\security\cacerts -storepass changeit
	@if defined CACERTS_PWD $(JAVA_HOME)\bin\keytool -import -alias $(CERT_ALIAS) -file $(EXPORTED_CER) -keystore $(JAVA_HOME)\jre\lib\security\cacerts -storepass $(CACERTS_PWD)

#import into windows keystore
	@echo importing certificate into root certificate store, answer yes to complete
	certmgr -add $(EXPORTED_CER) -s root


undo-setup: jboss-vars
	@if EXIST $(JBOSS_SSL_CONF_DIR) rmdir /q /s $(JBOSS_SSL_CONF_DIR)

	@if not defined CACERTS_PWD $(JAVA_HOME)\bin\keytool -delete -alias $(CERT_ALIAS) -keystore $(JAVA_HOME)\jre\lib\security\cacerts -storepass changeit
	@if defined CACERTS_PWD $(JAVA_HOME)\bin\keytool -delete -alias $(CERT_ALIAS) -keystore $(JAVA_HOME)\jre\lib\security\cacerts -storepass $(CACERTS_PWD)

#how can the certificate be deleted from windows store?



copy-meta-data: $(BIN)
	if not exist bin mkdir bin
        if not exist bin\META-INF mkdir bin\META-INF
	if exist *.xml copy *.xml bin\META-INF

build-classes: $(Test)Test.java $(Test)TestHome.java $(Test)TestBean.java $(Test)IntAdder.java $(Test)IntAdderHome.java $(Test)IntAdderBean.java $(Test)TestSerializableClassB1.java $(Test)TestSerializableClassB2.java $(Test)TestSerializableClassC.java $(Test)TestSerializableClassD.java $(Test)TestNonSerializableBaseClass.java
	javac -classpath $(JBOSS_CLP) -d bin $?

$(IDL):
	if not exist $@nul mkdir $@

$(BIN):
	if not exist $@nul mkdir $@

build-jar:
	cd bin
        jar cv0f $(BEAN_JAR) META-INF ch
        cd ..

build-idl: $(IDL)
        cd bin
        rmic -idl -classpath $(JBOSS_HOME_3_2_5)\server\$(JBOSS_SSL_CONF)\lib\jboss-j2ee.jar;. -d ..\$(IDL) $(TestPackage)TestHome
        rmic -idl -classpath $(JBOSS_HOME_3_2_5)\server\$(JBOSS_SSL_CONF)\lib\jboss-j2ee.jar;. -d ..\$(IDL) $(TestPackage)Test 
        rmic -idl -classpath $(JBOSS_HOME_3_2_5)\server\$(JBOSS_SSL_CONF)\lib\jboss-j2ee.jar;. -d ..\$(IDL) $(TestPackage)IntAdderHome
        rmic -idl -classpath $(JBOSS_HOME_3_2_5)\server\$(JBOSS_SSL_CONF)\lib\jboss-j2ee.jar;. -d ..\$(IDL) $(TestPackage)IntAdder 
        cd ..


deploy-bean: jboss-vars
	@copy bin\$(BEAN_JAR) $(JBOSS_HOME_3_2_5)\server\$(JBOSS_SSL_CONF)\deploy\

undeploy-bean: jboss-vars
	@if exist $(JBOSS_HOME_3_2_5)\server\$(JBOSS_SSL_CONF)\deploy\$(BEAN_JAR) del /q $(JBOSS_HOME_3_2_5)\server\$(JBOSS_SSL_CONF)\deploy\$(BEAN_JAR)

start-server: deploy-bean
        @echo Starting JBoss in configuration allSsl
        @$(LAUNCH) -d $(JBOSS_HOME_3_2_5)\bin run.bat -c $(JBOSS_SSL_CONF) >> pid

stop-server: stop undeploy-bean

stop:
	@echo stopping JBoss
        @$(LAUNCH) -d $(JBOSS_HOME_3_2_5)\bin shutdown.bat -S >> pid
        @$(DELAY) 30
	@for /F %%p in (pid) do @$(KILL) %%p
	@del /q pid


clean:
        if exist bin del /s /q bin
	if exist bin rmdir /s /q bin
	if exist IDL del /s /q IDL
	if exist IDL rmdir /s /q IDL
