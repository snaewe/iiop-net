%define _name	iiop-net

%if "%{_vendor}" == "suse"
%define _lib	lib
%define _libdir	%{_prefix}/lib
%endif

Summary:	Seamless interoperation between .NET and CORBA
Name:		%{_name}
Version:	1.9.0
Release:	1
License:	GPL/LGPL
Group:		System Environment/Libraries
Source0:	IIOPNet.src.%{version}.cvs.tar.gz
Source1:	IIOPNet.Key.snk.gz
URL:		http://www.iiop.net/
BuildArch:	noarch
BuildRequires:	mono-core mono-web mono-devel
BuildRequires:	pkgconfig
Requires:	mono-web
BuildRoot:	%{_tmppath}/%{name}-%{version}-buildroot

%description
IIOP.NET allows a seamless interoperation between .NET, CORBA and J2EE distributed objects. This is done by incorporating CORBA/IIOP support into .NET, leveraging the remoting framework.

%package -n %{_name}-devel
Summary:	Pkgconfig file needed for %{_name} development
Group:		Development/Libraries
Requires:	%{_name} = %{version}-%{release}

%description -n %{_name}-devel
IIOP.NET allows a seamless interoperation between .NET, CORBA and J2EE distributed objects. This is done by incorporating CORBA/IIOP support into .NET, leveraging the remoting framework.
This package includes the pkgconfig file needed to develop
components and programs using %{_name}.

%prep
%setup -q -n IIOPNet
[ -e Key.snk ] || [ ! -e %SOURCE1 ] || gunzip -c < %SOURCE1 > Key.snk

%build
make -f Makefile.mono clean build-base

%install
[ -d %buildroot ] && rm -rf %buildroot
mkdir %buildroot
install -d %buildroot%{_bindir}
install -d %buildroot%{_prefix}/%{_lib}/%{_name}
install -d %buildroot%{_datadir}/pkgconfig

gacutil -i IIOPChannel/bin/IIOPChannel.dll -f -root %buildroot%{_prefix}/%{_lib} -package %{_name}
gacutil -i IDLToCLSCompiler/IDLPreprocessor/bin/IDLPreprocessor.dll -f -root %buildroot%{_prefix}/%{_lib} -package %{_name}
install -m755 IDLToCLSCompiler/IDLCompiler/bin/IDLToCLSCompiler.exe %buildroot%{_prefix}/%{_lib}/%{_name}/
install -m755 CLSToIDLGenerator/bin/CLSIDLGenerator.exe %buildroot%{_prefix}/%{_lib}/%{_name}/

cat > %buildroot%{_bindir}/IDLToCLSCompiler <<EOF
#!/bin/sh
exec %{_bindir}/mono %{_prefix}/%{_lib}/%{_name}/IDLToCLSCompiler.exe "\$@"
EOF
chmod 755 %buildroot%{_bindir}/IDLToCLSCompiler

cat > %buildroot%{_bindir}/CLSIDLGenerator <<EOF
#!/bin/sh
exec %{_bindir}/mono %{_prefix}/%{_lib}/%{_name}/CLSIDLGenerator.exe "\$@"
EOF
chmod 755 %buildroot%{_bindir}/CLSIDLGenerator

cat > %buildroot%{_datadir}/pkgconfig/%{_name}.pc <<EOF
prefix=%{_prefix}
exec_prefix=%{_exec_prefix}
libdir=\${prefix}/%{_lib}

Name: IIOP.NET
Description: Seamless interoperation between .NET and CORBA
Version: %{version}
Requires:
Libs: -lib:\${libdir}/mono/%{_name} -r:IIOPChannel.dll
EOF

[ -e %SOURCE1 ] || gzip -c < Key.snk > %SOURCE1

%clean
[ -z %buildroot ] || rm -rf %buildroot

%files
%defattr (-,root,root)
%doc Doc/*
%{_bindir}/*
%{_prefix}/%{_lib}/%{_name}
%{_prefix}/%{_lib}/mono/gac/*
%{_prefix}/%{_lib}/mono/%{_name}

%files -n %{_name}-devel
%defattr(-,root,root)
%{_datadir}/pkgconfig/*

%changelog
* Sun Apr 16 2006 Dirk O. Siebnich <dok@dok-net.net> 1.9.0-1
- created spec file
