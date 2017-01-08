using CommNet;

namespace RemoteTech.Common.RemoteTechCommNet
{
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
