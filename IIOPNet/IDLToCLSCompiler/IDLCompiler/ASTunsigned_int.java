/* Generated By:JJTree: Do not edit this line. ASTunsigned_int.java */

package parser;

public class ASTunsigned_int extends SimpleNode {
  public ASTunsigned_int(int id) {
    super(id);
  }

  public ASTunsigned_int(IDLParser p, int id) {
    super(p, id);
  }


  /** Accept the visitor. **/
  public Object jjtAccept(IDLParserVisitor visitor, Object data) {
    return visitor.visit(this, data);
  }
}
