/*  SourceRemoteControl
    Copyright (C) 2013  Sebastian Block

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */
using System;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.IO.IsolatedStorage;
using System.Windows.Forms;
using CheckPluginInterface;

namespace PluginCheckSerial
{
    public class CheckPlugin:InterfaceCheckPlugin
    {
        private addTextCP addTextCB;
        private Form setupForm;
        private TextBox tbSendBeforeCheck;
        private TextBox tbCheck;
        private ComboBox cbComport;
        private ComboBox cbBaudrate;
        private ComboBox cbDataBits;
        private ComboBox cbParity;
        private ComboBox cbStopBits;
        private ComboBox cbProtocol;
        private NumericUpDown nudReadTimeout;

        private String optStrSendBeforeCheck;
        private String optStrCheck;
        private String optSerialPort;
        private int optParityIndex;
        private int optBaudrate;
        private int optDataBitIndex;
        private int optStopBitsIndex;
        private int optHandshakeIndex;
        private int optReadTimeout;

        private int inst;
        private string confDir;

        public string GetPluginName()
        {
            return "Serial Check Plugin";
        }

        public int runCheck()
        {
            SerialPort serialPort = new SerialPort(optSerialPort);
            byte[] buffer = new byte[1024];
            int readBytes = 0;
            string s;
            int c = 0, i = 0;
            int retval = 0;

            switch(optParityIndex) {
                case 0: 
                    serialPort.Parity = Parity.None;
                    break;
                case 1:
                    serialPort.Parity = Parity.Odd;
                    break;
                case 2:
                    serialPort.Parity = Parity.Even;
                    break;
                case 3:
                    serialPort.Parity = Parity.Mark;
                    break;
                case 4:
                    serialPort.Parity = Parity.Space;
                    break;
                default:
                    return -1;
            }
            switch (optStopBitsIndex)
            {
                case 0:
                    serialPort.StopBits = StopBits.One;
                    break;
                case 1:
                    serialPort.StopBits = StopBits.OnePointFive;
                    break;
                case 2:
                    serialPort.StopBits = StopBits.Two;
                    break;
                case 3:
                    serialPort.StopBits = StopBits.None;
                    break;
                default:
                    return -1;
            }
            
            serialPort.DataBits = optDataBitIndex+5;
            serialPort.BaudRate = optBaudrate;
            switch (optHandshakeIndex)
            {
                case 0:
                    serialPort.Handshake = Handshake.None;
                    break;
                case 1:
                    serialPort.Handshake = Handshake.XOnXOff;
                    break;
                case 2:
                    serialPort.Handshake = Handshake.RequestToSend;
                    break;
                default:
                    return -1;
            }
            serialPort.ReadTimeout = optReadTimeout;
            try
            {
                serialPort.Open();
            }
            catch
            {
                return -1;
            }

            for (i=0, c=0; i < optStrSendBeforeCheck.Length; i++)
            {
                if (optStrSendBeforeCheck[i] == '$')
                {
                    i++;
                    if (optStrSendBeforeCheck[i] == '$') buffer[c++] = (byte)'$';
                    else
                    {
                        string str = optStrSendBeforeCheck.Substring(i, 2);
                        i++; 
                        buffer[c++] = (byte) Convert.ToByte(str, 16);
                    }
                }
                else buffer[c++] = (byte) optStrSendBeforeCheck[i];
            }
            serialPort.Write(buffer, 0, c);

            i = 0;
            buffer[0] = 0;
            try
            {
                do
                {
                    readBytes = serialPort.Read(buffer, i, 1);
                    if (readBytes > 0)
                    {
                        i += readBytes;
                    }
                } while (i<1024);
            }
            catch (TimeoutException)
            {
                if (i == 0)
                {
                    addTextCB("Timeout while waiting for answer\r\n");
                    serialPort.Close();
                    return 2;
                }
            }
            s = System.Text.Encoding.ASCII.GetString(buffer, 0, i);

            if (s.Contains(optStrCheck))
            {
                addTextCB("expected string returned\r\n");
                retval = 0;
            }
            else
            {
                addTextCB("unexpected string returned: " + s + "\r\n");
                retval = 1;
            }

            serialPort.Close();
            return retval;
        }


