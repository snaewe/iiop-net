/* Generated By:JJTree: Do not edit this line. ASTwide_string_type.cs */

using System;

namespace parser {

public class ASTwide_string_type : SimpleNode {
  public ASTwide_string_type(int id) : base(id) {
  }

  public ASTwide_string_type(IDLParser p, int id) : base(p, id) {
  }


  /** Accept the visitor. **/
  public override Object jjtAccept(IDLParserVisitor visitor, Object data) {
    return visitor.visit(this, data);
  }
  
  public override string GetIdentification() {
    return "wstring";
  }
}


}

