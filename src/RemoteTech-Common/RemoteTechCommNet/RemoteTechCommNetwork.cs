using CommNet;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public class RemoteTechCommNetwork : CommNetwork
    {
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
