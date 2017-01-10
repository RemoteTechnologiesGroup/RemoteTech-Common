using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace RemoteTech.Common.UI
{
    public enum DialogOptions
    {
        HideDismissButton,
        AllowBgInputs,
        NonDraggable
    };

    public abstract class AbstractDialog
    {
        protected bool isDisplayed = false;
        protected string dialogTitle;
        protected int windowWidth;
        protected int windowHeight;
        protected float normalizedCenterX; //0.0f to 1.0f
        protected float normalizedCenterY; //0.0f to 1.0f

        protected string dismissButtonText = "Close";
        protected bool showDismissButton = true;
        protected bool blockBackgroundInputs = true;
        protected bool draggable = true;

        protected PopupDialog popupDialog = null;

        public AbstractDialog(string dialogTitle, float normalizedCenterX, float normalizedCenterY, int windowWidth, int windowHeight, DialogOptions[] args)
        {
            this.dialogTitle = dialogTitle;
            this.windowWidth = windowWidth;
            this.windowHeight = windowHeight;
            this.normalizedCenterX = normalizedCenterX;
            this.normalizedCenterY = normalizedCenterY;

            processArguments(args);
        }

        protected abstract List<DialogGUIBase> drawContentComponents();
        protected virtual void OnAwake(System.Object[] args) { }
        protected virtual void OnPreDismiss() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnResize() { }

        public void launch()
        {
            launch(new System.Object[] { });
        }

        public void launch(System.Object[] args)
        {
            if (this.isDisplayed)
                return;

            this.isDisplayed = true;
            OnAwake(args);
            popupDialog = spawnDialog();
        }

        public void dismiss()
        {
            if (this.isDisplayed && popupDialog != null)
            {
                OnPreDismiss();
                popupDialog.Dismiss();
                this.isDisplayed = false;
            }
        }

        private void processArguments(DialogOptions[] args)
        {
            if (args == null)
                return;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case DialogOptions.HideDismissButton:
                        this.showDismissButton = false;
                        break;
                    case DialogOptions.AllowBgInputs:
                        this.blockBackgroundInputs = false;
                        break;
                    case DialogOptions.NonDraggable:
                        this.draggable = false;
                        break;
                }
            }
        }

        private PopupDialog spawnDialog()
        {
            /* This dialog looks like below
             * -----------------------
             * |        TITLE        |
             * |---------------------|
             * |                     |
             * |       CONTENT       |
             * |                     |
             * |---------------------|
             * |      [DISMISS]      |
             * ----------------------- 
             */

            List<DialogGUIBase> dialogComponentList;

            //content
            var contentComponentList = drawContentComponents();

            if (contentComponentList == null)
            {
                dialogComponentList = new List<DialogGUIBase>(1);
            }
            else
            {
                dialogComponentList = new List<DialogGUIBase>(contentComponentList.Count + 1);
                dialogComponentList.AddRange(contentComponentList);
            }

            //close button and some info
            DialogGUIBase[] footer;
            if (showDismissButton)
            {
                footer = new DialogGUIBase[]
                    {
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIButton(dismissButtonText, dismiss),
                    new DialogGUIFlexibleSpace()
                    };
                dialogComponentList.Add(new DialogGUIHorizontalLayout(footer));
            }

            //Spawn the dialog
            var moDialog = new MultiOptionDialog("",
                                                dialogTitle,
                                                HighLogic.UISkin,
                                                new Rect(normalizedCenterX, normalizedCenterY, windowWidth, windowHeight),
                                                dialogComponentList.ToArray());

            moDialog.OnUpdate = OnUpdate;
            moDialog.OnResize = OnResize;

            var newDialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                                                        new Vector2(0.5f, 0.5f),
                                                        moDialog,
                                                        false,  // persistAcrossScreen
                                                        HighLogic.UISkin,
                                                        blockBackgroundInputs); // isModal

            newDialog.SetDraggable(draggable);
            return newDialog;
        }
    }
}
