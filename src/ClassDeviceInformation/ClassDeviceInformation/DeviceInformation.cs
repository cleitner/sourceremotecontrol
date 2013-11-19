﻿/*  SourceRemoteControl
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

namespace ClassDeviceInformation
{
    public class DeviceInformation
    {
        public string producer;
        public string type;
        public string serialNo;
        public string softwareRevision;
        public int outputs;
        public float currentMin = (float)0.0;
        public float currentMax = (float)0.0;
        public float currentLimitMin = (float)0.0;
        public float currentLimitMax = (float)0.0;
        public float voltageMin = (float)0.0;
        public float voltageMax = (float)0.0;
        public float voltageLimitMin = (float)0.0;
        public float voltageLimitMax = (float)0.0;
        public float voltageOVPMin = (float)0.0;
        public float voltageOVPMax = (float)0.0;
        public bool singleTurnOnOff = false;
    }
}
