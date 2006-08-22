include ..\..\..\MakeVars

build: check build-app

check:
	if not defined WAS_HOME echo "Environment variable WAS_HOME (home directory of WebSphere) not defined"
	if not defined WAS_HOME exit 2

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
