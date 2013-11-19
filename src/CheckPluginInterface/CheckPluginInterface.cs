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

public delegate void addTextCP(string text);

namespace CheckPluginInterface
{
    public interface InterfaceCheckPlugin
    {
        string GetPluginName();

        void initCheckPlugin(int instance, addTextCP callback, string configDir);

        /* return values 
         * 0 = check ok
         * 1 = check failed
         * 2 = check timeout 
         * -1 = could not run check */
        int runCheck();

        void setupCheckPlugin();
    }
}
