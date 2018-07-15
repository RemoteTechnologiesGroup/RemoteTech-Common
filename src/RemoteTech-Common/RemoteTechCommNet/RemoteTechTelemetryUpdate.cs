using CommNet;
using KSP.UI.Screens.Flight;
using KSP.UI.TooltipTypes;
using RemoteTech.Common.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static RemoteTech.Common.RemoteTechCommNet.RemoteTechCommNetUI;

namespace RemoteTech.Common.RemoteTechCommNet
{
    /// <summary>
    /// Simple data structure to store the populated data of destroyed stock interface temporarily
    /// </summary>
    public class TelemetryUpdateData
    {
        public CommNetUIModeButton modeButton;
        public Sprite NOSIG;
        public Sprite NOEP;
        public Sprite BLK;
        public Sprite AUP;
        public Sprite ADN;
        public Sprite EP0;
        public Sprite EP1;
        public Sprite EP2;
        public Sprite CK1;
        public Sprite CK2;
        public Sprite CK3;
        public Sprite CP1;
        public Sprite CP2;
        public Sprite CP3;
        public Sprite SS0;
        public Sprite SS1;
        public Sprite SS2;
        public Sprite SS3;
        public Sprite SS4;
        public Image arrow_icon;
        public Image firstHop_icon;
        public Image lastHop_icon;
        public Image control_icon;
        public Image signal_icon;
        public TooltipController_Text firstHop_tooltip;
        public TooltipController_Text arrow_tooltip;
        public TooltipController_Text lastHop_tooltip;
        public TooltipController_Text control_tooltip;
        public TooltipController_SignalStrength signal_tooltip;

        public TelemetryUpdateData(TelemetryUpdate stockTU)
        {
            this.modeButton = stockTU.modeButton;
            this.NOSIG = stockTU.NOSIG;
            this.NOEP = stockTU.NOEP;
            this.BLK = stockTU.BLK;
            this.AUP = stockTU.AUP;
            this.ADN = stockTU.ADN;
            this.EP0 = stockTU.EP0;
            this.EP1 = stockTU.EP1;
            this.EP2 = stockTU.EP2;
            this.CK1 = stockTU.CK1;
            this.CK2 = stockTU.CK2;
            this.CK3 = stockTU.CK3;
            this.CP1 = stockTU.CP1;
            this.CP2 = stockTU.CP2;
            this.CP3 = stockTU.CP3;
            this.SS0 = stockTU.SS0;
            this.SS1 = stockTU.SS1;
            this.SS2 = stockTU.SS2;
            this.SS3 = stockTU.SS3;
            this.SS4 = stockTU.SS4;
            this.arrow_icon = stockTU.arrow_icon;
            this.firstHop_icon = stockTU.firstHop_icon;
            this.lastHop_icon = stockTU.lastHop_icon;
            this.control_icon = stockTU.control_icon;
            this.signal_icon = stockTU.signal_icon;
            this.firstHop_tooltip = stockTU.firstHop_tooltip;
            this.arrow_tooltip = stockTU.arrow_tooltip;
            this.lastHop_tooltip = stockTU.lastHop_tooltip;
            this.control_tooltip = stockTU.control_tooltip;
            this.signal_tooltip = stockTU.signal_tooltip;

            //Doing Reflection to read and save attribute names and values seems too complex,
            //given most of attributes are not primitives
        }
    }

    public class RemoteTechTelemetryUpdate: TelemetryUpdate
    {
        public class CustomTextures
        {
            public Texture2D OmniDishTxt;
            public Texture2D ConeTxt;
            public Texture2D DishTargetTxt;
            public Texture2D VisualRangeTxt;
            public Texture2D TempTxt;

            public Sprite OmniDishSprite;
            public Sprite ConeSprite;
            public Sprite DishTargetSprite;
            public Sprite VisualRangeSprite;
            public Sprite TempSprite;

