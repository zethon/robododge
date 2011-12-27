// Adalid Claure
// CS325 - Fall 2010
// RoboDodge

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoboDodge
{
    public abstract class Character : ModelComponent
    {
        public float Health;
        public bool Destroyed = false;

        public NPCPath CurrentPath;
        public NPCPath DefaultPath;
        public NPCState CurrentState;
        public Terrain Terrain;

        public Character(Model model, RDGame game, ICamerable camera, Terrain terrain)
            : base(model, game, camera, terrain.BuildingBoxes)
        {
            Terrain = terrain;
        }

        public abstract void Fire();

        /// <summary>
        /// Determines if one Character object can see another
        /// TODO: this function needs to be written better
        /// </summary>
        /// <param name="target">Target to test if the Character can see</param>
        /// <returns>True of false indicatin if the target can be seen</returns>
        public bool CanSeeTarget(Character target)
        {
            if (Vector3.Distance(GetPosition(), target.GetPosition()) > 5)
            {
                return false;
            }

            return true;
        }
    }
}
