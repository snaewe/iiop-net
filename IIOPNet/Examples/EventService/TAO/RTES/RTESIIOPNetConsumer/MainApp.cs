using System;
using System.Threading;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

using System.Text;

using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.CorbaObjRef;
using omg.org.CORBA;
using omg.org.CosNaming;

namespace RTES {

	/// <summary>
	/// Form1에 대한 요약 설명입니다.
	/// </summary>
	public class MainApp : System.Windows.Forms.Form {

		#region Types


			internal class Logger {

				#region IFields
	
				private MainApp m_app;
				private Writer m_logMethod;

				#endregion IFields
				#region IConstructors
		
				public Logger(MainApp app, Writer logMethod) {
					m_app = app;
					m_logMethod = logMethod;
		                }

				#endregion IConstructors
				#region IMethods
	
				public void Log(string msg) {
					m_app.BeginInvoke(m_logMethod, new object[] { msg });
				}

				#endregion IMethods

			}


		#endregion Types


		private System.Windows.Forms.ListBox txtEventLog;
		private System.Windows.Forms.Button BtnListen;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtHost;
		private System.Windows.Forms.TextBox txtHostPort;
		private System.Windows.Forms.Label label3;
		private bool Run = false;
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button BtnEnd;
		private Thread EventThread;

		private Logger m_logger;

