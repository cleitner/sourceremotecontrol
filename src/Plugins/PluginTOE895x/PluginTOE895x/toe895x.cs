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
using SourcePluginInterface;
using System.IO.Ports;
using System.Globalization;
using System.Windows.Forms;
using ClassDeviceInformation;

namespace PluginTOE895x
{
    public class SourcePlugin : InterfaceSourcePlugin
    {
        private SerialPort serialPort;
        private int lastOutput = 0;
        private float[] lastVoltages = new float[2];
        private ComboBox cbComport  = null;
        private ComboBox cbBaudrate = null;
        private ComboBox cbFormat   = null;
        private ComboBox cbStopBits = null;
        private ComboBox cbProtocol = null;
        private DeviceInformation devInfo = new DeviceInformation();
        private string configDir;

        /* Konstanten */
        const float F_OUTPUT_INVALID = (float) -10010.0;
        const int I_OUTPUT_INVALID = -10;

        public int Connect(string connectionInformation)
        {
            string s;
            if (cbComport == null) return 1;
            if (serialPort != null)
            {
                if (serialPort.IsOpen) serialPort.Close();
                if (serialPort.PortName != cbComport.SelectedText) serialPort.PortName = cbComport.SelectedItem.ToString();
            }
            else
            {
                //tbOut.AppendText("Open Port: " + connectionData + "\n\r");
                serialPort = new SerialPort(cbComport.SelectedItem.ToString());
                //serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialDataRecvHandler);
            }
            
            switch (cbStopBits.SelectedIndex)
            {
                case 0:
                    serialPort.StopBits = StopBits.One;
                    break;
                case 1:
                    serialPort.StopBits = StopBits.Two;
                    break;
            }
            switch (cbFormat.SelectedIndex)
            {
                case 0:
                    serialPort.Parity = Parity.None;
                    serialPort.DataBits = 8;
                    break;
                case 1:
                    serialPort.DataBits = 7;
                    serialPort.Parity = Parity.Even;
                    break;
                case 2:
                    serialPort.DataBits = 7;
                    serialPort.Parity = Parity.Odd;
                    break;
            }
            switch (cbProtocol.SelectedIndex)
            {
                case 0:
                    serialPort.Handshake = Handshake.XOnXOff;
                    break;
                case 1:
                    serialPort.Handshake = Handshake.RequestToSend;
                    break;
            }
            serialPort.BaudRate = Int32.Parse(cbBaudrate.SelectedItem.ToString());

            serialPort.ReadTimeout = 500;

            serialPort.Open();

            s = TOESendRecv("*IDN?");

            if (s.Contains("TOE895"))
            {
                TOESendCmd("SYST:REM");
                TOEclearErrorLog();
                
                // switch to SCPI if needed
                s = TOESendRecv("SYST:LANG?");
                if(s.Contains("COMP")) 
                {
                    TOESendCmd("SYST:LANG CIIL");
                }
                
                TOEgetDeviceInformation();
                lastVoltages[0] = GetVoltage(1);
                lastVoltages[1] = GetVoltage(2);

                /** speichere Einstellungen */
                DataTable mySettings;

                // Create DataTable for Settings
                mySettings = new DataTable("SettingsPluginTOE895x");
                mySettings.Columns.Add("ComPort", typeof(string));
                mySettings.Columns.Add("Baudrate", typeof(Int32));
                mySettings.Columns.Add("FormatIndex", typeof(Int32));
                mySettings.Columns.Add("StopBitsIndex", typeof(Int32));
                mySettings.Columns.Add("ProtocolIndex", typeof(Int32));
            
                mySettings.Rows.Add(new object[5] { 
                    cbComport.SelectedItem.ToString(),
                    Int32.Parse(cbBaudrate.SelectedItem.ToString()),
                    cbFormat.SelectedIndex,
                    cbStopBits.SelectedIndex,
                    cbProtocol.SelectedIndex
                });

                mySettings.WriteXml(configDir + "\\config_plugin_toe895x.xml", XmlWriteMode.WriteSchema);

            }
            else
            {
                serialPort.Close();
                return 1;
            }
            return 0;
        }

