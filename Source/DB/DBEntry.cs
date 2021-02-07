﻿/*
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
using System.Linq;

namespace BriefingRoom4DCSWorld.DB
{
    /// <summary>
    /// Abstract parent class for all database entries. The Load() method loads data from an .ini file.
    /// </summary>
    public abstract class DBEntry : IDisposable
    {
        /// <summary>
        /// Unique ID used by this entry in the database dictionary. Same as the dictionary key.
        /// Duplicated here to make things easier in some methods using Linq queries on Database entries.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Human-readable display name to show in the UI instead of the ID.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Description to show in the UI.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DBEntry() { }

        /// <summary>
        /// Loads a database entry from an .ini file.
        /// </summary>
        /// <param name="id">Unique ID of the database entry</param>
        /// <param name="iniFilePath">Path to the .ini file where entry inforation is stored</param>
        /// <returns>True is successful, false if an error happened</returns>
        public bool Load(string id, string iniFilePath)
        {
            ID = id;

            using (INIFile ini = new INIFile(iniFilePath))
            {
                DisplayName = ini.GetValue<string>("GUI", "DisplayName");
                Description = ini.GetValue<string>("GUI", "Description");
     
                if (string.IsNullOrEmpty(DisplayName)) DisplayName = ID;
                if (string.IsNullOrEmpty(Description)) Description = DisplayName;
            }

            return OnLoad(iniFilePath);
        }

        /// <summary>
        /// Loads a database entry from an .ini file.
        /// </summary>
        /// <param name="iniFilePath">Path to the .ini file where entry inforation is stored</param>
        /// <returns>True is successful, false if an error happened</returns>
        protected abstract bool OnLoad(string iniFilePath);

        /// <summary>
        /// <see cref="IDisposable"/> implementation.
        /// </summary>
        public virtual void Dispose() { }

        /// <summary>
        /// Checks an array of <see cref="DBEntry"/> ids of type to see which ones exist in the database.
        /// Returns an array of valid IDs and outputs (rejected) an array of invalid ones so warnings can be displayed.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="DBEntry"/> to look for</typeparam>
        /// <param name="values">Array of <see cref="DBEntry"/> ids to check</param>
        /// <param name="rejected">Array of <see cref="DBEntry"/> or type T which are neither null nor empty but DO NOT exist in the database</param>
        /// <returns>Array of <see cref="DBEntry"/> or type T which are neither null nor empty AND exist in the database</returns>
        protected string[] GetValidDBEntryIDs<T>(string[] values, out string[] rejected) where T : DBEntry
        {
            values = values ?? new string[0]; // Make sure values is not null

            // Accepted values IDs are neither null nor empty AND exist in the database
            string[] accepted =
                (from string v in values
                 where !string.IsNullOrEmpty(v.Trim()) && Database.Instance.EntryExists<T>(v.Trim())
                 select v.Trim()).ToArray();

            // Accepted values IDs are neither null nor empty but DO NOT exist in the database
            rejected =
                (from string v in values
                 where !string.IsNullOrEmpty(v.Trim()) && !Database.Instance.EntryExists<T>(v.Trim())
                 select v.Trim()).ToArray();

            return accepted;
        }
    }
}
