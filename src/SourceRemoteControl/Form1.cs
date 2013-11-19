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
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using SourcePluginInterface;
using ClassDeviceInformation;
using FunctionPluginInterface;
using CheckPluginInterface;

namespace SourceRemoteControl
{
    public partial class Form1 : Form
    {
        static readonly object _timerLock = new object();
        delegate void addTextCallback(string text);
        
        List<InterfaceSourcePlugin> list_sourcePlugins = new List<InterfaceSourcePlugin>();
        InterfaceSourcePlugin selSourcePlugin = null;

        List<InterfaceFunctionPlugin> list_functionPlugins = new List<InterfaceFunctionPlugin>();
        InterfaceFunctionPlugin selFunctionPlugin = null;

        List<InterfaceCheckPlugin> list_checkPlugins = new List<InterfaceCheckPlugin>();
        List<InterfaceCheckPlugin> list_activeCheckPlugins = new List<InterfaceCheckPlugin>();
        List<Type> list_checkPluginTypes = new List<Type>();
        List<ComboBox> list_cbCheckPlugins = new List<ComboBox>();
        List<Button> list_btnCheckPluginsSetup = new List<Button>();
        List<Button> list_btnCheckPluginRunManual = new List<Button>();

        List<NumericUpDown> list_numUDVoltage = new List<NumericUpDown>();
        List<NumericUpDown> list_numUDCurrentLimits= new List<NumericUpDown>();
        List<Button> list_btnOutputs = new List<Button>();

        string optConfigDirectory;

        public Form1()
        {
            InitializeComponent();
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            optConfigDirectory = config.FilePath;
            optConfigDirectory = Path.GetDirectoryName(optConfigDirectory);
            if (Directory.Exists(optConfigDirectory) == false)
                Directory.CreateDirectory(optConfigDirectory);

            loadPlugins();

            string sourceName, functionName;
            int checkCount;
            List<string> checks = new List<string>();

            /** 
             * load settings 
             */
            try
            {
                // Create Settings Table
                DataTable mySettings = new DataTable("SettingsSourceRemoteControl");
                mySettings.ReadXml(optConfigDirectory + "\\config_user.xml");

                sourceName = mySettings.Rows[0]["SourcePluginName"].ToString();
                functionName = mySettings.Rows[0]["SelectedFunctionPluginName"].ToString();
                checkCount = Convert.ToInt32(mySettings.Rows[0]["CheckCount"].ToString(), 10);
                for (int i = 0; i < nudCheckCount.Maximum; i++)
                {
                    checks.Add(mySettings.Rows[0]["Check" + i.ToString()].ToString());
                }
            }
            catch
            {
                sourceName = "";
                functionName = "";
                checkCount = 0;
                for (int i = 0; i < nudCheckCount.Maximum; i++)
                {
                    checks.Add("");
                }
            }

            cbSourceType.SelectedIndex = cbSourceType.Items.IndexOf(sourceName);
            nudCheckCount.Value = checkCount;
            for (int i = 0; i < checkCount; i++)
            {
                list_cbCheckPlugins[i].SelectedIndex = list_cbCheckPlugins[i].Items.IndexOf(checks[i]);
            }
            lboxFunctions.SelectedIndex = lboxFunctions.Items.IndexOf(functionName);
        }

