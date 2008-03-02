include ..\MakeVars

PRCURRENTTIME = ..\Utils\currenttime.exe


build: nunit-var junit-var
    cd Basic
    $(MAKE) build
    cd ..
    cd TimeoutTest
    $(MAKE) build
    cd ..
    cd InterceptorTests
    $(MAKE) build
    cd ..
    cd Direct
    $(MAKE) build
    cd ..
    cd CodeSets
    $(MAKE) build
    cd ..
    cd Multithread
    $(MAKE) build
    cd ..
    cd Callback
    $(MAKE) build
    cd ..
    cd BidirGiop
    $(MAKE) build
    cd ..
    cd JavaRMIIIOP
    $(MAKE) build
    cd ..
    cd JavaRMIIIOPAsClient
    $(MAKE) build
    cd ..
    cd MappingPluginJava
    $(MAKE) build
    cd ..
    cd MappingPluginJavaAsClient
    $(MAKE) build
    cd ..
    cd JdkOrb
    $(MAKE) build
    cd ..
    cd MappingOptions
    $(MAKE) build
    cd ..
    cd SerializationOptions
    $(MAKE) build
    cd ..
    cd Jboss3.2.1
    @if not defined JBOSS_HOME echo JBOSS_HOME not set, skipping JBOSS integration test
    @if defined JBOSS_HOME $(MAKE) build
    cd ..
    cd Jboss4.0
    @if not defined JBOSS_HOME_4 echo JBOSS_HOME_4 not set, skipping JBOSS 4 integration test
    @if defined JBOSS_HOME_4 $(MAKE) build
    cd ..
    cd MICO
    @if not defined MICO_HOME echo MICO_HOME not set, skipping MICO integration test
    @if defined MICO_HOME $(MAKE) /F Makefile.Win32 /C build
    cd ..
    cd OmniORB
    @if not defined OMNIORB_HOME echo OMNIORB_HOME not set, skipping OmniORB integration test
    @if defined OMNIORB_HOME $(MAKE) /C build
    cd ..
#   cd JacORB
#   @if not defined JACORB_HOME echo JACORB_HOME not set, skipping JACORB integration test
#   @if defined JACORB_HOME $(MAKE) /C build
#   cd ..
    cd GacTest
    $(MAKE) build
    cd ..
    cd WAS5
    @if not defined WAS_HOME echo WAS_HOME not set, skipping WAS5 integration test
    @if defined WAS_HOME $(MAKE) build
    cd ..
    cd WAS5_1
    @if not defined WAS5_1_HOME echo WAS5_1_HOME not set, skipping WAS5_1 integration test
    @if defined WAS5_1_HOME $(MAKE) build
    cd ..

build-ssl:
    cd JBossSsl3.2.5
    @if not defined JBOSS_HOME_3_2_5 echo JBOSS_HOME_3_2_5 not set, skipping JBOSS SSL integration test
    @if not defined JAVA_HOME echo JAVA_HOME not set, skipping JBOSS SSL integration test
    @if defined JBOSS_HOME_3_2_5 @if defined JAVA_HOME $(MAKE) /C build-with-setup 
    cd ..
    cd DotnetDotNetSsl
    $(MAKE) build
    cd ..


