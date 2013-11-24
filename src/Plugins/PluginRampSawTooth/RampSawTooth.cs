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
using System.Threading;
using System.Windows.Forms;
using FunctionPluginInterface;
using SourcePluginInterface;
using ClassDeviceInformation;

namespace PluginRampTrapezoidal
{
    public class FunctionPlugin : InterfaceFunctionPlugin
    {
        delegate void btnSetEnableCallback(Button btn, bool enable);
        delegate void lblSetTextCallback(Label lbl, string text);

        private NumericUpDown nudVStart;
        private NumericUpDown nudVStop;
        private NumericUpDown nudtRamp;
        private NumericUpDown nudVRamp;
        private NumericUpDown nudDelayOff;
        private NumericUpDown nudDelayOn;
        private NumericUpDown nudCounts;
        private NumericUpDown nudOutput;
        private Button btnStop;
        private Button btnStart;
        private Label lblStatus;
        private Label lblVoltage;
        private CheckBox cbTestRun;
        private CheckBox cbTestStopWhenFailed;

        private InterfaceSourcePlugin curSource;

        private bool stopRamp = false;

        private addText addTextCallback;
        private runCheckFP runCheckCB;

        private string confDir;

        public void initFunction(object panel, SourcePluginInterface.InterfaceSourcePlugin source, addText callback, runCheckFP runchkCB, string configDir)
        {
            Panel pPanel = (Panel) panel;
            addTextCallback = callback;
            curSource = source;
            runCheckCB = runchkCB;
            confDir = configDir;
            DeviceInformation devInfo = curSource.GetDeviceInformation();

            float startSpg, stopSpg, rampSpg;
            int tRamp, tDelayOn, tDelayOff, counts, output;
            bool checkRun, checkStopFailed;
            /** 
             * load settings 
             */
            try
            {
                // Create Settings Table
                DataTable mySettings = new DataTable("SettingsPluginRampSawTooth");
                mySettings.ReadXml(confDir + Path.DirectorySeparatorChar + "config_plugin_rampsawtooth.xml");
                
                startSpg = (float)Convert.ToDecimal(mySettings.Rows[0]["VoltageStart"].ToString());
                stopSpg = (float)Convert.ToDecimal(mySettings.Rows[0]["VoltageStop"].ToString());
                rampSpg = (float)Convert.ToDecimal(mySettings.Rows[0]["VoltageRampDelta"].ToString());
                tRamp = Convert.ToInt32(mySettings.Rows[0]["DelayRamp"].ToString(), 10);
                tDelayOn = Convert.ToInt32(mySettings.Rows[0]["DelayOn"].ToString(), 10);
                tDelayOff = Convert.ToInt32(mySettings.Rows[0]["DelayOff"].ToString(), 10);
                counts = Convert.ToInt32(mySettings.Rows[0]["Counts"].ToString(), 10);
                output = Convert.ToInt32(mySettings.Rows[0]["Output"].ToString(), 10);
                checkRun = Convert.ToBoolean(mySettings.Rows[0]["RunCheck"].ToString());
                checkStopFailed = Convert.ToBoolean(mySettings.Rows[0]["StopOnFailedCheck"].ToString());
            }
            catch
            {
                startSpg = (float) 0.0;
                stopSpg = (float)24.0;
                rampSpg = (float)0.05;
                tRamp = 100;
                tDelayOn = 10000;
                tDelayOff = 10000;
                counts = 50;
                output = 1;
                checkRun = false;
                checkStopFailed = false;
            }
            
            /**
             * create for entries
             */
            Label lbl = new Label();
            lbl.Text = "Start-Spg [V]";
            lbl.Top = 10;
            lbl.Left = 5;
            lbl.Width = 85;
            lbl.Height = 13;
            pPanel.Controls.Add(lbl);

            nudVStart = new NumericUpDown();
            nudVStart.Left   = 95;
            nudVStart.Top    =  8;
            nudVStart.Width  = 74;
            nudVStart.Height = 20;
            nudVStart.DecimalPlaces = 2;
            nudVStart.Minimum = (decimal) devInfo.voltageMin;
            nudVStart.Maximum = (decimal) devInfo.voltageMax;
            if(startSpg >= devInfo.voltageMin && startSpg <= devInfo.voltageMax)
                nudVStart.Value = (decimal) startSpg;
            else
                nudVStart.Value = (decimal)devInfo.voltageMin;
            pPanel.Controls.Add(nudVStart);

            lbl = new Label();
            lbl.Text = "Stop-Spg [V]";
            lbl.Top = 10;
            lbl.Left = 182;
            lbl.Width = 80;
            lbl.Height = 13;
            pPanel.Controls.Add(lbl);

            nudVStop = new NumericUpDown();
            nudVStop.Left = 263;
            nudVStop.Top = 8;
            nudVStop.Width = 74;
            nudVStop.Height = 20;
            nudVStop.DecimalPlaces = 2;
            nudVStop.Minimum = (decimal)devInfo.voltageMin;
            nudVStop.Maximum = (decimal)devInfo.voltageMax;
            if (stopSpg >= devInfo.voltageMin && stopSpg <= devInfo.voltageMax)
                nudVStop.Value = (decimal)stopSpg;
            else
                nudVStop.Value = (decimal)devInfo.voltageMax;
            pPanel.Controls.Add(nudVStop);

            lbl = new Label();
            lbl.Text = "tRamp[ms]";
            lbl.Top = 36;
            lbl.Left = 5;
            lbl.Width = 61;
            lbl.Height = 13;
            pPanel.Controls.Add(lbl);

            nudtRamp = new NumericUpDown();
            nudtRamp.Left = 95;
            nudtRamp.Top = 34;
            nudtRamp.Width = 74;
            nudtRamp.Height = 20;
            nudtRamp.Minimum = curSource.GetMinimalUpdateTimeMS();
            nudtRamp.Maximum = 99999;
            if (tRamp >= nudtRamp.Minimum && tRamp <= nudtRamp.Maximum)
                nudtRamp.Value = tRamp;
            else
                nudtRamp.Value = nudtRamp.Minimum;
            pPanel.Controls.Add(nudtRamp);

            lbl = new Label();
            lbl.Text = "URamp [V]";
            lbl.Top = 36;
            lbl.Left = 182;
            lbl.Width = 67;
            lbl.Height = 13;
            pPanel.Controls.Add(lbl);

            nudVRamp = new NumericUpDown();
            nudVRamp.Left = 263;
            nudVRamp.Top = 34;
            nudVRamp.Width = 74;
            nudVRamp.Height = 20;
            nudVRamp.Minimum = (decimal) 0.01;
            nudVRamp.Increment = (decimal)0.1;
            nudVRamp.DecimalPlaces = 2;
            nudVRamp.Maximum = 200;
            if ((decimal)rampSpg >= nudVRamp.Minimum && (decimal) rampSpg <= nudVRamp.Maximum)
                nudVRamp.Value = (decimal)rampSpg;
            else
                nudVRamp.Value = nudVRamp.Minimum;

            pPanel.Controls.Add(nudVRamp);

            lbl = new Label();
            lbl.Text = "tDelayOn[ms]";
            lbl.Top = 62;
            lbl.Left = 5;
            lbl.Width = 70;
            lbl.Height = 13;
            pPanel.Controls.Add(lbl);

            nudDelayOn = new NumericUpDown();
            nudDelayOn.Left = 95;
            nudDelayOn.Top = 60;
            nudDelayOn.Width = 74;
            nudDelayOn.Height = 20;
            nudDelayOn.Minimum = curSource.GetMinimalUpdateTimeMS();
            nudDelayOn.Maximum = 999999;
            if (tDelayOn >= nudDelayOn.Minimum && tDelayOn <= nudDelayOn.Maximum)
                nudDelayOn.Value = tDelayOn;
            else
                nudDelayOn.Value = nudDelayOn.Minimum;

            pPanel.Controls.Add(nudDelayOn);

            lbl = new Label();
            lbl.Text = "tDelayOff[ms]";
            lbl.Top = 62;
            lbl.Left = 182;
            lbl.Width = 80;
            lbl.Height = 13;
            pPanel.Controls.Add(lbl);

            nudDelayOff = new NumericUpDown();
            nudDelayOff.Left = 263;
            nudDelayOff.Top = 60;
            nudDelayOff.Width = 74;
            nudDelayOff.Height = 20;
            nudDelayOff.Minimum = curSource.GetMinimalUpdateTimeMS();
            nudDelayOff.Maximum = 999999;
            if (tDelayOff >= nudDelayOff.Minimum && tDelayOff <= nudDelayOff.Maximum)
                nudDelayOff.Value = tDelayOff;
            else
                nudDelayOff.Value = nudDelayOff.Minimum;
            pPanel.Controls.Add(nudDelayOff);

            lbl = new Label();
            lbl.Text = "Wiederholungen";
            lbl.Top = 88;
            lbl.Left = 5;
            lbl.Width = 85;
            lbl.Height = 13;
            pPanel.Controls.Add(lbl);

            nudCounts = new NumericUpDown();
            nudCounts.Left = 95;
            nudCounts.Top = 86;
            nudCounts.Width = 74;
            nudCounts.Height = 20;
            nudCounts.Minimum = 1;
            nudCounts.Maximum = 100000;
            if (counts >= nudCounts.Minimum && counts <= nudCounts.Maximum)
                nudCounts.Value = counts;
            else
                nudCounts.Value = nudCounts.Minimum;
            pPanel.Controls.Add(nudCounts);

            lbl = new Label();
            lbl.Text = "Ausgang";
            lbl.Top = 88;
            lbl.Left = 182;
            lbl.Width = 80;
            lbl.Height = 13;
            pPanel.Controls.Add(lbl);

            nudOutput = new NumericUpDown();
            nudOutput.Left = 263;
            nudOutput.Top = 86;
            nudOutput.Width = 74;
            nudOutput.Height = 20;
            nudOutput.Minimum = 1;
            nudOutput.Maximum = curSource.GetOutputCount();
            if (output >= nudOutput.Minimum && output <= nudOutput.Maximum)
                nudOutput.Value = output;
            else
                nudOutput.Value = nudOutput.Minimum;
            pPanel.Controls.Add(nudOutput);

            cbTestRun = new CheckBox();
            cbTestRun.Left = 5;
            cbTestRun.Top = 114;
            cbTestRun.Text = "Überprüfung durchführen";
            cbTestRun.Checked = checkRun;
            pPanel.Controls.Add(cbTestRun);

            cbTestStopWhenFailed = new CheckBox();
            cbTestStopWhenFailed.Left = 182;
            cbTestStopWhenFailed.Top = 114;
            cbTestStopWhenFailed.Width = 120;
            cbTestStopWhenFailed.Text = "bei Fehler anhalten";
            cbTestStopWhenFailed.Checked = checkStopFailed;
            pPanel.Controls.Add(cbTestStopWhenFailed);
            
            btnStart = new Button();
            btnStart.Left = 5;
            btnStart.Top = 140;
            btnStart.Width = 75;
            btnStart.Height = 23;
            btnStart.Text = "Start";
            btnStart.Click += new System.EventHandler(btnStart_Click);
            pPanel.Controls.Add(btnStart);

            btnStop = new Button();
            btnStop.Left = 90;
            btnStop.Top = 140;
            btnStop.Width = 75;
            btnStop.Height = 23;
            btnStop.Text = "Stop";
            btnStop.Enabled = false;
            btnStop.Click += new System.EventHandler(btnStop_Click);
            pPanel.Controls.Add(btnStop);

            lblStatus = new Label();
            lblStatus.Text = "Status:";
            lblStatus.Top = 166;
            lblStatus.Left = 5;
            lblStatus.Width = 280;
            lblStatus.Height = 13;
            pPanel.Controls.Add(lblStatus);

            lblVoltage = new Label();
            lblVoltage.Text = "Voltage:";
            lblVoltage.Top = 192;
            lblVoltage.Left = 5;
            lblVoltage.Width = 280;
            lblVoltage.Height = 13;
            pPanel.Controls.Add(lblVoltage);
        }


