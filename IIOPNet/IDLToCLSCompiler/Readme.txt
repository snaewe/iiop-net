This directory contains the source code for the IDL to CLS Compiler.

This directory is structured in the following way:
- IDLCompiler subdirectory contains most of the compiler source code. The Compiler uses the IDLProcessor to accomplish
  it's work.
- IDLPreporcessor subdirectory contains the IDL preprocessor
- IDLParser subdirectory contains the javaCC file for generating the IDL-Parser.
- IDL subdirectory contains the orb.idl, which should be used together with the IDLToCompiler
