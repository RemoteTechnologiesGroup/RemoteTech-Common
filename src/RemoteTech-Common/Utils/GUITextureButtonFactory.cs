﻿using System;
using UnityEngine;

namespace RemoteTech.Common.Utils
{
    /// <summary>
    /// Button styles for Unity interface system only
    /// </summary>
    public static class GUITextureButtonFactory
    {
        // Replaced with new texture handling mechanics.
        public static GUIStyle CreateFromFilename(String normal)
        {
            Texture2D tex = UiUtils.LoadTexture(normal);
            return CreateFromTextures(tex, tex, tex, tex);
        }

        // Replaced with new texture handling mechanics.
        public static GUIStyle CreateFromFilename(String normal, String hover, String active, String focus)
        {
            return CreateFromTextures(UiUtils.LoadTexture(normal), 
                                        UiUtils.LoadTexture(hover),
                                        UiUtils.LoadTexture(active), 
                                        UiUtils.LoadTexture(focus));
        }

        private static GUIStyle CreateFromTextures(Texture2D texNormal, Texture2D texHover,
                                                   Texture2D texActive, Texture2D texFocus)
        {
            return new GUIStyle()
            {
                name = texNormal.name,
                normal = new GUIStyleState() { background = texNormal, textColor = Color.white },
                hover = new GUIStyleState() { background = texHover, textColor = Color.white },
                active = new GUIStyleState() { background = texActive, textColor = Color.white },
                onNormal = new GUIStyleState() { background = texActive, textColor = Color.white },
                onHover = new GUIStyleState() { background = texActive, textColor = Color.white },
                onActive = new GUIStyleState() { background = texActive, textColor = Color.white },
                focused = new GUIStyleState() { background = texFocus, textColor = Color.white },
                onFocused = new GUIStyleState() { background = texActive, textColor = Color.white },
                border = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                overflow = new RectOffset(0, 0, 0, 0),
                imagePosition = ImagePosition.ImageAbove,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                clipping = TextClipping.Clip,
                contentOffset = new Vector2(0, 0),
                stretchWidth = false,
                stretchHeight = false,
                font = null,
                fontSize = 0,
                fontStyle = FontStyle.Normal,
                richText = false,
            };
        }
    }
}