            public CustomTextures()
            {          
                OmniDishTxt = UiUtils.LoadTexture("DishOmniButtonBoth");
                ConeTxt = UiUtils.LoadTexture("ConeButtonActive");
                DishTargetTxt = UiUtils.LoadTexture("DishTargetButtonValid");
                VisualRangeTxt = UiUtils.LoadTexture("VisualRangeButtonOn");
                TempTxt = UiUtils.LoadTexture("Temp");

                OmniDishSprite = Sprite.Create(OmniDishTxt, new Rect(0, 0, OmniDishTxt.width, OmniDishTxt.height), new Vector2(0, 0));
                ConeSprite = Sprite.Create(ConeTxt, new Rect(0, 0, ConeTxt.width, ConeTxt.height), new Vector2(0, 0));
                DishTargetSprite = Sprite.Create(DishTargetTxt, new Rect(0, 0, DishTargetTxt.width, DishTargetTxt.height), new Vector2(0, 0));
                VisualRangeSprite = Sprite.Create(VisualRangeTxt, new Rect(0, 0, VisualRangeTxt.width, VisualRangeTxt.height), new Vector2(0, 0));
                TempSprite = Sprite.Create(TempTxt, new Rect(0, 0, TempTxt.width, TempTxt.height), new Vector2(0, 0));
            }
        }

        private static CustomTextures mTextures = null;
        public static CustomTextures Textures
        {
            get
            {
                if (mTextures == null) { mTextures = new CustomTextures(); }
                return mTextures;
            }
        }

        protected HorizontalLayoutGroup TelemetryLayoutGroup = null;

        protected Image LineTypeImage;
        protected Image DishReachImage;
        protected Image DishTargetImage;
        protected Image VisualRangeImage;

        protected const string LineTypeName = "linetype_name";
        protected const string DishReachName = "dishreach_name";
        protected const string DishTargetName = "dishtarget_name";
        protected const string VisualRangeName = "vrange_name";

        public class ClickAction : MonoBehaviour, IPointerClickHandler
        {
            public void OnPointerClick(PointerEventData eventData)
            {
                if (eventData.eligibleForClick)
                {
                    switch (eventData.pointerPress.name)
                    {
                        case LineTypeName:
                            RemoteTechTelemetryUpdate.OnClickLineType();
                            break;
                        case DishReachName:
                            RemoteTechTelemetryUpdate.OnClickDishReach();
                            break;
                        case DishTargetName:
                            RemoteTechTelemetryUpdate.OnClickDishTarget();
                            break;
                        case VisualRangeName:
                            RemoteTechTelemetryUpdate.OnClickVisualRange();
                            break;
                        default:
                            //do nothing
                            break;
                    }

                    //update icon
                    RemoteTechCommNetScenario.Instance.Telemetry.RefreshIcons();
                }
            }
        }

        public static new RemoteTechTelemetryUpdate Instance
        {
            get;
            protected set;
        }

        public void copyOf(TelemetryUpdateData stockTUData)
        {
            //replace the mode button
            var customModeButton = stockTUData.modeButton.gameObject.AddComponent<RemoteTechCommNetUIModeButton>();
            customModeButton.copyOf(stockTUData.modeButton);
            UnityEngine.Object.DestroyImmediate(stockTUData.modeButton);
            this.modeButton = customModeButton;

            this.NOSIG = stockTUData.NOSIG;
            this.NOEP = stockTUData.NOEP;
            this.BLK = stockTUData.BLK;
            this.AUP = stockTUData.AUP;
            this.ADN = stockTUData.ADN;
            this.EP0 = stockTUData.EP0;
            this.EP1 = stockTUData.EP1;
            this.EP2 = stockTUData.EP2;
            this.CK1 = stockTUData.CK1;
            this.CK2 = stockTUData.CK2;
            this.CK3 = stockTUData.CK3;
            this.CP1 = stockTUData.CP1;
            this.CP2 = stockTUData.CP2;
            this.CP3 = stockTUData.CP3;
            this.SS0 = stockTUData.SS0;
            this.SS1 = stockTUData.SS1;
            this.SS2 = stockTUData.SS2;
            this.SS3 = stockTUData.SS3;
            this.SS4 = stockTUData.SS4;
            this.arrow_icon = stockTUData.arrow_icon;
            this.firstHop_icon = stockTUData.firstHop_icon;
            this.lastHop_icon = stockTUData.lastHop_icon;
            this.control_icon = stockTUData.control_icon;
            this.signal_icon = stockTUData.signal_icon;
            this.firstHop_tooltip = stockTUData.firstHop_tooltip;
            this.arrow_tooltip = stockTUData.arrow_tooltip;
            this.lastHop_tooltip = stockTUData.lastHop_tooltip;
            this.control_tooltip = stockTUData.control_tooltip;
            this.signal_tooltip = stockTUData.signal_tooltip;
        }

