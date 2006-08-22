include ..\..\..\..\MakeVars

Chatroom=ch\elca\iiop\demo\ejbChatroom\ 


build: copy-meta-data build-classes build-jar build-stubs build-idl

copy-meta-data:
	if not exist bin mkdir bin
        if not exist bin\META-INF mkdir bin\META-INF
	if exist *.xml copy *.xml bin\META-INF

build-classes: $(Chatroom)Chatroom.java $(Chatroom)ChatroomHome.java $(Chatroom)ChatroomBean.java $(Chatroom)ChatroomServer.java $(Chatroom)Message.java $(Chatroom)MessageListener.java $(Chatroom)NotRegisteredException.java $(Chatroom)AlreadyRegisteredException.java
	javac -d bin $?

build-jar:
	cd bin
        jar cv0f chatroom_ejb.jar META-INF ch
        cd ..

build-stubs:
	java weblogic.ejbc -iiop -compiler javac bin\chatroom_ejb.jar bin\chatroom_ejb_app.jar

build-idl:
	java weblogic.ejbc -idl bin\chatroom_ejb.jar IDL
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
