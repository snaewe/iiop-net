using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.Remoting;


namespace ch.elca.iiop.demo.ejbChatroom {

    /// <summary>
    /// The chat client form.
    /// </summary>
    public class Chatform : System.Windows.Forms.Form {

        /// <summary>
        /// used for update message received list
        /// <summary>
        public delegate void UpdateInvoker(Message newMessage);

        #region Constants

        private const string CONNECTED_INFO = "connected";
        private const string NOT_CONNECTED_INFO = "not connected";


        #endregion Constants
        #region IFields

        private System.Windows.Forms.TextBox m_usernameTextbox;
        private System.Windows.Forms.Label m_userNameLabel;
        private System.Windows.Forms.TextBox m_messageTextbox;
        private System.Windows.Forms.Label m_messageLabel;
        private System.Windows.Forms.ListBox m_messages;
        private System.Windows.Forms.Button m_sendMsgButton;
        private System.Windows.Forms.Button m_sendMsgAsyncButton;
        private System.Windows.Forms.Panel m_userNamePanel;
        private System.Windows.Forms.Panel m_sendMsgPanel;
        private System.Windows.Forms.Button m_connectButton;
        private System.Windows.Forms.Button m_disconnectbutton;
        private System.Windows.Forms.Label m_statusLabel;
        private System.Windows.Forms.Label m_constatus;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        private Chatroom m_chatroom = null;
        private MessageListenerImpl m_listener = null;

        #endregion IFields