        public int Disconnect()
        {
            int counter = 5;
            int lastErr = 0;
            if (serialPort == null) return 1;
            if (serialPort.IsOpen)
            {
                TOEclearErrorLog();
                while (counter-- > 0 && (lastErr != -201))
                {
                    TOESendCmd("SYST:LOC");
                    TOESendCmd("DISP 0");

                    lastErr = TOEgetLastError();
                }
                serialPort.Close();
            }
            return 0;
        }

        public int EnableOutput(int output, bool value)
        {
            if (output > devInfo.outputs || output < 1) return I_OUTPUT_INVALID;
            if (lastOutput != output)
            {
                TOESendCmd("INST OUT" + output.ToString());
                lastOutput = output;
            }
            if (value)
            {
                SetVoltage(output, lastVoltages[(output - 1)]);
            }
            else
            {
                lastVoltages[(output - 1)] = GetVoltage(output);
                SetVoltage(output, 0);
            }
            return 0;
        }

        public int EnableOutputs(bool value)
        {
            if (value)
                TOESendCmd("OUTP 1");
            else
                TOESendCmd("OUTP 0");
            return 0;
        }

        public float GetCurrentLimit(int output)
        {
            if (output > devInfo.outputs || output < 1) return F_OUTPUT_INVALID;
            if (lastOutput != output)
            {
                TOESendCmd("INST OUT" + output.ToString());
                lastOutput = output;
            }
            string s = TOESendRecv("CURR?");
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        public string GetPluginDate()
        {
            return "2012-07-05 13:30";
        }

        public string GetPluginName()
        {
            return "TOELLNER TOE895x";
        }

        public int GetPluginVersion()
        {
            throw new NotImplementedException();
        }

        public float GetVoltage(int output)
        {
            if (output > devInfo.outputs || output < 1) return F_OUTPUT_INVALID;
            if (lastOutput != output)
            {
                TOESendCmd("INST OUT" + output.ToString());
                lastOutput = output;
            }
            string s = TOESendRecv("VOLT?");
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        public int SetCurrentLimit(int output, float current)
        {
            if (output > devInfo.outputs || output < 1) return I_OUTPUT_INVALID;
            string s = "CURR " + current.ToString("F2", CultureInfo.InvariantCulture);
            if (lastOutput != output)
            {
                TOESendCmd("INST OUT" + output.ToString());
                lastOutput = output;
            }
            TOESendCmd(s);
            return 0;
        }

        public int SetVoltage(int output, float voltage)
        {
            if (output > devInfo.outputs || output < 1) return I_OUTPUT_INVALID;
            string s = "VOLT " + voltage.ToString("F2", CultureInfo.InvariantCulture);
            if (lastOutput != output)
            {
                TOESendCmd("INST OUT" + output.ToString());
                lastOutput = output;
            }
            if (voltage > 0) lastVoltages[(output - 1)] = voltage;
            TOESendCmd(s);
            return 0;
        }

        public bool isOutputEnabled(int output)
        {
            if (output == -1) /* select all */
            {
                string s = TOESendRecv("OUTP?");
                if (s[0] == '1')
                {
                    return true;
                }
                return false;
            }
            else
                return false;
        }
        
        private void TOEclearErrorLog()
        {
            int counter = 20; /* maximale Fehleranzahl im Device */
            string s = "nan";
            string[] list;
            if (serialPort.IsOpen)
            {
                while ((s != "0") && (counter-- > 0))
                {
                    s = TOESendRecv("SYST:ERR?");
                    list = s.Split(",".ToCharArray());
                    s = list[0];
                }
            }
        }

        private int TOEgetLastError()
        {
            int retval = 0;
            string s = "nan";
            string[] list;
            if (serialPort.IsOpen)
            {
                s = TOESendRecv("SYST:ERR?");
                list = s.Split(",".ToCharArray());
                retval = Int32.Parse(list[0]);
            }
            return retval;
        }

        private string TOESendRecv(string cmd)
        {
            int i = 0;
            int readBytes = 0;
            byte[] buf = new byte[1024];
            string s = "";
            if (serialPort.IsOpen == false) return s;

            cmd += "\r\n";
            System.Text.Encoding.ASCII.GetBytes(cmd, 0, cmd.Length, buf, 0);
            serialPort.Write(buf, 0, cmd.Length);

            i = 0;
            buf[0] = 0;
            try
            {
                do
                {
                    readBytes = serialPort.Read(buf, i, 1);
                    if (readBytes > 0)
                    {
                        i += readBytes;
                    }
                } while (buf[i - 1] != 0x0A);
                s = System.Text.Encoding.ASCII.GetString(buf, 0, i);
            }
            catch (TimeoutException)
            {
                s = "timeout waiting for device";
            }
            //System.Threading.Thread.Sleep(50);
            return s;
        }

        private void TOESendCmd(string s)
        {
            //int counter = 10;
            if (serialPort.IsOpen)
            {
                s += "\r\n";
                serialPort.Write(s);
            }
            //while (counter-- > 0) System.Threading.Thread.Sleep(10);
        }

        private void TOEgetDeviceInformation()
        {
            string s = TOESendRecv("*IDN?");
            string[] sList = s.Split(',');
            devInfo.producer = sList[0].Trim();
            devInfo.type = sList[1].Trim();
            devInfo.serialNo = sList[2].Trim();
            devInfo.softwareRevision = sList[3].Trim();

            /* min/max - Werte abfragen */
            s = TOESendRecv("CURR? MIN");
            devInfo.currentMin = float.Parse(s, CultureInfo.InvariantCulture);
            s = TOESendRecv("CURR? MAX");
            devInfo.currentMax = float.Parse(s, CultureInfo.InvariantCulture);
            s = TOESendRecv("CURR:LIM? MIN");
            devInfo.currentLimitMin = float.Parse(s, CultureInfo.InvariantCulture);
            s = TOESendRecv("CURR:LIM? MAX");
            devInfo.currentLimitMax = float.Parse(s, CultureInfo.InvariantCulture);
            s = TOESendRecv("VOLT? MIN");
            devInfo.voltageMin = float.Parse(s, CultureInfo.InvariantCulture);
            s = TOESendRecv("VOLT? MAX");
            devInfo.voltageMax = float.Parse(s, CultureInfo.InvariantCulture);
            s = TOESendRecv("VOLT:LIM? MIN");
            devInfo.voltageLimitMin = float.Parse(s, CultureInfo.InvariantCulture);
            s = TOESendRecv("VOLT:LIM? MAX");
            devInfo.voltageLimitMax = float.Parse(s, CultureInfo.InvariantCulture);
            s = TOESendRecv("VOLT:PROT? MIN");
            devInfo.voltageOVPMin = float.Parse(s, CultureInfo.InvariantCulture);
            s = TOESendRecv("VOLT:PROT? MAX");
            devInfo.voltageOVPMax = float.Parse(s, CultureInfo.InvariantCulture);

            if (devInfo.type.Contains("8952"))
                devInfo.outputs = 2;
            else
                devInfo.outputs = 1;

            devInfo.singleTurnOnOff = false;
        }

        public int ConnectionSettingSetup(object panel, string confDir)
        {
            Panel pPanel = (Panel)panel;
            configDir = confDir;

            string comport;
            int baud, format, stopbits, protocol;
            /** 
            * load settings  
            */
            try
            {
                // Create Settings Table
                DataTable mySettings = new DataTable("SettingsPluginTOE895x");
                mySettings.ReadXml(confDir + "\\config_plugin_toe895x.xml");

                comport = mySettings.Rows[0]["ComPort"].ToString();
                baud = Convert.ToInt32(mySettings.Rows[0]["Baudrate"].ToString(), 10);
                format = Convert.ToInt32(mySettings.Rows[0]["FormatIndex"].ToString(), 10);
                stopbits = Convert.ToInt32(mySettings.Rows[0]["StopBitsIndex"].ToString(), 10);
                protocol = Convert.ToInt32(mySettings.Rows[0]["ProtocolIndex"].ToString(), 10);
            }
            catch
            {
                comport = "COM1";
                baud = 4800;
                format = 0;
                stopbits = 0;
                protocol = 0;
            }

            Label lblComport = new Label();
            lblComport.Text = "Comport:";
            lblComport.Top = 8;
            lblComport.Left = 5;
            lblComport.Width = 70;
            pPanel.Controls.Add(lblComport);

            Label lblBaudrate = new Label();
            lblBaudrate.Text = "Baudrate:";
            lblBaudrate.Top = 34;
            lblBaudrate.Left = 5;
            lblBaudrate.Width = 70;
            pPanel.Controls.Add(lblBaudrate);

            Label lblFormat = new Label();
            lblFormat.Text = "Format:";
            lblFormat.Top = 60;
            lblFormat.Left = 5;
            lblFormat.Width = 70;
            pPanel.Controls.Add(lblFormat);

            Label lblStopBits = new Label();
            lblStopBits.Text = "Stopbits:";
            lblStopBits.Top = 86;
            lblStopBits.Left = 5;
            lblStopBits.Width = 70;
            pPanel.Controls.Add(lblStopBits);

            Label lblProtocol = new Label();
            lblProtocol.Text = "Protokoll:";
            lblProtocol.Top = 112;
            lblProtocol.Left = 5;
            lblProtocol.Width = 70;
            pPanel.Controls.Add(lblProtocol);

            cbComport = new ComboBox();
            foreach (string port in SerialPort.GetPortNames())
            {
                cbComport.Items.Add(port);
            }
            cbComport.SelectedIndex = cbComport.Items.IndexOf(comport);
            cbComport.Top = 5;
            cbComport.Left = 84;
            cbComport.Width = 368;
            pPanel.Controls.Add(cbComport);

            cbBaudrate = new ComboBox();
            cbBaudrate.Items.Add("57600");
            cbBaudrate.Items.Add("38400");
            cbBaudrate.Items.Add("19200");
            cbBaudrate.Items.Add("9600");
            cbBaudrate.Items.Add("4800");
            cbBaudrate.Items.Add("2400");
            cbBaudrate.Items.Add("1200");
            cbBaudrate.SelectedIndex = cbBaudrate.Items.IndexOf(baud.ToString());
            cbBaudrate.Top = 31;
            cbBaudrate.Left = 84;
            cbBaudrate.Width = 368;
            pPanel.Controls.Add(cbBaudrate);

            cbFormat = new ComboBox();
            cbFormat.Items.Add("8 Bit");
            cbFormat.Items.Add("even 7 Bit");
            cbFormat.Items.Add("odd 7 Bit");
            cbFormat.SelectedIndex = format;
            cbFormat.Top = 57;
            cbFormat.Left = 84;
            cbFormat.Width = 368;
            pPanel.Controls.Add(cbFormat);

            cbStopBits = new ComboBox();
            cbStopBits.Items.Add("1");
            cbStopBits.Items.Add("2");
            cbStopBits.SelectedIndex = stopbits;
            cbStopBits.Top = 83;
            cbStopBits.Left = 84;
            cbStopBits.Width = 368;
            pPanel.Controls.Add(cbStopBits);

            cbProtocol = new ComboBox();
            cbProtocol.Items.Add("Xon/Xoff");
            cbProtocol.Items.Add("RTS/CTS");
            cbProtocol.SelectedIndex = protocol;
            cbProtocol.Top = 109;
            cbProtocol.Left = 84;
            cbProtocol.Width = 368;
            pPanel.Controls.Add(cbProtocol);

            return 0;
        }


        public int AdvancedSettingsSetup(object panel)
        {
            throw new NotImplementedException();
        }

        public int GetOutputCount()
        {
            return devInfo.outputs;
        }


        public DeviceInformation GetDeviceInformation()
        {
            return devInfo;
        }


        public int GetMinimalUpdateTimeMS()
        {
            return 50; /* 50 ms update rate .. TOE is very slow */
        }
    }
}
