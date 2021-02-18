/*
==========================================================================
This file is part of Briefing Room for DCS World, a mission
generator for DCS World, by @akaAgar
(https://github.com/akaAgar/briefing-room-for-dcs)

Briefing Room for DCS World is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Briefing Room for DCS World is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Briefing Room for DCS World.
If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using BriefingRoom4DCSWorld.Attributes;
using BriefingRoom4DCSWorld.DB;
using System;
using System.IO;
using System.Linq;

namespace BriefingRoom4DCSWorld.Template
{
    /// <summary>
    /// A mission template, to be used as input in the MissionGenerator class.
    /// </summary>
    [TreeViewExtraNodes("Environment", "Options")]
    public class MissionTemplate : IDisposable
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MissionTemplate()
        {
            Clear();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath">Path to the .ini file the template should be read from.</param>
        public MissionTemplate(string filePath)
        {
            LoadFromFile(filePath);
        }

        /// <summary>
        /// Who belongs to the players' coalition?
        /// </summary>
        [DatabaseSource(typeof(DBEntryCoalition))]
        public string CoalitionAllies { get; set; }

        /// <summary>
        /// Intensity and quality of friendly air defense.
        /// </summary>
        [TreeViewParentNode("CoalitionAllies")]
        public AmountN CoalitionAlliesAirDefense { get; set; }

        /// <summary>
        /// Skill level of AI wingmen and escort aircraft.
        /// </summary>
        [TreeViewParentNode("CoalitionAllies")]
        public BRSkillLevel CoalitionAlliesSkillLevel { get; set; }

        /// <summary>
        /// Which coalition does the player(s) belong to?
        /// </summary>
        [TreeViewParentNode("CoalitionAllies")]
        public Coalition CoalitionAlliesSide { get; set; }

        /// <summary>
        /// Who belongs to the enemy coalition?
        /// </summary>
        [DatabaseSource(typeof(DBEntryCoalition))]
        public string CoalitionEnemies { get; set; }

        /// <summary>
        /// Intensity and quality of enemy air defense.
        /// </summary>
        [TreeViewParentNode("CoalitionEnemies")]
        public AmountN CoalitionEnemiesAirDefense { get; set; }

        /// <summary>
        /// Relative power of the enemy air force.
        /// Enemy air force will always be proportional to the number and air-to-air efficiency of aircraft in the player mission package,
        /// so more player/AI friendly aircraft means more enemy aircraft, regardless of this setting.
        /// </summary>
        [TreeViewParentNode("CoalitionEnemies")]
        public AmountN CoalitionEnemiesAirForce { get; set; }

        /// <summary>
        /// Chance that enemy fighter planes will already be patrolling on mission start rather than popping up during the mission on objective completion.
        /// </summary>
        [TreeViewParentNode("CoalitionEnemiesAirForce")]
        public AmountN CoalitionEnemiesAirForceOnStationChance { get; set; }

        /// <summary>
        /// Skill level of enemy planes and helicopters.
        /// </summary>
        [TreeViewParentNode("CoalitionEnemies")]
        public BRSkillLevel CoalitionEnemiesSkillLevelAir { get; set; }

        /// <summary>
        /// Skill level of enemy ground units and air defense.
        /// </summary>
        [TreeViewParentNode("CoalitionEnemies")]
        public BRSkillLevel CoalitionEnemiesSkillLevelGround { get; set; }

        /// <summary>
        /// Can enemy units be spawned in any country (recommended) or only in countries aligned with a
        /// given coalition? Be aware that when choosing an option other than "Any", depending on the theater and the
        /// <see cref="TheaterRegionsCoalitions"/> setting, objectives may end up VERY far from the player(s) starting 
        /// location, no matter the value of <see cref="ObjectiveDistance"/>.
        /// Keep in mind that <see cref="TheaterRegionsCoalitions"/> has a influence on this setting.
        /// </summary>
        [TreeViewParentNode("CoalitionEnemies")]
        public SpawnPointPreferredCoalition CoalitionEnemiesUnitsLocation { get; set; }

        /// <summary>
        /// During which decade will this mission take place? This value is ignored if Briefing/Mission date is set.
        /// </summary>
        public Decade Decade { get; set; }

        /// <summary>
        /// Season during which the mission will take place.
        /// </summary>
        [TreeViewParentNode("Environment")]
        public Season EnvironmentSeason { get; set; }

        /// <summary>
        /// Time of the day at which the mission will start.
        /// </summary>
        [TreeViewParentNode("Environment")]
        public TimeOfDay EnvironmentTimeOfDay { get; set; }

        /// <summary>
        /// What the weather be like during the mission.
        /// </summary>
        [TreeViewParentNode("Environment")]
        public Weather EnvironmentWeather { get; set; }

        /// <summary>
        /// How windy will the weather be during the mission. "Auto" means "choose according to <see cref="EnvironmentWeather"/>".
        /// </summary>
        [TreeViewParentNode("EnvironmentWeather")]
        public Wind EnvironmentWeatherWind { get; set; }

        /// <summary>
        /// The type of task player must accomplish in this mission.
        /// </summary>
        [DatabaseSource(typeof(DBEntryObjective), true)]
        public string Objective { get; set; }

        /// <summary>
        /// How many objectives/targets will be present in the mission.
        /// </summary>
        [TreeViewParentNode("Objective")]
        [IntegerSource(1, TemplateTools.MAX_OBJECTIVES, 1)]
        public int ObjectiveCount { get; set; }

        /// <summary>
        /// How far from the player's starting location will the objectives be, in nautical miles. "Zero" means "random".
        /// </summary>
        [TreeViewParentNode("Objective")]
        [IntegerSource(TemplateTools.MIN_OBJECTIVE_DISTANCE, TemplateTools.MAX_OBJECTIVE_DISTANCE, 10, "%i nm", true, 0)]
        public int ObjectiveDistanceNM { get; set; }

        /// <summary>
        /// Unit system to use in the mission briefing.
        /// </summary>
        [TreeViewParentNode("Options")]
        public UnitSystem OptionsBriefingUnitSystem { get; set; }

        /// <summary>
        /// Amount of civilian traffic on the roads. Can affect performance if set too high.
        /// </summary>
        [TreeViewParentNode("Options")]
        public CivilianTraffic OptionsCivilianTraffic { get; set; }

        /// <summary>
        /// When (and if) should the mission automatically end after all objectives are complete?
        /// </summary>
        [TreeViewParentNode("Options")]
        public MissionEndMode OptionsEndMode { get; set; }

        /// <summary>
        /// Preferences and options to apply to this mission.
        /// </summary>
        [TreeViewParentNode("Options")]
        public MissionTemplatePreferences[] OptionsPreferences { get; set; }

        /// <summary>
        /// Realism options to apply to this mission.
        /// </summary>
        [TreeViewParentNode("Options")]
        public RealismOption[] OptionsRealism { get; set; }

        /// <summary>
        /// Script extensions to include in this mission to provide additional features.
        /// </summary>
        [TreeViewParentNode("Options")]
        [DatabaseSource(typeof(DBEntryExtension))]
        public string[] OptionsScriptExtensions { get; set; }

        /// <summary>
        /// Which unit mods should be enabled in this mission? Make sure units mods are installed and active in your version of DCS World or the units won't be spawned.
        /// </summary>
        [TreeViewParentNode("Options")]
        [DatabaseSource(typeof(DBEntryUnitMod))]
        public string[] OptionsUnitMods { get; set; }

        [PlayersFGParentNode()]
        public MissionPlayersType Players { get; set; }

        /// <summary>
        /// Multiplayer flight groups.
        /// If any flight group is specified here, the mission then becomes a multiplayer mission and all values
        /// in the "Player, single player only" are ignored.
        /// </summary>
        [TreeViewParentNode("Players")]
        public MissionTemplateFlightGroup[] PlayerFlightGroups { get; set; } = new MissionTemplateFlightGroup[0];

        /// <summary>
        /// Type of aircraft the player will fly.
        /// As with all values in the "Player, single player only" category, this value is ignored if any
        /// flight group is specified in <see cref="PlayerFlightGroups" />, the multiplayer flight groups
        /// are then used instead.
        /// </summary>
        [TreeViewParentNode("Players")]
        public string PlayerSPAircraft { get { return PlayerSPAircraft_; } set { PlayerSPAircraft_ = TemplateTools.CheckValuePlayerAircraft(value); } }
        private string PlayerSPAircraft_;

        /// <summary>
        /// Number of AI wingmen in the player's flight group.
        /// As with all values in the "Player, single player only" category, this value is ignored if any
        /// flight group is specified in <see cref="PlayerFlightGroups" />, the multiplayer flight groups
        /// are then used instead.
        /// </summary>
        [TreeViewParentNode("Players")]
        public int PlayerSPWingmen { get { return PlayerSPWingmen_; } set { PlayerSPWingmen_ = Toolbox.Clamp(value, 0, Toolbox.MAXIMUM_FLIGHT_GROUP_SIZE - 1); } }
        private int PlayerSPWingmen_;

        /// <summary>
        /// Type of aircraft carrier the player will be spawned on. If none, player will take off from an airbase. Make sure the player aircraft is suitable for the carrier type.
        /// </summary>
        [TreeViewParentNode("Players")]
        public string PlayerSPCarrier { get; set; }

        /// <summary>
        /// Number of AI aircraft tasked with escorting the player against enemy fighters.
        /// In single-player missions, escorts will be spawned on the ramp if the player starts from the ramp (cold or hot), or in the air above the airbase if the player starts on the runway.
        /// In multiplayer missions, escorts will be spawned as soon as one player takes off.
        /// </summary>
        [TreeViewParentNode("Players")]
        public int PlayerEscortCAP { get { return PlayerEscortCAP_; } set { PlayerEscortCAP_ = Toolbox.Clamp(value, 0, Toolbox.MAXIMUM_FLIGHT_GROUP_SIZE); } }
        private int PlayerEscortCAP_;

        /// <summary>
        /// Number of AI aircraft tasked with escorting the player against enemy SAMs.
        /// In single-player missions, escorts will be spawned on the ramp if the player starts from the ramp (cold or hot), or in the air above the airbase if the player starts on the runway.
        /// In multiplayer missions, escorts will be spawned as soon as one player takes off.
        /// </summary>
        [TreeViewParentNode("Players")]
        public int PlayerEscortSEAD { get { return PlayerEscortSEAD_; } set { PlayerEscortSEAD_ = Toolbox.Clamp(value, 0, Toolbox.MAXIMUM_FLIGHT_GROUP_SIZE); } }
        private int PlayerEscortSEAD_;

        /// <summary>
        /// Where should the player(s) take off from?
        /// </summary>
        [TreeViewParentNode("Options")]
        public PlayerStartLocation PlayerStartLocation { get; set; }

        /// <summary>
        /// DCS World theater in which the mission will take place.
        /// </summary>
        [DatabaseSource(typeof(DBEntryTheater))]
        public string Theater { get; set; }

        /// <summary>
        /// To which coalitions should the countries on the map (and their airbases) belong to?
        /// </summary>
        [TreeViewParentNode("Theater")]
        public CountryCoalition TheaterRegionsCoalitions { get; set; }

        /// <summary>
        /// Name of the airbase the player must take off from.
        /// If left empty, or if the airbase doesn't exist in this theater, a random airbase will be selected.
        /// </summary>
        [TreeViewParentNode("Theater")]
        public string TheaterStartingAirbase { get { return TheaterStartingAirbase_; } set { TheaterStartingAirbase_ = TemplateTools.CheckValueTheaterStartingAirbase(value); } }
        private string TheaterStartingAirbase_;

        /// <summary>
        /// Resets all properties to their default values.
        /// </summary>
        public void Clear()
        {
            //BriefingDate = new MissionTemplateDate();
            //BriefingDescription = "";
            //BriefingName = "";
            OptionsBriefingUnitSystem = UnitSystem.Imperial;

            CoalitionAllies = TemplateTools.CheckValue<DBEntryCoalition>(Database.Instance.Common.DefaultCoalitionBlue);
            CoalitionAlliesSide = Coalition.Blue;
            CoalitionEnemies = TemplateTools.CheckValue<DBEntryCoalition>(Database.Instance.Common.DefaultCoalitionRed);
            Decade = Decade.Decade2000;

            EnvironmentSeason = Season.Random;
            EnvironmentTimeOfDay = TimeOfDay.RandomDaytime;
            EnvironmentWeather = Weather.Random;
            EnvironmentWeatherWind = Wind.Auto;

            Objective = TemplateTools.CheckValue<DBEntryObjective>(Database.Instance.Common.DefaultObjective);
            ObjectiveCount = Database.Instance.Common.DefaultObjectiveCount;
            ObjectiveDistanceNM = TemplateTools.CheckObjectiveDistance(0);

            CoalitionEnemiesAirDefense = AmountN.Random;
            CoalitionEnemiesAirForce = AmountN.Random;
            CoalitionEnemiesAirForceOnStationChance = AmountN.Random;
            CoalitionEnemiesSkillLevelAir = BRSkillLevel.Random;
            CoalitionEnemiesSkillLevelGround = BRSkillLevel.Random;
            CoalitionEnemiesUnitsLocation = SpawnPointPreferredCoalition.Any;

            OptionsCivilianTraffic = CivilianTraffic.Low;
            OptionsEndMode = MissionEndMode.NoEnd;
            OptionsPreferences = new MissionTemplatePreferences[0];
            OptionsRealism = new RealismOption[] { RealismOption.NoBDA };
            OptionsScriptExtensions = new string[0];
            OptionsUnitMods = new string[0];

            CoalitionAlliesSkillLevel = BRSkillLevel.Random;
            Players = MissionPlayersType.SinglePlayer;
            PlayerEscortCAP = 0;
            PlayerEscortSEAD = 0;
            CoalitionAlliesAirDefense = AmountN.Random;
            PlayerStartLocation = PlayerStartLocation.Runway;

            PlayerFlightGroups = new MissionTemplateFlightGroup[] { new MissionTemplateFlightGroup() };
            PlayerSPAircraft = TemplateTools.CheckValuePlayerAircraft(Database.Instance.Common.DefaultPlayerAircraft);
            PlayerSPWingmen = 1;
            PlayerSPCarrier = "";

            Theater = TemplateTools.CheckValue<DBEntryTheater>(Database.Instance.Common.DefaultTheater);
            TheaterRegionsCoalitions = CountryCoalition.Default;
            TheaterStartingAirbase = "";
        }

        /// <summary>
        /// Loads a mission template from an .ini file.
        /// </summary>
        /// <param name="filePath">Path to the .ini file</param>
        /// <returns></returns>
        public bool LoadFromFile(string filePath)
        {
            Clear();
            if (!File.Exists(filePath)) return false;

            using (INIFile ini = new INIFile(filePath))
            {
                OptionsBriefingUnitSystem = ini.GetValue("Briefing", "UnitSystem", OptionsBriefingUnitSystem);

                CoalitionAllies = ini.GetValue("Context", "Coalition.Blue", CoalitionAllies);
                CoalitionAlliesSide = ini.GetValue("Context", "Coalition.Player", CoalitionAlliesSide);
                CoalitionEnemies = ini.GetValue("Context", "Coalition.Red", CoalitionEnemies);
                Decade = ini.GetValue("Context", "Decade", Decade);

                EnvironmentSeason = ini.GetValue("Environment", "Season", EnvironmentSeason);
                EnvironmentTimeOfDay = ini.GetValue("Environment", "TimeOfDay", EnvironmentTimeOfDay);
                EnvironmentWeather = ini.GetValue("Environment", "Weather", EnvironmentWeather);
                EnvironmentWeatherWind = ini.GetValue("Environment", "Wind", EnvironmentWeatherWind);

                Objective = TemplateTools.CheckValue<DBEntryObjective>(ini.GetValue("Objective", "Type", Objective));
                ObjectiveCount = Toolbox.Clamp(ini.GetValue("Objective", "Count", ObjectiveCount), 1, TemplateTools.MAX_OBJECTIVES);
                ObjectiveDistanceNM = TemplateTools.CheckObjectiveDistance(ini.GetValue("Objective", "Distance", ObjectiveDistanceNM));
                
                CoalitionEnemiesAirDefense = ini.GetValue("Opposition", "AirDefense", CoalitionEnemiesAirDefense);
                CoalitionEnemiesAirForce = ini.GetValue("Opposition", "AirForce", CoalitionEnemiesAirForce);
                CoalitionEnemiesAirForceOnStationChance = ini.GetValue("Opposition", "OnStationChance", CoalitionEnemiesAirForceOnStationChance);
                CoalitionEnemiesSkillLevelAir = ini.GetValue("Opposition", "SkillLevel.Air", CoalitionEnemiesSkillLevelAir);
                CoalitionEnemiesSkillLevelGround = ini.GetValue("Opposition", "SkillLevel.Ground", CoalitionEnemiesSkillLevelGround);
                CoalitionEnemiesUnitsLocation = ini.GetValue("Opposition", "UnitsLocation", CoalitionEnemiesUnitsLocation);

                OptionsCivilianTraffic = ini.GetValue("Options", "CivilianTraffic", OptionsCivilianTraffic);
                OptionsEndMode = ini.GetValue("Options", "EndMode", OptionsEndMode);
                OptionsPreferences = ini.GetValueArray<MissionTemplatePreferences>("Options", "Preferences");
                OptionsRealism = ini.GetValueArray<RealismOption>("Options", "Realism");
                OptionsScriptExtensions = ini.GetValueArray<string>("Options", "ScriptExtensions");
                OptionsUnitMods = ini.GetValueArray<string>("Options", "UnitMods");

                CoalitionAlliesSkillLevel = ini.GetValue("Player", "AISkillLevel", CoalitionAlliesSkillLevel);
                Players = ini.GetValue("Player", "Player", Players);
                PlayerEscortCAP = ini.GetValue("Player", "EscortCAP", PlayerEscortCAP);
                PlayerEscortSEAD = ini.GetValue("Player", "EscortSEAD", PlayerEscortSEAD);
                CoalitionAlliesAirDefense = ini.GetValue("Player", "FriendlyAirDefense", CoalitionAlliesAirDefense);
                PlayerStartLocation = ini.GetValue("Player", "StartLocation", PlayerStartLocation);

                int fgFlightGroupCount = Math.Max(0, ini.GetValue<int>("PlayerMP", "FGCount"));
                PlayerFlightGroups = new MissionTemplateFlightGroup[fgFlightGroupCount];
                for (int i = 0; i < fgFlightGroupCount; i++)
                    PlayerFlightGroups[i] = new MissionTemplateFlightGroup(ini, "PlayerMP", $"FG{i:000}");
                if (PlayerFlightGroups.Length == 0) PlayerFlightGroups = new MissionTemplateFlightGroup[] { new MissionTemplateFlightGroup() };

                PlayerSPAircraft = ini.GetValue("PlayerSP", "Aircraft", PlayerSPAircraft);
                PlayerSPWingmen = ini.GetValue("PlayerSP", "Wingmen", PlayerSPWingmen);
                PlayerSPCarrier = ini.GetValue("PlayerSP", "Carrier", PlayerSPCarrier);

                Theater = TemplateTools.CheckValue<DBEntryTheater>(ini.GetValue("Theater", "ID", Theater));
                TheaterRegionsCoalitions = ini.GetValue("Theater", "RegionsCoalitions", TheaterRegionsCoalitions);
                TheaterStartingAirbase = ini.GetValue("Theater", "StartingAirbase", TheaterStartingAirbase);
            }

            return true;
        }

        /// <summary>
        /// Save the mission template to an .ini file.
        /// </summary>
        /// <param name="filePath">Path to the .ini file.</param>
        public void SaveToFile(string filePath)
        {
            using (INIFile ini = new INIFile())
            {
                ini.SetValue("Briefing", "UnitSystem", OptionsBriefingUnitSystem);

                ini.SetValue("Context", "Coalition.Blue", CoalitionAllies);
                ini.SetValue("Context", "Coalition.Player", CoalitionAlliesSide);
                ini.SetValue("Context", "Coalition.Red", CoalitionEnemies);
                ini.SetValue("Context", "Decade", Decade);

                ini.SetValue("Environment", "Season", EnvironmentSeason);
                ini.SetValue("Environment", "TimeOfDay", EnvironmentTimeOfDay);
                ini.SetValue("Environment", "Weather", EnvironmentWeather);
                ini.SetValue("Environment", "Wind", EnvironmentWeatherWind);

                ini.SetValue("Objective", "Count", ObjectiveCount);
                ini.SetValue("Objective", "Distance", ObjectiveDistanceNM);
                ini.SetValue("Objective", "Type", Objective);

                ini.SetValue("Opposition", "AirDefense", CoalitionEnemiesAirDefense);
                ini.SetValue("Opposition", "AirForce", CoalitionEnemiesAirForce);
                ini.SetValue("Opposition", "OnStationChance", CoalitionEnemiesAirForceOnStationChance);
                ini.SetValue("Opposition", "SkillLevel.Air", CoalitionEnemiesSkillLevelAir);
                ini.SetValue("Opposition", "SkillLevel.Ground", CoalitionEnemiesSkillLevelGround);
                ini.SetValue("Opposition", "UnitsLocation", CoalitionEnemiesUnitsLocation);

                ini.SetValue("Options", "CivilianTraffic", OptionsCivilianTraffic);
                ini.SetValue("Options", "EndMode", OptionsEndMode);
                ini.SetValueArray("Options", "Preferences", OptionsPreferences);
                ini.SetValueArray("Options", "Realism", OptionsRealism);
                ini.SetValueArray("Options", "ScriptExtensions", OptionsScriptExtensions);
                ini.SetValueArray("Options", "UnitMods", OptionsUnitMods);

                ini.SetValue("Player", "Player", Players);
                ini.SetValue("Player", "AISkillLevel", CoalitionAlliesSkillLevel);
                ini.SetValue("Player", "EscortCAP", PlayerEscortCAP);
                ini.SetValue("Player", "EscortSEAD", PlayerEscortSEAD);
                ini.SetValue("Player", "FriendlyAirDefense", CoalitionAlliesAirDefense);
                ini.SetValue("Player", "StartLocation", PlayerStartLocation);

                ini.SetValue("PlayerSP", "Aircraft", PlayerSPAircraft);
                ini.SetValue("PlayerSP", "Wingmen", PlayerSPWingmen);
                ini.SetValue("PlayerSP", "Wingmen.SkillLevel", CoalitionAlliesSkillLevel);
                ini.SetValue("PlayerSP", "Carrier", PlayerSPCarrier);

                ini.SetValue("PlayerMP", "FGCount", PlayerFlightGroups.Length);
                for (int i = 0; i < PlayerFlightGroups.Length; i++)
                    PlayerFlightGroups[i].SaveToFile(ini, "PlayerMP", $"FG{i:000}");

                ini.SetValue("Theater", "ID", Theater);
                ini.SetValue("Theater", "RegionsCoalitions", TheaterRegionsCoalitions);
                ini.SetValue("Theater", "StartingAirbase", TheaterStartingAirbase);

                ini.SaveToFile(filePath);
            }
        }

        /// <summary>
        /// "Shortcut" method to get <see cref="CoalitionAllies"/> or <see cref="CoalitionEnemies"/> by using a <see cref="Coalition"/> parameter.
        /// </summary>
        /// <param name="coalition">Color of the coalition to return</param>
        /// <returns><see cref="CoalitionAllies"/> or <see cref="CoalitionEnemies"/></returns>
        public string GetCoalition(Coalition coalition)
        {
            if (coalition == Coalition.Red) return CoalitionEnemies;
            return CoalitionAllies;
        }

        /// <summary>
        /// Returns the total number of player-controllable aircraft in the mission.
        /// </summary>
        /// <returns>The number of player-controllable aircraft</returns>
        public int GetPlayerCount()
        {
            if (Players == MissionPlayersType.SinglePlayer) return 1;

            return (from MissionTemplateFlightGroup pfg in PlayerFlightGroups select pfg.Count).Sum();
        }

        /// <summary>
        /// Returns the total number of parking spots required for the misison package aircraft.
        /// </summary>
        /// <returns>Number of parking spots required</returns>
        public int GetMissionPackageRequiredParkingSpots()
        {
            if (Players == MissionPlayersType.SinglePlayer)
            {
                if (PlayerStartLocation == PlayerStartLocation.Runway) return 0; // Player and wingmen start on the runway, AI escort start in air above the airbase
                return PlayerSPWingmen_ + 1 + PlayerEscortCAP_ + PlayerEscortSEAD_;
            }

            return GetPlayerCount(); // AI escorts start in the air
        }

        /// <summary>
        /// <see cref="IDisposable"/> implementation.
        /// </summary>
        public void Dispose() { }
    }
}
