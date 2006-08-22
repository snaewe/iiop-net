include ..\..\..\..\MakeVars

build:
	cd netServer
	$(MAKE) build
	cd ..
        cd omniOrbClient
        $(MAKE) build
        cd ..

clean:
	cd netServer
	$(MAKE) clean
	cd ..
        cd omniOrbClient
        $(MAKE) clean
        cd ..

start-server:
	cd netServer
	$(MAKE) start-server
 	cd ..
 
stop-server:
 	cd netServer
 	$(MAKE) stop-server
 	cd ..
