using UnityEngine;

namespace RemoteTech.Common.UI.DialogGUI
{
    /// <summary>
    /// Subclass of DialogGUIScrollList to customise inner functions/objects
    /// </summary>
    public class CustomDialogGUIScrollList : DialogGUIScrollList
    {
        protected bool defaultTop = false;
        protected bool defaultBottom = false;

        public CustomDialogGUIScrollList(Vector2 size, bool hScroll, bool vScroll, DialogGUILayoutBase layout) :
            base(size, hScroll, vScroll, layout)
        {
            SetDefaultScrollToTop();
        }

#if !KSP13
        public CustomDialogGUIScrollList(Vector2 size, Vector2 contentSize, bool hScroll, bool vScroll, DialogGUILayoutBase layout) :
            base(size, contentSize, hScroll, vScroll, layout)
        {
            SetDefaultScrollToTop();
        }
#endif

        public override void Update()
        {
            base.Update();

            if (defaultTop)
            {
                this.scrollRect.content.pivot = new Vector2(0, 1);
                defaultTop = false;
            }
            if (defaultBottom)
            {
                this.scrollRect.content.pivot = new Vector2(0, 0);
                defaultBottom = false;
            }
        }

        public void SetDefaultScrollToTop()
        {
            defaultTop = true;
        }

        public void SetDefaultScrollToBottom()
        {
            defaultBottom = true;
        }
    }
}
