package java.util;


public class DateHelper {

  private static String  _id = "RMI:java.util.Date:AC117E28FE36587A:686A81014B597419";

  public static String id ()
  {
    return _id;
  }

  public static java.util.Date read (org.omg.CORBA.portable.InputStream istream)
  {
    return (java.util.Date)((org.omg.CORBA_2_3.portable.InputStream) istream).read_value (java.util.Date.class);
  }

  public static void write (org.omg.CORBA.portable.OutputStream ostream, java.util.Date value)
  {
    ((org.omg.CORBA_2_3.portable.OutputStream) ostream).write_value (value, java.util.Date.class);
  }    

}