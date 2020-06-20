using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Warehouse.Gui.Helpers
{
    public class ZoomBorder : Border
    {
        private UIElement _child = null;
        private Point _origin;
        private Point _start;

        private TranslateTransform _childTranslateTransform;
        private ScaleTransform _childScaleTransform;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform) ((TransformGroup) element.RenderTransform)
                                       .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform) ((TransformGroup) element.RenderTransform)
                                   .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get => base.Child;
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this._child = element;
            if (_child == null)
                return;
            TransformGroup transformGroup = new TransformGroup();
            ScaleTransform scaleTransform = new ScaleTransform();
            transformGroup.Children.Add(scaleTransform);
            TranslateTransform translateTransform = new TranslateTransform();
            transformGroup.Children.Add(translateTransform);

            _child.RenderTransform = transformGroup;
            _child.RenderTransformOrigin = new Point(0.0, 0.0);
            this.MouseWheel += ChildMouseWheel;
            this.MouseLeftButtonDown += ChildMouseLeftButtonDown;
            this.MouseLeftButtonUp += ChildMouseLeftButtonUp;
            this.MouseMove += ChildMouseMove;
            this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                                                                            ChildPreviewMouseRightButtonDown);
            _childTranslateTransform = GetTranslateTransform(_child);
            _childScaleTransform = GetScaleTransform(_child);
        }

        public void Reset()
        {
            if (_child != null)
            {
                // reset zoom
                _childScaleTransform.ScaleX = 1.0;
                _childScaleTransform.ScaleY = 1.0;

                // reset pan
                _childTranslateTransform.X = 0.0;
                _childTranslateTransform.Y = 0.0;
            }
        }

        #region Child Events

        private void ChildMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_child != null)
            {
                var st = _childScaleTransform;
                var tt = _childTranslateTransform;

                double zoom = e.Delta > 0 ? .1 : -.1;
                zoom *= st.ScaleX * 2;
                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                    return;

                Point relative = e.GetPosition(_child);
                double absoluteX;
                double absoluteY;

                absoluteX = relative.X * st.ScaleX + tt.X;
                absoluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = absoluteX - relative.X * st.ScaleX;
                tt.Y = absoluteY - relative.Y * st.ScaleY;
            }
        }

        private void ChildMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_child != null)
            {
                _start = e.GetPosition(this);
                _origin = new Point(_childTranslateTransform.X, _childTranslateTransform.Y);
                this.Cursor = Cursors.Hand;
                _child.CaptureMouse();
            }
        }

        private void ChildMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_child != null)
            {
                _child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        private void ChildPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void ChildMouseMove(object sender, MouseEventArgs e)
        {
            if (_child != null && _child.IsMouseCaptured)
            {
                Vector v = _start - e.GetPosition(this);
                _childTranslateTransform.X = _origin.X - v.X;
                _childTranslateTransform.Y = _origin.Y - v.Y;
            }
        }

        #endregion
    }
}