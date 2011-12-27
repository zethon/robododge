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
    class IdleState : NPCState
    {
        static ILog log = LogManager.GetLogger(typeof(IdleState));

        public IdleState(Character context)
            : base(context)
        {
            Context.CurrentPath = Context.DefaultPath;
        }

        public override void Update()
        {
            if (GameConfig.Instance.NPCAggro)
            {
                PlayerCharacter pc = (PlayerCharacter)Context.Game.Components.Where(c => c is PlayerCharacter).FirstOrDefault();
                if (Context.CanSeeTarget(pc))
                {
                    Context.CurrentState = new AttackState(Context, pc);
                }
            }

            // find the next point in our current path
            if (Context.CurrentPath != null)
            {
                Vector3? target = Context.CurrentPath.GetNextPoint(Context.Position);

                if (target.HasValue)
                {
                    Vector3 pos = Context.Position;
                    Vector3 np = target.Value;

                    Vector3 temp = (pos - np);
                    temp.Normalize();

                    Context.Velocity = temp * Context.Speed;

                    Matrix mx = Matrix.CreateWorld(Context.Position, Context.Velocity, Vector3.Up);
                    Context.Rotation = Quaternion.CreateFromRotationMatrix(mx);
                }
            }

            // update our position
            Context.Position += (-Context.Velocity);
        }

        public override void DoBallCollision(DodgeBall ball)
        {
            // update the reflected velocity & rotation
            Context.Velocity = -PhysicsHelper.ReflectVelocity(Context, ball);
            Matrix mx = Matrix.CreateWorld(Context.Position, Context.Velocity, Vector3.Up);
            Context.Rotation = Quaternion.CreateFromRotationMatrix(mx);

            // deduct some health
            Context.Health--;

            // let the stunned state take over
            //Context.CurrentState = new AttackState(Context, ball.Owner);
            Context.CurrentState = new StunnedState(Context);
        }
    }
}
