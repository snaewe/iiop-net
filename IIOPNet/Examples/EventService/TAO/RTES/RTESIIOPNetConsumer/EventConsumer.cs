using Ch.Elca.Iiop.Idl;
using System.Runtime.Remoting;


namespace RTES {


    [SupportedInterface(typeof(RtecEventComm.PushConsumer))]
    class EchoEventConsumerImpl :System.MarshalByRefObject, RtecEventComm.PushConsumer {
                
        private MainApp.Logger m_writeLog;

        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }

        public EchoEventConsumerImpl(MainApp.Logger logger) {
            m_writeLog = logger;            
        }
        
        public void push(RtecEventComm._Event[] events) {
            lock(this) {
                for(int i=0; i<events.Length; ++i) {
                    string eventData = "";
                    eventData += "type:" + events[i].header.type.ToString() +
                        " source:" + events[i].header.source.ToString();
                    if(events[i].data.any_value.ToString() != "") {
                        eventData += " text : " + events[i].data.any_value.ToString();
                    }                    

                    m_writeLog.Log(eventData);
                }
            }
        }

        public void disconnect_push_consumer() {
            return;
        }
        
    }

}