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

using System.Linq;

namespace BriefingRoom4DCSWorld.DB
{
    /// <summary>
    /// Stores information about a parking spot in a <see cref="DBEntryTheaterAirbase"/>
    /// </summary>
    public struct DBEntryTheaterAirbaseParkingSpot
    {
        /// <summary>
        /// Unique internal DCSID for this parking spot.
        /// </summary>
        public int DCSID { get; }

        /// <summary>
        /// Coordinates where this parking spot is located.
        /// </summary>
        public Coordinates Coordinates { get; }

        /// <summary>
        /// Type of parking spot.
        /// </summary>
        public ParkingSpotType ParkingType { get; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ini">The .ini file to load parking spot data from.</param>
        /// <param name="parkingKey">The top-level key (parking spot unique ID)</param>
        /// <param name="isRunway">Is this parking spot a runway spawn spot</param>
        public DBEntryTheaterAirbaseParkingSpot(INIFile ini, string parkingKey, bool isRunway)
        {
            DCSID = ini.GetValue<int>("Airbases", $"{parkingKey}.DCSID");
            Coordinates = ini.GetValue<Coordinates>("Airbases", $"{parkingKey}.Coordinates");

            ParkingType = ini.GetValue<ParkingSpotType>("Airbases", $"{parkingKey}.Type");
            if (isRunway) ParkingType = ParkingSpotType.Runway;
            else if (ParkingType == ParkingSpotType.Runway) ParkingType = ParkingSpotType.OpenAirSpawn; // Non-runways spots can't be of Runway type
        }
    }
}
