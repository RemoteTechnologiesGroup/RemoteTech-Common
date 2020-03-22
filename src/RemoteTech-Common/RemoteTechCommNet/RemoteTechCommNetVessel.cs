using System;
using CommNet;
using RemoteTech.Common.Api;
using RemoteTech.Common.Interfaces;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public class RemoteTechCommNetVessel : CommNetVessel, IPersistenceSave, IPersistenceLoad
    {
        private IDelayManager _delayManager;

        /// <summary>
        /// Call the stock OnNetworkInitialized() to be added to CommNetNetwork as node
        /// </summary>
        protected override void OnNetworkInitialized()
        {
            base.OnNetworkInitialized();
        }

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        protected override void OnStart()
        {
            if (this.vessel != null)
            {
                //if this connection is stock, replace it with this custom connection
                if (this.vessel.connection != null && !(this.vessel.connection is RemoteTechCommNetVessel) && CommNetNetwork.Initialized)
                {
                    CommNetNetwork.Remove(this.vessel.connection.Comm); //delete stock node from commnet network
                    //UnityEngine.Object.DestroyObject(this.vessel.connection); // don't do this. there are still action-call leftovers of stock CommNetVessel
                    this.vessel.connection = this;
                }
            }
            base.OnStart();
        }

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

        /// <summary>
        /// Perform the updates on CommNet vessel's communication
        /// </summary>
        protected override void UpdateComm()
        {
            base.UpdateComm();

            //Preventative measure on null antenna range curve due to 3rd-party mods
            //Effect: cause commNode.antennaRelay.rangeCurve.Evaluate(normalizedRange) to fail *without* throwing exception
            if (this.comm.antennaRelay.rangeCurve == null || this.comm.antennaTransmit.rangeCurve == null)
            {
                if (this.comm.antennaRelay.rangeCurve == null && this.comm.antennaTransmit.rangeCurve != null)
                {
                    this.comm.antennaRelay.rangeCurve = this.comm.antennaTransmit.rangeCurve;
                }
                else if (this.comm.antennaRelay.rangeCurve != null && this.comm.antennaTransmit.rangeCurve == null)
                {
                    this.comm.antennaTransmit.rangeCurve = this.comm.antennaRelay.rangeCurve;
                }
                else //failsafe
                {
                    Logging.Error("CommNetVessel '{0}' has no range curve for both relay and transmit AntennaInfo! Fall back to a simple curve.", this.Vessel.GetName());
                    this.comm.antennaTransmit.rangeCurve = this.comm.antennaRelay.rangeCurve = new DoubleCurve(new DoubleKeyframe[] { new DoubleKeyframe(0.0, 0.0), new DoubleKeyframe(1.0, 1.0) });
                }
            }
        }

        public void PersistenceLoad()
        {

        }

        public void PersistenceSave()
        {
            
        }
    }
}