        public void setupCheckPlugin()
        {
            setupForm = new Form();
            setupForm.Width = 420;
            setupForm.Height = 272;
            setupForm.FormBorderStyle = FormBorderStyle.FixedSingle;
            setupForm.MaximizeBox = false;
            setupForm.MinimizeBox = false;
            setupForm.FormClosing += new FormClosingEventHandler(setupForm_Closing);

            tbSendBeforeCheck = new TextBox();
            tbSendBeforeCheck.Height = 20;
            tbSendBeforeCheck.Width = 300;
            tbSendBeforeCheck.Top = 12;
            tbSendBeforeCheck.Left = 110;
            tbSendBeforeCheck.Text = optStrSendBeforeCheck;
            setupForm.Controls.Add(tbSendBeforeCheck);

            tbCheck = new TextBox();
            tbCheck.Height = 20;
            tbCheck.Width = 300;
            tbCheck.Top = 38;
            tbCheck.Left = 110;
            tbCheck.Text = optStrCheck;
            setupForm.Controls.Add(tbCheck);

            Label lbl = new Label();
            lbl.Text = "Data to send before";
            lbl.Top = 15;
            lbl.Left = 5;
            lbl.Width = 101;
            lbl.Height = 13;
            setupForm.Controls.Add(lbl);

            lbl = new Label();
            lbl.Text = "Data to check for";
            lbl.Top = 41;
            lbl.Left = 5;
            lbl.Width = 101;
            lbl.Height = 13;
            setupForm.Controls.Add(lbl);

            Label lblComport = new Label();
            lblComport.Text = "Comport:";
            lblComport.Top = 67;
            lblComport.Left = 5;
            lblComport.Width = 70;
            setupForm.Controls.Add(lblComport);

            Label lblBaudrate = new Label();
            lblBaudrate.Text = "Baudrate:";
            lblBaudrate.Top = 93;
            lblBaudrate.Left = 5;
            lblBaudrate.Width = 70;
            setupForm.Controls.Add(lblBaudrate);

            Label lblFormat = new Label();
            lblFormat.Text = "Data bits:";
            lblFormat.Top = 119;
            lblFormat.Left = 5;
            lblFormat.Width = 70;
            setupForm.Controls.Add(lblFormat);

            lbl = new Label();
            lbl.Text = "Parity:";
            lbl.Top = 145;
            lbl.Left = 5;
            lbl.Width = 101;
            lbl.Height = 13;
            setupForm.Controls.Add(lbl);

            Label lblStopBits = new Label();
            lblStopBits.Text = "Stopbits:";
            lblStopBits.Top = 171;
            lblStopBits.Left = 5;
            lblStopBits.Width = 70;
            setupForm.Controls.Add(lblStopBits);

            Label lblProtocol = new Label();
            lblProtocol.Text = "Protokoll:";
            lblProtocol.Top = 197;
            lblProtocol.Left = 5;
            lblProtocol.Width = 70;
            setupForm.Controls.Add(lblProtocol);

            lbl = new Label();
            lbl.Text = "Read Timeout:";
            lbl.Top = 223;
            lbl.Left = 5;
            lbl.Width = 101;
            lbl.Height = 13;
            setupForm.Controls.Add(lbl);

            cbComport = new ComboBox();
            foreach (string port in SerialPort.GetPortNames())
            {
                cbComport.Items.Add(port);
            }
            cbComport.Top = 64;
            cbComport.Left = 110;
            cbComport.Width = 300;
            cbComport.SelectedIndex = cbComport.Items.IndexOf(optSerialPort);
            setupForm.Controls.Add(cbComport);

            cbBaudrate = new ComboBox();
            cbBaudrate.Items.Add("115200");
            cbBaudrate.Items.Add("57600");
            cbBaudrate.Items.Add("38400");
            cbBaudrate.Items.Add("19200");
            cbBaudrate.Items.Add("9600");
            cbBaudrate.Items.Add("4800");
            cbBaudrate.Items.Add("2400");
            cbBaudrate.Items.Add("1200");
            cbBaudrate.Top = 90;
            cbBaudrate.Left = 110;
            cbBaudrate.Width = 300;
            cbBaudrate.SelectedIndex = cbBaudrate.Items.IndexOf(optBaudrate.ToString());
            setupForm.Controls.Add(cbBaudrate);

            cbDataBits = new ComboBox();
            cbDataBits.Items.Add("5 Bit");
            cbDataBits.Items.Add("6 Bit");
            cbDataBits.Items.Add("7 Bit");
            cbDataBits.Items.Add("8 Bit");
            cbDataBits.Top = 116;
            cbDataBits.Left = 110;
            cbDataBits.Width = 300;
            cbDataBits.SelectedIndex = optDataBitIndex;
            setupForm.Controls.Add(cbDataBits);

            cbParity = new ComboBox();
            cbParity.Items.Add("None");
            cbParity.Items.Add("odd");
            cbParity.Items.Add("even");
            cbParity.Items.Add("mark");
            cbParity.Items.Add("space");
            cbParity.Top = 145;
            cbParity.Left = 110;
            cbParity.Width = 300;
            cbParity.SelectedIndex = optParityIndex;
            setupForm.Controls.Add(cbParity);

            cbStopBits = new ComboBox();
            cbStopBits.Items.Add("1");
            cbStopBits.Items.Add("1.5");
            cbStopBits.Items.Add("2");
            cbStopBits.Items.Add("None");
            cbStopBits.Top = 168;
            cbStopBits.Left = 110;
            cbStopBits.Width = 300;
            cbStopBits.SelectedIndex = optStopBitsIndex;
            setupForm.Controls.Add(cbStopBits);

            cbProtocol = new ComboBox();
            cbProtocol.Items.Add("None");
            cbProtocol.Items.Add("Xon/Xoff");
            cbProtocol.Items.Add("RTS/CTS");
            cbProtocol.SelectedIndex = 0;
            cbProtocol.Top = 194;
            cbProtocol.Left = 110;
            cbProtocol.Width = 300;
            cbProtocol.SelectedIndex = optHandshakeIndex;
            setupForm.Controls.Add(cbProtocol);

            nudReadTimeout = new NumericUpDown();
            nudReadTimeout.Minimum = 10;
            nudReadTimeout.Maximum = 999999;
            nudReadTimeout.DecimalPlaces = 0;
            nudReadTimeout.Increment = 100;
            nudReadTimeout.Value = optReadTimeout;
            nudReadTimeout.Top = 220;
            nudReadTimeout.Left = 110;
            nudReadTimeout.Width = 300;
            setupForm.Controls.Add(nudReadTimeout);

            setupForm.ShowDialog();
        }

