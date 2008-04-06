%define _name	iiop-net

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
%if "%{_vendor}" == "mandriva"
BuildRequires:	mono mono-web
%else
BuildRequires:	mono-core mono-web
%endif
Requires:	mono-web
BuildRoot:	%{_tmppath}/%{name}-%{version}-buildroot

%description
IIOP.NET allows a seamless interoperation between .NET, CORBA and J2EE distributed objects. This is done by incorporating CORBA/IIOP support into .NET, leveraging the remoting framework.

%prep
%setup -q -n IIOPNet
[ -e Key.snk ] || [ ! -e %SOURCE1 ] || gunzip -c < %SOURCE1 > Key.snk

%build
make -f Makefile.mono clean build-base

%install
[ -d %buildroot ] && rm -rf %buildroot
install -d %buildroot%{_bindir}
install -d %buildroot%{_prefix}/lib/%{_name}
install -d %buildroot%{_prefix}/lib/pkgconfig

gacutil -i IIOPChannel/bin/IIOPChannel.dll -f -root %buildroot%{_prefix}/lib -package %{_name}
gacutil -i IDLToCLSCompiler/IDLPreprocessor/bin/IDLPreprocessor.dll -f -root %buildroot%{_prefix}/lib -package %{_name}
install -m755 IDLToCLSCompiler/IDLCompiler/bin/IDLToCLSCompiler.exe %buildroot%{_prefix}/lib/%{_name}/
install -m755 CLSToIDLGenerator/bin/CLSIDLGenerator.exe %buildroot%{_prefix}/lib/%{_name}/

cat > %buildroot%{_bindir}/IDLToCLSCompiler <<EOF
#!/bin/sh
exec %{_bindir}/mono %{_prefix}/lib/%{_name}/IDLToCLSCompiler.exe "\$@"
EOF
chmod 755 %buildroot%{_bindir}/IDLToCLSCompiler

cat > %buildroot%{_bindir}/CLSIDLGenerator <<EOF
#!/bin/sh
exec %{_bindir}/mono %{_prefix}/lib/%{_name}/CLSIDLGenerator.exe "\$@"
EOF
chmod 755 %buildroot%{_bindir}/CLSIDLGenerator

cat > %buildroot%{_prefix}/lib/pkgconfig/%{_name}.pc <<EOF
prefix=%{_prefix}
exec_prefix=%{_exec_prefix}
libdir=\${prefix}/lib

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
%{_prefix}/lib/%{_name}
%{_prefix}/lib/mono/gac/*
%{_prefix}/lib/mono/%{_name}
%{_prefix}/lib/pkgconfig/*

%changelog
* Sun Apr 16 2006 Dirk O. Siebnich <dok@dok-net.net> 1.9.0-1
- created spec file
