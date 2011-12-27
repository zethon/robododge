using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboDodge
{
    class GameConstants
    {
        //camera constants
        public const float NearPlane = 0.05f;
        public const float FarPlane = 1000.0f;
        public const float ViewAngle = 45.0f;
        
        public const float PlayerSphereRadius = 0.1f;
        public const float BallSphereRadius = 0.15f;

        public const float TerrainOffset = 0.5f;
    }
}
