include ..\..\MakeVars

DELAY = ..\..\Utils\delay.exe
PRCURRENTTIME = ..\..\Utils\currenttime.exe

build: build-server build-client

build-server:
	cd TestServer
	$(MAKE) build
        cd ..

build-client:
	cd TestClient
	$(MAKE) build	
	cd ..

start-server:
	@cd TestServer
	@$(MAKE) /C start-server
	@cd ..

stop-server:
	@cd TestServer
	@$(MAKE) /C stop-server
	@cd ..	

start-client:
	@cd TestClient
	@$(MAKE) /C /I start-client
	@cd ..

test:
        @echo Java RMI/IIOP as client integration test starting at:
        @$(PRCURRENTTIME)
	@$(MAKE) /C start-server
	@$(DELAY) 10
	@$(MAKE) /C start-client
	@$(MAKE) /C stop-server

clean:
	cd TestServer
	$(MAKE) clean
        cd ..
	cd TestClient
	$(MAKE) clean	
	cd ..