        private void loadPlugins()
        {
            string exepath = Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string pluginDir = Path.Combine(Path.GetDirectoryName(exepath), "Plugins");
            if (Directory.Exists(pluginDir))
            {
                addText("Loading plugins ...\r\n");
                foreach (string s in Directory.GetFiles(pluginDir, "*.dll"))
                {
                    Type[] pluginTypes = Assembly.LoadFile(s).GetTypes();
                    foreach (Type t in pluginTypes)
                    {
                        if (t.ToString().Contains("SourcePlugin"))
                        {
                            InterfaceSourcePlugin plugin = Activator.CreateInstance(t) as InterfaceSourcePlugin;
                            if(plugin.GetType().IsSubclassOf(typeof(InterfaceSourcePlugin))==true) addText("is class of isp\r\n");
                            else if(plugin.GetType() == typeof(InterfaceSourcePlugin)) addText("is class of isp 2\r\n");
                            list_sourcePlugins.Add(plugin);
                            addText(plugin.GetPluginName() + "\r\n");
                            cbSourceType.Items.Add(plugin.GetPluginName());
                            break;
                        }
                        else if (t.ToString().Contains("FunctionPlugin"))
                        {
                            InterfaceFunctionPlugin plugin = Activator.CreateInstance(t) as InterfaceFunctionPlugin;
                            if (plugin.GetType().IsSubclassOf(typeof(InterfaceFunctionPlugin)) == true) addText("is class of isp\r\n");
                            else if (plugin.GetType() == typeof(InterfaceFunctionPlugin)) addText("is class of isp 2\r\n");
                            list_functionPlugins.Add(plugin);
                            addText(plugin.getPluginName() + "\r\n");
                            lboxFunctions.Items.Add(plugin.getPluginName());
                            break;
                        }
                        else if (t.ToString().Contains("CheckPlugin"))
                        {
                            InterfaceCheckPlugin plugin = Activator.CreateInstance(t) as InterfaceCheckPlugin;
                            if (plugin.GetType().IsSubclassOf(typeof(InterfaceCheckPlugin)) == true) addText("is class of isp\r\n");
                            else if (plugin.GetType() == typeof(InterfaceCheckPlugin)) addText("is class of isp 2\r\n");
                            list_checkPlugins.Add(plugin);
                            list_checkPluginTypes.Add(t);
                            addText(plugin.GetPluginName() + "\r\n");
                            //lboxFunctions.Items.Add(plugin.getPluginName());
                            break;
                        }
                    }
                }
            }
            //if (list_sourcePlugins.Count > 0) cbSourceType.SelectedIndex = 0;
        }

        private void btnConnection_Click(object sender, EventArgs e)
        {
            if (selSourcePlugin == null) return;
            if (selSourcePlugin.Connect("egal was hier steht") > 0)
            {
                addText("Error connecting to source\r\n");
                return;
            }
            //tbOut.AppendText("Current Voltage: " + selSourcePlugin.GetVoltage(1).ToString("F2") + "V\r\n");
            addVoltageControls();
            addCurrentControls();
            addOutputControls();
            int old_selected = lboxFunctions.SelectedIndex;
            lboxFunctions.SelectedIndex = -1;
            lboxFunctions.SelectedIndex = old_selected;
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (selSourcePlugin != null)
            {
                selSourcePlugin.Disconnect();
                gboxVoltage.Controls.Clear();
                gboxVoltage.Hide();
                gbCurrentLimits.Controls.Clear();
                gbCurrentLimits.Hide();
                gbOutputs.Controls.Clear();
                gbOutputs.Hide();
            }
        }

        private void addText(string text)
		{
			// InvokeRequired required compares the thread ID of the// calling thread to the thread ID of the creating thread.// If these threads are different, it returns true.if (this.textBox1.InvokeRequired)
            if (this.tbOut.InvokeRequired)
            {	
				addTextCallback d = new addTextCallback(addText);
				this.Invoke(d, new object[] { text });
			}
			else
			{
                this.tbOut.AppendText(DateTime.Now.ToString("[HH:mm:ss.fff] "));
				this.tbOut.AppendText(text);
                this.tbOut.SelectionStart = this.tbOut.Text.Length;
                this.tbOut.ScrollToCaret();  
			}
		}

