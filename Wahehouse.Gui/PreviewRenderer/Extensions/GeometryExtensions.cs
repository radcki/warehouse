using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Warehouse.Gui.PreviewRenderer.Extensions
{
    public static class GeometryExtensions
    {
        public static void DrawGeometry(this StreamGeometryContext ctx, Geometry geo)
        {
            var pathGeometry = geo as PathGeometry ?? PathGeometry.CreateFromGeometry(geo);
            foreach (var figure in pathGeometry.Figures)
                ctx.DrawFigure(figure);
        }

        public static void DrawFigure(this StreamGeometryContext ctx, PathFigure figure)
        {
            ctx.BeginFigure(figure.StartPoint, figure.IsFilled, figure.IsClosed);
            foreach (var segment in figure.Segments)
            {
                if (segment is LineSegment lineSegment)
                {
                    ctx.LineTo(lineSegment.Point, lineSegment.IsStroked, lineSegment.IsSmoothJoin);
                    continue;
                }

                if (segment is BezierSegment bezierSegment)
                {
                    ctx.BezierTo(bezierSegment.Point1, bezierSegment.Point2, bezierSegment.Point3, bezierSegment.IsStroked, bezierSegment.IsSmoothJoin);
                    continue;
                }

                if (segment is QuadraticBezierSegment quadraticSegment)
                {
                    ctx.QuadraticBezierTo(quadraticSegment.Point1, quadraticSegment.Point2, quadraticSegment.IsStroked, quadraticSegment.IsSmoothJoin);
                    continue;
                }

                if (segment is PolyLineSegment polyLineSegment)
                {
                    ctx.PolyLineTo(polyLineSegment.Points, polyLineSegment.IsStroked, polyLineSegment.IsSmoothJoin);
                    continue;
                }

                if (segment is PolyBezierSegment polyBezierSegment)
                {
                    ctx.PolyBezierTo(polyBezierSegment.Points, polyBezierSegment.IsStroked, polyBezierSegment.IsSmoothJoin);
                    continue;
                }

                if (segment is PolyQuadraticBezierSegment polyQuadraticSegment)
                {
                    ctx.PolyQuadraticBezierTo(polyQuadraticSegment.Points, polyQuadraticSegment.IsStroked, polyQuadraticSegment.IsSmoothJoin);
                    continue;
                }

                if (segment is ArcSegment arcSegment)
                {
                    ctx.ArcTo(arcSegment.Point, arcSegment.Size, arcSegment.RotationAngle, arcSegment.IsLargeArc, arcSegment.SweepDirection, arcSegment.IsStroked, arcSegment.IsSmoothJoin);
                    continue;
                }
            }
        }
    }
}