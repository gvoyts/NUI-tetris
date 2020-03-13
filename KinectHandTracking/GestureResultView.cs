namespace KinectHandTracking
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;
    //using Microsoft.Samples.Kinect.ContinuousGestureBasics.Common;

    public sealed class GestureResultView 
    {
        private bool chomp = false;
        private float chompProgress = 0.0f;

        /// <summary> True, if the body is currently being tracked </summary>
        private bool isTracked = false;

        public GestureResultView(bool isTracked, bool left, bool right, bool straight, float progress, bool chomp, float chompProgress)
        {
            this.IsTracked = isTracked;
            this.Chomp = chomp;
            this.ChompProgress = chompProgress;
        }

        /// <summary> 
        /// Gets a value indicating whether or not the body associated with the gesture detector is currently being tracked 
        /// </summary>
        public bool IsTracked
        {
            get
            {
                return this.isTracked;
            }

            private set
            {
                this.isTracked = value;
            }
        }

        public bool Chomp
        {
            get
            {
                return this.chomp;
            }

            private set
            {
                this.chomp = value;
            }
        }

        public float ChompProgress
        {
            get
            {
                return this.chompProgress;
            }

            private set
            {
                this.chompProgress = value;
            }
        }

        public void UpdateGestureResult(bool isBodyTrackingIdValid, bool chomp, float chompProgress)
        {

            this.IsTracked = isBodyTrackingIdValid;

            if (!this.isTracked)
            {
                this.Chomp = false;
                this.ChompProgress = -1.0f;
            }
            else
            {
                this.Chomp = chomp;
                this.ChompProgress = chompProgress;
            }

            if (this.Chomp)
            {
                if (this.ChompProgress > 0.65)
                {

                    //this.Confidence = detectionConfidence;
                    //this.ImageSource = this.seatedImage;
                    //Console.WriteLine("DETECT CONF OVER 0.65) ");
                    // count++;
                    Extensions.startGame();
                }

            }

        }
    }
}