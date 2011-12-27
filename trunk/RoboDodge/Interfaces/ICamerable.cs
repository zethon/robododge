using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoboDodge
{
    public interface ICamerable
    {
        Matrix ViewMatrix { get; set; }
        Matrix ProjectionMatrix { get; set; }
        Viewport ViewPort { get; set; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
    }
}