        private void btnTest_Click(object sender, EventArgs e)
        {
            /* if (tabControl1.TabPages.IndexOf(tabPage2) > 0) tabControl1.TabPages.Remove(tabPage2);
            else tabControl1.TabPages.Add(tabPage2);
             */
            runCheck();

            
            /*foreach (InterfaceSourcePlugin plugin in list_sourcePlugins)
            {
                addText(plugin.GetPluginDate());
            } */
            /*TabPage tabPage1 = new TabPage();
            tabPage1.Text = "Tester";
            Button btnTester = new Button();
            btnTester.Text = "TesterBtn";
            btnTester.Click += new System.EventHandler(dynamicbutton_Click);

            tabPage1.Controls.Add(btnTester);

            tabControl1.Controls.Add(tabPage1);*/
        }

        private void addCurrentControls()
        {
            if (selSourcePlugin == null) return;

            gbCurrentLimits.Controls.Clear();
            gbCurrentLimits.Show();
            list_numUDCurrentLimits.Clear();
            Button btn = new Button();
            btn.Width = 75;
            btn.Height = 23;
            btn.Text = "Setzen";
            btn.Left = 402;
            btn.Top = 21;
            btn.Click += new System.EventHandler(btn_setCurrentLimits_Click);
            gbCurrentLimits.Controls.Add(btn);
            for (int i = 0; i < selSourcePlugin.GetOutputCount(); i++)
            {
                NumericUpDown tmp = new NumericUpDown();
                Label lTmp = new Label();
                lTmp.Width = 13;
                lTmp.Top = 26;
                lTmp.Left = 6 + 100 * i;
                lTmp.Text = (i + 1).ToString();
                gbCurrentLimits.Controls.Add(lTmp);
                tmp.Width = 74;
                tmp.Height = 20;
                tmp.Top = 24;
                tmp.Left = 25 + 100 * i;
                tmp.DecimalPlaces = 2;
                gbCurrentLimits.Controls.Add(tmp);
                tmp.KeyPress += new System.Windows.Forms.KeyPressEventHandler(numericCurrentUpDown);
                list_numUDCurrentLimits.Add(tmp);
            }
            updateCurrentLimitControls();
        }

        private void numericCurrentUpDown(object sender, KeyPressEventArgs e)
        {
            int index = list_numUDCurrentLimits.IndexOf((NumericUpDown)sender);
            if (e.KeyChar == (char)13)
            {
                if (selSourcePlugin == null) return;
                selSourcePlugin.SetCurrentLimit((index+1), (float)((NumericUpDown)sender).Value);
            }
        }

        private void btn_setCurrentLimits_Click(Object sender, System.EventArgs e)
        {
            if (selSourcePlugin == null) return;
            for (int i = 0; i < list_numUDCurrentLimits.Count; i++)
            {
                selSourcePlugin.SetCurrentLimit((i+1), (float)list_numUDCurrentLimits[i].Value);
            }
        }

        private void updateCurrentLimitControls()
        {
            if (selSourcePlugin == null) return;
            for (int i = 0; i < selSourcePlugin.GetOutputCount(); i++)
            {
                list_numUDCurrentLimits[i].Value = (decimal)selSourcePlugin.GetCurrentLimit((i+1));
            }
        }

        private void addVoltageControls() {
            if (selSourcePlugin == null) return;

            gboxVoltage.Controls.Clear();
            gboxVoltage.Show();
            list_numUDVoltage.Clear();
            Button btn = new Button();
            btn.Width = 75;
            btn.Height = 23;
            btn.Text = "Setzen";
            btn.Left = 402;
            btn.Top = 21;
            btn.Click += new System.EventHandler(btn_setVoltage_Click);
            gboxVoltage.Controls.Add(btn);
            for (int i = 0; i < selSourcePlugin.GetOutputCount(); i++)
            {
                NumericUpDown tmp = new NumericUpDown();
                Label lTmp = new Label();
                lTmp.Width = 13;
                lTmp.Top = 26;
                lTmp.Left = 6 + 100 * i;
                lTmp.Text = (i+1).ToString();
                gboxVoltage.Controls.Add(lTmp);
                tmp.Width  = 74;
                tmp.Height = 20;
                tmp.Top = 24;
                tmp.Left = 25 + 100 * i;
                tmp.DecimalPlaces = 2;
                gboxVoltage.Controls.Add(tmp);
                tmp.KeyPress += new System.Windows.Forms.KeyPressEventHandler(numericVoltageUpDown);
                list_numUDVoltage.Add(tmp);
            }
            updateVoltageControls();
        }

