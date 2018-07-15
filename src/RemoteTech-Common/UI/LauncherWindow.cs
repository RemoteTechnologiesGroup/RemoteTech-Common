using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteTech.Common.UI
{
    public class LauncherWindow : AbstractDialog
    {
        public LauncherWindow() : base("rtlauncherdialog", 
                                        "RemoteTech Applications", 
                                        0.5f, 
                                        0.5f, 
                                        300,
                                        300,
                                        new DialogOptions[] { DialogOptions.HideDismissButton })
        {
        }

        protected override List<DialogGUIBase> Draw()
        {
            List<DialogGUIBase> componments = new List<DialogGUIBase>();

            DialogGUIButton antennaSimButton = new DialogGUIButton("Antenna Simulator", delegate { }, false);
            DialogGUIButton visualStyleButton = new DialogGUIButton("Visual styles", delegate { new VisualStyleWindow().launch(); }, false);
            DialogGUIButton aboutButton = new DialogGUIButton("About", delegate { }, false);
            componments.Add(new DialogGUIVerticalLayout(new DialogGUIBase[] { antennaSimButton, visualStyleButton, aboutButton }));

            return componments;
        }
    }
}
