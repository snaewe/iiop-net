DELAY = ..\Utils\delay.exe

setup:
	cd TestBeans
	$(MAKE) setup
	cd ..

undo-setup:
	cd TestBeans
	$(MAKE) undo-setup
	cd ..


build:
	cd TestBeans
	$(MAKE) build
	cd ..\TestClientForJava1.4
	$(MAKE) build
	cd ..

start-server:
        @cd TestBeans
        @$(MAKE) /C start-server
        @cd ..

stop-server:
        @cd TestBeans
        @$(MAKE) /C stop-server
        @cd ..

start-client:
        @cd TestClientForJava1.4
        @$(MAKE) /C /I start-client
        @cd ..

test:
        @$(MAKE) /C start-server
        @$(DELAY) 150
        @$(MAKE) /C start-client
        @$(MAKE) /C stop-server

clean:
	cd TestBeans
	$(MAKE) clean
	cd ..\TestClientForJava1.4
	$(MAKE) clean
	cd ..
