using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace KinectHandTracking
{

    public static class Extensions
    {
        #region Camera

        ///private Bitmap bitmap1;
        public static ObservableCollection<BitmapSource> Bitmaps { get; set; }
        public static TransformedBitmap bitmap = new TransformedBitmap();

        public static ImageSource ToBitmap(this ColorFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            byte[] pixels = new byte[width * height * ((format.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(pixels);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        public static ImageSource ToBitmap(this DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;

            ushort[] pixelData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(pixelData);

            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex)
            {
                ushort depth = pixelData[depthIndex];

                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                pixels[colorIndex++] = intensity; // Blue
                pixels[colorIndex++] = intensity; // Green
                pixels[colorIndex++] = intensity; // Red

                ++colorIndex;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        public static ImageSource ToBitmap(this InfraredFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort[] frameData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(frameData);

            int colorIndex = 0;
            for (int infraredIndex = 0; infraredIndex < frameData.Length; infraredIndex++)
            {
                ushort ir = frameData[infraredIndex];

                byte intensity = (byte)(ir >> 7);

                pixels[colorIndex++] = (byte)(intensity / 1); // Blue
                pixels[colorIndex++] = (byte)(intensity / 1); // Green   
                pixels[colorIndex++] = (byte)(intensity / 0.4); // Red

                colorIndex++;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        #endregion

        #region Body

        public static Point Scale(this Joint joint, CoordinateMapper mapper)
        {
            Point point = new Point();

            ColorSpacePoint colorPoint = mapper.MapCameraPointToColorSpace(joint.Position);
            point.X = float.IsInfinity(colorPoint.X) ? 0.0 : colorPoint.X;
            point.Y = float.IsInfinity(colorPoint.Y) ? 0.0 : colorPoint.Y;

            return point;
        }

        #endregion

        #region Drawing

        public static void DrawSkeleton(this Canvas canvas, Body body, CoordinateMapper mapper)
        {
            if (body == null) return;

            foreach (Joint joint in body.Joints.Values)
            {
                canvas.DrawPoint(joint, mapper);
            }

            canvas.DrawLine(body.Joints[JointType.Head], body.Joints[JointType.Neck], mapper);
            canvas.DrawLine(body.Joints[JointType.Neck], body.Joints[JointType.SpineShoulder], mapper);
            canvas.DrawLine(body.Joints[JointType.SpineShoulder], body.Joints[JointType.ShoulderLeft], mapper);
            canvas.DrawLine(body.Joints[JointType.SpineShoulder], body.Joints[JointType.ShoulderRight], mapper);
            canvas.DrawLine(body.Joints[JointType.SpineShoulder], body.Joints[JointType.SpineMid], mapper);
            canvas.DrawLine(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft], mapper);
            canvas.DrawLine(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ElbowRight], mapper);
            canvas.DrawLine(body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft], mapper);
            canvas.DrawLine(body.Joints[JointType.ElbowRight], body.Joints[JointType.WristRight], mapper);
            canvas.DrawLine(body.Joints[JointType.WristLeft], body.Joints[JointType.HandLeft], mapper);
            canvas.DrawLine(body.Joints[JointType.WristRight], body.Joints[JointType.HandRight], mapper);
            canvas.DrawLine(body.Joints[JointType.HandLeft], body.Joints[JointType.HandTipLeft], mapper);
            canvas.DrawLine(body.Joints[JointType.HandRight], body.Joints[JointType.HandTipRight], mapper);
            canvas.DrawLine(body.Joints[JointType.HandTipLeft], body.Joints[JointType.ThumbLeft], mapper);
            canvas.DrawLine(body.Joints[JointType.HandTipRight], body.Joints[JointType.ThumbRight], mapper);
            canvas.DrawLine(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase], mapper);
            canvas.DrawLine(body.Joints[JointType.SpineBase], body.Joints[JointType.HipLeft], mapper);
            canvas.DrawLine(body.Joints[JointType.SpineBase], body.Joints[JointType.HipRight], mapper);
            canvas.DrawLine(body.Joints[JointType.HipLeft], body.Joints[JointType.KneeLeft], mapper);
            canvas.DrawLine(body.Joints[JointType.HipRight], body.Joints[JointType.KneeRight], mapper);
            canvas.DrawLine(body.Joints[JointType.KneeLeft], body.Joints[JointType.AnkleLeft], mapper);
            canvas.DrawLine(body.Joints[JointType.KneeRight], body.Joints[JointType.AnkleRight], mapper);
            canvas.DrawLine(body.Joints[JointType.AnkleLeft], body.Joints[JointType.FootLeft], mapper);
            canvas.DrawLine(body.Joints[JointType.AnkleRight], body.Joints[JointType.FootRight], mapper);
        }

        public static void DrawPoint(this Canvas canvas, Joint joint, CoordinateMapper mapper)
        {
            if (joint.TrackingState == TrackingState.NotTracked) return;

            Point point = joint.Scale(mapper);

            Ellipse ellipse = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(Colors.LightBlue)
            };

            Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

            canvas.Children.Add(ellipse);
        }

        public static void DrawHand(this Canvas canvas, Joint hand, CoordinateMapper mapper)
        {
            if (hand.TrackingState == TrackingState.NotTracked) return;

            Point point = hand.Scale(mapper);

            Ellipse ellipse = new Ellipse
            {
                Width = 100,
                Height = 100,
                Stroke = new SolidColorBrush(Colors.LightBlue),
                StrokeThickness = 4
            };

            Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

            canvas.Children.Add(ellipse);
        }

        public static void DrawThumb(this Canvas canvas, Joint thumb, CoordinateMapper mapper)
        {
            if (thumb.TrackingState == TrackingState.NotTracked) return;

            Point point = thumb.Scale(mapper);

            Ellipse ellipse = new Ellipse
            {
                Width = 40,
                Height = 40,
                Fill = new SolidColorBrush(Colors.LightBlue),
                Opacity = 0.7
            };

            Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

            canvas.Children.Add(ellipse);
        }

        public static void DrawLine(this Canvas canvas, Joint first, Joint second, CoordinateMapper mapper)
        {
            if (first.TrackingState == TrackingState.NotTracked || second.TrackingState == TrackingState.NotTracked) return;

            Point firstPoint = first.Scale(mapper);
            Point secondPoint = second.Scale(mapper);

            Line line = new Line
            {
                X1 = firstPoint.X,
                Y1 = firstPoint.Y,
                X2 = secondPoint.X,
                Y2 = secondPoint.Y,
                StrokeThickness = 8,
                Stroke = new SolidColorBrush(Colors.LightBlue)
            };

            canvas.Children.Add(line);
        }

        public static double DrawMovingRectangle(this Canvas canvas, Joint hand, double positionY, CoordinateMapper mapper)
        {
            if (hand.TrackingState == TrackingState.NotTracked) return 0.0;

            Point point = hand.Scale(mapper);
            Point point2 = new Point(0, positionY);
            Console.WriteLine("in moving: " + point.X + " and: " + point.Y);


            Rectangle tetrisPiece = new Rectangle
            {
                Width = 100,
                Height = 100,
                Stroke = new SolidColorBrush(Colors.Purple),
                StrokeThickness = 100
            };

            Canvas.SetLeft(tetrisPiece, point.X - tetrisPiece.Width / 2);
            Canvas.SetTop(tetrisPiece, point2.Y - tetrisPiece.Width / 2);

            canvas.Children.Add(tetrisPiece);


            return (point.X - tetrisPiece.Width / 2);
        }

        public static double DrawStationaryRectangle(this Canvas canvas, double position, double positionY, CoordinateMapper mapper)
        {
            //if (hand.TrackingState == TrackingState.NotTracked) return;
            Console.WriteLine("in stat: x: " + position);

            Point point = new Point(position, positionY);
            Console.WriteLine("in stationary: " + point.X + " and: " + point.Y);


            Rectangle tetrisPiece = new Rectangle
            {
                Width = 100,
                Height = 100,
                Stroke = new SolidColorBrush(Colors.Purple),
                StrokeThickness = 100
            };

            Canvas.SetLeft(tetrisPiece, point.X - tetrisPiece.Width / 2);
            Canvas.SetTop(tetrisPiece, point.Y - tetrisPiece.Width / 2);

            canvas.Children.Add(tetrisPiece);

            return (point.X - tetrisPiece.Width / 2);
        }

        /*public static double DrawMovingTetrisPiece(this Canvas canvas, Joint hand, double positionY, CoordinateMapper mapper, String pieceName)
        {
            if (hand.TrackingState == TrackingState.NotTracked) return 0.0;

            Point point = hand.Scale(mapper);
            Point point2 = new Point(0, positionY);


            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(pieceName, UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            var tetrisPiece = new Image
            {
                Height = 200,
                Width = 200,
                Source = bitmap
            };

            Canvas.SetLeft(tetrisPiece, point.X - tetrisPiece.Width / 2);
            Canvas.SetTop(tetrisPiece, point2.Y - tetrisPiece.Width / 2);

            canvas.Children.Add(tetrisPiece);


            return (point.X - tetrisPiece.Width / 2);
        }

        public static void setBitmap(string pieceName)
        {
            var bit = new BitmapImage();
            bit.BeginInit();
            bit.UriSource = new Uri(pieceName, UriKind.Relative);
            bit.CacheOption = BitmapCacheOption.OnLoad;
            bit.EndInit();

            bitmap.Source = bit;
        }

        public static double DrawStationaryTetrisPiece(this Canvas canvas, double position, double positionY, CoordinateMapper mapper, String pieceName, bool rotateLeftBool, bool rotateRightBool)
        {
            //pieceName = "tetrispieces.png";
            setBitmap(pieceName);
            Point point = new Point(position, positionY);
            TransformedBitmap tb = new TransformedBitmap();

            *//*var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(pieceName, UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();*//*

            var bit = new BitmapImage();
            bit.BeginInit();
            bit.UriSource = new Uri(pieceName, UriKind.Relative);
            bit.CacheOption = BitmapCacheOption.OnLoad;
            bit.EndInit();


            //Bitmaps[0].CreateOptions = BitmapCreateOptions.None;
            //Uri u = new Uri(pieceName, UriKind.Relative);
            //Uri u = new Uri(pieceName, UriKind.Relative);
            //StreamResourceInfo resourceInfo = Application.GetResourceStream(u);
            *//*BitmapImage img = new BitmapImage();
            //img.SetSource(resourceInfo.Stream);
            img.BeginInit();
            img.UriSource = u;
            img.CacheOption = BitmapCacheOption.OnLoad;
            //img.CreateOptions = BitmapCreateOptions.None;
            img.EndInit();

            Bitmaps.Add(img);*//*

            var tetrisPiece = new Image
            {
                Height = 200,
                Width = 200,
                Source = bitmap.Source
            };


            if (rotateLeftBool)
            {
               // Bitmaps.Add(new TransformedBitmap(Bitmaps[0], new RotateTransform(90)));

                Console.WriteLine("rotate left bool is true");
                tb.BeginInit();
                //tb.Source = bitmap.Source;
                tb.Source = bit;
                RotateTransform transform = new RotateTransform(90);
                tb.Transform = transform;
                tb.EndInit();
                bitmap = tb;

                //tetrisPiece.Source = tb;
            }

            if (rotateRightBool)
            {
                Console.WriteLine("rotate right bool is true");
                //Bitmaps.Add(new TransformedBitmap(Bitmaps[0], new RotateTransform(270)));


                tb.BeginInit();
                tb.Source = bitmap.Source;
                RotateTransform transform = new RotateTransform(270);
                tb.Transform = transform;
                tb.EndInit();
                bitmap = tb;
                //tetrisPiece.Source = tb;
            }

            Canvas.SetLeft(tetrisPiece, point.X - tetrisPiece.Width / 2);
            Canvas.SetTop(tetrisPiece, point.Y - tetrisPiece.Height / 2);

            canvas.Children.Add(tetrisPiece);

            *//*Image img = new Image
            {
                Width = 20,
                Height = 20,
                Source = new BitmapImage(new Uri("tetrisPiece2.png", UriKind.Relative)),
            };

            Canvas.SetLeft(img, point.X - img.Width / 2);
            Canvas.SetTop(img, point.Y - img.Height / 2);

            canvas.Children.Add(img);*//*

            return (point.X - tetrisPiece.Width / 2);
        }
*/

        public static double DrawMovingTetrisPiece(this Canvas canvas, Joint hand, double positionY, CoordinateMapper mapper, String pieceName, int rotationPosition)
        {
            if (hand.TrackingState == TrackingState.NotTracked) return 0.0;

            Point point = hand.Scale(mapper);
            Point point2 = new Point(0, positionY);
            Image tetrisPiece;


            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(pieceName, UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            /*var tetrisPiece = new Image
            {
                Height = 200,
                Width = 200,
                Source = bitmap
            };*/
            if (rotationPosition == 1)
            {
                TransformedBitmap tb = new TransformedBitmap();
                tb.BeginInit();
                tb.Source = bitmap;
                RotateTransform transform = new RotateTransform(270);
                tb.Transform = transform;
                tb.EndInit();
                tetrisPiece = new Image
                {
                    Height = 200,
                    Width = 200,
                    Source = tb
                };
            }
            else if (rotationPosition == 2)
            {
                TransformedBitmap tb = new TransformedBitmap();
                tb.BeginInit();
                tb.Source = bitmap;
                RotateTransform transform = new RotateTransform(180);
                tb.Transform = transform;
                tb.EndInit();
                tetrisPiece = new Image
                {
                    Height = 200,
                    Width = 200,
                    Source = tb
                };
            }
            else if (rotationPosition == 3)
            {
                TransformedBitmap tb = new TransformedBitmap();
                tb.BeginInit();
                tb.Source = bitmap;
                RotateTransform transform = new RotateTransform(90);
                tb.Transform = transform;
                tb.EndInit();
                tetrisPiece = new Image
                {
                    Height = 200,
                    Width = 200,
                    Source = tb
                };
            }
            else
            {

                tetrisPiece = new Image
                {
                    Height = 200,
                    Width = 200,
                    Source = bitmap
                };


            }

            Canvas.SetLeft(tetrisPiece, point.X - tetrisPiece.Width / 2);
            Canvas.SetTop(tetrisPiece, point2.Y - tetrisPiece.Width / 2);

            canvas.Children.Add(tetrisPiece);


            return (point.X - tetrisPiece.Width / 2);
        }

        public static double DrawStationaryTetrisPiece(this Canvas canvas, double position, double positionY, CoordinateMapper mapper, String pieceName, int rotationPosition)
        {

            Console.WriteLine("rotate position: " + rotationPosition);

            Point point = new Point(position, positionY);

            Image tetrisPiece;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(pieceName, UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            
            if (rotationPosition == 1)
            {
                TransformedBitmap tb = new TransformedBitmap();
                tb.BeginInit();
                tb.Source = bitmap;
                RotateTransform transform = new RotateTransform(270);
                tb.Transform = transform;
                tb.EndInit();
                tetrisPiece = new Image
                {
                    Height = 200,
                    Width = 200,
                    Source = tb
                };
            }
            else if(rotationPosition == 2)
            {
                TransformedBitmap tb = new TransformedBitmap();
                tb.BeginInit();
                tb.Source = bitmap;
                RotateTransform transform = new RotateTransform(180);
                tb.Transform = transform;
                tb.EndInit();
                tetrisPiece = new Image
                {
                    Height = 200,
                    Width = 200,
                    Source = tb
                };
            }
            else if (rotationPosition == 3)
            {
                TransformedBitmap tb = new TransformedBitmap();
                tb.BeginInit();
                tb.Source = bitmap;
                RotateTransform transform = new RotateTransform(90);
                tb.Transform = transform;
                tb.EndInit();
                tetrisPiece = new Image
                {
                    Height = 200,
                    Width = 200,
                    Source = tb
                };
            }
            else
            {

                tetrisPiece = new Image
                {
                    Height = 200,
                    Width = 200,
                    Source = bitmap
                };


            }



            Canvas.SetLeft(tetrisPiece, point.X - tetrisPiece.Width / 2);
            Canvas.SetTop(tetrisPiece, point.Y - tetrisPiece.Height / 2);

            canvas.Children.Add(tetrisPiece);

            /*Image img = new Image
            {
                Width = 20,
                Height = 20,
                Source = new BitmapImage(new Uri("tetrisPiece2.png", UriKind.Relative)),
            };
            Canvas.SetLeft(img, point.X - img.Width / 2);
            Canvas.SetTop(img, point.Y - img.Height / 2);
            canvas.Children.Add(img);*/

            return (point.X - tetrisPiece.Width / 2);
        }
        public static void startGame()
        {
            Console.WriteLine("!!!!!!!!!!!!GAME IS STARTING!!!!!!!!!!!!!!!!!!!!");
            //if count is between 1 and 3, tell user to do bigger chomp
        }

        public static void rotatePieceLeft(System.Object sender, System.EventArgs e)
        {
            Console.WriteLine("rotating piece LEFT");
            //if (bitmap1 != null)
            {

            }
        }


        #endregion
    
    }
}
