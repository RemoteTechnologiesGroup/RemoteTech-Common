using CommNet;

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
        /// This UpdateDisplay() handles everything related to the node and edge rendering
        /// </summary>
        protected override void UpdateDisplay()
        {
            base.UpdateDisplay();
        }
    }
}
