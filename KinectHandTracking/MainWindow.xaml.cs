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
using System.Windows.Threading;
using System.Media;

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
        static Boolean isStartGame;

        //private int rLeftCounter = 0;
        //private int rRightCounter = 0;
        private List<float> rLeftProgressArray = new List<float>();
        private List<float> rRightProgressArray = new List<float>();
        private List<float> dropBlockProgressArray = new List<float>();

        private int rotateRightAnomalyCount = 0;
        private int rotateLeftAnomalyCount = 0;
        private int dropBlockAnomalyCount = 0;

        private bool rotateLeftBool = false;
        private bool rotateRightBool = false;
        private bool dropBlockBool = false;
        private int rotationPosition = 0;

        double currentTetrisPieceTimer;
        //Point piecePosition;
        //List<Point> shapePointList = new List<Point>();
        List<string> listOfPieces = new List<string>();
        public List<DroppedPiece> finalTetrisBoard = new List<DroppedPiece>();
        int index;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;

        /// <summary> Current kinect status text to display </summary>
        private string statusText = null;
        private int activeBodyIndex = 0;

        /// <summary> Reader for body frames </summary>

        /// <summary> KinectBodyView object which handles drawing the active body to a view box in the UI </summary>
        private KinectBodyView kinectBodyView = null;

        /// <summary> Gesture detector which will be tied to the active body (closest skeleton to the sensor) </summary>
        private GestureDetector gestureDetector = null;

        /// <summary> GestureResultView for displaying gesture results associated with the tracked person in the UI </summary>
        private GestureResultView gestureResultView = null;
        private DispatcherTimer dispatcherTimer = null;
        //private DispatcherTimer dispatcherTimer = null;
        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            lastTetrisPiecePosition = 400.0;
            lastTetrisPiecePosition2 = 400.0;
            isStartGame = false;
            currentTetrisPieceTimer = 0;

            listOfPieces.Add("tetrisPieceS.png");
            listOfPieces.Add("tetrisPieceT.png");
            listOfPieces.Add("tetrisPieceL.png");
            listOfPieces.Add("tetrisPieceI.png");
            listOfPieces.Add("tetrisPieceO.png");
         
            Random rand = new Random();
            index = rand.Next(listOfPieces.Count);

            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                this.bodyFrameReader = this._sensor.BodyFrameSource.OpenReader();

                // initialize the BodyViewer object for displaying tracked bodies in the UI
                this.kinectBodyView = new KinectBodyView(this._sensor);

                // initialize the SpaceView object
                //this.spaceView = new SpaceView(this.spaceGrid, this.spaceImage);

                // initialize the GestureDetector object
                this.gestureResultView = new GestureResultView(false, false, -1.0f, false, false, -1.0f, false, -1.0f);
                this.gestureDetector = new GestureDetector(this._sensor, this.gestureResultView);
            }
        }

        #endregion

        #region Event handlers


        public static void startGame()
        {
            //gameMessage.Text = "The Game has begun.";
            isStartGame = true;
            //SoundPlayer simpleSound = new SoundPlayer(@"C:\Users\gvoyts\Documents\Spring2020\CEN4725\Group Project\tetris-gameboy-02.wav");
            SoundPlayer simpleSound = new SoundPlayer(@"tetris-gameboy-02.wav");

            //Play music on loop
            simpleSound.PlayLooping();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            CompositionTarget.Rendering += this.DispatcherTimer_Tick;

            // set the game timer to run at 60fps
            this.dispatcherTimer = new DispatcherTimer();
            this.dispatcherTimer.Tick += this.DispatcherTimer_Tick;
            this.dispatcherTimer.Interval = TimeSpan.FromSeconds(1 / 60);
            this.dispatcherTimer.Start();
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

        private int GetActiveBodyIndex()
        {
            int activeBodyIndex = -1;
            int maxBodies = this._sensor.BodyFrameSource.BodyCount;

            for (int i = 0; i < maxBodies; ++i)
            {
                // find the first tracked body and verify it has hands tracking enabled (by default, Kinect will only track handstate for 2 people)
                if (this.bodies[i].IsTracked && (this.bodies[i].HandRightState != HandState.NotTracked || this.bodies[i].HandLeftState != HandState.NotTracked))
                {
                    activeBodyIndex = i;
                    break;
                }
            }

            return activeBodyIndex;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.UpdateKinectFrameData();
            
        }
        /// <summary>
        /// Retrieves the latest body frame data from the sensor and updates the associated gesture detector object
        /// </summary>
        private void UpdateKinectFrameData()
        {
            bool dataReceived = false;

            using (var bodyFrame = this.bodyFrameReader.AcquireLatestFrame())
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

                    if (!this.bodies[this.activeBodyIndex].IsTracked)
                    {
                        // we lost tracking of the active body, so update to the first tracked body in the array
                        int bodyIndex = this.GetActiveBodyIndex();

                        if (bodyIndex > 0)
                        {
                            this.activeBodyIndex = bodyIndex;
                        }
                    }

                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                Body activeBody = this.bodies[this.activeBodyIndex];

                // visualize the new body data
                this.kinectBodyView.UpdateBodyData(activeBody);

                // visualize the new gesture data
                if (activeBody.TrackingId != this.gestureDetector.TrackingId)
                {
                    // if the tracking ID changed, update the detector with the new value
                    this.gestureDetector.TrackingId = activeBody.TrackingId;
                }

                if (this.gestureDetector.TrackingId == 0)
                {
                    // the active body is not tracked, pause the detector and update the UI
                    this.gestureDetector.IsPaused = true;
                    this.gestureDetector.ClosedHandState = false;
                    this.gestureResultView.UpdateGestureResult(false, false, -1.0f, false, false, -1.0f, false, -1.0f);
                }
                else
                {
                    // the active body is tracked, unpause the detector
                    this.gestureDetector.IsPaused = false;

                    // steering gestures are only valid when the active body's hand state is 'closed'
                    // update the detector with the latest hand state
                    if (activeBody.HandLeftState == HandState.Closed || activeBody.HandRightState == HandState.Closed)
                    {
                        this.gestureDetector.ClosedHandState = true;
                    }
                    else
                    {
                        this.gestureDetector.ClosedHandState = false;
                    }

                    // get the latest gesture frame from the sensor and updates the UI with the results
                    this.gestureDetector.UpdateGestureData();
                }
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

                                // Draw hands and thumbs
                                canvas.DrawHand(handRight, _sensor.CoordinateMapper);
                                canvas.DrawHand(handLeft, _sensor.CoordinateMapper);
                                canvas.DrawThumb(thumbRight, _sensor.CoordinateMapper);
                                canvas.DrawThumb(thumbLeft, _sensor.CoordinateMapper);

                                // Find the hand states
                                string rightHandState = "-";
                                //string leftHandState = "-";

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

                                canvas.DrawDroppedPieces(finalTetrisBoard);

                                //Adding rotate feature
                                if (rightHandState != "Closed")
                                {
                                    //left rotation
                                    if (this.gestureResultView.RotateLeft)
                                    {
                                        rLeftProgressArray.Add(this.gestureResultView.RotateProgress);
                                        Console.WriteLine("Progress for LEFT: " + this.gestureResultView.RotateProgress);
                                        
                                        List<float> tempList = new List<float>();

                                        for (int num = 0; num < rLeftProgressArray.Count - 1; num++)
                                        {
                                            if (rLeftProgressArray[num] < rLeftProgressArray[num + 1])
                                            {
                                                if (rotateLeftAnomalyCount > 3)
                                                {
                                                    //trend = false;
                                                    tempList.Clear();
                                                    rotateLeftAnomalyCount = 0;
                                                }
                                                rotateLeftAnomalyCount++;

                                            }
                                            else
                                            {
                                                tempList.Add(rLeftProgressArray[num]);
                                                //bool midWay = false;
                                                //bool midWay = false;
                                                foreach (float i in tempList)
                                                {
                                                    if (i >= 0.2 && i <= 0.3)
                                                    {
                                                        //midWay = true;
                                                    }
                                                }
                                                if (tempList[0] >= 0.45 &&
                                                    tempList[0] <= 0.6 &&
                                                    tempList[tempList.Count - 1] >= 0.0 &&
                                                    tempList[tempList.Count - 1] <= 0.28 && tempList.Count >= 8)
                                                {
                                                    //Console.WriteLine("###############################################################LEFT#################################################");
                                                    /*Console.WriteLine("TEMP LIST FOR WINNER: ");
                                                    foreach (float i in tempList)
                                                    {
                                                        Console.WriteLine(i);
                                                    }
                                                    Console.WriteLine("END");*/
                                                    //rotateLeftBool = true;
                                                    if (rotationPosition == 0)
                                                    {
                                                        rotationPosition = 3;
                                                    }
                                                    else
                                                    {
                                                        rotationPosition--;
                                                    }
                                                    tempList.Clear();
                                                    rLeftProgressArray.Clear();
                                                    break;
                                                }
                                            }
                                        }

                                    }
                                    //if right rotation
                                    if (this.gestureResultView.RotateRight)
                                    {
                                        rRightProgressArray.Add(this.gestureResultView.RotateProgress);

                                        Console.WriteLine("Progress for RIGHT: " + this.gestureResultView.RotateProgress);

                                        //bool trend = true;
                                        List<float> tempList2 = new List<float>();

                                        for (int num = 0; num < rRightProgressArray.Count - 1; num++)
                                        {
                                            if (rRightProgressArray[num] > rRightProgressArray[num + 1])
                                            {
                                                if (rotateRightAnomalyCount > 3)
                                                {
                                                    //trend = false;
                                                    tempList2.Clear();
                                                    rotateRightAnomalyCount = 0;
                                                }
                                                rotateRightAnomalyCount++;
                                            }
                                            else
                                            {
                                                tempList2.Add(rRightProgressArray[num]);
                                                //bool midWay = false;
                                                //bool midWay = true;
                                                foreach (float i in tempList2)
                                                {
                                                    //  if (i >= 0.7 && i <= 0.8)
                                                    {
                                                        //midWay = true;
                                                    }
                                                }
                                                if (tempList2[0] >= 0.45 &&
                                                    tempList2[0] <= 0.6 &&
                                                    tempList2[tempList2.Count - 1] >= 0.75 &&
                                                    tempList2[tempList2.Count - 1] <= 1.0 && tempList2.Count >= 8)
                                                {
                                                    Console.WriteLine("--------------------------------------------RIGHT----------------------------------------");
                                                    /*Console.WriteLine("TEMP LIST FOR WINNER: ");
                                                    foreach (float i in tempList2)
                                                    {
                                                        Console.WriteLine(i);
                                                    }
                                                    Console.WriteLine("END");*/
                                                    // rotateRightBool = true;
                                                    if (rotationPosition == 3)
                                                    {
                                                        rotationPosition = 0;
                                                    }
                                                    else
                                                    {
                                                        rotationPosition++;
                                                    }


                                                    tempList2.Clear();
                                                    rRightProgressArray.Clear();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                //drag block with closed hand
                                else if (rightHandState == "Closed")
                                {
                                    if (this.gestureResultView.DropBlock)
                                    {
                                        //Console.WriteLine("DROP BLOCK HERE: " + this.gestureResultView.DropBlockProgress);
                                        dropBlockProgressArray.Add(this.gestureResultView.DropBlockProgress);


                                        //bool trend = true;
                                        List<float> tempList3 = new List<float>();

                                        for (int num = 0; num < dropBlockProgressArray.Count - 1; num++)
                                        {
                                            if (dropBlockProgressArray[num] > dropBlockProgressArray[num + 1])
                                            {
                                                if (dropBlockAnomalyCount > 6)
                                                {
                                                    //trend = false;
                                                    tempList3.Clear();
                                                    dropBlockAnomalyCount = 0;
                                                }
                                                dropBlockAnomalyCount++;
                                            }
                                            else
                                            {
                                                tempList3.Add(dropBlockProgressArray[num]);
                                                //bool midWay = false;
                                               // bool midWay = true;
                                                foreach (float i in tempList3)
                                                {
                                                    //  if (i >= 0.7 && i <= 0.8)
                                                    {
                                                        //midWay = true;
                                                    }
                                                }
                                                if (tempList3[0] >= 0.0 &&
                                                    tempList3[0] <= 0.35 &&
                                                    tempList3[tempList3.Count - 1] >= 0.55 &&
                                                    tempList3[tempList3.Count - 1] <= 1.0 && tempList3.Count >= 5)
                                                {
                                                    Console.WriteLine("~~~~~~~~~~~~~~~~~~~~drop~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

                                                    dropBlockBool = true;


                                                    tempList3.Clear();
                                                    dropBlockProgressArray.Clear();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                //if chomp is detected, start game
                                if (isStartGame)
                                {

                                    gameMessage.Text = "Good Luck Playing Game!";

                                    //FIXME: borders
                                    Rectangle borderLeft = new Rectangle
                                    {
                                        Width = 200,
                                        Height = 1500,
                                        Stroke = new SolidColorBrush(Colors.Blue),
                                        StrokeThickness = 1000,
                                        Opacity = 0.75
                                    };
                                    Canvas.SetLeft(borderLeft, 0);
                                    Canvas.SetTop(borderLeft, 0);
                                    canvas.Children.Add(borderLeft);

                                    Rectangle borderRight = new Rectangle
                                    {
                                        Width = 600,
                                        Height = 1500,
                                        Stroke = new SolidColorBrush(Colors.Blue),
                                        StrokeThickness = 1000,
                                        Opacity = 0.75
                                    };
                                    Canvas.SetLeft(borderRight, 1600);
                                    Canvas.SetTop(borderRight, 0);
                                    canvas.Children.Add(borderRight);


                                    if (rightHandState == "Closed")
                                    {

                                        lastTetrisPiecePosition = canvas.DrawMovingTetrisPiece(handRight, currentTetrisPieceTimer, _sensor.CoordinateMapper, listOfPieces[index], rotationPosition);
                                    
                                    }
                                    else
                                    {
                                        //speed up the block if dropped
                                        if (dropBlockBool)
                                        {
                                            currentTetrisPieceTimer += 15;
                                        }
                                        
                                   
                                        lastTetrisPiecePosition2 = canvas.DrawStationaryTetrisPiece(lastTetrisPiecePosition, currentTetrisPieceTimer, _sensor.CoordinateMapper, listOfPieces[index], rotationPosition);
                                        
                                         rotateLeftBool = false;
                                        rotateRightBool = false;
                                    }
                                    currentTetrisPieceTimer += 4; //7 //2
                                    //currentTetrisPieceTimer = 1.0;

                                    if (currentTetrisPieceTimer > 950)
                                    {
                                        Debug.WriteLine("Before adding to list");

                                        //create a  matrix/list of all fallen pieces and store their locations
                                        Point finalPosition = new Point(lastTetrisPiecePosition2, currentTetrisPieceTimer);
                                        DroppedPiece droppedPiece = new DroppedPiece(finalPosition, listOfPieces[index], rotationPosition);
                                        finalTetrisBoard.Add(droppedPiece);

                                        //while loop through list and draw these pieces continuously
                                        //canvas.DrawStationaryTetrisPiece(lastTetrisPiecePosition2, 800, _sensor.CoordinateMapper);
                                        lastTetrisPiecePosition = 500;
                                        currentTetrisPieceTimer = 0;
                                        Random rand = new Random();
                                        index = rand.Next(listOfPieces.Count);
                                        rotationPosition = 0;
                                        dropBlockBool = false;

                                    }
                                }
                                else
                                {
                                    Rectangle startScreen = new Rectangle
                                    {
                                        Width = 3000,
                                        Height = 1500,
                                        Stroke = new SolidColorBrush(Colors.Purple),
                                        StrokeThickness = 1000,
                                        Opacity = 0.5
                                    };
                                    canvas.Children.Add(startScreen);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
