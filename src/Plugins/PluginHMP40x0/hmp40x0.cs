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

namespace PluginHMP40x0
{
	public class SourcePlugin : InterfaceSourcePlugin
	{
		private SerialPort serialPort;
		private int lastOutput = 0;
		private float[] lastVoltages = new float[4];
		private ComboBox cbComport  = null;
		private ComboBox cbBaudrate = null;
		private ComboBox cbParity   = null;
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
			switch (cbParity.SelectedIndex)
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
				serialPort.Handshake = Handshake.None;
				break;
			case 1:
				serialPort.Handshake = Handshake.RequestToSend;
				break;
			}
			serialPort.BaudRate = Int32.Parse(cbBaudrate.SelectedItem.ToString());
			
			serialPort.ReadTimeout = 500;
			
			serialPort.Open();
			
			s = HMPSendRecv("*IDN?");
			
			if (s.Contains("HMP40"))
			{
				HMPSendCmd("SYST:REM");
				HMPclearErrorLog();
				
				HMPgetDeviceInformation();
				lastVoltages[0] = GetVoltage(1);
				lastVoltages[1] = GetVoltage(2);
				lastVoltages[2] = GetVoltage(3);
				lastVoltages[3] = GetVoltage(4);
				
				/** speichere Einstellungen */
				DataTable mySettings;
				
				// Create DataTable for Settings
				mySettings = new DataTable("SettingsPluginHMP40x0");
				mySettings.Columns.Add("ComPort", typeof(string));
				mySettings.Columns.Add("Baudrate", typeof(Int32));
				mySettings.Columns.Add("ParityIndex", typeof(Int32));
				mySettings.Columns.Add("StopBitsIndex", typeof(Int32));
				mySettings.Columns.Add("ProtocolIndex", typeof(Int32));
				
				mySettings.Rows.Add(new object[5] { 
					cbComport.SelectedItem.ToString(),
					Int32.Parse(cbBaudrate.SelectedItem.ToString()),
					cbParity.SelectedIndex,
					cbStopBits.SelectedIndex,
					cbProtocol.SelectedIndex
				});
				
				mySettings.WriteXml(configDir + "\\config_plugin_hmp40x0.xml", XmlWriteMode.WriteSchema);
				
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
			if (serialPort == null) return 1;
			if (serialPort.IsOpen)
			{
				HMPclearErrorLog();
				HMPSendCmd("SYST:LOC");
				serialPort.Close();
			}
			return 0;
		}
		
		public int EnableOutput(int output, bool value)
		{
			if (output > devInfo.outputs || output < 1) return I_OUTPUT_INVALID;
			if (lastOutput != output)
			{
				HMPSendCmd("INST OUT" + output.ToString());
				lastOutput = output;
				System.Threading.Thread.Sleep(50);
			}
			if (value)
			{
				HMPSendCmd("OUTP 1");
			}
			else
			{
				HMPSendCmd("OUTP 0");
			}
			return 0;
		}
		
		public int EnableOutputs(bool value)
		{
			if (value)
				HMPSendCmd("OUTP:GEN 1");
			else
				HMPSendCmd("OUTP:GEN 0");
			return 0;
		}
		
		public float GetCurrentLimit(int output)
		{
			if (output > devInfo.outputs || output < 1) return F_OUTPUT_INVALID;
			if (lastOutput != output)
			{
				HMPSendCmd("INST OUT" + output.ToString());
				lastOutput = output;
				System.Threading.Thread.Sleep(50);
			}
			string s = HMPSendRecv("CURR?");
			return float.Parse(s, CultureInfo.InvariantCulture);
		}
		
		public string GetPluginDate()
		{
			return "2012-11-28 09:35";
		}
		
