include ..\..\..\MakeVars

build:
	cd java
	$(MAKE) build
	cd ..\netForJava1.4
	$(MAKE) build
	cd ..

clean:
	cd java
	$(MAKE) clean
	cd ..\netForJava1.4
	$(MAKE) clean
	cd ..

start-server:
        cd java
        $(MAKE) start-server
        cd ..

start-client:
        cd netForJava1.4
        $(MAKE) start-client
        cd ..