        public string getPluginName()
        {
            return "Sägezahn-Rampe";
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (curSource == null)
            {
                lblSetText(lblStatus, "Nicht mit Quelle verbunden!!\r\n");
                return;
            }
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            stopRamp = false;
            new Thread(delay).Start();

            /** speichere Einstellungen */
            DataTable mySettings;
            
            // Create DataTable for Settings
            mySettings = new DataTable("SettingsPluginRampSawTooth");
            mySettings.Columns.Add("VoltageStart", typeof(decimal));
            mySettings.Columns.Add("VoltageStop", typeof(decimal));
            mySettings.Columns.Add("VoltageRampDelta", typeof(decimal));
            mySettings.Columns.Add("DelayRamp", typeof(Int32));
            mySettings.Columns.Add("DelayOn", typeof(Int32));
            mySettings.Columns.Add("DelayOff", typeof(Int32));
            mySettings.Columns.Add("Counts", typeof(Int32));
            mySettings.Columns.Add("Output", typeof(Int32));
            mySettings.Columns.Add("RunCheck", typeof(Boolean));
            mySettings.Columns.Add("StopOnFailedCheck", typeof(Boolean));

            mySettings.Rows.Add(new object[10] { 
                nudVStart.Value,
                nudVStop.Value,
                nudVRamp.Value,
                nudtRamp.Value,
                nudDelayOn.Value,
                nudDelayOff.Value,
                nudCounts.Value,
                nudOutput.Value,
                cbTestRun.Checked,
                cbTestStopWhenFailed.Checked
            });
            
            mySettings.WriteXml(confDir + Path.DirectorySeparatorChar + "config_plugin_rampsawtooth.xml", XmlWriteMode.WriteSchema);

            
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            stopRamp = true;
        }

