using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using XNA = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrimitivesSample;
using System.Windows.Forms;

namespace SMX
{
    /// <summary>
    /// Author: Iñaki Ayucar (http://graphicdna.blogspot.com)
    /// Date: 23/11/2007
    /// 
    /// This software is distributed "for free" for any non-commercial usage. The software is provided “as-is.” 
    /// You bear the risk of using it. The contributors give no express warranties, guarantees or conditions.
    /// </summary>
    public class ReflectionMark
    {
        public enum eMouseMode
        {
            MovePoint,
            MoveMark,
            CreatePoint,
            None,
        }

        private const int cRadiusNodes = 8;
        private const int cHalfRadiusNodes = (int)((float)cRadiusNodes * 0.5f);

        private Color mColor = XNA.Graphics.Color.DarkGray;
        private int mWidth, mHeight;
        public int Width
        {
            get { return mWidth; }
            set
            {
                mWidth = value;
                this.ClientSizeChanged();
            }
        }
        public int Height
        {
            get { return mHeight; }
            set
            {
                mHeight = value;
                this.ClientSizeChanged();
            }
        }

        public List<Vector2> mPoints = null;               
        private PrimitiveBatch mPrimitiveBatch = null;

        private System.Drawing.Point mLastMousePos;
        private eMouseMode mMouseMode = eMouseMode.None;
        private int mPointMovingIdx = -1;
        public eMouseMode MouseMode
        {
            get { return mMouseMode; }
            set
            {
                mMouseMode = value;

                switch (value)
                {
                    case eMouseMode.MovePoint:
                        if (mPointMovingIdx > 0 && mPointMovingIdx < mPoints.Count - 1)
                            Cursor.Current = Cursors.NoMove2D;
                        else Cursor.Current = Cursors.NoMoveVert;
                        break;
                    case eMouseMode.MoveMark:
                        Cursor.Current = Cursors.NoMoveVert;
                        break;
                    case eMouseMode.CreatePoint:
                        Cursor.Current = Cursors.Cross;
                        break;
                    default:
                        Cursor.Current = Cursors.Default;
                        break;
                }

                if (this.MouseModeChanged != null)
                    this.MouseModeChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ReflectionMarkChanged = null;
        public event EventHandler ReflectionMarkChanging = null;
        public event EventHandler MouseModeChanged = null;


        /// <summary>
        /// 
        /// </summary>
        public ReflectionMark()
        {
            mPoints = new List<Vector2>();

            this.ResetMark();
        }
        /// <summary>
        /// 
        /// </summary>
        public void ResetMark()
        {
            this.mPoints.Clear();

            this.mPoints.Add(new Vector2());
            this.mPoints.Add(new Vector2());

            this.ClientSizeChanged();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void ClientSizeChanged()
        {
            if (this.mPoints == null || this.mPoints.Count == 0 || this.mWidth == 0 || this.mHeight == 0)
                return;

            if (this.mPoints[0] == Vector2.Zero)
                this.mPoints[0] = new Vector2(cHalfRadiusNodes, (float)this.mHeight * 0.5f);
            else this.mPoints[0] = new Vector2(cHalfRadiusNodes, this.mPoints[0].Y); ;

            if (this.mPoints[mPoints.Count - 1] == Vector2.Zero)
                this.mPoints[mPoints.Count - 1] = new Vector2(this.mWidth - cHalfRadiusNodes, (float)this.mHeight * 0.5f);
            else this.mPoints[mPoints.Count - 1] = new Vector2(this.mWidth - cHalfRadiusNodes, this.mPoints[mPoints.Count - 1].Y); ;
        }

        #region Device Events
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (this.mPrimitiveBatch != null)
                this.mPrimitiveBatch.Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDevice"></param>
        public void LoadGraphicsContent(GraphicsDevice pDevice)
        {
            mPrimitiveBatch = new PrimitiveBatch(pDevice);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDevice"></param>
        public void Render(GraphicsDevice pDevice)
        {
            if (this.mPoints.Count == 0)
                return;

            // Draw lines
            mPrimitiveBatch.Begin(PrimitiveType.LineList);
            for (int i = 0; i < this.mPoints.Count-1; i++)
            {
                mPrimitiveBatch.AddVertex(this.mPoints[i], this.mColor);
                mPrimitiveBatch.AddVertex(this.mPoints[i + 1], this.mColor);
                //batch.AddVertex(this.mPoints[(i + 1) % transformedVertices.Length], ShapeColor);
            }
            mPrimitiveBatch.End();

            // Draw Nodes
            for (int i = 0; i < this.mPoints.Count; i++)
            {
                mPrimitiveBatch.Begin(PrimitiveType.LineList);
                mPrimitiveBatch.AddVertex(this.mPoints[i] + new Vector2(-cHalfRadiusNodes, -cHalfRadiusNodes), this.mColor);
                mPrimitiveBatch.AddVertex(this.mPoints[i] + new Vector2(cHalfRadiusNodes, -cHalfRadiusNodes), this.mColor);

                mPrimitiveBatch.AddVertex(this.mPoints[i] + new Vector2(cHalfRadiusNodes, -cHalfRadiusNodes), this.mColor);
                mPrimitiveBatch.AddVertex(this.mPoints[i] + new Vector2(cHalfRadiusNodes, cHalfRadiusNodes), this.mColor);

                mPrimitiveBatch.AddVertex(this.mPoints[i] + new Vector2(cHalfRadiusNodes, cHalfRadiusNodes), this.mColor);
                mPrimitiveBatch.AddVertex(this.mPoints[i] + new Vector2(-cHalfRadiusNodes, cHalfRadiusNodes), this.mColor);

                mPrimitiveBatch.AddVertex(this.mPoints[i] + new Vector2(-cHalfRadiusNodes, cHalfRadiusNodes), this.mColor);
                mPrimitiveBatch.AddVertex(this.mPoints[i] + new Vector2(-cHalfRadiusNodes, -cHalfRadiusNodes), this.mColor);
                mPrimitiveBatch.End();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="pNewValue"></param>
        public void ChangePoint(int idx, Vector2 pDif)
        {
            if (this.mWidth == 0 || this.mHeight == 0)
                return;

            Vector2 newValue = this.mPoints[idx];
            newValue += pDif;

            float newx = Math.Max(cHalfRadiusNodes, newValue.X);
            newx = Math.Min(this.mWidth - cHalfRadiusNodes, newx);
            float newy = Math.Max(cHalfRadiusNodes, newValue.Y);
            newy = Math.Min(this.mHeight - cHalfRadiusNodes, newy);

            this.mPoints[idx] = new Vector2(newx, newy);

            if (this.ReflectionMarkChanging != null)
                this.ReflectionMarkChanging(this, EventArgs.Empty);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pVec"></param>
        public void CreatePoint(Vector2 pVec)
        {
            int idxInsert = 0;

            // Detect if point is too close
            foreach (Vector2 vec in this.mPoints)
            {
                if (vec == pVec)
                    return;

                float dist = (vec - pVec).Length();
                if (dist < 0.01f)
                    return;
            }

            // Calc the index in mPoints to insert in
            for (int i = 0; i < this.mPoints.Count; i++)
            {
                if (pVec.X < this.mPoints[i].X)
                {
                    idxInsert = i;
                    break;
                }
            }

            this.mPoints.Insert(idxInsert, pVec);
        }

        #region Mouse
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.mMouseMode != eMouseMode.None)
            {
                if (this.ReflectionMarkChanged != null)
                    this.ReflectionMarkChanged(this, EventArgs.Empty);
            }

            this.mPointMovingIdx = -1;
            this.MouseMode = eMouseMode.None;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                this.MouseMode = eMouseMode.None;
                return;
            }
            Vector2 mousepos = new Vector2(e.Location.X, e.Location.Y);
            switch (this.mMouseMode)
            {
                case eMouseMode.CreatePoint:

                    this.CreatePoint(mousepos);
                    break;
                default:
                    this.mPointMovingIdx = -1;
                    for (int i = 0; i < this.mPoints.Count; i++)
                    {
                        if (Vector2InsideCircle(mousepos, this.mPoints[i], cRadiusNodes))
                        {
                            this.mPointMovingIdx = i;
                            this.MouseMode = eMouseMode.MovePoint;
                            return;
                        }
                    }

                    if (Vector2InReflectionMark(mousepos))
                    {

                        this.MouseMode = eMouseMode.MoveMark;
                        return;
                    }


                    this.MouseMode = eMouseMode.None;

                    break;

            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            Vector2 dif = new Vector2(e.X - mLastMousePos.X, e.Y - mLastMousePos.Y);
            this.mLastMousePos = e.Location;
            Vector2 mousepos = new Vector2(e.Location.X, e.Location.Y);

            switch (this.mMouseMode)
            {
                case eMouseMode.MovePoint:
                    if (this.mPointMovingIdx != -1)
                    {
                        // Don´t allow points of sides to move in the X direction
                        if (this.mPointMovingIdx == 0 || this.mPointMovingIdx == this.mPoints.Count - 1)
                            dif.X = 0f;

                        this.ChangePoint(mPointMovingIdx, dif);
                    }
                    break;
                case eMouseMode.MoveMark:
                    for (int i = 0; i < this.mPoints.Count; i++)
                    {
                        // Don´t allow points of sides to move in the X direction
                        Vector2 dif2 = dif;
                        if (i == 0 || i == this.mPoints.Count - 1)
                            dif2.X = 0f;

                        this.ChangePoint(i, dif2);
                    }
                    break;
                case eMouseMode.CreatePoint:
                    if (this.Vector2InReflectionMark(mousepos))
                        Cursor.Current = Cursors.Cross;
                    else Cursor.Current = Cursors.Default;
                    break;
                default:
                    for (int i = 0; i < this.mPoints.Count; i++)
                    {
                        if (this.Vector2InsideCircle(mousepos, this.mPoints[i], cRadiusNodes))
                        {
                            if (i > 0 && i < mPoints.Count - 1)
                                Cursor.Current = Cursors.NoMove2D;
                            else Cursor.Current = Cursors.NoMoveVert;
                            return;
                        }
                    }

                    if (this.Vector2InReflectionMark(mousepos))
                        Cursor.Current = Cursors.NoMoveVert;
                    else Cursor.Current = Cursors.Default;

                    break;
            }


        }
        #endregion

        #region Maths
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPt"></param>
        /// <param name="pCirclePos"></param>
        /// <param name="pCircleRad"></param>
        /// <returns></returns>
        private bool Vector2InsideCircle(Vector2 pPt, Vector2 pCirclePos, int pCircleRad)
        {
            Vector2 dif = pPt - pCirclePos;
            return (dif.Length() <= pCircleRad);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="Vector2"></param>
        /// <param name="pVector2WasInsideSegment"></param>
        /// <returns></returns>
        private bool ClosestVector2On2DSegment(Vector2 p1, Vector2 p2, Vector2 point, out Vector2 pClosestVector2)
        {
            // Check if Vector2 is outside the segment in the p1 side
            Vector2 ca = point - p1;
            Vector2 segDir = p2 - p1;
            float dot_ta = Vector2.Dot(ca, segDir);
            if (dot_ta <= 0)
            {
                pClosestVector2 = p1;
                return false;
            }

            // Check if Vector2 is outside the segment in the p2 side
            Vector2 cb = point - p2;
            float dot_tb = Vector2.Dot(cb, -segDir);
            if (dot_tb <= 0)
            {
                pClosestVector2 = p2;
                return false;
            }

            float dot_tatb = dot_ta + dot_tb;
            Vector2 nearest = new Vector2(segDir.X * (dot_ta / dot_tatb), segDir.Y * (dot_ta / dot_tatb));
            nearest += p1;

            pClosestVector2 = nearest;
            return true;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPt"></param>
        /// <returns></returns>
        private bool Vector2InReflectionMark(Vector2 pPt)
        {
            Vector2 closest = Vector2.Zero;

            for (int i = 0; i < this.mPoints.Count - 1; i++)
            {
                if (ClosestVector2On2DSegment(this.mPoints[i], this.mPoints[i + 1], pPt, out closest))
                {
                    Vector2 dif = pPt - closest;
                    if (dif.Length() < cHalfRadiusNodes)
                        return true;

                }
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float GetMaxY()
        {
            float maxY = float.MinValue;
            foreach(Vector2 vec in mPoints)
            {
                if (vec.Y > maxY)
                    maxY = vec.Y;
            }
            return maxY;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pX"></param>
        /// <returns></returns>
        public float GetYAt(float pX)
        {
            float y = float.MinValue;

            for (int i = 0; i < this.mPoints.Count - 1; i++)
            {
                y = float.MinValue;
                y = this.GetYAtSegment(pX, this.mPoints[i], this.mPoints[i + 1]);
                if (y != float.MinValue)
                    return y;
            }
            return float.MinValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pX"></param>
        /// <returns>The Y coordinate that corresponds to pX</returns>
        private float GetYAtSegment(float pX, Vector2 p1, Vector2 p2)
        {
            // If point is outside the limits of this segment, just quit
            if (pX < Math.Min(p1.X-cHalfRadiusNodes, p2.X-cHalfRadiusNodes))
                return float.MinValue;
            if (pX > Math.Max(p1.X + cHalfRadiusNodes, p2.X + cHalfRadiusNodes))
                return float.MinValue;

            float difx = (p2.X+(float)cHalfRadiusNodes) - (p1.X-(float)cHalfRadiusNodes);
            if (difx == 0)
                return float.MinValue;

            float dify = p2.Y - p1.Y;
            float prop = dify / difx;
            return p1.Y + (prop * (pX - (p1.X - (float)cHalfRadiusNodes)));

        }

        #endregion
    }
}
