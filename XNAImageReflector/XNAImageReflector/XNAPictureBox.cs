using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using XNA = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SMX
{
    /// <summary>
    /// Author: Iñaki Ayucar (http://graphicdna.blogspot.com)
    /// Date: 23/11/2007
    /// 
    /// This software is distributed "for free" for any non-commercial usage. The software is provided “as-is.” 
    /// You bear the risk of using it. The contributors give no express warranties, guarantees or conditions.
    /// </summary>
    public class XNAPictureBox
    {
        private PrimitivesSample.PrimitiveBatch mPrimitiveBatch = null;
        private SpriteBatch mSpriteBatch = null;
        private Texture2D mTexture = null;
        private Texture2D mLogoTexture = null;
        public Texture2D Texture
        {
            get { return this.mTexture; }
            set
            {
                this.mTexture = value;
                this.RefreshZoom();
            }
        }

        private float mZoomFactor = 1f;
        public float ZoomFactor
        {
            get { return mZoomFactor; }
        }
        private XNA.Rectangle mDrawingRectangle = new Rectangle();
        public XNA.Rectangle DrawingRectangle
        {
            get { return mDrawingRectangle; }
        }

        private int mWidth, mHeight;
        public int Width
        {
            get { return mWidth; }
            set
            {
                mWidth = value;
                this.RefreshZoom();
            }
        }
        public int Height
        {
            get { return mHeight; }
            set
            {
                mHeight = value;
                this.RefreshZoom();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public XNAPictureBox()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public void Render(XNA.Graphics.GraphicsDevice pDevice)
        {
            if (this.mTexture == null)
            {
                mSpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
                int posX = (int)(((float)this.mWidth * 0.5f) - ((float)this.mLogoTexture.Width * 0.5f));
                int posY = (int)(((float)this.mHeight * 0.5f) - ((float)this.mLogoTexture.Height * 0.5f));
                mSpriteBatch.Draw(this.mLogoTexture, new Rectangle(posX,posY,this.mLogoTexture.Width, this.mLogoTexture.Height), XNA.Graphics.Color.White);
                mSpriteBatch.End();
                return;
            }

            // Draw current texture
            mSpriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            mSpriteBatch.Draw(this.mTexture, this.mDrawingRectangle, XNA.Graphics.Color.White);
            mSpriteBatch.End();

            // Draw frame
            mPrimitiveBatch.Begin(PrimitiveType.LineList);
            mPrimitiveBatch.AddVertex(new Vector2(DrawingRectangle.X, DrawingRectangle.Y), Color.DarkGray);
            mPrimitiveBatch.AddVertex(new Vector2(DrawingRectangle.X + DrawingRectangle.Width, DrawingRectangle.Y), Color.DarkGray);

            mPrimitiveBatch.AddVertex(new Vector2(DrawingRectangle.X + DrawingRectangle.Width, DrawingRectangle.Y), Color.DarkGray);
            mPrimitiveBatch.AddVertex(new Vector2(DrawingRectangle.X + DrawingRectangle.Width, DrawingRectangle.Y + DrawingRectangle.Height), Color.DarkGray);

            mPrimitiveBatch.AddVertex(new Vector2(DrawingRectangle.X + DrawingRectangle.Width, DrawingRectangle.Y + DrawingRectangle.Height), Color.DarkGray);
            mPrimitiveBatch.AddVertex(new Vector2(DrawingRectangle.X, DrawingRectangle.Y + DrawingRectangle.Height), Color.DarkGray);

            mPrimitiveBatch.AddVertex(new Vector2(DrawingRectangle.X, DrawingRectangle.Y + DrawingRectangle.Height), Color.DarkGray);
            mPrimitiveBatch.AddVertex(new Vector2(DrawingRectangle.X, DrawingRectangle.Y), Color.DarkGray);
            mPrimitiveBatch.End();

        }
        /// <summary>
        /// 
        /// </summary>
        private void RefreshZoom()
        {
            if (this.mTexture == null)
                return;

            float caMiddleHeight = (float)this.mHeight * 0.5f;
            float bmpMiddleHeight = (float)mTexture.Height * 0.5f;
            float caMiddleWidth = (float)this.mWidth * 0.5f;
            float bmpMiddleWidth = (float)mTexture.Width * 0.5f;
            
            float zoomFactorY = (float)this.mHeight / (float)mTexture.Height;
            float zoomFactorX = (float)this.mWidth / (float)mTexture.Width;
            this.mZoomFactor = 0f;
            if (zoomFactorX < zoomFactorY)
            {
                mZoomFactor = zoomFactorX;
                this.mDrawingRectangle.X = 0;
                this.mDrawingRectangle.Y = (int)(caMiddleHeight - bmpMiddleHeight * mZoomFactor);
            }
            else
            {
                mZoomFactor = zoomFactorY;
                this.mDrawingRectangle.X = (int)(caMiddleWidth - bmpMiddleWidth * mZoomFactor);
                this.mDrawingRectangle.Y = 0;
            }

            this.mDrawingRectangle.Width = (int)((float)mTexture.Width * mZoomFactor);
            this.mDrawingRectangle.Height = (int)((float)mTexture.Height * mZoomFactor);
        }

        #region Coordinates Conversion
        /// <summary>
        /// This function converts PictureBox coordinates to bitmap coordinates, when showing the image
        /// with the "zoom" sizemode in the picturebox
        /// </summary>
        /// <param name="pVec"></param>
        /// <returns></returns>
        public Vector2 PictureBoxToImage(Vector2 pVec)
        {
            Vector2 v = new Vector2();
            v.X = (pVec.X - this.mDrawingRectangle.X) / mZoomFactor;
            v.Y = (pVec.Y - this.mDrawingRectangle.Y) / mZoomFactor;

            return v;
        }
        /// <summary>
        /// This function converts bitmap coordinates to PictureBox coordinates, when showing the image
        /// with the "zoom" sizemode in the picturebox
        /// </summary>
        /// <param name="pPt"></param>
        /// <returns></returns>
        public Vector2 ImageToPictureBox(Vector2 pVec)
        {
            Vector2 v = new Vector2();
            v.X = this.mDrawingRectangle.X + (pVec.X * mZoomFactor);
            v.Y = this.mDrawingRectangle.Y + (pVec.Y * mZoomFactor);

            return v;
        }
        #endregion

        #region Device Events
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDevice"></param>
        public void LoadGraphicsContent(XNA.Graphics.GraphicsDevice pDevice)
        {
            mSpriteBatch = new SpriteBatch(pDevice);
            mPrimitiveBatch = new PrimitivesSample.PrimitiveBatch(pDevice);

            if(this.mLogoTexture == null)
                this.mLogoTexture = Texture2D.FromFile(pDevice, @"Content\Logo.png");
                
        }
        #endregion
    
    }
}
