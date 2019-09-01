using CommNet;
using KSP.UI.Screens.Flight;
using RemoteTech.Common.RangeModels;
using RemoteTech.Common.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RemoteTech.Common.RemoteTechCommNet
{
    /// <summary>
    /// This class is the key that allows to break into and customise KSP's CommNet. This is possibly the secondary model in the Model–view–controller sense
    /// </summary>
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER, GameScenes.EDITOR)]
    public class RemoteTechCommNetScenario : CommNetScenario
    {
        /* Note:
         * 1) On entering a desired scene, OnLoad() and then Start() are called.
         * 2) On leaving the scene, OnSave() is called
         * 3) GameScenes.SPACECENTER is recommended so that the RemoteTech data can be verified and error-corrected in advance
         */

        private RemoteTechCommNetUI customUI = null;
        private RemoteTechCommNetNetwork customNetwork = null;
        private RemoteTechTelemetryUpdate customCommNetTelemetry = null;
        public RemoteTechTelemetryUpdate Telemetry
        {
            get { return customCommNetTelemetry; }
        }
        private RemoteTechCommNetUIModeButton customCommNetModeButton = null;
        public CommNetUIModeButton ModeButton()
        {
            return customCommNetModeButton;
        }

        //Other settings
        public Color DishConnectionColor = new Color(1.0f, 0.70f, 0.03f, 1f);
        public Color OmniConnectionColor = new Color(0.55f, 0.51f, 0.40f, 1f);
        public Color ActiveConnectionColor = new Color(0.65f, 1.0f, 0.01f, 1f);
        public Color GroundStationDotColor = new Color(1.0f, 0.0f, 0.0f, 1f);
        public Color LowSignalConnectionColor = new Color(1.0f, 0.0f, 0.0f, 1f);
        public Color DirectConnectionColor = new Color(0.0f, 0.75f, 0.95f, 1f);//part of signal non-relay connection

        public List<RemoteTechCommNetHome> groundStations = new List<RemoteTechCommNetHome>();
        private List<RemoteTechCommNetHome> persistentGroundStations = new List<RemoteTechCommNetHome>();

        public static new RemoteTechCommNetScenario Instance
        {
            get;
            set;
        }

        protected override void Start()
        {
            //base.Start(); //turn off base.Start() contained the AddComponent() for the stock components

            switch (HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>().RangeModelType)
            {
                case RemoteTechCommonParams.RangeModel.Standard:
                    RangeModel = new StandardRangeModel();
                    break;
                case RemoteTechCommonParams.RangeModel.Root:
                    RangeModel = new RootRangeModel();
                    break;
                default:
                    RangeModel = new StandardRangeModel();
                    break;
            }

            RemoteTechCommNetScenario.Instance = this;

            //Replace the CommNet user interface
            var ui = FindObjectOfType<CommNetUI>(); // the order of the three lines is important
            customUI = gameObject.AddComponent<RemoteTechCommNetUI>(); // gameObject.AddComponent<>() is "new" keyword for Monohebaviour class
            UnityEngine.Object.Destroy(ui);

            //Replace the CommNet network
            var net = FindObjectOfType<CommNetNetwork>();
            customNetwork = gameObject.AddComponent<RemoteTechCommNetNetwork>();
            UnityEngine.Object.Destroy(net);

            //Replace the TelemetryUpdate
            TelemetryUpdate tel = TelemetryUpdate.Instance; //only appear in flight
            CommNetUIModeButton cnmodeUI = FindObjectOfType<CommNetUIModeButton>(); //only appear in tracking station; initialised separately by TelemetryUpdate in flight
            if (tel != null && HighLogic.LoadedSceneIsFlight)
            {
                TelemetryUpdateData tempData = new TelemetryUpdateData(tel);
                UnityEngine.Object.DestroyImmediate(tel); //seem like UE won't initialise CNCTelemetryUpdate instance in presence of TelemetryUpdate instance
                customCommNetTelemetry = gameObject.AddComponent<RemoteTechTelemetryUpdate>();
                customCommNetTelemetry.copyOf(tempData);
            }
            else if (cnmodeUI != null && HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                customCommNetModeButton = cnmodeUI.gameObject.AddComponent<RemoteTechCommNetUIModeButton>();
                customCommNetModeButton.copyOf(cnmodeUI);
                UnityEngine.Object.DestroyImmediate(cnmodeUI);
            }

            //Replace the CommNet ground stations
            groundStations.Clear();
            var homes = FindObjectsOfType<CommNetHome>();
            for (int i = 0; i < homes.Length; i++)
            {
                var customHome = homes[i].gameObject.AddComponent(typeof(RemoteTechCommNetHome)) as RemoteTechCommNetHome;
                customHome.copyOf(homes[i]);
                UnityEngine.Object.Destroy(homes[i]);
                groundStations.Add(customHome);
            }
            groundStations.Sort();

            //Apply the ground-station changes from persistent.sfs
            for (int i = 0; i < persistentGroundStations.Count; i++)
            {
                if (groundStations.Exists(x => x.ID.Equals(persistentGroundStations[i].ID)))
                {
                    groundStations.Find(x => x.ID.Equals(persistentGroundStations[i].ID)).applySavedChanges(persistentGroundStations[i]);
                }
            }
            persistentGroundStations.Clear();//dont need anymore

            //Replace the CommNet celestial bodies
            var bodies = FindObjectsOfType<CommNetBody>();
            for (int i = 0; i < bodies.Length; i++)
            {
                var customBody = bodies[i].gameObject.AddComponent(typeof(RemoteTechCommNetBody)) as RemoteTechCommNetBody;
                customBody.copyOf(bodies[i]);
                UnityEngine.Object.Destroy(bodies[i]);
            }

            Logging.Info("RemoteTech Scenario loading done!");
        }

        public override void OnAwake()
        {
            //base.OnAwake(); //turn off CommNetScenario's instance check

            //GameEvents.onVesselCreate.Add(new EventData<Vessel>.OnEvent(this.onVesselCountChanged));
            //GameEvents.onVesselDestroy.Add(new EventData<Vessel>.OnEvent(this.onVesselCountChanged));
        }

        private void OnDestroy()
        {
            if (this.customUI != null)
                UnityEngine.Object.Destroy(this.customUI);

            if (this.customNetwork != null)
                UnityEngine.Object.Destroy(this.customNetwork);

            if (this.customCommNetTelemetry != null)
                UnityEngine.Object.Destroy(this.customCommNetTelemetry);

            if (this.customCommNetModeButton != null)
                UnityEngine.Object.Destroy(this.customCommNetModeButton);

            //GameEvents.onVesselCreate.Remove(new EventData<Vessel>.OnEvent(this.onVesselCountChanged));
            //GameEvents.onVesselDestroy.Remove(new EventData<Vessel>.OnEvent(this.onVesselCountChanged));
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            try
            {
                Logging.Info("RemoteTech Scenario content to be read:\n{0}", gameNode);

                //Ground stations
                if (gameNode.HasNode("GroundStations"))
                {
                    ConfigNode stationNode = gameNode.GetNode("GroundStations");
                    ConfigNode[] stationNodes = stationNode.GetNodes();

                    if (stationNodes.Length < 1) // missing ground-station list
                    {
                        Logging.Error("The 'GroundStations' node is malformed! Reverted to the default list of ground stations.");
                        //do nothing since KSP provides this default list
                    }
                    else
                    {
                        persistentGroundStations.Clear();
                        for (int i = 0; i < stationNodes.Length; i++)
                        {
                            RemoteTechCommNetHome dummyGroundStation = new RemoteTechCommNetHome();
                            ConfigNode.LoadObjectFromConfig(dummyGroundStation, stationNodes[i]);
                            persistentGroundStations.Add(dummyGroundStation);
                        }
                    }
                }
                else
                {
                    Logging.Info("The 'GroundStations' node is not found. The default list of ground stations is loaded from KSP's data.");
                    //do nothing since KSP provides this default list
                }

                //Other variables
                for (int i = 0; i < gameNode.values.Count; i++)
                {
                    ConfigNode.Value value = gameNode.values[i];
                    var name = value.name;
                    switch (name)
                    {
                        case "DisplayModeTracking":
                            RemoteTechCommNetUI.CustomModeTrackingStation = (RemoteTechCommNetUI.CustomDisplayMode)((int)Enum.Parse(typeof(RemoteTechCommNetUI.CustomDisplayMode), value.value));
                            break;
                        case "DisplayModeFlight":
                            RemoteTechCommNetUI.CustomModeFlightMap = (RemoteTechCommNetUI.CustomDisplayMode)((int)Enum.Parse(typeof(RemoteTechCommNetUI.CustomDisplayMode), value.value));
                            break;
                        case "RemoteTechMapFilter":
                            RemoteTechCommNetUI.RTMapFilter = (RemoteTechCommNetUI.RemoteTechMapFilter)((int)Enum.Parse(typeof(RemoteTechCommNetUI.RemoteTechMapFilter), value.value));
                            break;
                        case "DishConnectionColor":
                            DishConnectionColor = UiUtils.StringToColor(value.value);
                            break;
                        case "OmniConnectionColor":
                            OmniConnectionColor = UiUtils.StringToColor(value.value);
                            break;
                        case "ActiveConnectionColor":
                            ActiveConnectionColor = UiUtils.StringToColor(value.value);
                            break;
                        case "GroundStationDotColor":
                            GroundStationDotColor = UiUtils.StringToColor(value.value);
                            break;
                        case "LowSignalConnectionColor":
                            LowSignalConnectionColor = UiUtils.StringToColor(value.value);
                            break;
                        case "DirectConnectionColor":
                            DirectConnectionColor = UiUtils.StringToColor(value.value);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Error("Exception '{0}' thrown when loading RT scenario", e.Message);
            }
        }

        public override void OnSave(ConfigNode gameNode)
        {
            try
            {
                //Ground stations
                if (gameNode.HasNode("GroundStations"))
                {
                    gameNode.RemoveNode("GroundStations");
                }

                ConfigNode stationNode = new ConfigNode("GroundStations");
                for (int i = 0; i < groundStations.Count; i++)
                {
                    ConfigNode newGroundStationNode = new ConfigNode("GroundStation");
                    newGroundStationNode = ConfigNode.CreateConfigFromObject(groundStations[i], newGroundStationNode);
                    stationNode.AddNode(newGroundStationNode);
                }

                if (groundStations.Count <= 0)
                {
                    Logging.Error("No ground stations to save!");
                }
                else
                {
                    gameNode.AddNode(stationNode);
                }

                //Other variables
                gameNode.AddValue("DisplayModeTracking", RemoteTechCommNetUI.CustomModeTrackingStation);
                gameNode.AddValue("DisplayModeFlight", RemoteTechCommNetUI.CustomModeFlightMap);
                gameNode.AddValue("RemoteTechMapFilter", RemoteTechCommNetUI.RTMapFilter);

                gameNode.AddValue("DishConnectionColor", UiUtils.ColorToString(DishConnectionColor));
                gameNode.AddValue("OmniConnectionColor", UiUtils.ColorToString(OmniConnectionColor));
                gameNode.AddValue("ActiveConnectionColor", UiUtils.ColorToString(ActiveConnectionColor));
                gameNode.AddValue("GroundStationDotColor", UiUtils.ColorToString(GroundStationDotColor));
                gameNode.AddValue("LowSignalConnectionColor", UiUtils.ColorToString(LowSignalConnectionColor));
                gameNode.AddValue("DirectConnectionColor", UiUtils.ColorToString(DirectConnectionColor));

                Logging.Info("RemoteTech Scenario content to be saved:\n{0}", gameNode);
            }
            catch(Exception e)
            {
                Logging.Error("Exception '{0}' thrown when saving RT scenario", e.Message);
            }
            base.OnSave(gameNode);
        }
    }
}
