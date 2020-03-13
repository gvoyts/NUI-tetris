using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
//using System.Threading;
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
        private List<GestureDetector> gestureDetectorList = null;
        Point piecePosition;
        //private static Timer timer;
        List<Rectangle> shapeList = new List<Rectangle>();
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private KinectBodyView kinectBodyView = null;
        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            lastTetrisPiecePosition = 0.0;
            lastTetrisPiecePosition2 = 0.0;

            currentTetrisPieceTimer = 0;
            this.gestureDetectorList = new List<GestureDetector>();
           
            //SetTimer();
        }

        #endregion

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("window_loaded");
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                Console.WriteLine("window_loaded 2");

                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                this.bodyFrameReader = this._sensor.BodyFrameSource.OpenReader();
                this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
                //piecePosition = new Point(0, 0);
                this.kinectBodyView = new KinectBodyView(this._sensor);
                GestureResultView result = new GestureResultView(0, false, false, 0.0f);
                GestureDetector detector = new GestureDetector(_sensor, result);
                this.gestureDetectorList.Add(detector);


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

        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                // visualize the new body data
                this.kinectBodyView.UpdateBodyFrame(this.bodies);

                // we may have lost/acquired bodies, so update the corresponding gesture detectors
                /*if (this.bodies != null)
                {
                    // loop through all bodies to see if any of the gesture detectors need to be updated
                    int maxBodies = this._sensor.BodyFrameSource.BodyCount;
                    //for (int i = 0; i < maxBodies; ++i)
                    int i = 0;
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;

                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                       // if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                            // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                            this.gestureDetectorList[i].IsPaused = trackingId == 0;
                        }
                    }
                }*/
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


                                if(rightHandState == "Closed")
                                {
                                    lastTetrisPiecePosition = canvas.DrawMovingTetrisPiece(handRight, currentTetrisPieceTimer, _sensor.CoordinateMapper);
                                }
                                else
                                {
                                    //canvas.DrawPic(lastTetrisPiecePosition, currentTetrisPieceTimer, _sensor.CoordinateMapper);

                                    lastTetrisPiecePosition2 = canvas.DrawStationaryTetrisPiece(lastTetrisPiecePosition, currentTetrisPieceTimer, _sensor.CoordinateMapper);
                                }
                                currentTetrisPieceTimer += 2;
                                //currentTetrisPieceTimer = 1.0;
                                if (currentTetrisPieceTimer > 800)
                                {

                                    currentTetrisPieceTimer = 0;
                                    //create a  matrix/list of all fallen pieces and store their locations
                                    //while loop through list and draw these pieces continuously
                                    //canvas.DrawStationaryTetrisPiece(lastTetrisPiecePosition2, 800, _sensor.CoordinateMapper);
                                    lastTetrisPiecePosition = 500;

                                }

                                //canvas.DrawPic(100, 100, _sensor.CoordinateMapper);

                                //Console.WriteLine("curr timer: " + currentTetrisPieceTimer);



                                

                                ContentControl contentControl = new ContentControl();
                                contentControl.Content = this.gestureDetectorList[0].GestureResultView;
                                canvas.Children.Add(contentControl);


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
