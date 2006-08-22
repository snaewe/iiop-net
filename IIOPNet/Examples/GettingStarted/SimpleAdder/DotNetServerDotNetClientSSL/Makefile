include ..\..\..\..\MakeVars

build:
	cd server
	$(MAKE) build
	cd ..
        cd client
        $(MAKE) build
        cd ..


clean:
	cd server
	$(MAKE) clean
	cd ..
        cd client
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
