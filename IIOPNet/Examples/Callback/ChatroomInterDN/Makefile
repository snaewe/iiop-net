include ..\..\..\MakeVars

DELAY = ..\..\..\Utils\delay.exe
PRCURRENTTIME = ..\..\..\Utils\currenttime.exe

build: build-server build-client

build-server:
	cd Server
	$(MAKE) build
	cd ..

build-client:
	cd Client
	$(MAKE) build
	cd ..

start-server:
	@cd Server
	@$(MAKE) /C start-server
	@cd ..

stop-server:
	@cd Server
	@$(MAKE) /C stop-server
	@cd ..	

start-client:
	@cd Client
	@$(MAKE) /C /I start-client
	@cd ..

test:
	@echo Chatroom example test starting at:
	@$(PRCURRENTTIME)
	@$(MAKE) /C start-server
	@$(DELAY) 10
	@$(MAKE) /C start-client
	@$(MAKE) /C stop-server	

clean:
	cd Server
	$(MAKE) clean
	cd ..
	cd Client
	$(MAKE) clean
	cd ..