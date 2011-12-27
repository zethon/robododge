using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RoboDodge
{
    interface ICollidable
    {
        BoundingBox[] TerrainBoxes
        { get; }

        void HandleCollisions();
    }
}
