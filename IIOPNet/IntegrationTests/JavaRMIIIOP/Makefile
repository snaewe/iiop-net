build: build-server build-client

build-server:
	cd TestServer
	$(MAKE) build
        cd ..

build-client:
	cd TestClient
	$(MAKE) build	
	cd ..


clean:
	cd TestServer
	$(MAKE) clean
        cd ..
	cd TestClient
	$(MAKE) clean	
	cd ..
