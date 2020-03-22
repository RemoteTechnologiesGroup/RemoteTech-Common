using CommNet;

namespace RemoteTech.Common.RemoteTechCommNet
{
    /// <summary>
    /// Extend the functionality of the KSP's CommNetNetwork (co-primary model in the Model–view–controller sense; CommNet<> is the other co-primary one)
    /// </summary>
    public class RemoteTechCommNetNetwork : CommNetNetwork
    {
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
