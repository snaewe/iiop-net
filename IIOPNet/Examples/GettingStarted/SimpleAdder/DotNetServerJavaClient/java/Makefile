include ..\..\..\..\..\MakeVars

IDLSource=..\net\IDL\Ch\Elca\Iiop\Tutorial\GettingStarted\*.idl
Adder=Ch\Elca\Iiop\Tutorial\GettingStarted\ 

Predef=..\net\IDL\Predef.idl

build: build-idl build-java

build-idl:
	if not exist IDL\nul mkdir IDL
	copy ..\..\..\..\..\IDL\*.idl IDL
	xcopy /s /y ..\net\IDL IDL
	cd IDL
	idlj -td .. $(Adder)AdderImpl.idl 
	idlj -td .. Predef.idl
	idlj -td .. Ch\Elca\Iiop\GenericUserException.idl
	cd ..

build-java: $(Adder)*.java *.java
	javac -classpath . $?

clean:
	if exist Ch\Elca\Iiop\*.java del /q Ch\Elca\Iiop\*.java
	if exist Ch\Elca\Iiop\*.class del /q Ch\Elca\Iiop\*.class
	if exist $(Adder)*.java del /q $(Adder)*.java
	if exist $(Adder)*.class del /q $(Adder)*.class
	if exist IDL\nul rmdir /q /s IDL
	if exist org\nul rmdir /q /s org
        if exist Ch\nul rmdir /q /s Ch
	if exist *.class del /q *.class

