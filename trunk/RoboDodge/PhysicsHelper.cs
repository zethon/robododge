using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using log4net;

namespace RoboDodge
{
    static public class PhysicsHelper
    {
        static ILog log = LogManager.GetLogger(typeof(PhysicsHelper));


        static public Vector3 Gravity
        {
            get
            {
                return new Vector3(0.0f, -9.81f, 0.0f);
            }
        }

        /// <summary>
        /// Returns the new velocity for ball1 after a collision
        /// </summary>
        /// <param name="ball1">The ball whose new velocity you want returned</param>
        /// <param name="ball2">The colliding ball</param>
        /// <returns></returns>
        static public Vector3 ReflectVelocity(ModelComponent ball1, ModelComponent ball2)
        {
            float ball1Radius = (ball1.BoundingBox.Max - ball1.BoundingBox.Min).Length();
            float ball2Radius = (ball2.BoundingBox.Max - ball2.BoundingBox.Min).Length();

            float cd = (ball1.Position - ball2.Position).Length();
            float rd = ball1Radius + ball2Radius;

            if (cd <= rd) // we have a collision 
            {
                float a = (ball1Radius * ball1Radius - ball2Radius * ball2Radius + rd * rd) / (2 * rd);
                Vector3 cp = ball1.Position + a * (ball2.Position - ball1.Position) / rd;

                Vector3 CollisionNormal = Vector3.Normalize(cp - ball2.Position);

                float dot = Vector3.Dot(CollisionNormal, Vector3.Normalize(ball1.Velocity));
                Vector3 normalV = Vector3.Normalize(ball1.Velocity);
                float dampening_factor = 1.0f; // no dampening 
                return Vector3.Normalize(2.0f * CollisionNormal * dot - normalV) * ball1.Velocity.Length() * dampening_factor;
            }

            // unchanged velocity
            return ball1.Velocity;
        }

        static public Vector3 ReflectVeloctiyOffBox(ModelComponent ball, BoundingBox b)
        {
            // this is what we search for
            Vector3 normal = Vector3.Zero;

            if (b.IsFloorBox())
            {
                normal = Vector3.Up;
            }
            else
            {

                List<Plane> planes = new List<Plane>();

                // where the ball is at and where it's being advanced to
                // the - here is a hack
                Ray r = new Ray(ball.Position, -ball.Velocity);

                // brute force seems to be the only reasonable solution right now
                Vector3 pointA = new Vector3(b.Max.X, b.Max.Y, b.Max.Z);
                Vector3 pointB = new Vector3(b.Max.X, b.Max.Y, b.Min.Z);
                Vector3 pointC = new Vector3(b.Max.X, b.Min.Y, b.Min.Z);
                Vector3 pointD = new Vector3(b.Min.X, b.Min.Y, b.Max.Z);
                Vector3 pointE = new Vector3(b.Min.X, b.Max.Y, b.Max.Z);
                Vector3 pointF = new Vector3(b.Min.X, b.Min.Y, b.Min.Z);

                Plane up = new Plane(pointA, pointB, pointE);
                Plane right = new Plane(pointA, pointB, pointC);
                Plane left = new Plane(pointD, pointE, pointF);
                Plane front = new Plane(pointA, pointE, pointD);
                Plane back = new Plane(pointB, pointC, pointF);
                planes.Add(up);
                planes.Add(right);
                planes.Add(front);
                planes.Add(left);
                planes.Add(back);

                // TODO: BUG: returns the wrong plane when near a box's edge
                float? fMin = null;
                foreach (Plane p in planes)
                {
                    float? f = r.Intersects(p);
                    if (f != null)
                    {
                        if (fMin == null || f < fMin)
                        {
                            normal = p.Normal;
                            fMin = f;
                        }
                    }
                }
            }
            


            return Vector3.Reflect(ball.Velocity, normal);
        }



    }
}
