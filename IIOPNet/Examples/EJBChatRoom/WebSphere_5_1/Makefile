include ..\..\..\MakeVars

build: check build-app

check:
	if not defined WAS5_1_HOME echo "Environment variable WAS5_1_HOME (home directory of WebSphere) not defined"
	if not defined WAS5_1_HOME exit 2

build-app:
	cd java
	$(MAKE) build
	cd ..\net
	$(MAKE) build
	cd ..

clean:
	cd java
	$(MAKE) clean
	cd ..\net
	$(MAKE) clean
	cd ..

start-server:
        cd java
        $(MAKE) start-server
        cd ..

stop-server:
        cd java
        $(MAKE) stop-server
        cd ..

start-client:
        cd net
        $(MAKE) start-client
        cd ..
