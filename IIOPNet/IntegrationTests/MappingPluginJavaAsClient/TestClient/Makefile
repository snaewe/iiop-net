TestService=Ch\Elca\Iiop\IntegrationTests\MappingPlugin\ 
JUNIT_JAR = $(JUNIT_HOME)\junit.jar

Predef=..\TestServer\IDL\Predef.idl

build: build-idl build-java build-helperJar

build-idl:
	if not exist IDL\nul mkdir IDL
	copy ..\..\..\IDL\*.idl IDL
        rem copy collection plugin idl
	xcopy /s /y ..\..\..\MappingPlugin\java\IDL IDL
        rem copy server idl
	xcopy /s /y ..\TestServer\IDL IDL
	cd IDL
	idlj -td .. $(TestService)TestService.idl 
	idlj -td .. $(TestService)TestSerializableClassB1.idl
	idlj -td .. Predef.idl
	cd ..

junit-jar:
	if not defined JUNIT_HOME echo define JUNIT_HOME first (e.g. C:\JUNIT\Junit3.8.1)
        if not defined JUNIT_HOME exit 2

copy-valuetypeImpls:
        copy valTypeImpls\*.java $(TestService)

build-java: junit-jar copy-valuetypeImpls compile-java

build-helperJar:
	jar -c0vf collectionHelpers.jar java\util\*.class

compile-java: $(TestService)*.java *.java
	javac -classpath .;$(JUNIT_JAR) $?

clean:
	if exist IDL\nul rmdir /q /s IDL
	if exist org\nul rmdir /q /s org
        if exist Ch\nul rmdir /q /s Ch
	if exist *.class del /q *.class
	if exist *.jar del /q *.jar