        protected override void Start()
        {
            if(TelemetryLayoutGroup == null)
            {
                TelemetryLayoutGroup = GameObject.Find("_UIMaster").GetChild("Background_Lg").GetComponent<HorizontalLayoutGroup>(); //KSP destroys it when exiting scene
            }

            LineTypeImage = createImageButton(this.BLK, LineTypeName);
            DishReachImage = createImageButton(this.BLK, DishReachName);
            DishTargetImage = createImageButton(this.BLK, DishTargetName);
            VisualRangeImage = createImageButton(this.BLK, VisualRangeName);

            MapView.OnEnterMapView += OnEnterMapView;
            MapView.OnExitMapView += OnExitMapView;
        }

        protected override void Awake()
        {
            //overrode to turn off stock's instance check
            if (TelemetryUpdate.Instance != null && TelemetryUpdate.Instance is TelemetryUpdate)
            {
                UnityEngine.Object.DestroyImmediate(TelemetryUpdate.Instance);
                TelemetryUpdate.Instance = this;
            }
        }

        protected override void OnDestroy()
        {
            MapView.OnEnterMapView -= OnEnterMapView;
            MapView.OnExitMapView -= OnExitMapView;

            DishTargetImage.gameObject.DestroyGameObjectImmediate();
            LineTypeImage.gameObject.DestroyGameObjectImmediate();
            DishReachImage.gameObject.DestroyGameObjectImmediate();
            VisualRangeImage.gameObject.DestroyGameObjectImmediate();
        }

        public void RefreshIcons()
        {
            OnEnterMapView();
        }

        public void OnEnterMapView()
        {
            SetIcon(LineTypeImage, LineTypeSprite, true);
            SetIcon(DishReachImage, DishReachSprite, true);
            SetIcon(DishTargetImage, DishTargetSprite, true);
            SetIcon(VisualRangeImage, VisualRangeSprite, true);

            SetTooltipText(LineTypeImage, LineTypeTooltip);
            SetTooltipText(DishReachImage, DishReachTooltip);
            SetTooltipText(DishTargetImage, DishTargetTooltip);
            SetTooltipText(VisualRangeImage, VisualRangeTooltip);
        }

        public void OnExitMapView()
        {
            SetIcon(LineTypeImage, this.BLK, true);
            SetIcon(DishReachImage, this.BLK, true);
            SetIcon(DishTargetImage, this.BLK, true);
            SetIcon(VisualRangeImage, this.BLK, true);
        }

        protected override void ClearGui()
        {
            base.ClearGui();
            OnExitMapView();
        }

        private Image createImageButton(Sprite sprite, string name)
        {
            Image NewImage = Object.Instantiate(this.control_icon);
            NewImage.gameObject.name = name;
            NewImage.sprite = sprite;
            NewImage.transform.SetParent(TelemetryLayoutGroup.transform);
            NewImage.gameObject.SetActive(false);
            NewImage.gameObject.AddComponent<ClickAction>();

            return NewImage;
        }

        private void SetTooltipText(Image img, string newText)
        {
            var tip = img.GetComponentInChildren<TooltipController_Text>();
            if (tip != null) { tip.SetText(newText); }
        }

        private Sprite DishReachSprite
        {
            get
            {
                RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
                return ((mask & RemoteTechMapFilter.DishCone) == RemoteTechMapFilter.DishCone) ? Textures.ConeSprite : Textures.TempSprite;
            }
        }

