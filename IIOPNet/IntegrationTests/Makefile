
build: nunit-var junit-var
    cd Utils
    $(MAKE) build
    cd ..
    cd Basic
    $(MAKE) build
    cd ..
        cd Direct
    $(MAKE) build
        cd ..
        cd Multithread
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
    cd Jboss3.2.1
    @if not defined JBOSS_HOME echo JBOSS_HOME not set, skipping JBOSS integration test
    @if defined JBOSS_HOME $(MAKE) build
    cd ..
    cd WAS5
    @if not defined WAS_HOME echo WAS_HOME not set, skipping WAS5 integration test
    @if defined WAS_HOME $(MAKE) build
    cd ..
    cd WAS5_1
    @if not defined WAS5_1_HOME echo WAS5_1_HOME not set, skipping WAS5_1 integration test
    @if defined WAS5_1_HOME $(MAKE) build
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

build-ssl:
    cd JBossSsl3.2.5
    @if not defined JBOSS_HOME_3_2_5 echo JBOSS_HOME_3_2_5 not set, skipping JBOSS SSL integration test
    @if not defined JAVA_HOME echo JAVA_HOME not set, skipping JBOSS SSL integration test
    @if defined JBOSS_HOME_3_2_5 @if defined JAVA_HOME $(MAKE) /C build-with-setup 
    cd ..


nunit-var:        
    @if not defined NUNITV2_HOME echo Set the NUNITV2_HOME variable to the NUnit V2 home directory. Please check TestInstructions.txt.
    @if not defined NUNITV2_HOME exit 2

junit-var:
    @if not defined JUNIT_HOME echo set the JUNIT_HOME variable to the JUNIT home directory. Please check TestInstructions.txt.
    @if not defined JUNIT_HOME exit 2


test: nunit-var junit-var
    cd Basic
    $(MAKE) test
    cd ..
        cd Direct
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
    cd Jboss3.2.1
    @if not defined JBOSS_HOME echo JBOSS_HOME not set, skipping JBOSS integration test
    @if defined JBOSS_HOME $(MAKE) test
    cd ..
    cd WAS5
    @if not defined WAS_HOME echo WAS_HOME not set, skipping WAS5 integration test
    @if defined WAS_HOME $(MAKE) test
    cd ..
    cd WAS5_1
    @if not defined WAS5_1_HOME echo WAS5_1_HOME not set, skipping WAS5_1 integration test
    @if defined WAS5_1_HOME $(MAKE) test
    cd ..
    cd MICO
    @if not defined MICO_HOME echo MICO_HOME not set, skipping MICO integration test
    @if defined MICO_HOME $(MAKE) /F Makefile.Win32 /C test
    cd ..   
    cd OmniORB
    @if not defined OMNIORB_HOME echo OMNIORB_HOME not set, skipping OmniORB integration test
    @if defined OMNIORB_HOME $(MAKE) /C test
    cd ..

test-ssl:
    cd JBossSsl3.2.5
    @if not defined JBOSS_HOME_3_2_5 echo JBOSS_HOME_3_2_5 not set, skipping JBOSS SSL integration test
    @if not defined JAVA_HOME echo JAVA_HOME not set, skipping JBOSS SSL integration test
    @if defined JBOSS_HOME_3_2_5 @if defined JAVA_HOME $(MAKE) test
    cd ..
    

clean:
    cd Utils
    $(MAKE) clean
    cd ..
    cd Basic
    $(MAKE) clean
    cd ..
        cd Direct
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
    cd Jboss3.2.1
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
