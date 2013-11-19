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
using System.Linq;
using System.Text;
using ClassDeviceInformation;

namespace SourcePluginInterface
{
    public interface InterfaceSourcePlugin
    {
        string GetPluginName();

        string GetPluginDate();

        int GetPluginVersion();

        int SetVoltage(int output, float voltage);

        float GetVoltage(int output);

        int SetCurrentLimit(int output, float current);

        float GetCurrentLimit(int output);

        int EnableOutputs(bool value);

        int EnableOutput(int output, bool value);

        int GetOutputCount();

        bool isOutputEnabled(int output);

        int Connect(string connectionInformation);

        int Disconnect();

        int ConnectionSettingSetup(Object panel, string confDir);

        int AdvancedSettingsSetup(Object panel);

        DeviceInformation GetDeviceInformation();

        int GetMinimalUpdateTimeMS();
    }
}