        private string DishReachTooltip
        {
            get
            {
                RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
                return ((mask & RemoteTechMapFilter.DishCone) == RemoteTechMapFilter.DishCone) ? "Show dish cones" : "No dish cones";
            }
        }

        private Sprite DishTargetSprite
        {
            get
            {
                return Textures.DishTargetSprite;
            }
        }

        private string DishTargetTooltip
        {
            get
            {
                return "Open a window on Active Vessel's all dishes and their targets."+
                        "\nDishes marked in green are activated and those marked in red are deactivated."+
                        "\nClicking on any dish in the list will pull up the target selection window for that dish.";
            }
        }

        private Sprite LineTypeSprite
        {
            get
            {
                RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
                if ((mask & (RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine)) == (RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine))
                {
                    return Textures.OmniDishSprite;
                }
                else if ((mask & RemoteTechMapFilter.OmniLine) == RemoteTechMapFilter.OmniLine)
                {
                    return Textures.TempSprite;
                }
                else if ((mask & RemoteTechMapFilter.DishLine) == RemoteTechMapFilter.DishLine)
                {
                    return Textures.TempSprite;
                }
                else //off
                {
                    return Textures.TempSprite;
                }
            }
        }

        private string LineTypeTooltip
        {
            get
            {
                RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
                if ((mask & (RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine)) == (RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine))
                {
                    return "Omni and dish links";
                }
                else if ((mask & RemoteTechMapFilter.OmniLine) == RemoteTechMapFilter.OmniLine)
                {
                    return "Omni links only";
                }
                else if ((mask & RemoteTechMapFilter.DishLine) == RemoteTechMapFilter.DishLine)
                {
                    return "Dish links only";
                }
                else //off
                {
                    return "No links";
                }
            }
        }

        private Sprite VisualRangeSprite
        {
            get
            {
                RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
                return ((mask & RemoteTechMapFilter.VisualRange) == RemoteTechMapFilter.VisualRange) ? Textures.VisualRangeSprite : Textures.TempSprite;
            }
        }

        private string VisualRangeTooltip
        {
            get
            {
                RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
                return ((mask & RemoteTechMapFilter.VisualRange) == RemoteTechMapFilter.VisualRange) ? "Show visual ranges" : "No visual ranges";
            }
        }

        public static void OnClickLineType()
        {
            RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
            if ((mask & (RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine)) == (RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine))
            {
                RemoteTechCommNetUI.RTMapFilter &= ~((RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine));
                return;
            }
            if ((mask & RemoteTechMapFilter.OmniLine) == RemoteTechMapFilter.OmniLine)
            {
                RemoteTechCommNetUI.RTMapFilter &= ~RemoteTechMapFilter.OmniLine;
                RemoteTechCommNetUI.RTMapFilter |= RemoteTechMapFilter.DishLine;
                return;
            }
            if ((mask & RemoteTechMapFilter.DishLine) == RemoteTechMapFilter.DishLine)
            {
                RemoteTechCommNetUI.RTMapFilter |= (RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine);
                return;
            }
            RemoteTechCommNetUI.RTMapFilter |= RemoteTechMapFilter.OmniLine;
        }

        public static void OnClickDishReach()
        {
            RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
            if ((mask & RemoteTechMapFilter.DishCone) == RemoteTechMapFilter.DishCone)
            {
                RemoteTechCommNetUI.RTMapFilter &= ~RemoteTechMapFilter.DishCone;
                return;
            }
            RemoteTechCommNetUI.RTMapFilter |= RemoteTechMapFilter.DishCone;
        }

        public static void OnClickDishTarget()
        {

        }

        public static void OnClickVisualRange()
        {
            RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
            if ((mask & RemoteTechMapFilter.VisualRange) == RemoteTechMapFilter.VisualRange)
            {
                RemoteTechCommNetUI.RTMapFilter &= ~RemoteTechMapFilter.VisualRange;
                return;
            }
            RemoteTechCommNetUI.RTMapFilter |= RemoteTechMapFilter.VisualRange;
        }
    }
}
