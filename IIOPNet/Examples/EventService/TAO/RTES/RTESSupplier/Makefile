include ..\..\..\..\..\MakeVars

build: EventSupplierExe

#----------------------------------------------------------------------------
# CosEventExample/Makefile
#----------------------------------------------------------------------------

# This example requires exceptions=1.
exceptions=1

.cpp.obj:
  cl /nologo /c $(CXXFLAGS) /Tp$<


#----------------------------------------------------------------------------
#   Local macros
#----------------------------------------------------------------------------

TAO_ROOT=$(ACE_ROOT)\TAO

# This generates the names of all the header and source files to build.
DEFS  = EchoEventConsumer_i.h EchoEventSupplier_i.h

SRC   = EchoEventSupplierMain.cpp \
        EchoEventSupplier_i.cpp   \
        EchoEventConsumerMain.cpp \
        EchoEventConsumer_i.cpp

# List the object files
FILE_SUP_OBJS = EchoEventSupplierMain.obj EchoEventSupplier_i.obj

# This defines the binary executable targets to build.
BIN = EchoEventConsumer EchoEventSupplier

#----------------------------------------------------------------------------
#       Include macros and targets
#----------------------------------------------------------------------------

CXXFLAGS = -I$(TAO_ROOT) -I$(TAO_ROOT)\orbsvcs -I$(ACE_ROOT) -DWIN32 /EHsc /MD /Fp


TAO_SUP_LIBS = TAO_PortableServerd.lib aced.lib taod.lib TAO_CosNamingd.lib TAO_RTEventd.lib

LDFLAGS = -libpath:$(TAO_ROOT)\tao -libpath:$(ACE_ROOT)\ace -libpath:$(TAO_ROOT)\orbsvcs\orbsvcs -libpath:$(TAO_ROOT)\tao\PortableServer


.SUFFIXES: .cpp

EventServiceSupplier: EvenServiceSupplierObj EventSupplierExe

EvenServiceSupplierObj: $(FILE_SUP_OBJS)

EventSupplierExe: EchoEventSupplier.exe

EchoEventSupplier.exe: $(FILE_SUP_OBJS)
	link $(LDFLAGS) /out:$@ $(TAO_SUP_LIBS) $(FILE_SUP_OBJS)


clean:
	if exist *.obj del /q *.obj
	if exist *.exe del /q del *.exe
    
    
