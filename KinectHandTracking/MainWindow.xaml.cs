using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectHandTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Members

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;
        double lastTetrisPiecePosition;
        double lastTetrisPiecePosition2;

        double currentTetrisPieceTimer;
        Point piecePosition;
        //private static Timer timer;
        List<Point> shapePointList = new List<Point>();
        List<string> listOfPieces = new List<string>();
        List<KeyValuePair<Point, string>> finalTetrisBoard = new List<KeyValuePair<Point, string>>();
        int index; 
        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            lastTetrisPiecePosition = 0.0;
            lastTetrisPiecePosition2 = 0.0;

            currentTetrisPieceTimer = 0;
            //piecePosition = new Point(0, 0);
            //SetTimer();
            listOfPieces.Add("tetrisPieceS.png");
            listOfPieces.Add("tetrisPieceT.png");
            listOfPieces.Add("tetrisPieceL.png");
            listOfPieces.Add("tetrisPieceI.png");
            listOfPieces.Add("tetrisPieceO.png");
         
            Random rand = new Random();
            index = rand.Next(listOfPieces.Count);
        }

        #endregion

        #region Event handlers



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {

            
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Find the joints
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];

                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];

                                //Get x,y,z coordinates of right hand
                                float handRightX = body.Joints[JointType.HandRight].Position.X;
                                float handRightY = body.Joints[JointType.HandRight].Position.Y;
                                float handRightZ = body.Joints[JointType.HandRight].Position.Z;

                                // Draw hands and thumbs
                                canvas.DrawHand(handRight, _sensor.CoordinateMapper);
                                canvas.DrawHand(handLeft, _sensor.CoordinateMapper);
                                canvas.DrawThumb(thumbRight, _sensor.CoordinateMapper);
                                canvas.DrawThumb(thumbLeft, _sensor.CoordinateMapper);

                                

                                // Find the hand states
                                string rightHandState = "-";
                                string leftHandState = "-";

                                switch (body.HandRightState)
                                {
                                    case HandState.Open:
                                        rightHandState = "Open";
                                        break;
                                    case HandState.Closed:
                                        rightHandState = "Closed";
                                        break;
                                    case HandState.Lasso:
                                        rightHandState = "Lasso";
                                        break;
                                    case HandState.Unknown:
                                        rightHandState = "Unknown...";
                                        break;
                                    case HandState.NotTracked:
                                        rightHandState = "Not tracked";
                                        break;
                                    default:
                                        break;
                                }

                                switch (body.HandLeftState)
                                {
                                    case HandState.Open:
                                        leftHandState = "Open";
                                        break;
                                    case HandState.Closed:
                                        leftHandState = "Closed";
                                        break;
                                    case HandState.Lasso:
                                        leftHandState = "Lasso";
                                        break;
                                    case HandState.Unknown:
                                        leftHandState = "Unknown...";
                                        break;
                                    case HandState.NotTracked:
                                        leftHandState = "Not tracked";
                                        break;
                                    default:
                                        break;
                                }

                                

                                for (int i = 0; i < finalTetrisBoard.Count(); i++)
                                {

                                    var bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.UriSource = new Uri(finalTetrisBoard[i].Value, UriKind.Relative);
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.EndInit();
                                    var bottomTetrisPiece = new Image
                                    {
                                        Height = 200,
                                        Width = 200,
                                        Source = bitmap
                                    };

                                    
                                    Canvas.SetLeft(bottomTetrisPiece, finalTetrisBoard[i].Key.X - bottomTetrisPiece.Width / 2);
                                    Canvas.SetTop(bottomTetrisPiece, finalTetrisBoard[i].Key.Y - bottomTetrisPiece.Width / 2);
                                    canvas.Children.Add(bottomTetrisPiece);

                      
                                }


                                if(rightHandState == "Closed")
                                {

                                    lastTetrisPiecePosition = canvas.DrawMovingTetrisPiece(handRight, currentTetrisPieceTimer, _sensor.CoordinateMapper, listOfPieces[index]);
                                }
                                else
                                {
                                    //canvas.DrawPic(lastTetrisPiecePosition, currentTetrisPieceTimer, _sensor.CoordinateMapper);

                                    lastTetrisPiecePosition2 = canvas.DrawStationaryTetrisPiece(lastTetrisPiecePosition, currentTetrisPieceTimer, _sensor.CoordinateMapper, listOfPieces[index]);
                                }
                                currentTetrisPieceTimer += 7;
                                //currentTetrisPieceTimer = 1.0;

                                if (currentTetrisPieceTimer > 800)
                                {
                                    Debug.WriteLine("Before adding to list");

                                    //create a  matrix/list of all fallen pieces and store their locations
                                    Point finalPosition = new Point(lastTetrisPiecePosition2, currentTetrisPieceTimer);
                                    KeyValuePair<Point,string> finalPair = new KeyValuePair<Point, string>(finalPosition,listOfPieces[index]);
                                    finalTetrisBoard.Add(finalPair);

                                    //while loop through list and draw these pieces continuously
                                    //canvas.DrawStationaryTetrisPiece(lastTetrisPiecePosition2, 800, _sensor.CoordinateMapper);
                                    lastTetrisPiecePosition = 500;
                                    currentTetrisPieceTimer = 0;
                                    Random rand = new Random();
                                    index = rand.Next(listOfPieces.Count);


                                }

                                //canvas.DrawPic(100, 100, _sensor.CoordinateMapper);

                                Console.WriteLine("curr timer: " + currentTetrisPieceTimer);

                                tblRightHandState.Text = rightHandState;
                                tblLeftHandState.Text = leftHandState;
                                tblRightHandPosition.Text = "x:" + handRightX + " \ny:" + handRightY + "\n z:" + handRightZ; 

                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