		public MainApp()
		{
			//
			// Windows Form 디자이너 지원에 필요합니다.
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent를 호출한 다음 생성자 코드를 추가합니다.
			//
			Writer internalEventLogWriter = new Writer(this.WriteEventLogInternal);
			m_logger = new Logger(this, internalEventLogWriter);
		}

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form 디자이너에서 생성한 코드
		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다.
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
		/// </summary>
		private void InitializeComponent()
		{
			this.txtEventLog = new System.Windows.Forms.ListBox();
			this.BtnListen = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.txtHost = new System.Windows.Forms.TextBox();
			this.txtHostPort = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.BtnEnd = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txtEventLog
			// 
			this.txtEventLog.Location = new System.Drawing.Point(8, 40);
			this.txtEventLog.Name = "txtEventLog";
			this.txtEventLog.Size = new System.Drawing.Size(504, 312);
			this.txtEventLog.TabIndex = 0;
			this.txtEventLog.Text = "";
			// 
			// BtnListen
			// 
			this.BtnListen.Location = new System.Drawing.Point(360, 8);
			this.BtnListen.Name = "BtnListen";
			this.BtnListen.TabIndex = 1;
			this.BtnListen.Text = "Start";
			this.BtnListen.Click += new System.EventHandler(this.BtnListen_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(14, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(34, 16);
			this.label1.TabIndex = 4;
			this.label1.Text = "Host";
			// 
			// txtHost
			// 
			this.txtHost.Location = new System.Drawing.Point(56, 8);
			this.txtHost.Name = "txtHost";
			this.txtHost.Size = new System.Drawing.Size(144, 21);
			this.txtHost.TabIndex = 6;
			this.txtHost.Text = "localhost";
			// 
			// txtHostPort
			// 
			this.txtHostPort.Location = new System.Drawing.Point(248, 8);
			this.txtHostPort.Name = "txtHostPort";
			this.txtHostPort.Size = new System.Drawing.Size(104, 21);
			this.txtHostPort.TabIndex = 8;
			this.txtHostPort.Text = "12345";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(208, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(34, 16);
			this.label3.TabIndex = 7;
			this.label3.Text = "Port";
			// 
			// BtnEnd
			// 
			this.BtnEnd.Location = new System.Drawing.Point(440, 8);
			this.BtnEnd.Name = "BtnEnd";
			this.BtnEnd.TabIndex = 9;
			this.BtnEnd.Text = "End";
			this.BtnEnd.Click += new System.EventHandler(this.BtnEnd_Click);
			// 
			// MainApp
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(528, 357);
			this.Controls.Add(this.BtnEnd);
			this.Controls.Add(this.txtHostPort);
			this.Controls.Add(this.txtHost);
			this.Controls.Add(this.txtEventLog);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.BtnListen);
			this.Name = "MainApp";
			this.Text = "Real Time Event Service Test";
			this.Closed += new System.EventHandler(this.MainApp_Closed);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// 해당 응용 프로그램의 주 진입점입니다.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainApp());
		}

		#region IMethods


		private void BtnListen_Click(object sender, System.EventArgs e)
		{
			BtnListen.Enabled = false;
			EventThread = new Thread(new ThreadStart(StartListening));
			EventThread.Start();
		}

		public delegate void Writer(string Msg);

		private RtecEventChannelAdmin.EventChannel ResolveEventChannel(NamingContext nameService) {
			NameComponent[] name = new NameComponent[] { new NameComponent("EventService", "") };

			//Downcast the object reference to an EventChannel reference
			RtecEventChannelAdmin.EventChannel ec =
					(RtecEventChannelAdmin.EventChannel)nameService.resolve(name);				
			return ec;			
		}


                private RtecEventComm.PushConsumer RegisterConsumer(RtecEventChannelAdmin.ProxyPushSupplier supplier) {				
			EchoEventConsumerImpl servant = 
				new EchoEventConsumerImpl(m_logger);

			string objectURI = "consumer";
                        RemotingServices.Marshal(servant, objectURI);

			RtecEventComm.PushConsumer consumer = (RtecEventComm.PushConsumer)servant;
	
			// ------------------------------------------------------
			// Connect as a consumer
			///------------------------------------------------------
			RtecEventChannelAdmin.ConsumerQOS qos = new RtecEventChannelAdmin.ConsumerQOS();
			qos.is_gateway = false;
			qos.dependencies = new RtecEventChannelAdmin.Dependency[2];
				
			qos.dependencies[0] = new RtecEventChannelAdmin.Dependency();
			qos.dependencies[0].@event = new RtecEventComm._Event();
			qos.dependencies[0].@event.data = new RtecEventData();
			qos.dependencies[0].@event.data.any_value = 0;				
			qos.dependencies[0].@event.data.pad1 = 0;				
			qos.dependencies[0].@event.data.payload = new byte[1];
			qos.dependencies[0].@event.header = new RtecEventComm.EventHeader();
			qos.dependencies[0].@event.header.type = 9;
			qos.dependencies[0].@event.header.source = 0;
			qos.dependencies[0].rt_info = 0;

			qos.dependencies[1] = new RtecEventChannelAdmin.Dependency();
			qos.dependencies[1].@event = new RtecEventComm._Event();
			qos.dependencies[1].@event.data = new RtecEventData();
			qos.dependencies[1].@event.data.any_value = 0;
			//qos.Dependencies[1].Event.Data.AnyValue.InsertLong(0);
			qos.dependencies[1].@event.data.pad1 = 0;				
			qos.dependencies[1].@event.data.payload = new byte[1];
			qos.dependencies[1].@event.header = new RtecEventComm.EventHeader();
			qos.dependencies[1].@event.header.type = 17;
			qos.dependencies[1].@event.header.source = 1;
			qos.dependencies[1].rt_info = 0;					

			supplier.connect_push_consumer(consumer, qos);
			return consumer;
    
                }


		private void StartListening() {
			IiopChannel channel = null;
			RtecEventChannelAdmin.ProxyPushSupplier supplier = null;
			RtecEventComm.PushConsumer consumer = null;

			try {
				string host = txtHost.Text;
				int port = Convert.ToInt32(txtHostPort.Text);
				m_logger.Log("HostName : " + host);						
				m_logger.Log("Port : " + port.ToString());

				IDictionary property = new Hashtable();
				channel = new IiopChannel(0);
				
				ChannelServices.RegisterChannel(channel, false);
				CorbaInit init = CorbaInit.GetInit();
				NamingContext nameService = (NamingContext)RemotingServices.Connect(typeof(NamingContext),
					String.Format("corbaloc:iiop:{0}:{1}/NameService", host, port));

				RtecEventChannelAdmin.EventChannel ec = ResolveEventChannel(nameService);
				if(ec != null) {
					m_logger.Log("Found the EchoEventChannel");

					//Obtain a reference to the consumer administration object
					RtecEventChannelAdmin.ConsumerAdmin admin = ec.for_consumers();
	        			// Obtain a reference to the push supplier proxy.
					supplier = admin.obtain_push_supplier();
					consumer = RegisterConsumer(supplier);
				
                
					m_logger.Log("Ready to Receive Messages...");				
					this.Run = true;
					while(this.Run)	{
						Thread.Sleep(1);
					}
				} else {
					m_logger.Log("Not Found the EchoEventChannel");

				}
			} catch(Exception exception) {
				m_logger.Log(exception.Message);
				BtnListen.Enabled = true;
			} finally {

				if (supplier != null) {
					supplier.disconnect_push_supplier();
				}
				if (consumer != null) {
					consumer.disconnect_push_consumer();
					RemotingServices.Disconnect((MarshalByRefObject)consumer);
				}
				ChannelServices.UnregisterChannel(channel);
				m_logger.Log("Cleanup event consumer complete.");
			}

		}

		private void WriteEventLogInternal(string Msg) {
			txtEventLog.Items.Add(Msg);
			txtEventLog.TopIndex = (txtEventLog.Items.Count <= 10 ? 10 : txtEventLog.Items.Count - 10);
		}

		private void BtnEnd_Click(object sender, System.EventArgs e) {
			BtnListen.Enabled = true;
			this.Run = false;
		}

		private void MainApp_Closed(object sender, System.EventArgs e) {
			if ((EventThread != null) && (EventThread.ThreadState == ThreadState.Running)) {
				EventThread.Abort();
			}
		}

		#endregion IMethods

	}



}
