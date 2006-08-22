include ..\..\..\MakeVars

Test=ch\elca\iiop\integrationTests\ 


build: copy-meta-data build-classes build-jar build-stubs build-idl

copy-meta-data:
	if not exist bin mkdir bin
        if not exist bin\META-INF mkdir bin\META-INF
	if exist *.xml copy *.xml bin\META-INF

build-classes: $(Test)Test.java $(Test)TestHome.java $(Test)TestBean.java $(Test)IntAdder.java $(Test)IntAdderHome.java $(Test)IntAdderBean.java $(Test)TestSerializableClassB1.java $(Test)TestSerializableClassB2.java $(Test)TestSerializableClassC.java $(Test)TestSerializableClassD.java $(Test)TestNonSerializableBaseClass.java
	javac -d bin $?

build-jar:
	cd bin
        jar cv0f wls6integration_ejb.jar META-INF ch
        cd ..

build-stubs:
	java weblogic.ejbc -iiop -compiler javac bin\wls6integration_ejb.jar bin\wls6integration_ejb_app.jar

build-idl:
	java weblogic.ejbc -idl bin\wls6integration_ejb.jar IDL
        cd IDL
        del /s /q *.class
        del /s /q *.xml
	del /s /q META-INF
	rmdir META-INF
        cd ..



clean:
        if exist bin del /s /q bin
	if exist bin rmdir /s /q bin
	if exist IDL del /s /q IDL
	if exist IDL rmdir /s /q IDL