        private void setupForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            addTextCB("Setup geschlossen\r\n");
            optStrCheck = tbCheck.Text;
            optStrSendBeforeCheck = tbSendBeforeCheck.Text;
            optBaudrate = Convert.ToInt32(cbBaudrate.SelectedItem.ToString(), 10);
            optDataBitIndex = cbDataBits.SelectedIndex;
            optHandshakeIndex = cbProtocol.SelectedIndex;
            optParityIndex = cbParity.SelectedIndex;
            optReadTimeout = (int)nudReadTimeout.Value;
            optSerialPort = cbComport.SelectedItem.ToString();
            optStopBitsIndex = cbStopBits.SelectedIndex;

            DataTable mySettings;
            try
            {
                // open config file
                mySettings = new DataTable("SettingsPluginCheckSerial");
                mySettings.ReadXml(confDir + "\\config_plugin_checkserial.xml");
            }
            catch 
            {
                /* file does not exists -> create one */
                // Create DataTable for Settings
                mySettings = new DataTable("SettingsPluginCheckSerial");
                mySettings.Columns.Add("ComPort", typeof(string));
                mySettings.Columns.Add("Baudrate", typeof(Int32));
                mySettings.Columns.Add("DataBits", typeof(Int32));
                mySettings.Columns.Add("Handshake", typeof(Int32));
                mySettings.Columns.Add("Parity", typeof(Int32));
                mySettings.Columns.Add("ReadTimeout", typeof(Int32));
                mySettings.Columns.Add("StopBits", typeof(Int32));
                mySettings.Columns.Add("CheckStr", typeof(string));
                mySettings.Columns.Add("SendBeforeCheckStr", typeof(string));
            }
            
            while(mySettings.Rows.Count <= inst) {
                mySettings.Rows.Add(new object[9] { "COM1", 57600, 3, 0, 0, 500, 0, "CheckString", "BeforeCheckString"});
            }

            mySettings.Rows[inst]["ComPort"]            = optSerialPort;
            mySettings.Rows[inst]["Baudrate"]           = optBaudrate;
            mySettings.Rows[inst]["DataBits"]           = optDataBitIndex;
            mySettings.Rows[inst]["Handshake"]          = optHandshakeIndex;
            mySettings.Rows[inst]["Parity"]             = optParityIndex;
            mySettings.Rows[inst]["ReadTimeout"]        = optReadTimeout;
            mySettings.Rows[inst]["StopBits"]           = optStopBitsIndex;
            mySettings.Rows[inst]["CheckStr"]           = optStrCheck;
            mySettings.Rows[inst]["SendBeforeCheckStr"] = optStrSendBeforeCheck;

            // Write to Isolated Storage
            mySettings.WriteXml(confDir + "\\config_plugin_checkserial.xml", XmlWriteMode.WriteSchema);
        }


        public void initCheckPlugin(int instance, addTextCP callback, string configDir)
        {
            addTextCB = callback;
            this.inst = instance;
            confDir = configDir;

            try
            {
                 // Create Settings Table
                DataTable mySettings = new DataTable("SettingsPluginCheckSerial");
                mySettings.ReadXml(confDir + "\\config_plugin_checkserial.xml");
                
                optSerialPort = mySettings.Rows[instance]["ComPort"].ToString();
                optBaudrate = Convert.ToInt32(mySettings.Rows[instance]["Baudrate"].ToString(), 10);
                optDataBitIndex = Convert.ToInt32(mySettings.Rows[instance]["DataBits"].ToString(), 10);
                optHandshakeIndex = Convert.ToInt32(mySettings.Rows[instance]["Handshake"].ToString(), 10);
                optParityIndex = Convert.ToInt32(mySettings.Rows[instance]["Parity"].ToString(), 10);
                optReadTimeout = Convert.ToInt32(mySettings.Rows[instance]["ReadTimeout"].ToString(), 10);
                optStopBitsIndex = Convert.ToInt32(mySettings.Rows[instance]["StopBits"].ToString(), 10);
                optStrCheck = mySettings.Rows[instance]["CheckStr"].ToString();
                optStrSendBeforeCheck = mySettings.Rows[instance]["SendBeforeCheckStr"].ToString();
            }
            catch
            {
                optSerialPort = "COM1";
                optBaudrate = 57600;
                optDataBitIndex = 3;
                optParityIndex = 0;
                optStopBitsIndex = 0;
                optHandshakeIndex = 0;
                optReadTimeout = 500;
            }
        }
    }
}
