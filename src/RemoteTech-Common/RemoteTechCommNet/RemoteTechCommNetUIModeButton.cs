﻿using CommNet;
using KSP.Localization;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public class RemoteTechCommNetUIModeButton: CommNetUIModeButton
    {
        private bool initialised = false;

        public void copyOf(CommNetUIModeButton stockTUButton)
        {
            this.button = stockTUButton.button;
            this.stateImage = stockTUButton.stateImage;
            this.tooltip = stockTUButton.tooltip;
            this.initialised = true;
        }

        protected override void Awake()
        {
            //if (CommNet.CommNetScenario.CommNetEnabled)
            if(true)//TODO: tie to RT setting
            {
                base.gameObject.SetActive(true);
                //GameEvents.CommNet.OnNetworkInitialized.Add(new EventVoid.OnEvent(this.OnNetworkInitialized));
                //Issue: For unknown reason, OnNetworkInitialized() is never called in tracking station or flight
            }
        }

        protected override void OnDestroy()
        {
            //GameEvents.CommNet.OnNetworkInitialized.Remove(new EventVoid.OnEvent(this.OnNetworkInitialized)); //see Awake()
        }

        /// <summary>
        /// Overrode to add custom display mode
        /// </summary>
        public override void UpdateUI()
        {
            if (this.initialised)
            {
                var text = Localizer.Format("#autoLOC_6002257") + ": " + RemoteTechCommNetUI.CustomMode.displayDescription();
                if (this.tooltip.textString != text)
                {
                    this.tooltip.SetText(text);
                }

                if (RemoteTechCommNetUI.CustomMode == RemoteTechCommNetUI.CustomDisplayMode.MultiPaths)
                {
                    this.stateImage.SetState((int)RemoteTechCommNetUI.CustomDisplayMode.Network);//need to set to network img 1st before multipaths
                }
                this.stateImage.SetState((int)RemoteTechCommNetUI.CustomMode);
            }
        }
    }
}
