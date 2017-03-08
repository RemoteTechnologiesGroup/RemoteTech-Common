using CommNet;

namespace RemoteTech.Common.RemoteTechCommNet
{
    /// <summary>
    /// Copy required for the customised CommNet
    /// </summary>
    public class RemoteTechCommNetBody : CommNetBody
    {
        public void copyOf(CommNetBody stockBody)
        {
            Logging.Info("CommNet Body '{0}' added", stockBody.name);

            this.body = stockBody.GetComponentInChildren<CelestialBody>();

            //this.occluder is initalised by OnNetworkInitialized() later
        }
    }
}
