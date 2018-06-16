using CommNet;
using KSP.UI.Screens.Flight;
using RemoteTech.Common.RangeModels;
using System;
using System.Collections.Generic;

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
        private RemoteTechCommNetUIModeButton customCommNetModeButton = null;

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
            var homes = FindObjectsOfType<CommNetHome>();
            for (int i = 0; i < homes.Length; i++)
            {
                var customHome = homes[i].gameObject.AddComponent(typeof(RemoteTechCommNetHome)) as RemoteTechCommNetHome;
                customHome.copyOf(homes[i]);
                UnityEngine.Object.Destroy(homes[i]);
            }

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
                //Other variables
                gameNode.AddValue("DisplayModeTracking", RemoteTechCommNetUI.CustomModeTrackingStation);
                gameNode.AddValue("DisplayModeFlight", RemoteTechCommNetUI.CustomModeFlightMap);

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