test: nunit-var junit-var
    @echo starting integration tests at: 
    @$(PRCURRENTTIME)
    cd Basic
    $(MAKE) test
    cd ..
    cd TimeoutTest
    $(MAKE) test
    cd ..
    cd InterceptorTests
    $(MAKE) test
    cd ..
    cd Direct
    $(MAKE) test
    cd ..
    cd CodeSets
    $(MAKE) test
    cd ..
    cd Callback
    $(MAKE) test
    cd ..
    cd BidirGiop
    $(MAKE) test
    cd ..
    cd Multithread
    $(MAKE) test
    cd ..
    cd JavaRMIIIOP
    $(MAKE) test
    cd ..
    cd JavaRMIIIOPAsClient
    $(MAKE) test
    cd ..
    cd MappingPluginJava
    $(MAKE) test
    cd ..
    cd MappingPluginJavaAsClient
    $(MAKE) test
    cd ..
    cd JdkOrb
    $(MAKE) test
    cd ..
    cd MappingOptions
    $(MAKE) test
    cd ..
    cd SerializationOptions
    $(MAKE) test
    cd ..
    cd Jboss3.2.1
    @if not defined JBOSS_HOME echo JBOSS_HOME not set, skipping JBOSS integration test
    @if defined JBOSS_HOME $(MAKE) test
    cd ..
    cd Jboss4.0
    @if not defined JBOSS_HOME_4 echo JBOSS_HOME_4 not set, skipping JBOSS 4 integration test
    @if defined JBOSS_HOME_4 $(MAKE) test
    cd ..
    cd MICO
    @if not defined MICO_HOME echo MICO_HOME not set, skipping MICO integration test
    @if defined MICO_HOME $(MAKE) /F Makefile.Win32 /C test
    cd ..   
    cd OmniORB
    @if not defined OMNIORB_HOME echo OMNIORB_HOME not set, skipping OmniORB integration test
    @if defined OMNIORB_HOME $(MAKE) /C test
    cd ..
    cd GacTest
    @if not defined SKIP_IIOP_NET_GAC_TESTS $(MAKE) test
    cd ..
    cd WAS5
    @if not defined WAS_HOME echo WAS_HOME not set, skipping WAS5 integration test
    @if defined WAS_HOME $(MAKE) test
    cd ..
    cd WAS5_1
    @if not defined WAS5_1_HOME echo WAS5_1_HOME not set, skipping WAS5_1 integration test
    @if defined WAS5_1_HOME $(MAKE) test
    cd ..

test-ssl:
    cd JBossSsl3.2.5
    @if not defined JBOSS_HOME_3_2_5 echo JBOSS_HOME_3_2_5 not set, skipping JBOSS SSL integration test
    @if not defined JAVA_HOME echo JAVA_HOME not set, skipping JBOSS SSL integration test
    @if defined JBOSS_HOME_3_2_5 @if defined JAVA_HOME $(MAKE) test
    cd ..
    cd DotnetDotNetSsl
    $(MAKE) test
    cd ..    

clean:
    cd Basic
    $(MAKE) clean
    cd ..
    cd InterceptorTests
    $(MAKE) clean
    cd ..
    cd Direct
    $(MAKE) clean
    cd ..
    cd CodeSets
    $(MAKE) clean
    cd ..
    cd Callback
    $(MAKE) clean
    cd ..
    cd BidirGiop
    $(MAKE) clean
    cd ..
    cd Multithread
    $(MAKE) clean
    cd ..
    cd JavaRMIIIOP
    $(MAKE) clean
    cd ..
    cd JavaRMIIIOPAsClient
    $(MAKE) clean
    cd ..
    cd MappingPluginJava
    $(MAKE) clean
    cd ..
    cd MappingPluginJavaAsClient
    $(MAKE) clean
    cd ..
    cd JdkOrb
    $(MAKE) clean
    cd ..
    cd MappingOptions
    $(MAKE) clean
    cd ..
    cd SerializationOptions
    $(MAKE) clean
    cd ..
    cd Jboss3.2.1
    $(MAKE) clean
    cd ..
    cd Jboss4.0
    $(MAKE) clean
    cd ..
    cd WAS5
    $(MAKE) clean
    cd ..
    cd WAS5_1
    $(MAKE) clean
    cd ..
    cd MICO
    $(MAKE) /F Makefile.Win32 /C clean
    cd ..
    cd OmniORB
    $(MAKE) /C clean
    cd ..
#   cd JacORB
#   $(MAKE) /C clean
#   cd ..
    cd JBossSsl3.2.5
    $(MAKE) /C clean
    cd ..
    cd GacTest
    $(MAKE) clean
    cd ..
    cd DotnetDotNetSsl
    $(MAKE) clean
    cd ..    

