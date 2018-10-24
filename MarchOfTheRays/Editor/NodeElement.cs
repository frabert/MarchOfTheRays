using System;
using System.Collections.Generic;

using System.Drawing;

namespace MarchOfTheRays.Editor
{
    interface IClippable
    {
        RectangleF ClipRegion { get; }
    }

    class NodeElement : IClippable
    {
        public RectangleF ClipRegion => new RectangleF(Location, Size);

        #region Location
        PointF m_Location;
        public PointF Location
        {
            get => m_Location;
            set
            {
                m_Location = value;
                OnLocationChanged();
            }
        }

        public event EventHandler LocationChanged;

        protected virtual void OnLocationChanged()
        {
            LocationChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region Size
        SizeF m_Size;
        public SizeF Size
        {
            get => m_Size;
            set
            {
                m_Size = value;
                OnSizeChanged();
            }
        }

        public event EventHandler SizeChanged;

        protected virtual void OnSizeChanged()
        {
            SizeChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        public object Tag { get; set; }

        public float Width
        {
            get => Size.Width;
            set
            {
                Size = new SizeF(value, Size.Height);
            }
        }

        public float Height
        {
            get => Size.Height;
            set
            {
                Size = new SizeF(Size.Width, value);
            }
        }

        public int IsOverInputHandle(PointF p)
        {
            float spacing = m_Size.Height / (m_InputCount + 1);
            for (int i = 0; i < m_InputCount; i++)
            {
                var rect = new RectangleF(0, spacing * (i + 1) - m_HandleSize / 2.0f, m_HandleSize, m_HandleSize);
                if (rect.Contains(p)) return i;
            }
            return -1;
        }

        public bool IsOverOutputHandle(PointF p)
        {
            if (!m_HasOutput) return false;
            var rect = new RectangleF(m_Size.Width - m_HandleSize, m_Size.Height / 2.0f - m_HandleSize / 2.0f, m_HandleSize, m_HandleSize);
            return rect.Contains(p);
        }

        #region Text
        string m_Text;
        public string Text
        {
            get => m_Text;
            set
            {
                m_Text = value;
                OnTextChanged();
            }
        }

        public event EventHandler TextChanged;

        protected virtual void OnTextChanged()
        {
            TextChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region Font
        Font m_Font = SystemFonts.DefaultFont;
        public Font Font
        {
            get => m_Font;
            set
            {
                m_Font = value;
                OnFontChanged();
            }
        }

        public event EventHandler FontChanged;

        protected virtual void OnFontChanged()
        {
            FontChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region BackColor
        Color m_BackColor = SystemColors.Window;
        public Color BackColor
        {
            get => m_BackColor;
            set
            {
                m_BackColor = value;
                OnBackColorChanged();
            }
        }

        public event EventHandler BackColorChanged;

        protected virtual void OnBackColorChanged()
        {
            BackColorChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region BorderColor
        Color m_BorderColor = Color.Black;
        public Color BorderColor
        {
            get => m_BorderColor;
            set
            {
                m_BorderColor = value;
                OnBorderColorChanged();
            }
        }

        public event EventHandler BorderColorChanged;

        protected virtual void OnBorderColorChanged()
        {
            BorderColorChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region ForeColor
        Color m_ForeColor = SystemColors.WindowText;
        public Color ForeColor
        {
            get => m_ForeColor;
            set
            {
                m_ForeColor = value;
                OnForeColorChanged();
            }
        }

        public event EventHandler ForeColorChanged;

        protected virtual void OnForeColorChanged()
        {
            ForeColorChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region HandleSize
        float m_HandleSize = 10;
        public float HandleSize
        {
            get => m_HandleSize;
            set
            {
                m_HandleSize = value;
                OnHandleSizeChanged();
            }
        }

        public event EventHandler HandleSizeChanged;

        protected virtual void OnHandleSizeChanged()
        {
            HandleSizeChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region InputCount
        int m_InputCount = 1;
        public int InputCount
        {
            get => m_InputCount;
            set
            {
                m_InputCount = value;
                OnInputCountChanged();
            }
        }

        public event EventHandler InputCountChanged;

        protected virtual void OnInputCountChanged()
        {
            InputCountChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region HasOutput
        bool m_HasOutput = true;
        public bool HasOutput
        {
            get => m_HasOutput;
            set
            {
                m_HasOutput = value;
                OnHasOutputChanged();
            }
        }

        public event EventHandler HasOutputChanged;

        protected virtual void OnHasOutputChanged()
        {
            HasOutputChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region Selected
        bool m_Selected;

        public bool Selected
        {
            get => m_Selected;
            set
            {
                m_Selected = value;
            }
        }

        public event EventHandler SelectedChanged;

        protected virtual void OnSelectedChanged()
        {
            SelectedChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        public event EventHandler NeedsRepaint;

        protected virtual void OnNeedsRepaint()
        {
            NeedsRepaint?.Invoke(this, new EventArgs());
        }

        public void Paint(Graphics g)
        {
            var backRect = new RectangleF(m_HandleSize / 2.0f, 0, m_Size.Width - m_HandleSize, m_Size.Height);

            using (var backBrush = new SolidBrush(BackColor))
            using (var borderPen = new Pen(BorderColor))
            using (var foreBrush = new SolidBrush(ForeColor))
            {
                g.FillRectangle(backBrush, backRect);
                g.DrawRectangle(borderPen, backRect.X, backRect.Y, backRect.Width, backRect.Height);

                float spacing = m_Size.Height / (m_InputCount + 1);
                for (int i = 0; i < m_InputCount; i++)
                {
                    var rect = new RectangleF(0, spacing * (i + 1) - m_HandleSize / 2.0f, m_HandleSize, m_HandleSize);
                    g.FillRectangle(backBrush, rect);
                    g.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                if (m_HasOutput)
                {
                    var rect = new RectangleF(m_Size.Width - m_HandleSize, m_Size.Height / 2.0f - m_HandleSize / 2.0f, m_HandleSize, m_HandleSize);
                    g.FillRectangle(backBrush, rect);
                    g.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                backRect.Inflate(-(m_HandleSize + 2), -2);
                var sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                g.DrawString(m_Text, m_Font, foreBrush, backRect, sf);
            }
        }
    }
}
