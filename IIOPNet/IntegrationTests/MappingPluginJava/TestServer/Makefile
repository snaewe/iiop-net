bin         = bin\ 
IDL         = IDL\ 

build: build-server build-stubs build-idl

$(bin):
        if not exist $@nul mkdir $@

build-server: $(bin) *.java Ch\Elca\Iiop\IntegrationTests\MappingPlugin\*.java
    javac -d bin Ch\Elca\Iiop\IntegrationTests\MappingPlugin\*.java
    javac -d bin TestServer.java    
    

build-stubs:
    cd bin
    rmic -iiop Ch.Elca.Iiop.IntegrationTests.MappingPlugin.TestServiceImpl
    cd ..

build-idl:
    if not exist $(IDL) mkdir $(IDL)
    cd bin
    rmic -d ..\$(IDL) -idl Ch.Elca.Iiop.IntegrationTests.MappingPlugin.TestServiceImpl
    rmic -d ..\$(IDL) -idl Ch.Elca.Iiop.IntegrationTests.MappingPlugin.TestSerializableClassB1
    cd ..

clean:
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin
