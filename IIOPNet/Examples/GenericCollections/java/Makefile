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
	idlj -td .. Predef.idl
	idlj -td .. org\omg\BoxedArray\Ch\Elca\Iiop\Demo\StorageSystem\seq1_Container.idl
	idlj -td .. org\omg\BoxedArray\Ch\Elca\Iiop\Demo\StorageSystem\seq1_Entry.idl
	cd ..

build-java: $(StorageSystem)*.java *.java
	javac -classpath . $?

clean:
	move $(EntryImpl) tmp.java
	if exist IDL\nul rmdir /q /s IDL
	if exist  Ch\nul rmdir /q /s Ch
	if exist org\nul rmdir /q /s org
	if exist *.class del /s *.class
	mkdir $(StorageSystem)
	move tmp.java $(EntryImpl)


