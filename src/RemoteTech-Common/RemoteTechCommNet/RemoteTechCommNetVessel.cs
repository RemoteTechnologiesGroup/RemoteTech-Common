using System;
using CommNet;
using RemoteTech.Common.Api;
using RemoteTech.Common.Interfaces;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public class RemoteTechCommNetVessel : CommNetVessel, IPersistenceSave, IPersistenceLoad
    {
        private IDelayManager _delayManager;

        public override void OnNetworkPostUpdate()
        {
            base.OnNetworkPostUpdate();

            UpdateDelay();
        }

        public virtual void UpdateDelay()
        {
            if (!RemoteTechModules.RemoteTechDelayAssemblyLoaded)
                return;

            if (_delayManager == null)
                _delayManager =
                    RemoteTechModules.GetObjectFromInterface<IDelayManager>(
                        RemoteTechModules.RemoteTechDelayAssemblyName, Type.EmptyTypes);

            // set up the delay
            signalDelay = _delayManager?.GetVesselDelay(vessel) ?? 0;
        }

        /// <summary>
        /// On-demand method to do network update manually
        /// </summary>
        public void computeUnloadedUpdate()
        {
            this.unloadedDoOnce = true;
        }

        protected override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);

            if (gameNode.HasNode(GetType().FullName))
                gameNode.RemoveNode(GetType().FullName);

            gameNode.AddNode(ConfigNode.CreateConfigFromObject(this));
        }

        protected override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);

            if (gameNode.HasNode(GetType().FullName))
                ConfigNode.LoadObjectFromConfig(this, gameNode.GetNode(GetType().FullName));
        }

        public void PersistenceLoad()
        {

        }

        public void PersistenceSave()
        {
            
        }
    }
}