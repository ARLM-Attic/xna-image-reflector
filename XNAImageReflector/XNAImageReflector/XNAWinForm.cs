using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
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
    public partial class XNAWinForm : Form
    {
        #region XNA vars
        public enum eRefreshMode
        {
            Always,
            OnPanelPaint,        
        }

        private GraphicsDevice mDevice;
        public GraphicsDevice Device
        {
            get { return mDevice; }
        }
        private eRefreshMode mRefreshMode = eRefreshMode.Always;
        public eRefreshMode RefreshMode
        {
            get { return mRefreshMode; }
            set
            {
                mRefreshMode = value;
            }
        }
        private Microsoft.Xna.Framework.Graphics.Color mBackColor = Microsoft.Xna.Framework.Graphics.Color.AliceBlue;
        #endregion

        private Bitmap mBitmapOriginal;
        private uint[] mProcessedColors;
        public ProcessingSettings mSettings = new ProcessingSettings();
        private XNAPictureBox mXNAPictureBox = null;
        private ReflectionMark mReflectionMark = null;
        

        #region Events
        public delegate void GraphicsDeviceDelegate(GraphicsDevice pDevice);
        public delegate void EmptyEventHandler();
        public event GraphicsDeviceDelegate OnFrameRender = null;
        public event GraphicsDeviceDelegate OnFrameMove = null;
        public event EmptyEventHandler DisposeAll = null;
        public event GraphicsDeviceDelegate ReCreateAll = null;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public XNAWinForm()
        {
            InitializeComponent();

            

            // First initialize of dimensions for XNAPictureBox
            this.mXNAPictureBox = new XNAPictureBox();
            this.mXNAPictureBox.Width = panelViewport.ClientRectangle.Width;
            this.mXNAPictureBox.Height = panelViewport.ClientRectangle.Height;

            this.mReflectionMark = new ReflectionMark();
            this.mReflectionMark.Width = panelViewport.ClientRectangle.Width;
            this.mReflectionMark.Height = panelViewport.ClientRectangle.Height;
            this.mReflectionMark.MouseModeChanged += new EventHandler(mReflectionMark_MouseModeChanged);
            this.mReflectionMark.ReflectionMarkChanged += new EventHandler(mReflectionMark_ReflectionMarkChanged);
            this.mReflectionMark.ReflectionMarkChanging += new EventHandler(mReflectionMark_ReflectionMarkChanging);

            // Create Settings and use as source for the propertyGrid
            //this.mSettings = new ProcessingSettings();
            this.propertyGrid1.SelectedObject = mSettings;

            // Foce color resfresh. (if panel has default backcolor, BackColorChanged won´t be called)
            this.panelViewport_BackColorChanged(null, EventArgs.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFullName"></param>
        private void LoadBitmap(string pFullName)
        {
            // Read from file
            Bitmap aux = (Bitmap)Bitmap.FromFile(pFullName);

            System.Drawing.Imaging.FrameDimension dim = new System.Drawing.Imaging.FrameDimension(aux.FrameDimensionsList[0]);
            int a = aux.GetFrameCount(dim);

            // Ensure bitmap is in 32bppARGB
            this.mBitmapOriginal = aux.Clone(new System.Drawing.Rectangle(0,0, aux.Width, aux.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            this.pictureBox1.Image = this.mBitmapOriginal;
            mSettings.Width = mBitmapOriginal.Width;
            mSettings.Height = mBitmapOriginal.Height;
            mSettings.AlphaGradientRange = (int)((float)mBitmapOriginal.Height * 0.5f);

            // Set texture colors
            this.mXNAPictureBox.Texture = new Texture2D(this.mDevice, mBitmapOriginal.Width, mBitmapOriginal.Height, 1, TextureUsage.None, SurfaceFormat.Color);
            int[] colors = this.GetBitmapColors(mBitmapOriginal,0,0,mBitmapOriginal.Width, mBitmapOriginal.Height);
            this.mXNAPictureBox.Texture.SetData<int>(colors);

            // Update settings to new bitmap properties
            mSettings.NewWidth = mBitmapOriginal.Width;
            mSettings.NewHeight = mBitmapOriginal.Height;
            this.propertyGrid1.SelectedObject = mSettings;
        }

        #region Bitmap Processing
        /// <summary>
        /// 
        /// </summary>
        private void ProcessBitmap()
        {
            if (this.mBitmapOriginal == null)
                return;

            Cursor.Current = Cursors.WaitCursor;

            this.mProcessedColors = this.ReflectBitmap();

            if(mSettings.BlurSteps > 0)
                this.BlurBitmap();
            if (mSettings.TextureAlpha != null)
                this.TextureAlpha();


            this.mXNAPictureBox.Texture = new Texture2D(this.mDevice, mBitmapOriginal.Width, mBitmapOriginal.Height, 1, TextureUsage.None, SurfaceFormat.Color);
            this.mXNAPictureBox.Texture.SetData<uint>(mProcessedColors);

            Cursor.Current = Cursors.Default;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pBitmap"></param>
        /// <returns></returns>
        private int[] GetBitmapColors(Bitmap pBitmap, int pMinX, int pMinY, int pMaxX, int pMaxY)
        {
            System.Drawing.Imaging.BitmapData bmData = pBitmap.LockBits(new System.Drawing.Rectangle(0, 0, pBitmap.Width, pBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int pixelBytes = 4;
            int[] result = new int[pBitmap.Width * pBitmap.Height];
            unsafe
            {
                byte* pointer = (byte*)(void*)bmData.Scan0;
                byte alpha, red, green, blue;
                for (int x = pMinX; x < pMaxX; x++)
                {
                    for (int y = pMinY; y < pMaxY; y++)
                    {
                        int address = (y * bmData.Stride) + (x * pixelBytes);

                        // Format is ARGB but GDI lies to us, its stored in BGRA
                        blue = pointer[address];
                        green = pointer[address + 1];
                        red = pointer[address + 2];
                        alpha = pointer[address + 3];
                        result[(y * pBitmap.Width) + x] = alpha << 24 | red << 16 | green << 8 | blue;
                    }

                }
            }
            pBitmap.UnlockBits(bmData);
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReflMark"></param>
        public unsafe uint[] ReflectBitmap()
        {
            if (this.mBitmapOriginal == null)
                return null;

            // Create a canvas of the correct size and with Alpha Pixel format
            Bitmap finalBitmap = new Bitmap(this.mBitmapOriginal.Width, this.mBitmapOriginal.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Locate bitmap
            System.Drawing.Graphics g = Graphics.FromImage(finalBitmap);
            
            if(mSettings.KeepTransparency)
                g.Clear(System.Drawing.Color.Transparent);
            else g.Clear(this.panelViewport.BackColor);

            float sx = (float)this.mSettings.NewWidth / (float)this.mBitmapOriginal.Width;
            float sy = (float)this.mSettings.NewHeight / (float)this.mBitmapOriginal.Height;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.ScaleTransform(sx, sy);
            System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(mSettings.NewLocation, finalBitmap.Size);
            System.Drawing.Rectangle srcRect = new System.Drawing.Rectangle(new System.Drawing.Point(), finalBitmap.Size);
            g.DrawImage(this.mBitmapOriginal, destRect, srcRect, GraphicsUnit.Pixel);
            
            System.Drawing.Imaging.BitmapData bmData = finalBitmap.LockBits(new System.Drawing.Rectangle(0, 0, finalBitmap.Width, finalBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int pixelBytes = 4;
            byte* pointer = (byte*)(void*)bmData.Scan0;

            // Make the reflection
            uint[] result = new uint[finalBitmap.Width * finalBitmap.Height];
            for (int x = 0; x < finalBitmap.Width; x++)
            {
                int pbX = (int)this.mXNAPictureBox.ImageToPictureBox(new Vector2(x, 0)).X;
                int initialy = (int)this.mReflectionMark.GetYAt(pbX);
                if (initialy == float.MinValue)
                    continue;
                int initialy_onImage = (int)this.mXNAPictureBox.PictureBoxToImage(new Vector2(0, initialy)).Y;
                int address, adBytes;
                for (int y = 0; y < finalBitmap.Height; y++)
                {
                    address = (y * finalBitmap.Width) + x; 
                    if (y <= initialy_onImage)
                    {
                        adBytes = (y * bmData.Stride) + (x * pixelBytes);
                        // alpha << 24 | red << 16 | green << 8 | blue;
                        result[address] = (uint)(pointer[adBytes + 3] << 24 | pointer[adBytes + 2] << 16 | pointer[adBytes + 1] << 8 | pointer[adBytes]);
                        continue;
                    }
                    int disty = y - initialy_onImage;
                    int reflectY = initialy_onImage - disty;
                    if (reflectY >= 0 && reflectY < finalBitmap.Height)
                    {
                        adBytes = (reflectY * bmData.Stride) + (x * pixelBytes);
                        // alpha << 24 | red << 16 | green << 8 | blue;
                        if (mSettings.KeepTransparency)
                        {
                            int alpha = (int)((float)pointer[adBytes + 3] * this.GetAlphaGradient(disty));
                            result[address] = (uint)(alpha << 24 | pointer[adBytes + 2] << 16 | pointer[adBytes + 1] << 8 | pointer[adBytes]);
                        }
                        else
                        {
                            // Mix up backColor and reflection color
                            float falpha = this.GetAlphaGradient(disty);
                            byte red = (byte)(((float)pointer[adBytes + 2] * falpha) + ((float)panelViewport.BackColor.R * (1 - falpha)));
                            byte green = (byte)(((float)pointer[adBytes + 1] * falpha) + ((float)panelViewport.BackColor.G * (1 - falpha)));
                            byte blue = (byte)(((float)pointer[adBytes] * falpha) + ((float)panelViewport.BackColor.B * (1 - falpha)));
                            result[address] = (uint)((byte)255 << 24 | red << 16 | green << 8 | blue);
                        }
                    }
                    else if (!mSettings.KeepTransparency)
                    {
                        // Just copy backcolor
                        result[address] = (uint)(panelViewport.BackColor.A << 24 | panelViewport.BackColor.R << 16 | panelViewport.BackColor.G << 8 | panelViewport.BackColor.B);
                    }
                }
            }


            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disty"></param>
        /// <returns></returns>
        private float GetAlphaGradient(int disty)
        {
            switch (this.mSettings.AlphaGradientMode)
            {
                case ProcessingSettings.eGradientMode.Linear:
                    
                    float alpha = 1f-((float)disty / this.mSettings.AlphaGradientRange);
                    alpha += mSettings.AlphaGradientOffset;
                    alpha = Math.Min(1f, alpha);
                    alpha = Math.Max(0f, alpha);
                    return alpha;
                default:
                    return 1;
            }

        }
        /// <summary>
        /// Applies the blur postprocessing effect 
        /// </summary>
        private void BlurBitmap()
        {
            List<uint> candidates = new List<uint>();

            for (int x = 0; x < mBitmapOriginal.Width; x++)
            {
                int pbX = (int)this.mXNAPictureBox.ImageToPictureBox(new Vector2(x, 0)).X;
                int initialy = (int)this.mReflectionMark.GetYAt(pbX);
                if (initialy == float.MinValue)
                    continue;
                int initialy_onImage = (int)this.mXNAPictureBox.PictureBoxToImage(new Vector2(0, initialy)).Y;

                for (int y = 0; y < mBitmapOriginal.Height; y++)
                {
                    if (y <= initialy_onImage)
                        continue;
                    int disty = y - initialy_onImage;
                    if (disty < 1)
                        continue;


                    candidates.Clear();
                    int address = (y * mBitmapOriginal.Width) + x;
                    candidates.Add(mProcessedColors[address]);
                    if ((mSettings.BlurDirection & ProcessingSettings.eDirection.Vertical) != 0)
                    {
                        for (int i = 1; i < mSettings.BlurSteps + 1; i++)
                        {
                            if (y - i < 0)
                                continue;
                            candidates.Add(mProcessedColors[((y - i) * mBitmapOriginal.Width) + x]);
                        }
                        for (int i = 1; i < mSettings.BlurSteps + 1; i++)
                        {
                            if ((y + i) >= mBitmapOriginal.Height)
                                continue;

                            candidates.Add(mProcessedColors[((y + i) * mBitmapOriginal.Width) + x]);
                        }

                    }
                    if ((mSettings.BlurDirection & ProcessingSettings.eDirection.Horizontal) != 0)
                    {
                        for (int i = 1; i < mSettings.BlurSteps + 1; i++)
                        {
                            if (x - i < 0)
                                continue;
                            candidates.Add(mProcessedColors[(y * mBitmapOriginal.Width) + (x - i)]);
                        }
                        for (int i = 1; i < mSettings.BlurSteps + 1; i++)
                        {
                            if (x + i >= mBitmapOriginal.Width)
                                continue;
                            candidates.Add(mProcessedColors[(y * mBitmapOriginal.Width) + (x + i)]);
                        }

                    }
                    if ((mSettings.BlurDirection & ProcessingSettings.eDirection.Diagonal1) != 0)
                    {
                        for (int i = 1; i < mSettings.BlurSteps + 1; i++)
                        {
                            if (y - i < 0 || x - i < 0)
                                continue;
                            candidates.Add(mProcessedColors[((y - i) * mBitmapOriginal.Width) + (x - i)]);
                        }
                        for (int i = 1; i < mSettings.BlurSteps + 1; i++)
                        {
                            if (y + i >= mBitmapOriginal.Height || x + i >= mBitmapOriginal.Width)
                                continue;
                            candidates.Add(mProcessedColors[((y + i) * mBitmapOriginal.Width) + (x + i)]);
                        }

                    }
                    if ((mSettings.BlurDirection & ProcessingSettings.eDirection.Diagonal2) != 0)
                    {
                        for (int i = 1; i < mSettings.BlurSteps + 1; i++)
                        {
                            if (y - i < 0 || x + i >= mBitmapOriginal.Width)
                                continue;
                            candidates.Add(mProcessedColors[((y - i) * mBitmapOriginal.Width) + (x + i)]);
                        }
                        for (int i = 1; i < mSettings.BlurSteps + 1; i++)
                        {
                            if (y + i >= mBitmapOriginal.Height || x - i < 0)
                                continue;
                            candidates.Add(mProcessedColors[((y + i) * mBitmapOriginal.Width) + (x - i)]);
                        }
                    }


                    // alpha << 24 | red << 16 | green << 8 | blue;
                    uint r = 0, g = 0, b = 0, a = 0;
                    foreach (uint color in candidates)
                    {
                        r += GetRedComponent(color);
                        g += GetGreenComponent(color);
                        b += GetBlueComponent(color);
                        a += GetAlphaComponent(color);
                    }
                    r /= (uint)candidates.Count;
                    g /= (uint)candidates.Count;
                    b /= (uint)candidates.Count;
                    a /= (uint)candidates.Count;
                    //uint a = GetAlphaComponent(mProcessedColors[address]);
                    mProcessedColors[address] = a << 24 | r << 16 | g << 8 | b;
                }
            }
        }
        /// <summary>
        /// Applies the TextureAlpha postprocessing effect
        /// </summary>
        private void TextureAlpha()
        {
            if (mSettings.TextureAlphaChannel == ProcessingSettings.eChannel.None)
                return;

            for (int x = 0; x < mBitmapOriginal.Width; x++)
            {
                int pbX = (int)this.mXNAPictureBox.ImageToPictureBox(new Vector2(x, 0)).X;
                int initialy = (int)this.mReflectionMark.GetYAt(pbX);
                if (initialy == float.MinValue)
                    continue;
                int initialy_onImage = (int)this.mXNAPictureBox.PictureBoxToImage(new Vector2(0, initialy)).Y;

                for (int y = 0; y < mBitmapOriginal.Height; y++)
                {
                    if (y <= initialy_onImage)
                        continue;
                    int disty = y - initialy_onImage;
                    if (disty < 1)
                        continue;

                    int address = (y * mBitmapOriginal.Width) + x;

                    // alpha << 24 | red << 16 | green << 8 | blue;
                    uint r = GetRedComponent(mProcessedColors[address]);
                    uint g = GetGreenComponent(mProcessedColors[address]);
                    uint b = GetBlueComponent(mProcessedColors[address]);
                    uint a = GetAlphaComponent(mProcessedColors[address]);

                    int alpha_x = x % mSettings.TextureAlpha.Width;
                    int alpha_y = y % mSettings.TextureAlpha.Height;
                    System.Drawing.Color c = mSettings.TextureAlpha.GetPixel(alpha_x, alpha_y);
                    float channel = 0f;
                    switch (mSettings.TextureAlphaChannel)
                    {
                        case ProcessingSettings.eChannel.None:
                            channel = 1f;
                            break;
                        case ProcessingSettings.eChannel.Red:
                            channel = (float)c.R / 255f;
                            break;
                        case ProcessingSettings.eChannel.Green:
                            channel = (float)c.G / 255f;
                            break;
                        case ProcessingSettings.eChannel.Blue:
                            channel = (float)c.B / 255f;
                            break;
                        case ProcessingSettings.eChannel.Alpha:
                            channel = (float)c.A / 255f;
                            break;
                    }

                    a = (uint)((float)a * channel);

                    mProcessedColors[address] = a << 24 | r << 16 | g << 8 | b;
                }
            }
        }
        #endregion

        #region Color Helpers
        /// <summary>
        /// Get the red component of a int color
        /// </summary>
        /// <param name="pColor"></param>
        /// <returns></returns>
        private uint GetRedComponent(uint pColor)
        {
            return (uint)(pColor & 0x00FF0000) >> 16;
        }
        /// <summary>
        /// Get the green component of a int color
        /// </summary>
        /// <param name="pColor"></param>
        /// <returns></returns>
        private uint GetGreenComponent(uint pColor)
        {
            return (uint)(pColor & 0x0000FF00) >> 8;
        }
        /// <summary>
        /// Get the blue component of a int color
        /// </summary>
        /// <param name="pColor"></param>
        /// <returns></returns>
        private uint GetBlueComponent(uint pColor)
        {
            return (uint)(pColor & 0x000000FF);
        }
        /// <summary>
        /// Get the alpha component of a int color
        /// </summary>
        /// <param name="pColor"></param>
        /// <returns></returns>
        private uint GetAlphaComponent(uint pColor)
        {
            return (uint)(pColor & 0xFF000000) >> 24;
        }      
        #endregion

        #region XNA methods
        /// <summary>
        /// Creates the graphics device when form is loaded
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CreateGraphicsDevice();

            ResetGraphicsDevice();
        }
        /// <summary>
        /// Creates a graphic device with default settings
        /// </summary>
        private void CreateGraphicsDevice()
        {
            // Create Presentation Parameters
            PresentationParameters pp = new PresentationParameters();
            pp.BackBufferCount = 1;
            pp.IsFullScreen = false;
            pp.SwapEffect = SwapEffect.Discard;
            pp.BackBufferWidth = panelViewport.Width;
            pp.BackBufferHeight = panelViewport.Height;
            pp.AutoDepthStencilFormat = DepthFormat.Depth24Stencil8;
            pp.EnableAutoDepthStencil = true;
            pp.PresentationInterval = PresentInterval.Default;
            pp.BackBufferFormat = SurfaceFormat.Unknown;
            pp.MultiSampleType = MultiSampleType.None;

            // Create device
            mDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter,
                DeviceType.Hardware, this.panelViewport.Handle, pp);
        }
        /// <summary>
        /// Resets the graphics device and calls the disposing and re-creating events. Usally
        /// needed when the viewport size changes, because resets the device to the new size.
        /// </summary>
        private void ResetGraphicsDevice()
        {       
            // Avoid entering until panelViewport is setup and device created
            if (mDevice== null || panelViewport.Width == 0 || panelViewport.Height == 0)
                return;

            if (this.DisposeAll != null)
                this.DisposeAll();

            // Reset device
            mDevice.PresentationParameters.BackBufferWidth = panelViewport.Width;
            mDevice.PresentationParameters.BackBufferHeight = panelViewport.Height;
            mDevice.Reset();

            this.mXNAPictureBox.LoadGraphicsContent(this.mDevice);
            this.mReflectionMark.LoadGraphicsContent(this.mDevice);

            if (this.ReCreateAll != null)
                this.ReCreateAll(this.mDevice);
        }  
        /// <summary>
        /// Performs all the rendering of the viewport
        /// </summary>
        public void Render()
        {
            if (this.OnFrameMove != null)
                this.OnFrameMove(this.mDevice);

            mDevice.Clear(this.mBackColor);


            this.mXNAPictureBox.Render(this.mDevice);

            if (this.mXNAPictureBox.Texture != null)
            {
                if(mSettings.DisplayMark)
                    this.mReflectionMark.Render(this.mDevice);
            }

            if (this.OnFrameRender != null)
                this.OnFrameRender(this.mDevice);
          
            mDevice.Present();

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewportResize(object sender, EventArgs e)
        {
            ResetGraphicsDevice();

            this.mXNAPictureBox.Width = panelViewport.ClientRectangle.Width;
            this.mXNAPictureBox.Height = panelViewport.ClientRectangle.Height;
            this.mReflectionMark.Width = panelViewport.ClientRectangle.Width;
            this.mReflectionMark.Height = panelViewport.ClientRectangle.Height;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVieweportPaint(object sender, PaintEventArgs e)
        {
            if (this.mRefreshMode != eRefreshMode.Always)
                this.Render();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelViewport_BackColorChanged(object sender, EventArgs e)
        {
            this.mBackColor = new Microsoft.Xna.Framework.Graphics.Color(panelViewport.BackColor.R, panelViewport.BackColor.G, panelViewport.BackColor.B);
        }
        #endregion

        #region Menu
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog(this) != DialogResult.OK)
                return;

            this.LoadBitmap(this.openFileDialog1.FileName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.saveFileDialog1.ShowDialog(this) != DialogResult.OK)
                return;

            string ext = System.IO.Path.GetExtension(this.saveFileDialog1.FileName);
            switch(ext.ToLower())
            {
                case ".bmp":
                    this.mXNAPictureBox.Texture.Save(saveFileDialog1.FileName, ImageFileFormat.Bmp);
                    break;
                case ".dds":
                    this.mXNAPictureBox.Texture.Save(saveFileDialog1.FileName, ImageFileFormat.Dds);
                    break;
                case ".dib":
                    this.mXNAPictureBox.Texture.Save(saveFileDialog1.FileName, ImageFileFormat.Dib);
                    break;
                case ".hdr":
                    this.mXNAPictureBox.Texture.Save(saveFileDialog1.FileName, ImageFileFormat.Hdr);
                    break;
                case ".jpg":
                    this.mXNAPictureBox.Texture.Save(saveFileDialog1.FileName, ImageFileFormat.Jpg);
                    break;
                case ".pfm":
                    this.mXNAPictureBox.Texture.Save(saveFileDialog1.FileName, ImageFileFormat.Pfm);
                    break;
                case ".png":
                    this.mXNAPictureBox.Texture.Save(saveFileDialog1.FileName, ImageFileFormat.Png);
                    break;
                case ".ppm":
                    this.mXNAPictureBox.Texture.Save(saveFileDialog1.FileName, ImageFileFormat.Ppm);
                    break;
                case ".tga":
                    this.mXNAPictureBox.Texture.Save(saveFileDialog1.FileName, ImageFileFormat.Tga);
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            About f = new About();
            f.ShowDialog(this);
        }
        #endregion

        #region ToolStrip
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbCreatePoint_CheckedChanged(object sender, EventArgs e)
        {
            if (this.tsbCreatePoint.Checked)
                this.mReflectionMark.MouseMode = ReflectionMark.eMouseMode.CreatePoint;
            else this.mReflectionMark.MouseMode = ReflectionMark.eMouseMode.None;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbAlphaGradient_CheckedChanged(object sender, EventArgs e)
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            this.ProcessBitmap();

            this.Render();
        }
        /// <summary>
        /// Select a different color for the picturebox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbColor_Click(object sender, EventArgs e)
        {
            if (this.colorDialog1.ShowDialog(this) != DialogResult.OK)
                return;

            this.panelViewport.BackColor = this.colorDialog1.Color;
        }
        #endregion

        #region Mouse
        /// <summary>
        /// Used to inform the XNA components when the mouse moves inside the panel viewport
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelViewport_MouseMove(object sender, MouseEventArgs e)
        {
            this.mReflectionMark.OnMouseMove(e);
        }
        /// <summary>
        /// Used to inform the XNA components when the mouse up event happens inside the panel viewport
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelViewport_MouseUp(object sender, MouseEventArgs e)
        {
            this.mReflectionMark.OnMouseUp(e);

            this.Render();
        }
        /// <summary>
        /// Used to inform the XNA components when the mouse down event happens inside the panel viewport
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelViewport_MouseDown(object sender, MouseEventArgs e)
        {
            this.mReflectionMark.OnMouseDown(e);
        }
        #endregion

        #region Reflection Mark events
        /// <summary>
        /// Occurs when the reflection mark is being changed. Needed to refresh the view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mReflectionMark_ReflectionMarkChanging(object sender, EventArgs e)
        {
            this.Render();
        }
        /// <summary>
        /// Occurs when the reflection mark ended changing. Useful event to perfom 
        /// the bitmap processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mReflectionMark_ReflectionMarkChanged(object sender, EventArgs e)
        {
            if(mSettings.RefreshAlways)
                this.ProcessBitmap();

            this.Render();
        }
        /// <summary>
        /// Mouse mode in the reflection mark was changed. Needed to synchronize with the 
        /// toolbar button tsbCreatePoint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mReflectionMark_MouseModeChanged(object sender, EventArgs e)
        {
            this.tsbCreatePoint.CheckedChanged -= new System.EventHandler(this.tsbCreatePoint_CheckedChanged);
            this.tsbCreatePoint.Checked = (this.mReflectionMark.MouseMode == ReflectionMark.eMouseMode.CreatePoint);
            this.tsbCreatePoint.CheckedChanged += new System.EventHandler(this.tsbCreatePoint_CheckedChanged);

        }
        #endregion

        #region Property Grid
        /// <summary>
        /// Occurs when any property of the propertyGrid changes it´s value. Useful to process
        /// the bitmap after a change
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (mSettings.RefreshAlways)
                this.ProcessBitmap();
            
            this.Render();
        }
        #endregion

    

      

    






    }

}