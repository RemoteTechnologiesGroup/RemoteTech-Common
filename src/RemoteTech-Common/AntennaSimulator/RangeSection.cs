using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace RemoteTech.Common.AntennaSimulator
{
    public class RangeSection : SimulatorWindowSection
    {
        public override DialogGUIBase[] create(List<Part> parts, AntennaSimulatorWindow primary)
        {
            List<DialogGUIBase> components = new List<DialogGUIBase>();
            DialogGUILabel massiveMessageLabel = new DialogGUILabel("GO GO GO RANGERS!!", true, false);
            components.Add(massiveMessageLabel);;
            return components.ToArray();
        }
    }
}
