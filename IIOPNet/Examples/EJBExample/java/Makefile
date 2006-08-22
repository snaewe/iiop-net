include ..\..\..\MakeVars

Adder=ch\elca\iiop\demo\ejbAdder\ 


build: copy-meta-data build-classes build-jar build-stubs build-idl

copy-meta-data:
	if not exist bin mkdir bin
        if not exist bin\META-INF mkdir bin\META-INF
	if exist *.xml copy *.xml bin\META-INF

build-classes: $(Adder)Adder.java $(Adder)AdderHome.java $(Adder)AdderBean.java
	javac -d bin $(Adder)Adder.java $(Adder)AdderHome.java $(Adder)AdderBean.java

build-jar:
	cd bin
        jar cv0f adder_ejb.jar META-INF ch
        cd ..

build-stubs:
	java weblogic.ejbc -iiop -compiler javac bin\adder_ejb.jar bin\adder_ejb_app.jar

build-idl:
	java weblogic.ejbc -idl bin\adder_ejb.jar IDL
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
