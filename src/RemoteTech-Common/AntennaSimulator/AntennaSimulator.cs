using System;
using System.Collections.Generic;
using System.Linq;
using RemoteTech.Common.UI;
using UnityEngine;
using UnityEngine.UI;
using static RemoteTech.Common.AntennaSimulator.SimulatorSection;

namespace RemoteTech.Common.AntennaSimulator
{
    public abstract class SimulatorSection
    {
        public enum SimulationType : short { RANGE, POWER, SCIENCE };
        public SimulationType sectionType{ get; set;}
        protected AntennaSimulator simulator { get; set; }

        public SimulatorSection(SimulationType sectionType, AntennaSimulator simulator)
        {
            this.sectionType = sectionType;
            this.simulator = simulator;
        }

        public virtual void analyse(List<Part> parts) { }
        public abstract DialogGUIBase[] draw();
        public virtual void awake() { }
        public virtual void destroy() { }

        public static void registerLayoutComponents(DialogGUIVerticalLayout layout)
        {
            if (layout.children.Count < 1)
                return;

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(layout.uiItem.gameObject.transform);
            for (int i = 0; i < layout.children.Count; i++)
            {
                if (!(layout.children[i] is DialogGUIContentSizer)) // avoid if DialogGUIContentSizer is detected
                    layout.children[i].Create(ref stack, HighLogic.UISkin);
            }
        }

        public static void deregisterLayoutComponents(DialogGUIVerticalLayout layout)
        {
            if (layout.children.Count < 1)
                return;

            int size = layout.children.Count;
            for (int i = size - 1; i >= 0; i--)
            {
                DialogGUIBase thisChild = layout.children[i];
                if (!(thisChild is DialogGUIContentSizer)) // avoid if DialogGUIContentSizer is detected
                {
                    layout.children.RemoveAt(i);
                    thisChild.uiItem.gameObject.DestroyGameObjectImmediate();
                }
            }
        }
    }

    public class AntennaSimulator : AbstractDialog
    {
        private SimulationType currentSectionType;
        private List<SimulatorSection> pageSections;
        private DialogGUIVerticalLayout contentPaneLayout;

        public static readonly int dialogWidth = 650;
        public static readonly int dialogHeight = 500;

        public AntennaSimulator() : base("RemoteTech Antenna Simulator",
                                                0.75f,
                                                0.5f,
                                                dialogWidth,
                                                dialogHeight,
                                                new DialogOptions[] { DialogOptions.HideDismissButton, DialogOptions.AllowBgInputs})
        {
            pageSections = new List<SimulatorSection>(Enum.GetNames(typeof(SimulationType)).Length);
            pageSections.Add(new RangeSection(this));
            pageSections.Add(new PowerSection(this));
            pageSections.Add(new ScienceSection(this));
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> contentComponents = new List<DialogGUIBase>();

            string vesselName;
            if (HighLogic.LoadedSceneIsFlight)
                vesselName = FlightGlobals.ActiveVessel.vesselName;
            else
                vesselName = EditorLogic.fetch.ship.shipName;

            DialogGUILabel descrptionLabel = new DialogGUILabel(string.Format("Based on this vessel '{0}', a number of estimations are computed and displayed below.\n\n", vesselName), true, false);
            contentComponents.Add(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { descrptionLabel }));

            // BUTTON TABS
            DialogGUIButton rangeButton = new DialogGUIButton("Antenna range", delegate { displayContent(SimulationType.RANGE); }, false);
            DialogGUIButton scienceButton = new DialogGUIButton("Science data", delegate { displayContent(SimulationType.SCIENCE); }, false);
            DialogGUIButton powerButton = new DialogGUIButton("Power system", delegate { displayContent(SimulationType.POWER); }, false);
            DialogGUIButton refreshButton = new DialogGUIButton("Reset", delegate { displayContent(currentSectionType); }, false);

            DialogGUIHorizontalLayout tabbedButtonRow = new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { rangeButton, powerButton });
            if (ResearchAndDevelopment.Instance != null)
                tabbedButtonRow.AddChild(scienceButton);
            tabbedButtonRow.AddChild(new DialogGUIFlexibleSpace());
            tabbedButtonRow.AddChild(refreshButton);
            contentComponents.Add(tabbedButtonRow);

            DialogGUIBase[] rows = new DialogGUIBase[] { new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true) };
            contentPaneLayout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(10, 25, 10, 10), TextAnchor.UpperLeft, rows);
            contentComponents.Add(new DialogGUIScrollList(Vector2.one, false, true, contentPaneLayout));

            return contentComponents;
        }

        protected override void OnAwake(object[] args)
        {
            for(int i=0; i< pageSections.Count; i++)
                pageSections[i].awake();

            displayContent(SimulationType.RANGE); // the info panel a player sees for the first time
        }

        protected override void OnPreDismiss()
        {
            for (int i = 0; i < pageSections.Count; i++)
                pageSections[i].destroy();
        }

        private void displayContent(SimulationType newType)
        {
            currentSectionType = newType;

            List<Part> parts;
            if (HighLogic.LoadedSceneIsFlight)
                parts = FlightGlobals.ActiveVessel.Parts;
            else
                parts = EditorLogic.fetch.ship.Parts;

            for (int i = 0; i < pageSections.Count; i++)
                pageSections[i].analyse(parts);

            deregisterLayoutComponents(contentPaneLayout);
            contentPaneLayout.AddChildren(getSection(newType).draw());
            registerLayoutComponents(contentPaneLayout);
        }

        public SimulatorSection getSection(SimulationType thisType)
        {
            return pageSections.Find(x => x.sectionType == thisType);
        }
    }
}
