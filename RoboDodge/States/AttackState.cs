using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using log4net;
using Microsoft.Xna.Framework;
using System.Threading;

namespace RoboDodge
{
    class AttackState : NPCState
    {
        static ILog log = LogManager.GetLogger(typeof(AttackState));
        Stopwatch _timer;
        Stopwatch _fireTimer;
        Character _target;
        PathFinder _pf;
        Thread _pathThread;

        public AttackState(Character context, Character target)
            : base(context)
        {
            _target = target;
            _timer = new Stopwatch();
            _fireTimer = new Stopwatch();

            PathFinder pf = new PathFinder(context.Terrain);
            context.CurrentPath = pf.GetPath(context.GetPosition(), _target.GetPosition());

            _timer.Start();

            Context.Fire();
            _fireTimer.Start();

            _pathThread = new Thread(new ParameterizedThreadStart(FindNewPathThread));
        }

        private void FindNewPathThread(object o)
        {
            object[] os = o as object[];
            Character src = os[0] as Character;
            Character tar = os[1] as Character;

            PathFinder pf = new PathFinder(src.Terrain);
            NPCPath path = pf.GetPath(src.GetPosition(), tar.GetPosition());

            lock (src)
            {
                src.CurrentPath = path;
            }
        }

        public override void Update()
        {
            if (Vector3.Distance(Context.GetPosition(), _target.GetPosition()) > 15.0f)
            {
                Context.CurrentState = new IdleState(Context);
            }

            if (_fireTimer.ElapsedMilliseconds > 500)
            {
                Context.Fire();
                _fireTimer.Reset();
                _fireTimer.Start();
            }

            if (Vector3.Distance(Context.GetPosition(), _target.GetPosition()) < 1.5f)
            {
                Context.CurrentPath = null;
                return;
            }

            if (_timer.ElapsedMilliseconds > 1000)
            {
                PathFinder pf = new PathFinder(Context.Terrain);

                // run the pathfinding on a seperate thread
                // this assumes that pathfinding will never take longer than the > value above
                Thread worker = new Thread(new ParameterizedThreadStart(FindNewPathThread));
                worker.Start(new object[] { Context, _target });

                //Context.CurrentPath = pf.GetPath(Context.GetPosition(), _target.GetPosition());

                _timer.Reset();
                _timer.Start();
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
            if (ball.Owner is PlayerCharacter)
            {
                Context.CurrentState = new StunnedState(Context);

                Context.Health--;

                if (Context.Health < 3)
                {
                    Context.CurrentState = new StunnedState(Context);
                }
            }
            
        }
    }
}
