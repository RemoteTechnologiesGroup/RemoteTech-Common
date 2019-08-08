using System.ComponentModel;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public enum RemoteTechAntennaType
    {
        [Description("Internal")]
        INTERNAL = 0,
        [Description("Dish")]
        DISH = 1,
        [Description("Omni")]
        OMNI = 2,
        [Description("Phased Array")]
        PHASEDARRAY = 3,
        [Description("Optical")]
        OPTICAL = 4
    }
}
