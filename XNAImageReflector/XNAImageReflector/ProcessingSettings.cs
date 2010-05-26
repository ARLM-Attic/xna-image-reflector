using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using XNA = Microsoft.Xna.Framework;

namespace SMX
{
    /// <summary>
    /// Author: Iñaki Ayucar (http://graphicdna.blogspot.com)
    /// Date: 23/11/2007
    /// 
    /// This software is distributed "for free" for any non-commercial usage. The software is provided “as-is.” 
    /// You bear the risk of using it. The contributors give no express warranties, guarantees or conditions.
    /// </summary>
    public class ProcessingSettings
    {
        #region Enums
        [Flags]
        public enum eDirection
        {
            All = 0xFFFF,
            Horizontal = 0x000F,
            Vertical = 0x00F0,
            Diagonal1 = 0x0F00,
            Diagonal2 = 0xF000,
        }
        public enum eGradientMode
        {
            None,
            Linear,
        }
        public enum eAutoScale
        {
            None,
            div1_25,
            div1_5,
            div1_75,
            div2,
            div4,
            mult1_25,
            mult1_5,
            mult1_75,
            mult2,
            mult4,
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static float GetScaleFactor(eAutoScale value)
        {
            float factor = 1f;
            switch (value)
            {
                case eAutoScale.div1_25:
                    factor = 1f / 1.25f;
                    break;
                case eAutoScale.div1_5:
                    factor = 1f / 1.5f;
                    break;
                case eAutoScale.div1_75:
                    factor = 1f / 1.75f;
                    break;
                case eAutoScale.div2:
                    factor = 1f / 2f;
                    break;
                case eAutoScale.div4:
                    factor = 1f / 4f;
                    break;
                case eAutoScale.mult1_25:
                    factor = 1.25f;
                    break;
                case eAutoScale.mult1_5:
                    factor = 1.5f;
                    break;
                case eAutoScale.mult1_75:
                    factor = 1.75f;
                    break;
                case eAutoScale.mult2:
                    factor = 2f;
                    break;
                case eAutoScale.mult4:
                    factor = 4f;
                    break;
            }
            return factor;
        }
        public enum eChannel
        {
            None,
            Red,
            Green,
            Blue,
            Alpha,
        }

    
        #endregion

        private int mWidth, mHeight;
        [Browsable(false)]
        public int Width
        {
            get { return mWidth; }
            set { mWidth = value; }
        }
        [Browsable(false)]
        public int Height
        {
            get { return mHeight; }
            set { mHeight = value; }
        }
        private int mNewWidth, mNewHeight;
        [Category("Scaling")]
        [Description("Manually sets the width of the bitmap inside the canvas")]
        public int NewWidth
        {
            get { return mNewWidth; }
            set { mNewWidth = value; }
        }
        [Category("Scaling")]
        [Description("Manually sets the height of the bitmap inside the canvas")]
        public int NewHeight
        {
            get { return mNewHeight; }
            set { mNewHeight = value; }
        }
        private eAutoScale mAutoScale = eAutoScale.None;
        [RefreshProperties(RefreshProperties.All)]
        [Category("Scaling")]
        [Description("Gets or sets one of the predefined autoscale modes")]
        public eAutoScale AutoScale
        {
            get { return mAutoScale; }
            set
            {
                mAutoScale = value;

                float factor = GetScaleFactor(value);
                mNewHeight = (int)((float)mHeight * factor);
                mNewWidth = (int)((float)mWidth * factor);
            }
        }

        private Point mNewLocation;
        [Category("Translation")]
        [Description("Relocates the bitmap inside the canvas. Useful when resizing")]
        public Point NewLocation
        {
            get { return mNewLocation; }
            set { mNewLocation = value; }
        }
        private eGradientMode mGradientMode = eGradientMode.Linear;
        [Category("Alpha Gradient")]
        [DisplayName("Mode")]
        [Description("Gets or Sets the gradient mode for alpha")]
        public eGradientMode AlphaGradientMode
        {
            get { return mGradientMode; }
            set { mGradientMode = value; }
        }
        private float mGradientRange = 150f;
        [Category("Alpha Gradient")]
        [DisplayName("Range")]
        [Description("Gets or Sets the length (in pixels) of the alpha gradient")]
        public float AlphaGradientRange
        {
            get { return mGradientRange; }
            set
            {
                if (value < 1f)
                    throw new System.FormatException("Value must be greater that 1");
                mGradientRange = value;
            }
        }
        private float mAlphaOffset = 0f;
        [Category("Alpha Gradient")]
        [DisplayName("Offset")]
        [Description("Gets or sets an offset for the entire alpha gradient. Allows to increase or decrease alpha.")]
        public float AlphaGradientOffset
        {
            get { return mAlphaOffset; }
            set
            {
                mAlphaOffset = value;
            }
        }
        

        private bool mRefreshAlways = true;
        [Category("Behavior")]
        [Description("The bitmap will be processed each time the ReflectionMark or any setting changes")]
        public bool RefreshAlways
        {
            get{return mRefreshAlways;}
            set { mRefreshAlways = value; }
        }

        private bool mDisplayReflectionMark = true;
        [Category("Behavior")]
        [Description("Shows or hides the reflection mark in the viewport")]
        public bool DisplayMark
        {
            get { return mDisplayReflectionMark; }
            set { mDisplayReflectionMark = value; }
        }

        private bool mKeepTransparency = true;
        [Category("Behavior")]
        [Description("If true, the final bitmap will keep original picture´s transparency (if present). If false, final bitmap´s background will be cleared to working area´s back color")]
        public bool KeepTransparency
        {
            get { return mKeepTransparency; }
            set { mKeepTransparency = value; }
        }


        private int mBlurSteps = 0;
        [Category("Post Processing")]
        [Description("Number of steps for the blur effect. The more steps, the more blur (and more time taken for processing). Set 0 steps to disable blur")]
        public int BlurSteps
        {
            get { return mBlurSteps; }
            set { mBlurSteps = value; }
        }
        private eDirection mBlurDirection = eDirection.All;
        [Category("Post Processing")]
        [Description("Determines the blur direction, to allow motion blur. The default value is All, which will apply a normal blur (no motion)")]
        public eDirection BlurDirection
        {
            get { return mBlurDirection; }
            set { mBlurDirection = value; }
        }


        private Bitmap mAlphaBitmap = null;
        [Category("Post Processing")]
        [DisplayName("Alpha Texture")]
        [Description("Allows you to set a texture as an alpha source for the reflection. If you also apply a gradient, both effects will be applied")]
        public Bitmap TextureAlpha
        {
            get { return mAlphaBitmap; }
            set
            {
                mAlphaBitmap = value;
            }
        }
        private eChannel mAlphaChannel = eChannel.Alpha;
        [Category("Post Processing")]
        [DisplayName("Alpha Channel")]
        [Description("Allows you to select the channel of the texture to use as alpha source. Select None to disable alpha texture.")]
        public eChannel TextureAlphaChannel
        {
            get { return mAlphaChannel; }
            set { mAlphaChannel = value; }
        }
        

    }
}
