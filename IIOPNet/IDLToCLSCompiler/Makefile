include ..\MakeVars

build: 
	cd IDLPreprocessor
	$(MAKE) build
	@cd ..
	
	cd IDLCompiler
	$(MAKE) build
	@cd ..


build-unit-tests:
	cd IDLCompiler
	$(MAKE) build-tests
	cd ..

test:
	cd IDLCompiler
	$(MAKE) test
	cd ..


clean:
	cd IDLCompiler
	$(MAKE) clean
	@cd ..

	cd IDLPreprocessor
	$(MAKE) clean
	@cd ..
	
