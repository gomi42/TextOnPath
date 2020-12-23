# TextOnPath
WPF control that displays text along a given path

![intro](/Readme.PNG)

# Features
* define the path in several different way like PathGeometry, Geometry, PathFigure, Line, Rectangle, Ellipse
* show text warped or unwarped
* stretch the text along the full path (=> automatic font size) or use the given FontSize (text might be shorter or longer than the path)
* set fill and stroke independently
* supports gradients of any kind that is applied on the result as a single entity (not each character individually)

# Usage
Define paths in your resources...
```xaml
        <Grid.Resources>
            <PathGeometry x:Key="path1"
                          Figures="M 50,50 C 100,0 200,100 250,50" />
            <Geometry x:Key="path2">M 50,50 C 100,0 200,100 250,50</Geometry>
            <LineGeometry x:Key="path3"
                          StartPoint="100,100"
                          EndPoint="400,200" />
            <RectangleGeometry x:Key="path4"
                               Rect="100,100,130,130" />
            <EllipseGeometry x:Key="path5"
                             Center="120,130"
                             RadiusX="100"
                             RadiusY="30" />
            <PathFigure x:Key="path6"
                        IsClosed="False"
                        StartPoint="100,100">
                <BezierSegment Point1="200,150"
                               Point2="350,0"
                               Point3="400,150" />
            </PathFigure>
            <PathGeometry x:Key="path7"
                          Figures="M10,90 Q90,90 90,45 Q90,10 50,10 Q10,10 10,40 Q10,70 45,70 Q70,70 75,50">
                <Geometry.Transform>
                    <ScaleTransform ScaleX="3" ScaleY="3" />
                </Geometry.Transform>
            </PathGeometry>
        </Grid.Resources>
```
...and use them
```xaml
        <textonpath:TextOnPath Margin="22,29,0,0"
                               HorizontalAlignment="Left"
                               VerticalAlignment="Top"
                               FontSize="20"
                               FontWeight="Black"
                               Path="{StaticResource path7}"
                               Stretch="True"
                               StrokeThickness="3"
                               Text="Quick brown fox jumps over the lazy dog."
                               Warp="True">
            <textonpath:TextOnPath.Stroke>
                <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                    <GradientStop Offset="0.085" Color="#FF02B9A0" />
                    <GradientStop Offset="0.892" Color="#FFD4C100" />
                    <GradientStop Offset="0.499" Color="#FFB65959" />
                </LinearGradientBrush>
            </textonpath:TextOnPath.Stroke>
        </textonpath:TextOnPath>
```

or directy assign the path (again any type of Geometry or PathFigure will work) to the Path property of the TextOnPath control 
```xaml
        <textonpath:TextOnPath Margin="445,250,0,0"
                               HorizontalAlignment="Left"
                               VerticalAlignment="Top"
                               FontSize="40"
                               FontWeight="Bold"
                               Stretch="True"
                               Stroke="#FF830483"
                               StrokeThickness="0"
                               Text="Hello colorful path"
                               Warp="True">
            <textonpath:TextOnPath.Path>
                <PathFigure IsClosed="False"
                            StartPoint="0,0">
                    <BezierSegment Point1="20,30"
                                   Point2="70,30"
                                   Point3="100,30" />
                    <BezierSegment Point1="120,30"
                                   Point2="190,30"
                                   Point3="200,0" />
                </PathFigure>
            </textonpath:TextOnPath.Path>
            <textonpath:TextOnPath.Fill>
                <LinearGradientBrush StartPoint="0,0" EndPoint="1,0">
                    <GradientStop Offset="1" Color="#FF7D9BFF" />
                    <GradientStop Offset="0" Color="#FFF17D7D" />
                    <GradientStop Offset="0.519" Color="#FFBEC08C" />
                </LinearGradientBrush>
            </textonpath:TextOnPath.Fill>
        </textonpath:TextOnPath>
```

# Notes
* because of its simplicity the TextOnPath control is part of the test application
* this control is based on Charles Petzold's work, ideas and code
