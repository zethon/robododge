// Adalid Claure
// CS325 - Fall 2010
// RoboDodge

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using log4net;

namespace RoboDodge
{
    static public class BoundingBoxExtension
    {
        static ILog log = LogManager.GetLogger(typeof(NPCOrb));

        // I wish I could have stored this at creation time but property extensions aren't coming 'til C# 5
        // so I'll have to figure it out on the fly
        public static bool IsFloorBox(this BoundingBox box)
        {
            // this is a total hack but if I had more time or could start over this entire game would be written differently
            // Vector3(1000, 0, 1000) comes from Terrain.cs
            if (box.Max.X == 1000 && box.Max.Z == 1000)
            {
                return true;
            }

            return false;
        }
    }
}
