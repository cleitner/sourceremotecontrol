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
using System.IO;
using System.IO.Ports;
using System.Globalization;
using System.Windows.Forms;
using ClassDeviceInformation;

using Ivi.Visa.Interop;

namespace PluginDP800
{
    public class SourcePlugin : InterfaceSourcePlugin
    {
        private DeviceInformation devInfo = new DeviceInformation();
        private ResourceManager rm = new ResourceManager();
        private ComboBox cbResource;
        private FormattedIO488 io = new FormattedIO488();
        private int lastOutput = 0;
        private float[] lastVoltages = new float[2];
        private string configDir;

        const float F_OUTPUT_INVALID = (float)-10010.0;
        const int I_OUTPUT_INVALID = -10;

        public string GetPluginName()
        {
            return "Rigol DP800";
        }

        public string GetPluginDate()
        {
            return "2013-11-27";
        }

        public int GetPluginVersion()
        {
            return 1;
        }

        public int SetVoltage(int output, float voltage)
        {
            if (!SelectOutput(output)) return I_OUTPUT_INVALID;

            io.WriteString("VOLT " + voltage.ToString("F2", CultureInfo.InvariantCulture));

            return 0;
        }

        public float GetVoltage(int output)
        {
            if (!SelectOutput(output)) return I_OUTPUT_INVALID;

            return ReadValue("VOLT?");
        }

        public int SetCurrentLimit(int output, float current)
        {
            if (!SelectOutput(output)) return I_OUTPUT_INVALID;

            io.WriteString("CURR " + current.ToString("F2", CultureInfo.InvariantCulture));
            
            return 0;
        }

        public float GetCurrentLimit(int output)
        {
            if (!SelectOutput(output)) return I_OUTPUT_INVALID;

            return ReadValue("CURR?");
        }

        public int EnableOutputs(bool value)
        {
            if (io.IO == null) throw new InvalidOperationException();

            if (value)
            {
                io.WriteString("OUTP CH1,ON");
                io.WriteString("OUTP CH2,ON");
            }
            else
            {
                io.WriteString("OUTP CH1,OFF");
                io.WriteString("OUTP CH2,OFF");
            }
            return 0;
        }

        public int EnableOutput(int output, bool value)
        {
            if (!SelectOutput(output)) return I_OUTPUT_INVALID;

            if (value)
            {
                io.WriteString("OUTP ON");
            }
            else
            {
                io.WriteString("OUTP OFF");
            }
            return 0;
        }

        public int GetOutputCount()
        {
            return devInfo.outputs;
        }

        public bool isOutputEnabled(int output)
        {
            if (io.IO == null) return false;

            if (output == -1)
            {   /* select all */
                io.WriteString("OUTP? CH1");
                bool ch1 = io.ReadString().Trim() == "OFF";
                io.WriteString("OUTP? CH2");
                bool ch2 = io.ReadString().Trim() == "OFF";

                return !(ch1 && ch2);
            }

            if (!SelectOutput(output)) return false;

            io.WriteString("OUTP?");
            return io.ReadString().Trim() != "OFF";
        }

        public int Connect(string connectionInformation)
        {
            if (cbResource == null) return 1;

            if (io.IO != null)
            {
                io.IO.Close();
                io.IO = null;
            }

            string resource = cbResource.Text;
            if (resource == "")
            {
                string[] resources = rm.FindRsrc("USB?::0x1AB1::0x0E11?*");
                if (resources.Length == 0)
                {
                    return 1;
                }

                resource = resources[0];
            }

            io.IO = (IMessage) rm.Open(resource);

            io.WriteString("*IDN?");
            string s = io.ReadString();

            if (!s.Contains("DP8"))
            {
                return 1;
            }

            io.WriteString("SYST:REM");
            ClearErrorLog();
            ReadDeviceInformation();

            lastOutput = 0;
            lastVoltages[0] = GetVoltage(1);
            lastVoltages[1] = GetVoltage(2);

            DataTable mySettings;
            mySettings = new DataTable("SettingsPluginDP800");
            mySettings.Columns.Add("Resource", typeof(string));
            mySettings.Rows.Add(new object[1] { resource });
            mySettings.WriteXml(configDir + Path.DirectorySeparatorChar + "config_plugin_dp800.xml", XmlWriteMode.WriteSchema);

            return 0;
        }

        private bool SelectOutput(int output)
        {
            if (io.IO == null) return false;

            if (output > devInfo.outputs || output < 1) return false;

            if (lastOutput != output)
            {
                io.WriteString("INST CH" + output.ToString());
                lastOutput = output;
                System.Threading.Thread.Sleep(50);
            }

            return true;
        }

        private int GetLastError()
        {
            io.WriteString("SYST:ERR?");
            string result = io.ReadString();

            return int.Parse(result.Split(',')[0]);
        }

        private void ClearErrorLog()
        {
            while (GetLastError() != 0) ;
        }

        public int Disconnect()
        {
            if (io.IO != null)
            {
                ClearErrorLog();
                io.WriteString("SYST:LOC");

                io.IO.Close();
                io.IO = null;
            }

            return 0;
        }

        private float ReadValue(string cmd)
        {
            io.WriteString(cmd);
            return float.Parse(io.ReadString(), CultureInfo.InvariantCulture);
        }

        public int ConnectionSettingSetup(Object panel, string confDir)
        {
            Panel pPanel = (Panel)panel;
            configDir = confDir;

            string resource;

            try
            {
                // Create Settings Table
                DataTable mySettings = new DataTable("SettingsPluginDP800");
                mySettings.ReadXml(configDir + Path.DirectorySeparatorChar + "config_plugin_dp800.xml");

                resource = mySettings.Rows[0]["Resource"].ToString();
            }
            catch
            {
                resource = "";
            }

            Label lblResource = new Label();
			lblResource.Text = "Resource:";
			lblResource.Top = 8;
			lblResource.Left = 5;
			lblResource.Width = 70;
			pPanel.Controls.Add(lblResource);

            cbResource = new ComboBox();
            cbResource.Top = 8;
            cbResource.Left = 84;
            cbResource.Width = 368;
            pPanel.Controls.Add(cbResource);

            cbResource.Items.AddRange(rm.FindRsrc("USB?::0x1AB1::0x0E11?*"));

            return 0;
        }

        public int AdvancedSettingsSetup(Object panel)
        {
            throw new NotImplementedException();
        }

        private void ReadDeviceInformation()
        {
            io.WriteString("*IDN?");
            string s = io.ReadString();
            string[] sList = s.Split(',');
            devInfo.producer = sList[0].Trim();
            devInfo.type = sList[1].Trim();
            devInfo.serialNo = sList[2].Trim();
            devInfo.softwareRevision = sList[3].Trim();

            /* min/max - Werte abfragen */
            devInfo.currentLimitMin = devInfo.currentMin = ReadValue("CURR? MIN");
            devInfo.currentLimitMax = devInfo.currentMax = ReadValue("CURR? MAX");
            devInfo.voltageLimitMin = devInfo.voltageMin = ReadValue("VOLT? MIN");
            devInfo.voltageLimitMax = devInfo.voltageMax = ReadValue("VOLT? MAX");
            devInfo.voltageOVPMin = ReadValue("VOLT:PROT? MIN");
            devInfo.voltageOVPMax = ReadValue("VOLT:PROT? MAX");

            // We have three outputs, but output 3 is limited
            devInfo.outputs = 2;
        }

        public DeviceInformation GetDeviceInformation()
        {
            return devInfo;
        }

        public int GetMinimalUpdateTimeMS()
        {
            return 20;
        }
    }
}