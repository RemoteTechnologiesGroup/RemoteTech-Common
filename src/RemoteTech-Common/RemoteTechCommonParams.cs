using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RemoteTech.Common
{
    public class RemoteTechCommonParams : GameParameters.CustomParameterNode
    {
        public enum RangeModel
        {
            [Description("Standard")]
            Standard,
            [Description("Root")]
            Root,
        }

        [GameParameters.CustomParameterUI("Range Model Mode", toolTip = "This setting controls how the game determines whether two antennas are in range of each other.\nRead more on our online manual about the difference for each rule.")]
        public RangeModel RangeModelType = RangeModel.Standard;

        [GameParameters.CustomParameterUI("Hide Ground Stations completely", toolTip = "ON: Ground Stations will not be shown at all times.\nOFF: Ground Stations are shown conditionally.")]
        public bool HideGroundStationsFully = false;

        [GameParameters.CustomParameterUI("Hide Ground Stations behind planet or moon", toolTip = "ON: Ground Stations are occluded by the planet or moon, and are not visible behind it.\nOFF: Ground Stations are always shown (see range option below).")]
        public bool HideGroundStationsBehindBody = true;

        [GameParameters.CustomParameterUI("Hide Ground Stations at a defined distance", toolTip = "ON: Ground Stations will not be shown past a defined distance to the mapview camera.\nOFF: Ground Stations are shown regardless of distance.")]
        public bool HideGroundStationsOnDistance = true;

        [GameParameters.CustomParameterUI("Mouseover of Ground Stations", toolTip = "ON: Some useful information is shown when you mouseover a Ground Station on the map view or Tracking Station.\nOFF: Information isn't shown during mouseover.")]
        public bool ShowMouseOverInfoGroundStations = true;

        [GameParameters.CustomParameterUI("Upgradeable Mission Control antennas", toolTip = "ON: Mission Control antenna range is upgraded when the Tracking Center is upgraded.\nOFF: Mission Control antenna range isn't upgradeable.")]
        public bool UpgradeableMissionControlAntennas = true;

        [GameParameters.CustomParameterUI("Planets and moons will block a signal", toolTip = "ON: Antennas and dishes will not need line-of-sight to maintain a connection, as long as they have adequate range and power.\nOFF: Antennas and dishes need line-of-sight to maintain a connection.")]
        public bool EnforceLineOfSight = true;

        public override string DisplaySection
        {
            get
            {
                return "RemoteTech";
            }
        }

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return false;
            }
        }

        public override string Section
        {
            get
            {
                return "RemoteTech";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 0;
            }
        }

        public override string Title
        {
            get
            {
                return "Core";
            }
        }
    }
}
