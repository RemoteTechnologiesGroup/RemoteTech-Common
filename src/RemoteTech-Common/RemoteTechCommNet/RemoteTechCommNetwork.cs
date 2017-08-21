using CommNet;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public class RemoteTechCommNetwork : CommNetwork
    {
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

            return base.SetNodeConnection(a, b);
        }
    }
}