        private void numericVoltageUpDown(object sender, KeyPressEventArgs e)
        {
            int index = list_numUDVoltage.IndexOf((NumericUpDown) sender);
            if (e.KeyChar == (char)13)
            {
                if (selSourcePlugin == null) return;
                selSourcePlugin.SetVoltage((index+1), (float) ((NumericUpDown)sender).Value);
            }
        }

        private void btn_setVoltage_Click(Object sender, System.EventArgs e)
        {
            if (selSourcePlugin == null) return;
            for (int i = 0; i < list_numUDVoltage.Count; i++)
            {
                selSourcePlugin.SetVoltage((i+1), (float) list_numUDVoltage[i].Value);
            }
        }

        private void updateVoltageControls()
        {
            if (selSourcePlugin == null) return;
            for (int i = 0; i < selSourcePlugin.GetOutputCount(); i++)
            {
                list_numUDVoltage[i].Value = (decimal) selSourcePlugin.GetVoltage((i+1));
            }
        }

        public void addOutputControls()
        {
            bool isGlobalOn = selSourcePlugin.isOutputEnabled(-1);
            gbOutputs.Controls.Clear();
            gbOutputs.Show();
            list_btnOutputs.Clear();
            Button btn = new Button();
            btn.Width = 75;
            btn.Height = 23;
            if (isGlobalOn)
                btn.Text = "Ausschalten";
            else
                btn.Text = "Einschalten";
            btn.Left = 402;
            btn.Top = 21;
            btn.Click += new System.EventHandler(btn_OutputChange_Click);
            gbOutputs.Controls.Add(btn);
            list_btnOutputs.Add(btn);
            for (int i = 0; i < selSourcePlugin.GetOutputCount(); i++)
            {
                Label lTmp = new Label();
                lTmp.Width = 13;
                lTmp.Top = 26;
                lTmp.Left = 6 + 100 * i;
                lTmp.Text = (i + 1).ToString();
                gbOutputs.Controls.Add(lTmp);
                Button tmp = new Button();
                tmp.Width = 74;
                tmp.Height = 20;
                tmp.Top = 24;
                tmp.Left = 25 + 100 * i;
                if(selSourcePlugin.isOutputEnabled((i+1)))
                    tmp.Text = "Ausschalten";
                else
                    tmp.Text = "Einschalten";    
                tmp.Enabled = isGlobalOn;
                tmp.Click += new System.EventHandler(btn_OutputChange_Click);
                gbOutputs.Controls.Add(tmp);
                list_btnOutputs.Add(tmp);
            }
        }

        private void btn_OutputChange_Click(Object sender, System.EventArgs e)
        {
            Button btn = (Button) sender;
            int index = list_btnOutputs.IndexOf(btn);
            
            if(index == 0) {
                /* globaler Schalter */
                bool subBtnEnable = false;
                if(btn.Text == "Einschalten") {
                    selSourcePlugin.EnableOutputs(true);
                    btn.Text = "Ausschalten";
                    subBtnEnable = true;
                }
                else {
                    selSourcePlugin.EnableOutputs(false);
                    btn.Text = "Einschalten";
                    subBtnEnable = false;
                }

                for (int i = 1; i < list_btnOutputs.Count; i++)
                {
                    list_btnOutputs[i].Enabled = subBtnEnable;
                }
            }
            else {
                if (selSourcePlugin.isOutputEnabled(-1))
                {
                    if (btn.Text == "Einschalten")
                    {
                        selSourcePlugin.EnableOutput(index, true);
                        btn.Text = "Ausschalten";
                    }
                    else
                    {
                        selSourcePlugin.EnableOutput(index, false);
                        btn.Text = "Einschalten";
                    }
                }
            }
        }

