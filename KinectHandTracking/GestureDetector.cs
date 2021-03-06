﻿namespace KinectHandTracking
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;

    /*
    The purpose of the GestureDetector class is to define the methods and objects 
    for pulling frames from the Kinect sensor and using the trained gesture 
    database to gather gesture results.

    @author     Sydney Achinger
    @author     Anushri Marar
    @author     Ganna Voytseshko
    @author     John Woodman
    @version    1.1
    @since      2020-04-17
    */
    public sealed class GestureDetector : IDisposable
    {
        /// <summary> Path to the gesture database that was trained with VGB </summary>
        private readonly string gestureDatabase = @"Database\GatorChomp.gbd";

        private readonly string rotateGestureDatabase = @"Database\RotateBlock.gbd";

        private readonly string dropBlockDatabase = @"Database\DropBlock.gbd";

        private readonly string chompDiscreteGestureName = "Chomp";
        private readonly string chompContGestureName = "ChompProgress";

        private readonly string rotateLeftDiscreteGestureName = "RotateLeft";
        private readonly string rotateRightDiscreteGestureName = "RotateRight";
        private readonly string rotateContGestureName = "RotateProgress";


        private readonly string dropBlockDiscreteGestureName = "DropBlock";
        private readonly string dropBlockContGestureName = "DropBlockProgress";

        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        /*
        This method is responsible for creating the source objects
        to read in sensor data from the Kinect. It also creates 
        three database objects that read in the training data from 
        each of the three gesture databases: GatorChomp, RotateBlock, 
        and DropBlock.
        */
        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView)
        {
            //Console.WriteLine("in gesture detector in");
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            if (gestureResultView == null)
            {
                throw new ArgumentNullException("gestureResultView");
            }

            this.GestureResultView = gestureResultView;
            this.ClosedHandState = false;

            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
            }

            // load all gestures from the gesture database
            using (var database = new VisualGestureBuilderDatabase(this.gestureDatabase))
            {
                this.vgbFrameSource.AddGestures(database.AvailableGestures);
            }
            //load all gestures from the rotate gesture database
            using (var database = new VisualGestureBuilderDatabase(this.rotateGestureDatabase))
            {
                this.vgbFrameSource.AddGestures(database.AvailableGestures);
            }

            using (var database = new VisualGestureBuilderDatabase(this.dropBlockDatabase))
            {
                this.vgbFrameSource.AddGestures(database.AvailableGestures);
            }
        }

        /// <summary> 
        /// Gets the GestureResultView object which stores the detector results for display in the UI 
        /// </summary>
        public GestureResultView GestureResultView { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the body associated with the detector has at least one hand closed
        /// </summary>
        public bool ClosedHandState { get; set; }

        /// <summary>
        /// Gets or sets the body tracking ID associated with the current detector
        /// The tracking ID can change whenever a body comes in/out of scope
        /// </summary>
        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the detector is currently paused
        /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /*
        This method actually retrieves and sets the
        gesture detection results read in from the source 
        objects and based on the database objects.
        */
        public void UpdateGestureData()
        {
            //Console.WriteLine("UPDATEGESTURE data");
            //Console.WriteLine("UPDATEGESTURE data");
            using (var frame = this.vgbFrameReader.CalculateAndAcquireLatestFrame())
            {
                if (frame != null)
                {
                    // get all discrete and continuous gesture results that arrived with the latest frame
                    var discreteResults = frame.DiscreteGestureResults;
                    var continuousResults = frame.ContinuousGestureResults;

                    if (discreteResults != null)
                    {
                        bool chomp = this.GestureResultView.Chomp;
                        float chompProgress = this.GestureResultView.ChompProgress;

                        bool rotateLeft = this.GestureResultView.RotateLeft;
                        bool rotateRight = this.GestureResultView.RotateRight;
                        float rotateProgress = this.GestureResultView.RotateProgress;

                        bool dropBlock = this.GestureResultView.DropBlock;
                        float dropBlockProgress = this.GestureResultView.DropBlockProgress;

                        foreach (var gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    if (gesture.Name.Equals(this.chompDiscreteGestureName))
                                    {
                                        //Console.WriteLine("discre chomp ges");
                                        chomp = result.Detected;
                                    } else if (gesture.Name.Equals(this.rotateLeftDiscreteGestureName))
                                    {
                                        rotateLeft = result.Detected;
                                    } else if (gesture.Name.Equals(this.rotateRightDiscreteGestureName))
                                    {
                                        rotateRight = result.Detected;
                                    } else if (gesture.Name.Equals(this.dropBlockDiscreteGestureName))
                                    {
                                        dropBlock = result.Detected;
                                    }
                                }
                            }

                            if (continuousResults != null)
                            {
                                if (gesture.Name.Equals(this.chompContGestureName) && gesture.GestureType == GestureType.Continuous)
                                {
                                    //Console.WriteLine("in chomp cont gest)");
                                    ContinuousGestureResult result = null;
                                    continuousResults.TryGetValue(gesture, out result);

                                    if (result != null)
                                    {
                                        chompProgress = result.Progress;
                                    }

                                } else if (gesture.Name.Equals(this.rotateContGestureName) && gesture.GestureType == GestureType.Continuous)
                                {
                                    ContinuousGestureResult result = null;
                                    continuousResults.TryGetValue(gesture, out result);

                                    if (result != null)
                                    {
                                        rotateProgress = result.Progress;
                                    }
                                } else if (gesture.Name.Equals(this.dropBlockContGestureName) && gesture.GestureType == GestureType.Continuous)
                                {
                                    ContinuousGestureResult result = null;
                                    continuousResults.TryGetValue(gesture, out result);

                                    if (result != null)
                                    {
                                        dropBlockProgress = result.Progress;
                                    }
                                }
                            }
                        }

                        // Continuous gestures will always report a value while the body is tracked. 
                        // We need to provide context to this value by mapping it to one or more discrete gestures.
                        // For this sample, we will ignore the progress value whenever the user is not performing any of the discrete gestures.
                        if (!chomp)
                        {
                            chompProgress = -1;
                        }

                        if (!rotateRight && !rotateLeft)
                        {
                            rotateProgress = -1;
                        }

                        if (!dropBlock)
                        {
                            dropBlockProgress = -1;
                        }

                        // update the UI with the latest gesture detection results
                        this.GestureResultView.UpdateGestureResult(true, chomp, chompProgress, rotateLeft, rotateRight, rotateProgress, dropBlock, dropBlockProgress);
                    }
                }
            }
        }

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        public void Dispose()
        {
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.Dispose();
                this.vgbFrameReader = null;
            }

            if (this.vgbFrameSource != null)
            {
                this.vgbFrameSource.Dispose();
                this.vgbFrameSource = null;
            }
        }
    }
}