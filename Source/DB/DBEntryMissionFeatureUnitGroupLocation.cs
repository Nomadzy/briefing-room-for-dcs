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

namespace BriefingRoom4DCSWorld.DB
{
    public enum DBEntryMissionFeatureUnitGroupLocation
    {
        /// <summary>
        /// Spawn near the players home base.
        /// </summary>
        Homebase,

        /// <summary>
        /// Spawn ON TOP OF each objective.
        /// </summary>
        Objective,

        /// <summary>
        /// Spawn a few nautical miles away from the objective.
        /// </summary>
        ObjectiveNear,

        /// <summary>
        /// Spawn ON TOP OF the objectives center.
        /// </summary>
        ObjectiveCenter,

        /// <summary>
        /// Spawn a few nautical miles away from the objectives center.
        /// </summary>
        ObjectiveCenterNear,

        /// <summary>
        /// Spawn ON TOP OF each objective waypoint.
        /// </summary>
        Waypoint,

        /// <summary>
        /// Spawn a few nautical miles away from each objective waypoint.
        /// </summary>
        WaypointNear
    }
}