        private void cbSourceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            /* lösche bestehende Elemente */
            panelSettings.Controls.Clear();
            /* füge vom aktuell selectierten Plugin hinzu */
            selSourcePlugin = list_sourcePlugins[cbSourceType.SelectedIndex];
            selSourcePlugin.ConnectionSettingSetup(panelSettings, optConfigDirectory);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            /* beende laufende Funktion */
            if (selFunctionPlugin != null)
                selFunctionPlugin.stopFromApplication();
            /* disconnecte von source */
            btnDisconnect_Click(sender, e);

            /** speichere Einstellungen */
            DataTable mySettings;

            // Create DataTable for Settings
            mySettings = new DataTable("SettingsSourceRemoteControl");
            mySettings.Columns.Add("SourcePluginName", typeof(string));
            mySettings.Columns.Add("SelectedFunctionPluginName", typeof(string));
            mySettings.Columns.Add("CheckCount", typeof(decimal));
            mySettings.Columns.Add("Check0", typeof(string));
            mySettings.Columns.Add("Check1", typeof(string));
            mySettings.Columns.Add("Check2", typeof(string));
            mySettings.Columns.Add("Check3", typeof(string));
            mySettings.Columns.Add("Check4", typeof(string));
            mySettings.Columns.Add("Check5", typeof(string));
            mySettings.Columns.Add("Check6", typeof(string));
            mySettings.Columns.Add("Check7", typeof(string));
            mySettings.Columns.Add("Check8", typeof(string));
            mySettings.Columns.Add("Check9", typeof(string));
            mySettings.Columns.Add("Check10", typeof(string));
            mySettings.Columns.Add("Check11", typeof(string));

            mySettings.Rows.Add(new object[15] { 
                "",
                "",
                0, 
                "", "", "", "", 
                "", "", "", "", 
                "", "", "", ""
            });

            if (selSourcePlugin != null) 
                mySettings.Rows[0]["SourcePluginName"] = selSourcePlugin.GetPluginName();
            if (selFunctionPlugin != null)
                mySettings.Rows[0]["SelectedFunctionPluginName"] = selFunctionPlugin.getPluginName();
            mySettings.Rows[0]["CheckCount"] = nudCheckCount.Value;
            for (int i = 0; i < list_activeCheckPlugins.Count; i++)
            {
                if (list_activeCheckPlugins[i] != null)
                    mySettings.Rows[0]["Check" + i.ToString()] = list_activeCheckPlugins[i].GetPluginName();
                else
                    mySettings.Rows[0]["Check" + i.ToString()] = "";
            }
            mySettings.WriteXml(optConfigDirectory + "\\config_user.xml", XmlWriteMode.WriteSchema);
        }

