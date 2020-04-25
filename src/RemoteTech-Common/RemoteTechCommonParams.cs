using RemoteTech.Common.RemoteTechCommNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

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

        [Persistent]
        private string UpgradeableGroundStationCosts = String.Empty;
        private int[] _groundStationUpgradeableCosts;
        public int[] GroundStationUpgradeableCosts
        {
            get
            {
                return _groundStationUpgradeableCosts;
            }
        }

        [Persistent]
        private string UpgradeableGroundStationPowers = String.Empty;
        private double[] _groundStationUpgradeablePowers;
        public double[] GroundStationUpgradeablePowers
        {
            get
            {
                return _groundStationUpgradeablePowers;
            }
        }

        [Persistent]
        private string KSCMissionControlPowers = String.Empty;
        private double[] _KSCStationPowers;
        public double[] KSCStationPowers
        {
            get
            {
                return _KSCStationPowers;
            }
        }

        //[GameParameters.CustomStringParameterUI("", autoPersistance = false, lines = 2)]
        //public string description = "Core functionality";

        [GameParameters.CustomParameterUI("Range Model Mode", toolTip = "This setting controls how the game determines whether two antennas are in range of each other.\nRead more on our online manual about the difference for each rule.")]
        public RangeModel RangeModelType = RangeModel.Standard;

        [GameParameters.CustomParameterUI("Upgradeable Mission Control antennas", toolTip = "ON: Mission Control antenna range is upgraded when the Tracking Center is upgraded.\nOFF: Mission Control antenna range isn't upgradeable.")]
        public bool UpgradeableMissionControlAntennas = true;

        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines = 2)]
        public string visualDesc = " \n<b><u>Visual styles</u></b>";

        [GameParameters.CustomParameterUI("Hide Ground Stations completely", toolTip = "ON: Ground Stations will not be shown at all times.\nOFF: Ground Stations are shown conditionally.")]
        public bool HideGroundStationsFully = false;

        [GameParameters.CustomParameterUI("Hide Ground Stations behind planet or moon", toolTip = "ON: Ground Stations are occluded by planet or moon, and are not visible behind it.\nOFF: Ground Stations are always shown.")]
        public bool HideGroundStationsBehindBody = true;

        [GameParameters.CustomParameterUI("Mouseover of Ground Stations", toolTip = "ON: Some useful information is shown when you mouseover a Ground Station on the map view or Tracking Station.\nOFF: Information isn't shown during mouseover.")]
        public bool ShowMouseOverInfoGroundStations = true;

        [GameParameters.CustomFloatParameterUI("Distance from Ground Stations", toolTip = "If distance in meter between Ground Stations and you is greater than this,\nGround Stations will not be displayed.", minValue = 1000000, maxValue = 30000000f, stepCount = 500000)]
        public float DistanceToHideGroundStations = 8000000;

        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines = 2)]
        public string cheatDesc = " \n<b><u>Cheats</u></b>";

        [GameParameters.CustomParameterUI("Connection required to control antennas", toolTip = "ON: Antennas can be activated, deactivated and targeted without a connection.\nOFF: No control without a working connection.")]
        public bool ControlAntennaWithConnection = true;

        [GameParameters.CustomParameterUI("Planets and moons will block a signal", toolTip = "ON: Antennas and dishes will not need line-of-sight to maintain a connection, as long as they have adequate range and power.\nOFF: Antennas and dishes need line-of-sight to maintain a connection.")]
        public bool EnforceLineOfSight = true;

        public List<RemoteTechCommNetHome> DefaultGroundStations = new List<RemoteTechCommNetHome>();

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

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            //disable all gs options when overall gs option is disabled
            switch (member.Name)
            {
                case "HideGroundStationsBehindBody":
                case "ShowMouseOverInfoGroundStations":
                case "DistanceToHideGroundStations":
                    return !HideGroundStationsFully;
            }

            return true;
        }

        protected static string _configDirectory = null;
        protected string configDirectory
        {
            get
            {
                if (_configDirectory == null)
                {
                    _configDirectory = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name.Equals("RemoteTech-Common")).url.Replace("/Plugins", "") + "/Configs/";
                }
                return _configDirectory;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            UrlDir.UrlConfig[] cfgs;

            cfgs = GameDatabase.Instance.GetConfigs("RemoteTechTNGCommonSettings");
            for (int i = 0; i < cfgs.Length; i++)
            {
                if (cfgs[i].url.Equals(configDirectory + "RemoteTechCommon_Settings/RemoteTechTNGCommonSettings"))
                {
                    if (!ConfigNode.LoadObjectFromConfig(this, cfgs[i].config))
                    {
                        Logging.Error("Unable to load RemoteTechCommon_Settings.cfg");
                    }

                    //Load default ground station parameters
                    ConfigNode[] stationNodes = cfgs[i].config.GetNode("GroundStations").GetNodes();
                    for (int j = 0; j < stationNodes.Length; j++)
                    {
                        RemoteTechCommNetHome dummyGroundStation = new RemoteTechCommNetHome();
                        ConfigNode.LoadObjectFromConfig(dummyGroundStation, stationNodes[j]);
                        DefaultGroundStations.Add(dummyGroundStation);
                    }
                    break;
                }
            }

            if (this.UpgradeableGroundStationCosts != String.Empty)
            {
                var tokens = this.UpgradeableGroundStationCosts.Split(';');
                _groundStationUpgradeableCosts = new int[tokens.Length];
                for (int i = 0; i < tokens.Length; i++)
                {
                     int.TryParse(tokens[i], out _groundStationUpgradeableCosts[i]);
                }
            }
            if (this.UpgradeableGroundStationPowers != String.Empty)
            {
                var tokens = this.UpgradeableGroundStationPowers.Split(';');
                _groundStationUpgradeablePowers = new double[tokens.Length];
                for (int i = 0; i < tokens.Length; i++)
                {
                    double.TryParse(tokens[i], out _groundStationUpgradeablePowers[i]);
                }
            }
            if (this.KSCMissionControlPowers != String.Empty)
            {
                var tokens = this.KSCMissionControlPowers.Split(';');
                _KSCStationPowers = new double[tokens.Length];
                for (int i = 0; i < tokens.Length; i++)
                {
                    double.TryParse(tokens[i], out _KSCStationPowers[i]);
                }
            }
        }


    }
}
