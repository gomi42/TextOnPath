// MIT License
// 
// Copyright (c) 2020 Michael Göricke
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// This control is based on Charles Petzold's work, ideas and code. This control
// combines warped and unwarped text into one single control and adds some more
// features:
// - define the path in some more ways than just a PathFigure
// - show text warped or unwarped
// - stretch the text along the full path (=> automatic font size) or use the 
//   given FontSize (text might be shorter or longer than the path)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Control.TextOnPath
{
    public class TextOnPath : FrameworkElement
    {
        private delegate Point ProcessPoint(Point pointSrc);

        private Typeface typeface;
        private List<FormattedText> formattedChars = new List<FormattedText>();
        private double pathLength;
        private double textLength;
        private PathGeometry paintGeometry;

        public TextOnPath()
        {
            typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        }

        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(typeof(TextOnPath),
                new FrameworkPropertyMetadata(OnFontPropertyChanged));

        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(typeof(TextOnPath),
                new FrameworkPropertyMetadata(OnFontPropertyChanged));

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty FontStyleProperty =
            TextElement.FontStyleProperty.AddOwner(typeof(TextOnPath),
                new FrameworkPropertyMetadata(OnFontPropertyChanged));

        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        public static readonly DependencyProperty FontWeightProperty =
            TextElement.FontWeightProperty.AddOwner(typeof(TextOnPath),
                new FrameworkPropertyMetadata(OnFontPropertyChanged));

        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public static readonly DependencyProperty FontStretchProperty =
            TextElement.FontStretchProperty.AddOwner(typeof(TextOnPath),
                new FrameworkPropertyMetadata(OnFontPropertyChanged));

        public FontStretch FontStretch
        {
            get => (FontStretch)GetValue(FontStretchProperty);
            set => SetValue(FontStretchProperty, value);
        }

        static void OnFontPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as TextOnPath).OnFontPropertyChanged();
        }

        public static readonly DependencyProperty TextProperty =
            TextBlock.TextProperty.AddOwner(typeof(TextOnPath),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, OnTextPropertyChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        static void OnTextPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as TextOnPath).PrepareText();
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(object), typeof(TextOnPath), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, OnPathPropertyChanged));

        public object Path
        {
            get => GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        private static void OnPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TextOnPath).OnPathPropertyChanged();
        }

        public static readonly DependencyProperty WarpProperty =
            DependencyProperty.Register("Warp", typeof(bool), typeof(TextOnPath), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, OnWarpPropertyChanged));

        public bool Warp
        {
            get => (bool)GetValue(WarpProperty);
            set => SetValue(WarpProperty, value);
        }

        private static void OnWarpPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TextOnPath).CreatePaintGeometry();
        }

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(bool), typeof(TextOnPath), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool Stretch
        {
            get => (bool)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(TextOnPath), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(TextOnPath), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        [Category("Appearance")]
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(TextOnPath), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        private void OnFontPropertyChanged()
        {
            typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            PrepareText();
            InvalidateVisual();
        }

        private void PrepareText()
        {
            formattedChars.Clear();
            textLength = 0;

            if (Text == null)
            {
                return;
            }

            foreach (char ch in Text)
            {
                FormattedText formattedText = new FormattedText(ch.ToString(),
                                                                CultureInfo.CurrentCulture,
                                                                FlowDirection.LeftToRight,
                                                                typeface,
                                                                FontSize,
                                                                Brushes.Black,
                                                                96);

                formattedChars.Add(formattedText);
                textLength += formattedText.WidthIncludingTrailingWhitespace;
            }
        
            CreatePaintGeometry();
        }

        private void OnPathPropertyChanged()
        {
            pathLength = GetPathFigureLength();
            CreatePaintGeometry();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (paintGeometry == null)
            {
                return base.MeasureOverride(availableSize);
            }

            Rect rect = paintGeometry.Bounds;

            return rect.Size;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (paintGeometry == null || pathLength == 0 || textLength == 0)
            {
                return;
            }

            dc.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), paintGeometry);
        }

        private void CreatePaintGeometry()
        {
            if (Path == null)
            {
                return;
            }

            double scalingFactor;

            if (Stretch)
            {
                scalingFactor = pathLength / textLength;
            }
            else
            {
                scalingFactor = 1;
            }

            double progress = 0;

            PathGeometry pathGeometry = GetPathGeometry();

            paintGeometry = new PathGeometry();
            paintGeometry.FillRule = FillRule.Nonzero;

            foreach (FormattedText formText in formattedChars)
            {
                var geometry = formText.BuildGeometry(new Point());
                geometry = formText.BuildGeometry(new Point()).CloneCurrentValue();

                if (Warp)
                {
                    ModifyGeometryPoints(geometry,
                                            delegate (Point pointSrc)
                                            {
                                                double fractionLength = Math.Max(0, Math.Min(1, scalingFactor * (progress + pointSrc.X) / pathLength));
                                                double offsetFromBaseline = scalingFactor * (formText.Baseline - pointSrc.Y);

                                                pathGeometry.GetPointAtFractionLength(fractionLength, out Point pathPoint, out Point pathTangent);
                                                double angle = Math.Atan2(pathTangent.Y, pathTangent.X);

                                                Point pointDst = new Point();
                                                pointDst.X = pathPoint.X + offsetFromBaseline * Math.Sin(angle);
                                                pointDst.Y = pathPoint.Y - offsetFromBaseline * Math.Cos(angle);

                                                return pointDst;
                                            });

                    progress += formText.WidthIncludingTrailingWhitespace;
                }
                else
                {
                    double width = scalingFactor * formText.WidthIncludingTrailingWhitespace;
                    double baseline = scalingFactor * formText.Baseline;
                    var charHalfWidthPercent = width / 2 / pathLength;
                    progress += charHalfWidthPercent;

                    pathGeometry.GetPointAtFractionLength(progress, out Point point, out Point tangent);

                    Matrix matrix = Matrix.Identity;
                    matrix.Scale(scalingFactor, scalingFactor);
                    matrix.RotateAt(Math.Atan2(tangent.Y, tangent.X) * 180 / Math.PI, width / 2, baseline);
                    matrix.Translate(point.X - width / 2, point.Y - baseline);
                    geometry.Transform = new MatrixTransform(matrix);

                    //ProcessGeometryPoints(geometry,
                    //        delegate (Point pointSrc)
                    //        {
                    //            pathGeometry.GetPointAtFractionLength(progress, out Point point, out Point tangent);

                    //            Matrix matrix = Matrix.Identity;
                    //            matrix.Scale(scalingFactor, scalingFactor);
                    //            matrix.RotateAt(Math.Atan2(tangent.Y, tangent.X) * 180 / Math.PI, width / 2, baseline);
                    //            matrix.Translate(point.X - width / 2, point.Y - baseline);

                    //            return pointSrc * matrix;
                    //        });

                    progress += charHalfWidthPercent;
                }

                paintGeometry.AddGeometry(geometry);
            }

            Rect boundsRect = paintGeometry.Bounds;

            //ModifyGeometryPoints(newGeometry,
            //                        delegate (Point pointSrc)
            //                        {
            //                            Point pointDst = new Point();
            //                            pointDst.X = pointSrc.X - boundsRect.Left;
            //                            pointDst.Y = pointSrc.Y - boundsRect.Top;

            //                            return pointDst;
            //                        });

            paintGeometry.Transform = new TranslateTransform(-boundsRect.Left, -boundsRect.Top);
        }

        private PathGeometry GetPathGeometry()
        {
            PathGeometry pathGeometry = new PathGeometry();

            if (Path is PathFigure figure)
            {
                pathGeometry.Figures.Add(figure);
            }
            else
            {
                pathGeometry.AddGeometry(Path as Geometry);
            }

            return pathGeometry;
        }

        private void ModifyGeometryPoints(Geometry geometry, ProcessPoint callback)
        {
            switch (geometry)
            {
                case GeometryGroup group:
                {
                    foreach (var child in group.Children)
                    {
                        ModifyGeometryPoints(child, callback);
                    }

                    break;
                }

                case PathGeometry path:
                {
                    foreach (var figure in path.Figures)
                    {
                        figure.StartPoint = callback(figure.StartPoint);

                        foreach (var segment in figure.Segments)
                        {
                            switch (segment)
                            {
                                case LineSegment line:
                                {
                                    line.Point = callback(line.Point);
                                    break;
                                }

                                case BezierSegment bezier:
                                {
                                    bezier.Point1 = callback(bezier.Point1);
                                    bezier.Point2 = callback(bezier.Point2);
                                    bezier.Point3 = callback(bezier.Point3);
                                    break;
                                }

                                case PolyLineSegment polyLine:
                                {
                                    for (int i = 0; i < polyLine.Points.Count; i++)
                                    {
                                        polyLine.Points[i] = callback(polyLine.Points[i]);
                                    }
                                    break;
                                }

                                case PolyBezierSegment polyBezier:
                                {
                                    for (int i = 0; i < polyBezier.Points.Count; i += 3)
                                    {
                                        polyBezier.Points[i] = callback(polyBezier.Points[i]);
                                        polyBezier.Points[i + 1] = callback(polyBezier.Points[i + 1]);
                                        polyBezier.Points[i + 2] = callback(polyBezier.Points[i + 2]);
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    break;
                }
            }
        }

        private double GetPathFigureLength()
        {
            if (Path == null)
            {
                return 0;
            }

            PathFigure pathFigure;

            if (Path is PathFigure figure)
            {
                pathFigure = figure;
            }
            else
            {
                var pathGeometry = new PathGeometry();
                pathGeometry.AddGeometry(Path as Geometry);

                pathFigure = pathGeometry.Figures[0];
            }

            if (pathFigure == null)
            {
                return 0;
            }

            bool isAlreadyFlattened = true;

            foreach (PathSegment pathSegment in pathFigure.Segments)
            {
                if (!(pathSegment is PolyLineSegment) && !(pathSegment is LineSegment))
                {
                    isAlreadyFlattened = false;
                    break;
                }
            }

            PathFigure pathFigureFlattened = isAlreadyFlattened ? pathFigure : pathFigure.GetFlattenedPathFigure();
            double length = 0;
            Point from = pathFigureFlattened.StartPoint;

            foreach (PathSegment pathSegment in pathFigureFlattened.Segments)
            {
                switch (pathSegment)
                {
                    case LineSegment line:
                    {
                        Point to = line.Point;
                        length += (to - from).Length;
                        from = to;

                        break;
                    }

                    case PolyLineSegment poly:
                    {
                        PointCollection pointCollection = poly.Points;

                        foreach (Point to in pointCollection)
                        {
                            length += (to - from).Length;
                            from = to;
                        }

                        break;
                    }
                }
            }

            return length;
        }
    }
}
