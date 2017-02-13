using CommNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public class RemoteTechCommNetNetwork : CommNetNetwork
    {
        public static new RemoteTechCommNetNetwork Instance
        {
            get;
            protected set;
        }

        protected override void Awake()
        {
            Logging.Info("CommNet Network booting");

            CommNetNetwork.Instance = this;
            this.CommNet = new RemoteTechCommNetwork();

            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                GameEvents.onPlanetariumTargetChanged.Add(new EventData<MapObject>.OnEvent(this.OnMapFocusChange));
            }

            GameEvents.OnGameSettingsApplied.Add(new EventVoid.OnEvent(this.ResetNetwork));
            ResetNetwork(); // Please retain this so that KSP can properly reset
        }

        protected new void ResetNetwork()
        {
            Logging.Info("CommNet Network rebooted");

            this.CommNet = new RemoteTechCommNetwork();
            GameEvents.CommNet.OnNetworkInitialized.Fire();
        }
    }
}
