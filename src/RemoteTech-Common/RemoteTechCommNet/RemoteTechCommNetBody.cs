using CommNet;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public class RemoteTechCommNetBody : CommNetBody
    {
        public void copyOf(CommNetBody stockBody)
        {
            this.body = stockBody.GetComponentInChildren<CelestialBody>();
            //this.occluder = stockBody.GetComponentInChildren<Occluder>(); // maybe too early as it is null at beginning
        }
    }
}
