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
        private bool rotateLeft = false;
        private bool rotateRight = false;
        private float rotateProgress = 0.0f;
        private bool dropBlock = false;
        private float dropBlockProgress = 0.0f;

        /// <summary> True, if the body is currently being tracked </summary>
        private bool isTracked = false;

        public GestureResultView(bool isTracked, bool chomp, float chompProgress, bool rotateLeft, bool rotateRight, float rotateProgress, bool dropBlock, float dropBlockProgress)
        {
            this.IsTracked = isTracked;
            this.Chomp = chomp;
            this.ChompProgress = chompProgress;
            this.RotateLeft = rotateLeft;
            this.RotateRight = rotateRight;
            this.RotateProgress = rotateProgress;
            this.DropBlock = dropBlock;
            this.DropBlockProgress = dropBlockProgress;
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

        public bool RotateLeft
        {
            get
            {
                return this.rotateLeft;
            }

            private set
            {
                this.rotateLeft = value;
            }
        }

        public bool RotateRight
        {
            get
            {
                return this.rotateRight;
            }

            private set
            {
                this.rotateRight = value;
            }
        }

        public bool DropBlock
        {
            get
            {
                return this.dropBlock;
            }

            private set
            {
                this.dropBlock = value;
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

        public float RotateProgress
        {
            get
            {
                return this.rotateProgress;
            }

            private set
            {
                this.rotateProgress = value;
            }
        }

        public float DropBlockProgress
        {
            get
            {
                return this.dropBlockProgress;
            }

            private set
            {
                this.dropBlockProgress = value;
            }
        }

        public void UpdateGestureResult(bool isBodyTrackingIdValid, bool chomp, float chompProgress, bool rotateLeft, bool rotateRight, float rotateProgress, bool dropBlock, float dropBlockProgress)
        {

            this.IsTracked = isBodyTrackingIdValid;

            if (!this.isTracked)
            {
                this.Chomp = false;
                this.ChompProgress = -1.0f;
                this.RotateLeft = false;
                this.RotateRight = false;
                this.RotateProgress = -1.0f;
                this.DropBlock = false;
                this.DropBlockProgress = -1.0f;
            }
            else
            {
                this.Chomp = chomp;
                this.ChompProgress = chompProgress;
                this.RotateLeft = rotateLeft;
                this.RotateRight = rotateRight;
                this.RotateProgress = rotateProgress;
                this.DropBlock = dropBlock;
                this.DropBlockProgress = dropBlockProgress;
            }

            if (this.Chomp)
            {
                if (this.ChompProgress > 0.65)
                {

                    //this.Confidence = detectionConfidence;
                    //this.ImageSource = this.seatedImage;
                    //Console.WriteLine("DETECT CONF OVER 0.65) ");
                    // count++;
                    MainWindow.startGame();
                }

            }

            //if (this.RotateLeft)
            //{
            //    //Console.WriteLine("IDENTIFIED LEFT WITH PROGRESS OF" + this.RotateProgress);
            //    if (this.RotateProgress < 0.2)
            //    {
            //        Console.WriteLine("LEFT ROTATE");
            //    }
            //}

            //if (this.RotateRight)
            //{
            //    //Console.WriteLine("IDENTIFIED RIGHT WITH PROGRESS OF" + this.RotateProgress);
            //    if (this.RotateProgress > 0.8)
            //    {
            //        Console.WriteLine("RIGHT ROTATE");
            //    }
            //}

        }
    }
}