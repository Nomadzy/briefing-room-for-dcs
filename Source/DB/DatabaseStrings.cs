/*
==========================================================================
This file is part of Briefing Room for DCS World, a mission
generator for DCS World, by @akaAgar (https://github.com/akaAgar/briefing-room-for-dcs)

Briefing Room for DCS World is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Briefing Room for DCS World is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Briefing Room for DCS World. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using System;

namespace BriefingRoom4DCSWorld.DB
{
    public class DatabaseStrings : IDisposable
    {
        public const Language DEFAULT_LANGUAGE = Language.English;

        private readonly INIFile INI;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public DatabaseStrings()
        {
            INI = new INIFile($"{BRPaths.DATABASE}Strings.ini");
        }

        public string GetString(string section, string key)
        {
            return INI.GetValue<string>(section, key);
        }

        /// <summary>
        /// <see cref="IDisposable"/> implementation.
        /// </summary>
        public void Dispose()
        {
            INI.Dispose();
        }
    }
}