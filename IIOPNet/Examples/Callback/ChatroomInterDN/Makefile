build: build-server build-client

build-server:
	cd Server
	$(MAKE) build
	cd ..

build-client:
	cd Client
	$(MAKE) build
	cd ..


clean:
	cd Server
	$(MAKE) clean
	cd ..
	cd Client
	$(MAKE) clean
	cd ..