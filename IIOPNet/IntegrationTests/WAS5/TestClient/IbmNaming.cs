using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Services;

namespace com.ibm.WsnOptimizedNaming {


    [RepositoryIDAttribute("IDL:com.ibm/WsnOptimizedNaming/NamingContext:1.0")]
    [InterfaceTypeAttribute(IdlTypeInterface.ConcreteInterface)]
    public interface NamingContext : omg.org.CosNaming.NamingContext, IIdlEntity {
    }

}