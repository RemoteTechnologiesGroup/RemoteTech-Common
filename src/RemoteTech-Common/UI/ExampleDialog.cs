using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

namespace RemoteTech.Common.UI
{
    public class ExampleDialog : AbstractDialog
    {
        private DialogGUIVerticalLayout rowLayout;
        private int nextId;

        //`live` image of graph
        private DialogGUIImage funImage = null;
        private Texture2D imageTxt = null;
        private int txtWidth = 200, txtHeight = 200;
        private float angle = 0f;

        /*
        The purpose of this class is to demonstrate how a RemoteTech window is created with KSP 1.2's new DialogGUI components,
        to show how to create/edit/delete some information, and to show how to render texture onto DialogGUIImage/DialogGUISprite
        */

        public ExampleDialog() : base("Demonstration",
                                    0.5f, //x
                                    0.5f, //y
                                    250, //width
                                    450, //height
                                    new DialogOptions[] { DialogOptions.AllowBgInputs })
        {
            /*
            //How to launch this dialog
            ExampleDialog dialog = new ExampleDialog();
            dialog.launch(); 
            */

            //draw texture background
            imageTxt = new Texture2D(txtWidth, txtHeight, TextureFormat.ARGB32, false);
            for (int y = 0; y < imageTxt.height; y++)
            {
                for (int x = 0; x < imageTxt.width; x++)
                    imageTxt.SetPixel(x, y, Color.grey);
            }
            imageTxt.Apply();//finalize the texture
        }

        protected override void OnUpdate()
        {
            //WARNING: Be sure to deallocate a texture when no longer used by invoking 'UnityEngine.GameObject.DestroyImmediate(someTexture, true);'
            //Otherwise, there would be a memory bomb if the texture is allocated repeatedly in a rapid succession within a short time without
            //dealloaction. The automatic garbage collection is not fast enough to catch them.

            if (imageTxt == null || funImage == null)
                return;

            //shift the texture to right by 1 pixel in x direction
            for (int x = imageTxt.width-2; x >=0 ; x--)
            {
                for (int y = 0; y < imageTxt.height; y++)
                    imageTxt.SetPixel(x+1, y, imageTxt.GetPixel(x, y));
            }

            //math cos magic
            float v2 = Mathf.Cos(angle * Mathf.Deg2Rad) * ((txtHeight / 2) - 20);
            int cosY = (int)(txtHeight / 2 + v2);

            if (angle == 359)
                angle = 0f;
            else
                angle += 1f;

            //draw a fun graph
            for (int y = 0; y < imageTxt.height; y++)
                imageTxt.SetPixel(0, y, Color.grey);
            imageTxt.SetPixel(0, cosY, Color.black);

            //finalize the texture
            imageTxt.Apply();

            //update DialogGUIImage's underlying texture 
            funImage.uiItem.GetComponent<RawImage>().texture = imageTxt;
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> contentComponents = new List<DialogGUIBase>();

            //texture image
            funImage = new DialogGUIImage(new Vector2(txtWidth, txtHeight), Vector2.zero, Color.white, imageTxt);
            // Color.white is recommended. Color.transparent only makes the image invisble and Color.black draws the black image
            contentComponents.Add(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { funImage }));

            //create button
            DialogGUIButton createButton = new DialogGUIButton("Create", createClick, false);
            contentComponents.Add(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), createButton, new DialogGUIFlexibleSpace() }));

            //scrolllist of some rows
            List<DialogGUIHorizontalLayout> eachRowGroupList = new List<DialogGUIHorizontalLayout>();
            for (nextId = 0; nextId < 3; nextId++)
                eachRowGroupList.Add(createRow(nextId));

            DialogGUIBase[] rows = new DialogGUIBase[eachRowGroupList.Count + 1];
            rows[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true); // important because otherwise the scrolling of content won't work
            for (int i = 0; i < eachRowGroupList.Count; i++)
                rows[i + 1] = eachRowGroupList[i];

            rowLayout = new DialogGUIVerticalLayout(10, 10, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperCenter, rows); // you would keep the reference to the rows for dynamic modifications
            contentComponents.Add(new DialogGUIScrollList(Vector2.one, false, true, rowLayout));

            return contentComponents;
        }

        private DialogGUIHorizontalLayout createRow(int id)
        {
            DialogGUILabel nameLabel = new DialogGUILabel("Name " + id, 50, 12);
            DialogGUIButton updateButton = new DialogGUIButton("Update", delegate { nameLabel.OptionText += id; }, 50, 32, false);
            DialogGUIButton deleteButton = new DialogGUIButton("Delete", delegate { deleteClick(id); }, 50, 32, false);
            DialogGUIHorizontalLayout layout = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { nameLabel, new DialogGUIFlexibleSpace(), updateButton, deleteButton });

            return layout;
        }

        private void createClick()
        {
            Stack<Transform> stack = new Stack<Transform>(); // some data on hierarchy of GUI components
            stack.Push(rowLayout.uiItem.gameObject.transform); // need the reference point of the parent GUI component for position and size
            List<DialogGUIBase> rows = rowLayout.children;
            rows.Add(createRow(nextId++)); // new row
            rows.Last().Create(ref stack, HighLogic.UISkin); // required to force the GUI creation
        }

        private void deleteClick(int id)
        {
            List<DialogGUIBase> rows = rowLayout.children;

            for (int i = 1; i < rows.Count; i++)
            {
                DialogGUIBase thisChild = rows.ElementAt(i);
                if (thisChild is DialogGUIHorizontalLayout) // avoid if DialogGUIContentSizer is detected
                {
                    DialogGUILabel label = thisChild.children.ElementAt(0) as DialogGUILabel;
                    if (label.OptionText.EndsWith("" + id))
                    {
                        rows.RemoveAt(i); // drop from the scrolllist rows
                        thisChild.uiItem.gameObject.DestroyGameObjectImmediate(); // necessary to free memory up
                        break;
                    }
                }
            }
        }
    }
}
