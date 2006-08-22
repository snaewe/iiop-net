include ..\..\..\MakeVars

IDLSource=..\net\IDL\Ch\Elca\Iiop\Demo\StorageSystem\*.idl
StorageSystem=Ch\Elca\Iiop\Demo\StorageSystem\ 
EntryImpl=$(StorageSystem)EntryImpl.java

Predef=..\net\IDL\Predef.idl

build: build-idl build-java

build-idl:
	if not exist IDL\nul mkdir IDL
	copy ..\..\..\IDL\*.idl IDL
	xcopy /s /y ..\net\IDL IDL
	cd IDL
	idlj -td .. Ch\Elca\Iiop\Demo\StorageSystem\Manager.idl 
	idlj -td .. Ch\Elca\Iiop\Demo\StorageSystem\Container.idl 
	idlj -td .. Ch\Elca\Iiop\Demo\StorageSystem\Entry.idl
	idlj -td .. Ch\Elca\Iiop\GenericUserException.idl
	idlj -td .. Predef.idl
	idlj -td .. org\omg\BoxedArray\Ch\Elca\Iiop\Demo\StorageSystem\seq1_Container.idl
	idlj -td .. org\omg\BoxedArray\Ch\Elca\Iiop\Demo\StorageSystem\seq1_Entry.idl
	cd ..

build-java: $(StorageSystem)*.java *.java
	javac -classpath . $?

clean:
	if exist Ch\Elca\Iiop\*.java del /q Ch\Elca\Iiop\*.java
	if exist Ch\Elca\Iiop\*.class del /q Ch\Elca\Iiop\*.class
	move $(EntryImpl) tmp.java
	if exist Ch\Elca\Iiop\Demo\StorageSystem\*.java del /q Ch\Elca\Iiop\Demo\StorageSystem\*.java
	if exist Ch\Elca\Iiop\Demo\StorageSystem\*.class del /q Ch\Elca\Iiop\Demo\StorageSystem\*.class
	if exist IDL\nul rmdir /q /s IDL
	if exist org\nul rmdir /q /s org
	if exist *.class del /q *.class
	move tmp.java $(EntryImpl)


