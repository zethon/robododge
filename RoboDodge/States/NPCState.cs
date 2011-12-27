using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RoboDodge
{
    public abstract class NPCState
    {
        protected Character Context;

        public NPCState(Character context)
        {
            Context = context;
        }

        public abstract void Update();
        public abstract void DoBallCollision(DodgeBall ball);
    }
    //interface INPCState
    //{
    //    void Attack(Character su
    //}
}