        private void lboxFunctions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selSourcePlugin == null)
            {
                lboxFunctions.SelectedIndex = -1;
            }
            else
            {
                panelFunction.Controls.Clear();
                if (lboxFunctions.SelectedIndex == -1) {
                    selFunctionPlugin = null;
                }
                else {
                    selFunctionPlugin = list_functionPlugins[lboxFunctions.SelectedIndex];
                    selFunctionPlugin.initFunction(panelFunction, selSourcePlugin, this.addText, this.runCheck, optConfigDirectory);
                }
            }
        }

        private int runCheck()
        {
            int retval = 0;
            for (int i = 0; i < list_activeCheckPlugins.Count; i++)
            {
                if (list_activeCheckPlugins[i] != null)
                {
                    retval |= list_activeCheckPlugins[i].runCheck();
                    addText("Run check on \"" + list_activeCheckPlugins[i].GetPluginName() + "\" at instance "  + i.ToString() +  " with result: " + retval.ToString() + "\r\n");
                    if (retval != 0)
                        break;
                }
            }
            return retval;
        }

        private void nudCheckCount_ValueChanged(object sender, EventArgs e)
        {
            panelChecks.Controls.Clear();
            list_activeCheckPlugins.Clear(); /* delete all active check plugins */
            list_cbCheckPlugins.Clear();
            list_btnCheckPluginsSetup.Clear();
            list_btnCheckPluginRunManual.Clear();

            for(int i = 0; i<nudCheckCount.Value; i++) {
                list_activeCheckPlugins.Add(null);

                Label lbl = new Label();
                lbl.Width = 36;
                lbl.Height = 13;
                lbl.Left = 12;
                lbl.Top = 10 + i * 30;
                lbl.Text = "Plugin";
                panelChecks.Controls.Add(lbl);

                ComboBox cb = new ComboBox();
                cb.Width = 121;
                cb.Height = 21;
                cb.Left = 60;
                cb.Top = 7 + i*30;
                list_checkPlugins.ForEach( delegate(InterfaceCheckPlugin plugin) {
                    cb.Items.Add(plugin.GetPluginName());
                });
                cb.SelectedIndexChanged += new System.EventHandler(this.cbCheckPlugin_SelectedIndexChanged);
                list_cbCheckPlugins.Add(cb);
                panelChecks.Controls.Add(cb);

                Button btn = new Button();
                btn.Width = 75;
                btn.Height = 23;
                btn.Left = 187;
                btn.Top = 5 + i * 30;
                btn.Text = "Setup";
                btn.Click += new System.EventHandler(btnCheckPluginSetup_Click);
                list_btnCheckPluginsSetup.Add(btn);
                panelChecks.Controls.Add(btn);

                btn = new Button();
                btn.Width = 75;
                btn.Height = 23;
                btn.Left = 267;
                btn.Top = 5 + i * 30;
                btn.Text = "Run Test";
                btn.Click += new System.EventHandler(btnCheckPluginManualTest_Click);
                list_btnCheckPluginRunManual.Add(btn);
                panelChecks.Controls.Add(btn);
            }
        }

        private void cbCheckPlugin_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox) sender;
            int index = list_cbCheckPlugins.IndexOf(cb);

            //addText("Plugin changed");
            //addText(cb.SelectedItem.ToString() + " SelIndex:" + cb.SelectedIndex.ToString() + " CB-Index: " + index.ToString());

            InterfaceCheckPlugin plugin = Activator.CreateInstance(list_checkPluginTypes[cb.SelectedIndex]) as InterfaceCheckPlugin;
            list_activeCheckPlugins[index] = plugin;
            list_activeCheckPlugins[index].initCheckPlugin(index, addText, optConfigDirectory);
            //addText(list_activeCheckPlugins[index].GetPluginName() + "\r\n");
        }

        private void btnCheckPluginManualTest_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int index = list_btnCheckPluginRunManual.IndexOf(btn);
            if (list_activeCheckPlugins[index] != null)
            {
                int retval = list_activeCheckPlugins[index].runCheck();
                addText("Run check on \"" + list_activeCheckPlugins[index].GetPluginName() + "\" with result: " + retval.ToString());
            }
        }

        private void btnCheckPluginSetup_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int index = list_btnCheckPluginsSetup.IndexOf(btn);
            if (list_activeCheckPlugins[index] != null)
            {
                addText("have to call plugin setup for " + index.ToString() + "\r\n");
                list_activeCheckPlugins[index].setupCheckPlugin();
            }
        }

        private void btnLogClear_Click(object sender, EventArgs e)
        {
            tbOut.Clear();
        }

        private void btnLogSave_Click(object sender, EventArgs e)
        {
            Stream s = null;
            StreamWriter sw = null;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Log Datei|*.log|Textdatei|*.txt|All Files|*.*";
            saveFileDialog.Title = "Save Log to File";
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                s = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate);
                sw = new StreamWriter(s);
                sw.Write(tbOut.Text);

                sw.Close();
                s.Close();
            }
        }

    }
}
