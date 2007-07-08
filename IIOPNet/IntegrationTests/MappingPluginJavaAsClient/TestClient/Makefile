include ..\..\..\MakeVars

TestService=Ch\Elca\Iiop\IntegrationTests\MappingPlugin\ 

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
	idlj -td .. $(TestService)ByteArrayContainer.idl
	idlj -td .. Predef.idl
	idlj -td .. Ch\Elca\Iiop\GenericUserException.idl
	idlj -td .. org\omg\BoxedArray\_System\seq1_octet.idl
	cd ..

copy-valuetypeImpls:
        copy valTypeImpls\*.java $(TestService)

build-java: junit-var copy-valuetypeImpls compile-java

build-helperJar:
	jar -c0vf collectionHelpers.jar java\util\*.class

compile-java: $(TestService)*.java *.java
	javac -classpath .;$(JUNIT_JAR) $?

start-client:
	@echo "Mapping plugin java client IntegrationTest"
	java -Xbootclasspath/a:collectionHelpers.jar -Djava.naming.factory.initial=com.sun.jndi.cosnaming.CNCtxFactory -Djava.naming.provider.url=iiop://localhost:8087 -cp .;$(JUNIT_JAR) TestClient

clean:
	if exist IDL\nul rmdir /q /s IDL
	if exist org\nul rmdir /q /s org
        if exist Ch\nul rmdir /q /s Ch
	if exist *.class del /q *.class
	if exist *.jar del /q *.jar



