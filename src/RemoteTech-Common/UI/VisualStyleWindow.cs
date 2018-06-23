using CommNetConstellation.UI;
using RemoteTech.Common.RemoteTechCommNet;
using RemoteTech.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteTech.Common.UI
{
    public class VisualStyleWindow : AbstractDialog
    {
        private short colorIndex;
        private DialogGUIImage dcColorImage, ocColorImage, gsColorImage, lsColorImage, acColorImage, dc2ColorImage;

        public VisualStyleWindow() : base("visualstylewin", 
                                            "Visual Styles", 
                                            0.5f, 
                                            0.5f, 
                                            250, 
                                            300, 
                                            new DialogOptions[] {})
        {
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> componments = new List<DialogGUIBase>();

            DialogGUILabel dishConnectionLabel = new DialogGUILabel("Dish Connection");
            DialogGUILabel omniConnectionLabel = new DialogGUILabel("Omni Connection");
            DialogGUILabel groundStationLabel = new DialogGUILabel("Ground Station");
            DialogGUILabel lowSignalLabel = new DialogGUILabel("Low Signal");
            DialogGUILabel activeConnectionLabel = new DialogGUILabel("Active Connection");
            DialogGUILabel directConnectionLabel = new DialogGUILabel("Direct Connection");

            dcColorImage = new DialogGUIImage(new Vector2(80, 10), new Vector2(0, 0), RemoteTechCommNetScenario.Instance.DishConnectionColor, UiUtils.CreateAndColorize(80, 10, Color.white));
            ocColorImage = new DialogGUIImage(new Vector2(80, 10), new Vector2(0, 0), RemoteTechCommNetScenario.Instance.OmniConnectionColor, UiUtils.CreateAndColorize(80, 10, Color.white));
            gsColorImage = new DialogGUIImage(new Vector2(80, 10), new Vector2(0, 0), RemoteTechCommNetScenario.Instance.GroundStationDotColor, UiUtils.CreateAndColorize(80, 10, Color.white));
            lsColorImage = new DialogGUIImage(new Vector2(80, 10), new Vector2(0, 0), RemoteTechCommNetScenario.Instance.LowSignalConnectionColor, UiUtils.CreateAndColorize(80, 10, Color.white));
            acColorImage = new DialogGUIImage(new Vector2(80, 10), new Vector2(0, 0), RemoteTechCommNetScenario.Instance.ActiveConnectionColor, UiUtils.CreateAndColorize(80, 10, Color.white));
            dc2ColorImage = new DialogGUIImage(new Vector2(80, 10), new Vector2(0, 0), RemoteTechCommNetScenario.Instance.DirectConnectionColor, UiUtils.CreateAndColorize(80, 10, Color.white));

            DialogGUIButton dcColorButton = new DialogGUIButton("", delegate { colorIndex = 0; new ColorPickerDialog(RemoteTechCommNetScenario.Instance.DishConnectionColor, userSelectColor).launch(); }, 32, 32, false, new DialogGUIHorizontalLayout(TextAnchor.MiddleCenter, new DialogGUIBase[] { dcColorImage }));
            DialogGUIButton ocColorButton = new DialogGUIButton("", delegate { colorIndex = 1; new ColorPickerDialog(RemoteTechCommNetScenario.Instance.OmniConnectionColor, userSelectColor).launch(); }, 32, 32, false, new DialogGUIHorizontalLayout(TextAnchor.MiddleCenter, new DialogGUIBase[] { ocColorImage }));
            DialogGUIButton gsColorButton = new DialogGUIButton("", delegate { colorIndex = 2; new ColorPickerDialog(RemoteTechCommNetScenario.Instance.GroundStationDotColor, userSelectColor).launch(); }, 32, 32, false, new DialogGUIHorizontalLayout(TextAnchor.MiddleCenter, new DialogGUIBase[] { gsColorImage }));
            DialogGUIButton lsColorButton = new DialogGUIButton("", delegate { colorIndex = 3; new ColorPickerDialog(RemoteTechCommNetScenario.Instance.LowSignalConnectionColor, userSelectColor).launch(); }, 32, 32, false, new DialogGUIHorizontalLayout(TextAnchor.MiddleCenter, new DialogGUIBase[] { lsColorImage }));
            DialogGUIButton acColorButton = new DialogGUIButton("", delegate { colorIndex = 4; new ColorPickerDialog(RemoteTechCommNetScenario.Instance.ActiveConnectionColor, userSelectColor).launch(); }, 32, 32, false, new DialogGUIHorizontalLayout(TextAnchor.MiddleCenter, new DialogGUIBase[] { acColorImage }));
            DialogGUIButton dc2ColorButton = new DialogGUIButton("", delegate { colorIndex = 5; new ColorPickerDialog(RemoteTechCommNetScenario.Instance.DirectConnectionColor, userSelectColor).launch(); }, 32, 32, false, new DialogGUIHorizontalLayout(TextAnchor.MiddleCenter, new DialogGUIBase[] { dc2ColorImage }));

            DialogGUIGridLayout gt = new DialogGUIGridLayout(new RectOffset(0,0,5,5), new Vector2(100, 20), new Vector2(0,0), 
                                                        GridLayoutGroup.Corner.UpperLeft, GridLayoutGroup.Axis.Vertical, TextAnchor.MiddleLeft, GridLayoutGroup.Constraint.FixedColumnCount, 2,
                                                        new DialogGUIBase[] { dishConnectionLabel, omniConnectionLabel, lowSignalLabel, groundStationLabel, activeConnectionLabel, directConnectionLabel,
                                                                              dcColorButton, ocColorButton, lsColorButton, gsColorButton, acColorButton, dc2ColorButton});

            componments.Add(gt);

            return componments;
        }

        public void userSelectColor(Color newColor)
        {
            switch(colorIndex)
            {
                case 0:
                    RemoteTechCommNetScenario.Instance.DishConnectionColor = newColor;
                    dcColorImage.uiItem.GetComponent<RawImage>().color = newColor;
                    break;
                case 1:
                    RemoteTechCommNetScenario.Instance.OmniConnectionColor = newColor;
                    ocColorImage.uiItem.GetComponent<RawImage>().color = newColor;
                    break;
                case 2:
                    RemoteTechCommNetScenario.Instance.GroundStationDotColor = newColor;
                    gsColorImage.uiItem.GetComponent<RawImage>().color = newColor;
                    break;
                case 3:
                    RemoteTechCommNetScenario.Instance.LowSignalConnectionColor = newColor;
                    lsColorImage.uiItem.GetComponent<RawImage>().color = newColor;
                    break;
                case 4:
                    RemoteTechCommNetScenario.Instance.ActiveConnectionColor = newColor;
                    acColorImage.uiItem.GetComponent<RawImage>().color = newColor;
                    break;
                case 5:
                    RemoteTechCommNetScenario.Instance.DirectConnectionColor = newColor;
                    dc2ColorImage.uiItem.GetComponent<RawImage>().color = newColor;
                    break;
            }
        }
    }
}
