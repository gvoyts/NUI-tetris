using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KinectHandTracking
{
    public class DroppedPiece
    {

        private readonly Point _finalPosition;
        private readonly string _pieceName;
        private readonly int _rotationPosition;

        public Point FinalPosition { get { return _finalPosition; } }
        public string PieceName { get { return _pieceName; } }
        public int RotationPosition { get { return _rotationPosition; } }

        public DroppedPiece(Point finalPosition, string pieceName, int rotationPosition)
        {
            _finalPosition = finalPosition;
            _pieceName = pieceName;
            _rotationPosition = rotationPosition;
        }

    }
}
