using CommNet;
using KSP.UI.Screens.Mapview;

namespace RemoteTech.Common.RemoteTechCommNet
{
    /// <summary>
    /// CommNetUI is the view in the Model–view–controller sense. Everything a player is seeing goes through this class
    /// </summary>
    public class RemoteTechCommNetUI : CommNetUI
    {
        public static new RemoteTechCommNetUI Instance
        {
            get;
            protected set;
        }

        /// <summary>
        /// Activate things when the player enter a scene that uses CommNet UI
        /// </summary>
        public override void Show()
        {
            registerMapNodeIconCallbacks();
            base.Show();
        }

        /// <summary>
        /// Clean up things when the player exits a scene that uses CommNet UI
        /// </summary>
        public override void Hide()
        {
            deregisterMapNodeIconCallbacks();
            base.Hide();
        }

        /// <summary>
        /// This UpdateDisplay() handles everything related to the node and edge rendering
        /// </summary>
        protected override void UpdateDisplay()
        {
            base.UpdateDisplay();
        }

        /// <summary>
        /// Register own callbacks
        /// </summary>
        protected void registerMapNodeIconCallbacks()
        {
            var vessels = FlightGlobals.fetch.vessels;

            for (int i = 0; i < vessels.Count; i++)
            {
                MapObject mapObj = vessels[i].mapObject;

                if (mapObj.type == MapObject.ObjectType.Vessel)
                    mapObj.uiNode.OnUpdateVisible += new Callback<MapNode, MapNode.IconData>(this.OnMapNodeUpdateVisible);
            }
        }

        /// <summary>
        /// Remove own callbacks
        /// </summary>
        protected void deregisterMapNodeIconCallbacks()
        {
            var vessels = FlightGlobals.fetch.vessels;

            for (int i = 0; i < vessels.Count; i++)
            {
                MapObject mapObj = vessels[i].mapObject;
                mapObj.uiNode.OnUpdateVisible -= new Callback<MapNode, MapNode.IconData>(this.OnMapNodeUpdateVisible);
            }
        }

        /// <summary>
        /// Update the MapNode object of each CommNet vessel
        /// </summary>
        private void OnMapNodeUpdateVisible(MapNode node, MapNode.IconData iconData)
        {
            var thisVessel = node.mapObject.vessel.connection;

            //do some thing on this vessel
        }
    }
}
