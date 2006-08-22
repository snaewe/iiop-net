include ..\..\..\MakeVars

DELAY = ..\..\..\Utils\delay.exe

build: build-server build-client

build-server:
	cd TestServer
	$(MAKE) /f Makefile /C build
        cd ..

build-client:
	cd TestClient
	$(MAKE) /f Makefile build
	cd ..

start-server:
	@cd TestServer
	@$(MAKE) /f Makefile /C start-server
	@cd ..

stop-server:
	@cd TestServer
	@$(MAKE) /f Makefile /C stop-server
	@cd ..	

start-client:
	@cd TestClient
	@$(MAKE) /F Makefile /C /I start-client
	@cd ..

test:
	@$(MAKE) /F Makefile /C start-server
	@$(DELAY) 10
	@$(MAKE) /F Makefile /C start-client
	@$(MAKE) /F Makefile /C stop-server

clean:
	cd TestServer
	$(MAKE) /F Makefile /C clean
        cd ..
	cd TestClient
	$(MAKE) /F Makefile clean	
	cd ..
