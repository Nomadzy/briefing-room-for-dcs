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

using BriefingRoom4DCSWorld.DB;
using BriefingRoom4DCSWorld.Debug;
using BriefingRoom4DCSWorld.Mission;
using BriefingRoom4DCSWorld.Template;
using System;

namespace BriefingRoom4DCSWorld.Generator
{
    /// <summary>
    /// Generates a <see cref="DCSMission"/> date and time parameters
    /// </summary>
    public class MissionGeneratorDateTime : IDisposable
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MissionGeneratorDateTime() { }

        /// <summary>
        /// Picks a date (day, month and year) for the mission.
        /// </summary>
        /// <param name="mission">The mission</param>
        /// <param name="template">Mission template to use</param>
        /// <param name="coalitions">Coalitions database entries</param>
        public void GenerateMissionDate(DCSMission mission, MissionTemplate template, DBEntryCoalition[] coalitions)
        {
            int day, year;
            Month month;

            // Select a random year from the most recent coalition's decade
            year = Toolbox.GetRandomYearFromDecade(template.Decade);

            DebugLog.Instance.WriteLine($"No fixed date provided in the mission template, generating date in decade {template.Decade}", 1);

            if (template.EnvironmentSeason == Season.Random) // Random season, pick any day of the year
            {
                month = (Month)Toolbox.RandomInt(12);
                day = Toolbox.RandomMinMax(1, Toolbox.GetDaysPerMonth(month, year));
            }
            else // Pick a date according to the desired season
            {
                Month[] seasonMonths = GetMonthsForSeason(template.EnvironmentSeason);

                int monthIndex = Toolbox.RandomInt(4);
                month = seasonMonths[monthIndex];
                switch (monthIndex)
                {
                    case 0: // First month of the season, season begins on the 21st
                        day = Toolbox.RandomMinMax(21, Toolbox.GetDaysPerMonth(month, year)); break;
                    case 3: // Last month of the season, season ends on the 20th
                        day = Toolbox.RandomMinMax(1, 20); break;
                    default:
                        day = Toolbox.RandomMinMax(1, Toolbox.GetDaysPerMonth(month, year)); break;
                }
            }

            mission.DateTime.Day = day;
            mission.DateTime.Month = month;
            mission.DateTime.Year = year;

            DebugLog.Instance.WriteLine($"Misson date set to {mission.DateTime.ToDateString(false)}", 1);
        }

        /// <summary>
        /// Picks a starting time for the mission.
        /// Must be called after <see cref="GenerateMissionDate(DCSMission, MissionTemplate, DBEntryCoalition[])"/> because sunrise/sunset
        /// time depends on the selected date.
        /// </summary>
        /// <param name="mission">The mission.</param>
        /// <param name="template">Mission template to use</param>
        /// <param name="theaterDB">Theater database entry</param>
        public void GenerateMissionTime(DCSMission mission, MissionTemplate template, DBEntryTheater theaterDB)
        {
            Month month = mission.DateTime.Month;

            double totalMinutes;
            int hour, minute;

            switch (template.EnvironmentTimeOfDay)
            {
                default: // case TimeOfDay.Random
                    totalMinutes = Toolbox.RandomInt(Toolbox.MINUTES_PER_DAY);
                    break;

                case TimeOfDay.RandomDaytime:
                    totalMinutes = Toolbox.RandomInt(theaterDB.DayTime[(int)month].Min, theaterDB.DayTime[(int)month].Max - 60);
                    break;

                case TimeOfDay.Dawn:
                    totalMinutes = Toolbox.RandomInt(theaterDB.DayTime[(int)month].Min, theaterDB.DayTime[(int)month].Min + 120);
                    break;

                case TimeOfDay.Noon:
                    totalMinutes = Toolbox.RandomInt(
                        (theaterDB.DayTime[(int)month].Min + theaterDB.DayTime[(int)month].Max) / 2 - 90,
                        (theaterDB.DayTime[(int)month].Min + theaterDB.DayTime[(int)month].Max) / 2 + 90);
                    break;

                case TimeOfDay.Twilight:
                    totalMinutes = Toolbox.RandomInt(theaterDB.DayTime[(int)month].Max - 120, theaterDB.DayTime[(int)month].Max + 30);
                    break;

                case TimeOfDay.Night:
                    totalMinutes = Toolbox.RandomInt(0, theaterDB.DayTime[(int)month].Min - 120);
                    break;
            }

            hour = Toolbox.Clamp((int)Math.Floor(totalMinutes / 60), 0, 23);
            minute = Toolbox.Clamp((int)Math.Floor((totalMinutes - hour * 60) / 15) * 15, 0, 45);

            mission.DateTime.Hour = hour;
            mission.DateTime.Minute = minute;

            DebugLog.Instance.WriteLine($"Starting time set to {mission.DateTime.ToTimeString()}", 1);
        }

        /// <summary>
        /// Returns the months of a given season.
        /// Season begins on the 21st of the first month and ends on the 20th of the last month.
        /// </summary>
        /// <param name="season">A season</param>
        /// <returns>An array of four months</returns>
        private Month[] GetMonthsForSeason(Season season)
        {
            switch (season)
            {
                default: return new Month[] { Month.March, Month.April, Month.May, Month.June }; // case Season.Spring or Season.Random
                case Season.Summer: return new Month[] { Month.June, Month.July, Month.August, Month.September };
                case Season.Fall: return new Month[] { Month.September, Month.October, Month.November, Month.December };
                case Season.Winter: return new Month[] { Month.December, Month.January, Month.February, Month.March };
            }
        }

        /// <summary>
        /// IDispose implementation.
        /// </summary>
        public void Dispose() { }
    }
}
