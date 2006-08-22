include ..\..\..\..\MakeVars

build:
	cd server
	$(MAKE) build
	cd ..
        cd directclient
        $(MAKE) build
        cd ..
        cd idlclient
        $(MAKE) build
        cd ..


clean:
	cd server
	$(MAKE) clean
	cd ..
        cd directClient
        $(MAKE) clean
        cd ..
        cd idlClient
        $(MAKE) clean
        cd ..

start-server:
	cd server
	$(MAKE) start-server
	cd ..

stop-server:
	cd server
	$(MAKE) stop-server
	cd ..