		public Chatform(Chatroom chatroom) {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			            
                        m_chatroom = chatroom;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing) {
			if( disposing )	{
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.m_usernameTextbox = new System.Windows.Forms.TextBox();
            this.m_userNameLabel = new System.Windows.Forms.Label();
            this.m_messageTextbox = new System.Windows.Forms.TextBox();
            this.m_messageLabel = new System.Windows.Forms.Label();
            this.m_messages = new System.Windows.Forms.ListBox();
            this.m_userNamePanel = new System.Windows.Forms.Panel();
            this.m_constatus = new System.Windows.Forms.Label();
            this.m_statusLabel = new System.Windows.Forms.Label();
            this.m_disconnectbutton = new System.Windows.Forms.Button();
            this.m_connectButton = new System.Windows.Forms.Button();
            this.m_sendMsgPanel = new System.Windows.Forms.Panel();
            this.m_sendMsgButton = new System.Windows.Forms.Button();
            this.m_sendMsgAsyncButton = new System.Windows.Forms.Button();
            this.m_userNamePanel.SuspendLayout();
            this.m_sendMsgPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_usernameTextbox
            // 
            this.m_usernameTextbox.Location = new System.Drawing.Point(120, 16);
            this.m_usernameTextbox.MaxLength = 50;
            this.m_usernameTextbox.Name = "m_usernameTextbox";
            this.m_usernameTextbox.Size = new System.Drawing.Size(408, 20);
            this.m_usernameTextbox.TabIndex = 0;
            this.m_usernameTextbox.Text = "";
            this.m_usernameTextbox.WordWrap = false;
            // 
            // m_userNameLabel
            // 
            this.m_userNameLabel.Location = new System.Drawing.Point(8, 16);
            this.m_userNameLabel.Name = "m_userNameLabel";
            this.m_userNameLabel.Size = new System.Drawing.Size(100, 16);
            this.m_userNameLabel.TabIndex = 1;
            this.m_userNameLabel.Text = "User name:";
            // 
            // m_messageTextbox
            // 
            this.m_messageTextbox.Location = new System.Drawing.Point(16, 48);
            this.m_messageTextbox.Name = "m_messageTextbox";
            this.m_messageTextbox.Size = new System.Drawing.Size(520, 20);
            this.m_messageTextbox.TabIndex = 2;
            this.m_messageTextbox.Text = "";
            // 
            // m_messageLabel
            // 
            this.m_messageLabel.Location = new System.Drawing.Point(16, 8);
            this.m_messageLabel.Name = "m_messageLabel";
            this.m_messageLabel.TabIndex = 3;
            this.m_messageLabel.Text = "message to send:";
            // 
            // m_messages
            // 
            this.m_messages.Location = new System.Drawing.Point(8, 248);
            this.m_messages.Name = "m_messages";
            this.m_messages.Size = new System.Drawing.Size(536, 355);
            this.m_messages.TabIndex = 4;
            this.m_messages.TabStop = false;
            // 
            // m_userNamePanel
            // 
            this.m_userNamePanel.Controls.Add(this.m_constatus);
            this.m_userNamePanel.Controls.Add(this.m_statusLabel);
            this.m_userNamePanel.Controls.Add(this.m_disconnectbutton);
            this.m_userNamePanel.Controls.Add(this.m_connectButton);
            this.m_userNamePanel.Controls.Add(this.m_userNameLabel);
            this.m_userNamePanel.Controls.Add(this.m_usernameTextbox);
            this.m_userNamePanel.Location = new System.Drawing.Point(8, 16);
            this.m_userNamePanel.Name = "m_userNamePanel";
            this.m_userNamePanel.Size = new System.Drawing.Size(544, 88);
            this.m_userNamePanel.TabIndex = 5;
            // 
            // m_constatus
            // 
            this.m_constatus.Location = new System.Drawing.Point(296, 56);
            this.m_constatus.Name = "m_constatus";
            this.m_constatus.TabIndex = 5;
            this.m_constatus.Text = NOT_CONNECTED_INFO;
            // 
            // m_statusLabel
            // 
            this.m_statusLabel.Location = new System.Drawing.Point(232, 56);
            this.m_statusLabel.Name = "m_statusLabel";
            this.m_statusLabel.Size = new System.Drawing.Size(48, 23);
            this.m_statusLabel.TabIndex = 4;
            this.m_statusLabel.Text = "Status:";
            // 
            // m_disconnectbutton
            // 
            this.m_disconnectbutton.Location = new System.Drawing.Point(112, 56);
            this.m_disconnectbutton.Name = "m_disconnectbutton";
            this.m_disconnectbutton.TabIndex = 3;
            this.m_disconnectbutton.Text = "Disconnect";
            this.m_disconnectbutton.Click += new System.EventHandler(this.m_disconnectbutton_Click);
            // 
            // m_connectButton
            // 
            this.m_connectButton.Location = new System.Drawing.Point(16, 56);
            this.m_connectButton.Name = "m_connectButton";
            this.m_connectButton.TabIndex = 2;
            this.m_connectButton.Text = "Connect";
            this.m_connectButton.Click += new System.EventHandler(this.m_connectButton_Click);
            // 
            // m_sendMsgPanel
            // 
            this.m_sendMsgPanel.Controls.Add(this.m_sendMsgButton);
            this.m_sendMsgPanel.Controls.Add(this.m_sendMsgAsyncButton);
            this.m_sendMsgPanel.Controls.Add(this.m_messageLabel);
            this.m_sendMsgPanel.Controls.Add(this.m_messageTextbox);
            this.m_sendMsgPanel.Location = new System.Drawing.Point(8, 120);
            this.m_sendMsgPanel.Name = "m_sendMsgPanel";
            this.m_sendMsgPanel.Size = new System.Drawing.Size(544, 112);
            this.m_sendMsgPanel.TabIndex = 6;
            // 
            // m_sendMsgButton
            // 
            this.m_sendMsgButton.Location = new System.Drawing.Point(16, 80);
            this.m_sendMsgButton.Name = "m_sendMsgButton";
            this.m_sendMsgButton.Size = new System.Drawing.Size(96, 23);
            this.m_sendMsgButton.TabIndex = 4;
            this.m_sendMsgButton.Text = "Send message";
            this.m_sendMsgButton.Click += new System.EventHandler(this.m_sendMsgButton_Click);
            // 
            // m_sendMsgAsyncButton
            // 
            this.m_sendMsgAsyncButton.Location = new System.Drawing.Point(120, 80);
            this.m_sendMsgAsyncButton.Name = "m_sendMsgAsyncButton";
            this.m_sendMsgAsyncButton.Size = new System.Drawing.Size(96, 23);
            this.m_sendMsgAsyncButton.TabIndex = 7;
            this.m_sendMsgAsyncButton.Text = "Send msg async";
            this.m_sendMsgAsyncButton.Click += new System.EventHandler(this.m_sendMsgAsyncButton_Click);
            // 
            // Chatform
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(560, 613);
            this.Controls.Add(this.m_sendMsgPanel);
            this.Controls.Add(this.m_userNamePanel);
            this.Controls.Add(this.m_messages);
            this.Name = "Chatform";
            this.Text = "chatclient";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Chatform_Closing);
            this.m_userNamePanel.ResumeLayout(false);
            this.m_sendMsgPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion

        private void m_connectButton_Click(object sender, System.EventArgs e) {
            if (m_listener == null) {
                try {
	            m_listener = new MessageListenerImpl(m_usernameTextbox.Text, this);
        	    m_chatroom.registerMe(m_listener, m_listener.userName);
                    m_constatus.Text = CONNECTED_INFO;
                } catch (AlreadyRegisteredEx) {
                    m_listener = null;
                    MessageBox.Show("this user name is already in use, try to use another one!");
                } catch (Exception ex) {
                    m_listener = null;
                    Console.WriteLine("exception: " + ex);
                    MessageBox.Show("an exception occured, while trying to connect!");
                }
            } else {
                MessageBox.Show("Already connected, disconnect first");
            }
        }


        private void m_disconnectbutton_Click(object sender, System.EventArgs e) {
            if (m_listener != null) {
                try {
                    DisconnectListener();
                } catch (Exception ex) {
                    Console.WriteLine("exception while disconnecting: " + ex);
                    MessageBox.Show("an exception occured, while trying to disconnect!");
                }
                m_constatus.Text = NOT_CONNECTED_INFO;
            } else {
                MessageBox.Show("Not connected!");            
            }
        }

        private void m_sendMsgButton_Click(object sender, System.EventArgs e) {
            if (m_listener != null) {
                MessageImpl msg = new MessageImpl(m_listener.userName, m_messageTextbox.Text);
                try {
                    m_chatroom.broadCast(msg);
                } catch (Exception ex) {
                    Console.WriteLine("exception encountered, while broadcasting: " + ex);
                    MessageBox.Show("an exception occured, while broadcasting!");
                }
            } else {
                MessageBox.Show("not connected, connect first!");
            }
        }

        delegate void BroadCastDelegate(Message msg);

        private void m_sendMsgAsyncButton_Click(object sender, System.EventArgs e) {
            if (m_listener != null) {
                MessageImpl msg = new MessageImpl(m_listener.userName, m_messageTextbox.Text);
                try {
                    BroadCastDelegate bcd = new BroadCastDelegate(m_chatroom.broadCast);
                    // async call to broadcast
                    bcd.BeginInvoke(msg, null, null);
                    // do not wait for response
                } catch (Exception ex) {
                    Console.WriteLine("exception encountered, while broadcasting: " + ex);
                    MessageBox.Show("an exception occured, while broadcasting!");
                }
            } else {
                MessageBox.Show("not connected, connect first!");
            }
        }

        private void Chatform_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            try {
                DisconnectListener();
            } catch (Exception) {
            }
            try {
                m_chatroom.remove();
            } catch (Exception) {                
            }
            Environment.Exit(0);                 
        }

        /// <summary>
        /// disconnects the listener, if its connected
        /// </summary>
        private void DisconnectListener() {
            if (m_listener != null) {
                try {
                    m_chatroom.unregisterMe(m_listener.userName);
                } finally {
                    RemotingServices.Disconnect(m_listener);
                }
                m_listener = null;
            }
        }

        /// <summary>adds a new message to the message list</summary>
        internal void AddMessage(Message newMessage) {
            MessageImpl theMsg = (MessageImpl) newMessage;
            string entry = "[" + theMsg.originator + "] " + theMsg.msg;
            Console.WriteLine("received new: " + entry);
            m_messages.Items.Add(entry);
        }


    }
}
