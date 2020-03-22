using CommNet;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public class RemoteTechCommNetwork : CommNetwork
    {
        private const int REFRESH_TICKS = 50;
        private int mTick = 0, mTickIndex = 0;

        //IEqualityComparer<CommNode> comparer = commNode.Comparer; // a combination of third-party mods somehow  affects CommNode's IEqualityComparer on two objects
        //return commVessels.Find(x => comparer.Equals(commNode, x.Comm)).Vessel;
        /// <summary>
        /// Check if two CommNodes are the exact same object
        /// </summary>
        public static bool AreSame(CommNode a, CommNode b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return a.precisePosition == b.precisePosition;
        }

        /// <summary>
        /// Override the CommNet's satisfaction check on a potential connection or add extra satisfaction checks
        /// </summary>
        /// <returns>Tell if there should be a connection between both a and b</returns>
        protected override bool SetNodeConnection(CommNode a, CommNode b)
        {
            /*
            //Code sample
            if (!InRange(a,b))
            {
                this.Disconnect(a, b, true);
                return false;
            }
            */

            // disconnect connection between "itself"
            if (AreSame(a, b))
            {
                this.Disconnect(a, b, true);
            }

            return base.SetNodeConnection(a, b);
        }

        /// <summary>
        /// Perform updates on connection network of vessels
        /// </summary>
        protected override void UpdateNetwork()
        {
            var count = this.nodes.Count;
            if (count == 0) { return; }

            //This optimisation is to spread the full workload of connection check to few frames, instead of every frame.
            var baseline = (count / REFRESH_TICKS);
            var takeCount = baseline + (((mTick++ % REFRESH_TICKS) < (count - baseline * REFRESH_TICKS)) ? 1 : 0);

            for (int i = mTickIndex; i < mTickIndex + takeCount; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    this.SetNodeConnection(this.nodes[i], this[j]);
                }
            }

            mTickIndex += takeCount;
            mTickIndex = mTickIndex % this.nodes.Count;
        }
    }
}
