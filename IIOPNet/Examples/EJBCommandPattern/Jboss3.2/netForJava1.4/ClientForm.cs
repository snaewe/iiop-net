using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.Remoting;


namespace ch.elca.iiop.demo.ejbCommand {

    /// <summary>
    /// The chat client form.
    /// </summary>
    public class Commandform : System.Windows.Forms.Form {

        #region IFields

        private System.Windows.Forms.TextBox m_operand1Textbox;        
        private System.Windows.Forms.Label m_operand1Label;
        private System.Windows.Forms.TextBox m_operand2Textbox;        
        private System.Windows.Forms.Label m_operand2Label;
        private System.Windows.Forms.Label m_resultLabel;
        private System.Windows.Forms.Label m_resultValue;
        
        private System.Windows.Forms.Button m_addButton;
        private System.Windows.Forms.Button m_subButton;
        
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        private CommandTarget m_commandTarget = null;        

        #endregion IFields

		public Commandform(CommandTarget commandTarget) {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            m_commandTarget = commandTarget;
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
    		
            this.m_operand1Textbox = new System.Windows.Forms.TextBox();        
            this.m_operand1Label = new System.Windows.Forms.Label();
            this.m_operand2Textbox = new System.Windows.Forms.TextBox();
            this.m_operand2Label = new System.Windows.Forms.Label();
            this.m_resultLabel = new System.Windows.Forms.Label();
            this.m_resultValue = new System.Windows.Forms.Label();
        
            this.m_addButton = new System.Windows.Forms.Button();
            this.m_subButton = new System.Windows.Forms.Button();

            this.SuspendLayout();
            // 
            // m_operand1Textbox
            // 
            this.m_operand1Textbox.Location = new System.Drawing.Point(120, 16);
            this.m_operand1Textbox.MaxLength = 20;
            this.m_operand1Textbox.Name = "m_operand1Textbox";
            this.m_operand1Textbox.Size = new System.Drawing.Size(200, 20);
            this.m_operand1Textbox.TabIndex = 0;
            this.m_operand1Textbox.Text = "0";
            this.m_operand1Textbox.WordWrap = false;
            // 
            // m_operand1Label
            // 
            this.m_operand1Label.Location = new System.Drawing.Point(8, 16);
            this.m_operand1Label.Name = "m_operand1Label";
            this.m_operand1Label.Size = new System.Drawing.Size(100, 16);
            this.m_operand1Label.TabIndex = 1;
            this.m_operand1Label.Text = "Operand 1:";
            // 
            // m_operand2Textbox
            // 
            this.m_operand2Textbox.Location = new System.Drawing.Point(120, 42);
            this.m_operand2Textbox.MaxLength = 20;
            this.m_operand2Textbox.Name = "m_operand2Textbox";
            this.m_operand2Textbox.Size = new System.Drawing.Size(200, 20);
            this.m_operand2Textbox.TabIndex = 2;
            this.m_operand2Textbox.Text = "0";
            this.m_operand2Textbox.WordWrap = false;
            // 
            // m_operand2Label
            // 
            this.m_operand2Label.Location = new System.Drawing.Point(8, 42);
            this.m_operand2Label.Name = "m_operand2Label";
            this.m_operand2Label.Size = new System.Drawing.Size(100, 16);
            this.m_operand2Label.TabIndex = 3;
            this.m_operand2Label.Text = "Operand 2:";            
            // 
            // m_resultLabel
            // 
            this.m_resultLabel.Location = new System.Drawing.Point(8, 78);
            this.m_resultLabel.Name = "m_resultLabel";
            this.m_resultLabel.Size = new System.Drawing.Size(100, 16);
            this.m_resultLabel.TabIndex = 4;
            this.m_resultLabel.Text = "Result:";
            // 
            // m_resultValue
            // 
            this.m_resultValue.Location = new System.Drawing.Point(120, 78);
            this.m_resultValue.Name = "m_resultValue";
            this.m_resultValue.Size = new System.Drawing.Size(100, 16);
            this.m_resultValue.TabIndex = 5;
            this.m_resultValue.Text = "";            
            // 
            // m_addButton
            // 
            this.m_addButton.Location = new System.Drawing.Point(12, 120);
            this.m_addButton.Name = "m_addButton";
            this.m_addButton.TabIndex = 6;
            this.m_addButton.Text = "Add";
            this.m_addButton.Click += new System.EventHandler(this.m_addButton_Click);
            // 
            // m_subButton
            // 
            this.m_subButton.Location = new System.Drawing.Point(112, 120);
            this.m_subButton.Name = "m_subButton";
            this.m_subButton.TabIndex = 7;
            this.m_subButton.Text = "Sub";
            this.m_subButton.Click += new System.EventHandler(this.m_subButton_Click);

            // 
            // Commandform
            //             
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(560, 200);
            this.Controls.Add(this.m_operand1Textbox);
            this.Controls.Add(this.m_operand1Label);
            this.Controls.Add(this.m_operand2Textbox);
            this.Controls.Add(this.m_operand2Label);
            this.Controls.Add(this.m_resultLabel);
            this.Controls.Add(this.m_resultValue);
            this.Controls.Add(this.m_addButton);
            this.Controls.Add(this.m_subButton);            
            
            this.Name = "Commandform";
            this.Text = "commandclient";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Commandform_Closing);
            this.ResumeLayout(false);

        }
		#endregion

        private void m_addButton_Click(object sender, System.EventArgs e) {                       
            ProcessDyadicCommand(new AddOpImpl());
        }

        private void m_subButton_Click(object sender, System.EventArgs e) {
            ProcessDyadicCommand(new SubOpImpl());
        }
        
        private void ProcessDyadicCommand(DyadicOp cmd) {
            try {                
                cmd.operand1 = Int32.Parse(m_operand1Textbox.Text);
                cmd.operand2 = Int32.Parse(m_operand2Textbox.Text);
                DyadicOp cmdResult = (DyadicOp)m_commandTarget.executeCommand(cmd);
                m_resultValue.Text = cmdResult.result.ToString();
            } catch (System.FormatException) {
                MessageBox.Show("Invalid integer, check operands!");
            } catch (Exception ex) {
                Console.WriteLine("exception encountered, while executing command: " + ex);
                MessageBox.Show("an exception occured, while executing command!");
            }        
        }

        private void Commandform_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            try {
                m_commandTarget.remove();
            } catch (Exception) {                
            }
            Environment.Exit(0);                 
        }

    }
    
    
}
