using CommNet;
using RemoteTech.Common.RangeModels;

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

            //Replace the CommNet user interface
            var ui = FindObjectOfType<CommNetUI>(); // the order of the three lines is important
            customUI = gameObject.AddComponent<RemoteTechCommNetUI>(); // gameObject.AddComponent<>() is "new" keyword for Monohebaviour class
            UnityEngine.Object.Destroy(ui);

            //Replace the CommNet network
            var net = FindObjectOfType<CommNetNetwork>();
            customNetwork = gameObject.AddComponent<RemoteTechCommNetNetwork>();
            UnityEngine.Object.Destroy(net);

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
        }

        public override void OnAwake()
        {
            //base.OnAwake(); //turn off CommNetScenario's instance check

        }

        private void OnDestroy()
        {
            if (this.customUI != null)
                UnityEngine.Object.Destroy(this.customUI);

            if (this.customNetwork != null)
                UnityEngine.Object.Destroy(this.customNetwork);
        }
    }
}
