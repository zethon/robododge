// RoboDodge

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using log4net;
using Microsoft.Xna.Framework;

namespace RoboDodge
{
    public class StunnedState : NPCState
    {
        static ILog log = LogManager.GetLogger(typeof(StunnedState));

        Character _context;
        Stopwatch _stopWatch;

        public StunnedState(Character Context)
            : base(Context)
        {
            _context = Context;

            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }


        public override void Update()
        {
            if (_stopWatch.Elapsed.TotalSeconds > 5)
            {
                // NOTE: this is a hacking of the PlayerCharacter
                // if I had time the Context object would have a Target property
                PlayerCharacter pc = (PlayerCharacter)this.Context.Game.Components.Where(c => c is PlayerCharacter).FirstOrDefault();
                if (pc == null)
                {
                    throw new Exception("Could not determine PlayerCharacter");
                }
                
                if (Vector3.Distance(Context.GetPosition(), pc.GetPosition()) > 15.0f)
                {   
                    // if the player is far away enough, then go back to idle
                    Context.CurrentState = new IdleState(Context);
                }
                else
                {   
                    // otherwise attack
                    Context.CurrentState = new AttackState(Context, pc);
                }

                return;
            }

            float tmti = _stopWatch.ElapsedMilliseconds;
            float damper = -0.000001f;

            if (Context.Position.Y <= Terrain.MinOffset)
            {
                Context.Velocity = Vector3.Zero;
                Context.Position = new Vector3(Context.Position.X, 0.2f, Context.Position.Z);
            }
            else
            {
                Context.Velocity += (PhysicsHelper.Gravity * tmti * damper);
                Context.Position += (-Context.Velocity);
            }
        }


        public override void DoBallCollision(DodgeBall ball)
        {
            Context.Health -= (Context.Health * .15f);
        }

    }
}