		private void sleepThread(int ms)
		{
			if(ms < 500) System.Threading.Thread.Sleep(ms);
			else 
			{
				while(ms >= 500 && !stopRamp) {
					ms -= 500;
					System.Threading.Thread.Sleep(500);
				}
				if(!stopRamp && ms>0) 
					System.Threading.Thread.Sleep(ms);
			}
		}

        private void delay()
        {
            float vStart = (float)nudVStart.Value;
            float vStop = (float)nudVStop.Value;
            float vStep = (float)nudVRamp.Value;
            int tStep = (int)nudtRamp.Value;
            int tDelayOn = (int)nudDelayOn.Value;
            int tDelayOff = (int)nudDelayOff.Value;
            int cycles = (int)nudCounts.Value;
            int output = (int)nudOutput.Value;
            float f = (float)0.0;
            float vBefore = curSource.GetVoltage(output);
            int i = 0;
            lblSetText(lblStatus, "Starte Rampe mit folgenden Einstellungen\r\nStart-Spannung: " + vStart.ToString("F2") + "V\r\nStop-Spannung: " + vStop.ToString("F2") + "V\r\n");

            while (cycles-- > 0 && !stopRamp)
            {
                i++;
                lblSetText(lblStatus, "Laufe Rampe: " + i.ToString());
                addTextCallback("Laufe Rampe: " + i.ToString() + "\r\n");
                f = vStart;
                while (Math.Abs(f - vStop) > (vStep - 0.002) && !stopRamp)
                {
                    curSource.SetVoltage(output, f);
                    lblSetText(lblVoltage, "Aktuelle Spannung: " + f.ToString("F2") + " V");
                    if (vStart < vStop) f += vStep;
                    else f -= vStep;
                    sleepThread(tStep);
                }

                curSource.SetVoltage(output, f);
                lblSetText(lblVoltage, "Aktuelle Spannung: " + f.ToString("F2") + " V");
                if (stopRamp == false) sleepThread(tDelayOn);
                if (stopRamp == false && cbTestRun.Checked)
                {
                    if (runCheckCB() != 0 && cbTestStopWhenFailed.Checked && stopRamp==false)
                    {
                        addTextCallback("Überprüfung fehlgeschlagen...stoppe Rampe\r\n");
                        btnSetEnable(btnStart, true);
                        btnSetEnable(btnStop, false);
                        return;
                    }
                }
                curSource.SetVoltage(output, vStart);
                lblSetText(lblVoltage, "Aktuelle Spannung: " + vStart.ToString("F2") + " V");
                if (stopRamp == false) sleepThread(tDelayOff);
            }

            curSource.SetVoltage(output, vBefore);
            lblSetText(lblVoltage, "Aktuelle Spannung: " + vBefore.ToString("F2") + " V");
            btnSetEnable(btnStart, true);
            btnSetEnable(btnStop, false);
        }

        private void lblSetText(Label lbl, string text)
        {
            if (lbl.InvokeRequired)
            {
                lblSetTextCallback d = new lblSetTextCallback(lblSetText);
                lbl.BeginInvoke(d, new object[] { lbl, text });
            }
            else
            {
                lbl.Text = text;
            }
        }

        private void btnSetEnable(Button btn, bool enable)
        {
            if (btn.InvokeRequired)
            {
                btnSetEnableCallback d = new btnSetEnableCallback(btnSetEnable);
                btn.BeginInvoke(d, new object[] { btn, enable });
            }
            else
            {
                btn.Enabled = enable;
            }
        }

        public void stopFromApplication()
        {
            stopRamp = true;
        }
    }
}