		public string GetPluginName()
		{
			return "HAMEG HMP40x0";
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
				HMPSendCmd("INST OUT" + output.ToString());
				lastOutput = output;
				System.Threading.Thread.Sleep(50);
			}
			string s = HMPSendRecv("VOLT?");
			return float.Parse(s, CultureInfo.InvariantCulture);
		}
		
		public int SetCurrentLimit(int output, float current)
		{
			if (output > devInfo.outputs || output < 1) return I_OUTPUT_INVALID;
			string s = "CURR " + current.ToString("F2", CultureInfo.InvariantCulture);
			if (lastOutput != output)
			{
				HMPSendCmd("INST OUT" + output.ToString());
				lastOutput = output;
				System.Threading.Thread.Sleep(50);
			}
			HMPSendCmd(s);
			return 0;
		}
		
		public int SetVoltage(int output, float voltage)
		{
			if (output > devInfo.outputs || output < 1) return I_OUTPUT_INVALID;
			string s = "VOLT " + voltage.ToString("F2", CultureInfo.InvariantCulture);
			if (lastOutput != output)
			{
				HMPSendCmd("INST OUT" + output.ToString());
				lastOutput = output;
				System.Threading.Thread.Sleep(50);
			}
			if (voltage > 0) lastVoltages[(output - 1)] = voltage;
			HMPSendCmd(s);
			return 0;
		}
		
		public bool isOutputEnabled (int output)
		{
			if (output == -1) { /* select all */
				string s = HMPSendRecv ("OUTP:GEN?");
				if (s [0] == '1') {
					return true;
				}
				return false;
			} else if (output > devInfo.outputs || output < 1) 
				return false;
			else {
				if (lastOutput != output)
				{
					HMPSendCmd("INST OUT" + output.ToString());
					lastOutput = output;
					System.Threading.Thread.Sleep(50);
				}
				string s = HMPSendRecv ("OUTP?");
				if (s [0] == '1') {
					return true;
				}
				return false;
			}
		}
		
		private void HMPclearErrorLog()
		{
			int counter = 20; /* maximale Fehleranzahl im Device */
			string s = "nan";
			string[] list;
			if (serialPort.IsOpen)
			{
				while ((s != "0") && (counter-- > 0))
				{
					s = HMPSendRecv("SYST:ERR?");
					list = s.Split(",".ToCharArray());
					s = list[0];
				}
			}
		}
		
		private int HMPgetLastError()
		{
			int retval = 0;
			string s = "nan";
			string[] list;
			if (serialPort.IsOpen)
			{
				s = HMPSendRecv("SYST:ERR?");
				list = s.Split(",".ToCharArray());
				retval = Int32.Parse(list[0]);
			}
			return retval;
		}
		
		private string HMPSendRecv(string cmd)
		{
			int i = 0;
			int readBytes = 0;
			byte[] buf = new byte[1024];
			string s = "";
			if (serialPort.IsOpen == false) return s;
			
			cmd += "\n";
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
		
		private void HMPSendCmd(string s)
		{
			//int counter = 10;
			if (serialPort.IsOpen)
			{
				s += "\n";
				serialPort.Write(s);
			}
			//while (counter-- > 0) System.Threading.Thread.Sleep(10);
		}
		
		private void HMPgetDeviceInformation()
		{
			string s = HMPSendRecv("*IDN?");
			string[] sList = s.Split(',');
			devInfo.producer = sList[0].Trim();
			devInfo.type = sList[1].Trim();
			devInfo.serialNo = sList[2].Trim();
			devInfo.softwareRevision = sList[3].Trim();
			
			/* min/max - Werte abfragen */
			s = HMPSendRecv("CURR? MIN");
			devInfo.currentLimitMin = devInfo.currentMin = float.Parse(s, CultureInfo.InvariantCulture);
			s = HMPSendRecv("CURR? MAX");
			devInfo.currentLimitMax = devInfo.currentMax = float.Parse(s, CultureInfo.InvariantCulture);
			s = HMPSendRecv("VOLT? MIN");
			devInfo.voltageLimitMin = devInfo.voltageMin = float.Parse(s, CultureInfo.InvariantCulture);
			s = HMPSendRecv("VOLT? MAX");
			devInfo.voltageLimitMax = devInfo.voltageMax = float.Parse(s, CultureInfo.InvariantCulture);
			s = HMPSendRecv("VOLT:PROT? MIN");
			devInfo.voltageOVPMin = float.Parse(s, CultureInfo.InvariantCulture);
			s = HMPSendRecv("VOLT:PROT? MAX");
			devInfo.voltageOVPMax = float.Parse(s, CultureInfo.InvariantCulture);
			if (devInfo.type.Contains("HMP4040"))
				devInfo.outputs = 4;
			else
				devInfo.outputs = 3;
			
			devInfo.singleTurnOnOff = true;
		}
		
		public int ConnectionSettingSetup(object panel, string confDir)
		{
			Panel pPanel = (Panel)panel;
			configDir = confDir;
			
			string comport;
			int baud, parity, stopbits, protocol;
			/** 
            * load settings  
            */
			try
			{
				// Create Settings Table
				DataTable mySettings = new DataTable("SettingsPluginHMP40x0");
				mySettings.ReadXml(confDir + "\\config_plugin_hmp40x0.xml");
				
				comport = mySettings.Rows[0]["ComPort"].ToString();
				baud = Convert.ToInt32(mySettings.Rows[0]["Baudrate"].ToString(), 10);
				parity = Convert.ToInt32(mySettings.Rows[0]["ParityIndex"].ToString(), 10);
				stopbits = Convert.ToInt32(mySettings.Rows[0]["StopBitsIndex"].ToString(), 10);
				protocol = Convert.ToInt32(mySettings.Rows[0]["ProtocolIndex"].ToString(), 10);
			}
			catch
			{
				comport = "COM1";
				baud = 9600;
				parity = 0;
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
			lblFormat.Text = "Parity:";
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
			cbBaudrate.Items.Add("115200");
			cbBaudrate.Items.Add("38400");
			cbBaudrate.Items.Add("19200");
			cbBaudrate.Items.Add("9600");
			cbBaudrate.SelectedIndex = cbBaudrate.Items.IndexOf(baud.ToString());
			cbBaudrate.Top = 31;
			cbBaudrate.Left = 84;
			cbBaudrate.Width = 368;
			pPanel.Controls.Add(cbBaudrate);
			
			cbParity = new ComboBox();
			cbParity.Items.Add("None");
			cbParity.Items.Add("Even");
			cbParity.Items.Add("Odd");
			cbParity.SelectedIndex = parity;
			cbParity.Top = 57;
			cbParity.Left = 84;
			cbParity.Width = 368;
			pPanel.Controls.Add(cbParity);
			
			cbStopBits = new ComboBox();
			cbStopBits.Items.Add("1");
			cbStopBits.Items.Add("2");
			cbStopBits.SelectedIndex = stopbits;
			cbStopBits.Top = 83;
			cbStopBits.Left = 84;
			cbStopBits.Width = 368;
			pPanel.Controls.Add(cbStopBits);
			
			cbProtocol = new ComboBox();
			cbProtocol.Items.Add("None");
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
