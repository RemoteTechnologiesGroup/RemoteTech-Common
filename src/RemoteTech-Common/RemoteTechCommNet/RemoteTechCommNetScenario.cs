using CommNet;
using RemoteTech.Common.RangeModels;

namespace RemoteTech.Common.RemoteTechCommNet
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER, GameScenes.EDITOR)]
    public class RemoteTechCommNetScenario : CommNetScenario
    {
        private RemoteTechCommNetUI customUI = null;

        /* Note:
         * 1) On entering a desired scene, OnLoad() and then Start() are called.
         * 2) On leaving the scene, OnSave() is called
         * 3) GameScenes.SPACECENTER is recommended so that the RemoteTech data can be verified and error-corrected in advance
         */

        public static new RemoteTechCommNetScenario Instance
        {
            get;
            set;
        }

        protected override void Start()
        {
            //base.Start(); //turn off base.Start() contained the AddComponent() for the stock components

            //TODO pick a range model depending on settings
            RangeModel = new StandardRangeModel();

            RemoteTechCommNetScenario.Instance = this;
            CommNetNetwork.Instance.CommNet = new RemoteTechCommNetwork();

            var ui = FindObjectOfType<CommNetUI>();
            customUI = ui.gameObject.AddComponent<RemoteTechCommNetUI>();
            UnityEngine.Object.Destroy(ui);           

            var homes = FindObjectsOfType<CommNetHome>();
            for (int i = 0; i < homes.Length; i++)
            {
                var customHome = homes[i].gameObject.AddComponent(typeof(RemoteTechCommNetHome)) as RemoteTechCommNetHome;
                customHome.copyOf(homes[i]);
                UnityEngine.Object.Destroy(homes[i]);
            }

            var bodies = FindObjectsOfType<CommNetBody>();
            for (int i = 0; i < bodies.Length; i++)
            {
                var customBody = bodies[i].gameObject.AddComponent(typeof(RemoteTechCommNetBody)) as RemoteTechCommNetBody;
                customBody.copyOf(bodies[i]);
                UnityEngine.Object.Destroy(bodies[i]);
            }
        }

        public override void OnAwake()
        {
            //base.OnAwake(); //turn off CommNetScenario's instance check

            GameEvents.OnGameSettingsApplied.Add(new EventVoid.OnEvent(this.ResetNetwork));
        }

        private void OnDestroy()
        {
            if (this.customUI != null)
                UnityEngine.Object.Destroy(this.customUI);

            GameEvents.OnGameSettingsApplied.Remove(new EventVoid.OnEvent(this.ResetNetwork));
        }

        public void ResetNetwork()
        {
            CommNetNetwork.Instance.CommNet = new RemoteTechCommNetwork();
            GameEvents.CommNet.OnNetworkInitialized.Fire();
        }
    }
}
