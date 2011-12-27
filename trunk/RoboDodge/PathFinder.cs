// Adalid Claure
// CS325 - Fall 2010
// RoboDodge

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using log4net;
using System.Diagnostics;

namespace RoboDodge
{
    public class PlannerNode
    {
        public Vector3 Position;
        public PlannerNode Parent;
        public float FinalCost;
    }

    // hoping this class fufills the math requirement of this project
    public class PathFinder
    {
        static ILog log = LogManager.GetLogger(typeof(PathFinder));

        Terrain Terrain;

        public List<PlannerNode> PathNodes;

        public PathFinder(Terrain terrain)
        {
            Terrain = terrain;
            PathNodes = new List<PlannerNode>();
        }

        private Vector3 GetNearestOpenNode(Vector3 v)
        {
            Vector3 nearest = Vector3.Zero;
            float? fNearest = null;

            foreach (Vector3 iv in Terrain.OpenNodes)
            {
                if (fNearest == null || Vector3.Distance(v,iv) < fNearest)
                {
                    fNearest = Vector3.Distance(v, iv);
                    nearest = iv;
                }
            }

            return nearest;
        }

        private List<Vector3> GetNearByPoints(Vector3 v)
        {
            Vector3 nearest = GetNearestOpenNode(v);
            List<Vector3> ret = new List<Vector3>();

            foreach (Vector3 iv in Terrain.OpenNodes)
            {
                float d = Vector3.Distance(nearest, iv);

                if (d > 0 && d <= 1) //Math.Sqrt(2))//1)
                {
                    ret.Add(iv);
                }
            }

            return ret;
        }

        public PlannerNode PopLowestFinalCostNode()
        {
            PlannerNode retNode = null;
            float? fFinalCost = null;

            foreach (PlannerNode p in PathNodes)
            {
                if (fFinalCost == null || p.FinalCost < fFinalCost)
                {
                    retNode = p;
                    fFinalCost = p.FinalCost;
                }
            }

            PathNodes.Remove(retNode);
            return retNode;
        }


        /// <summary>
        /// Returns a node of a given position if it's already been created
        /// </summary>
        /// <param name="src"></param>
        /// <returns>Existing node or null</returns>
        private PlannerNode GetAlreadyCreatedNode(PlannerNode src)
        {
            foreach (PlannerNode n in PathNodes)
            {
                if (Vector3.Distance(src.Position, n.Position) == 0)
                {
                    return n;
                }
            }

            return null;
        }

        private void RemoveAlreadyCreatedNode(PlannerNode src)
        {
            PathNodes.RemoveAll(n => Vector3.Distance(src.Position, n.Position) == 0);
        }

        public NPCPath GetPath(Vector3 start, Vector3 finish)
        {
            // TODO: off the map testing
            NPCPath retPath = new NPCPath { StartPoint = start, EndPoint = finish, FollowPathType = FollowPathType.Circular };

            Vector3 newStart = GetNearestOpenNode(start);
            Vector3 newEnd = GetNearestOpenNode(finish);
            PlannerNode current = null;

            if (Vector3.Distance(newStart, newEnd) > 0)
            {
                PlannerNode root = new PlannerNode { Parent = null, Position = newStart, FinalCost = 0 };
                PathNodes.Add(root);

                while (PathNodes.Count > 0)
                {
                    current = PopLowestFinalCostNode();

                    if (Vector3.Distance(current.Position, newEnd) == 0)
                    {
                        break;
                    }

                    foreach (Vector3 nbp in GetNearByPoints(current.Position))
                    {
                        // A* - would test here if spot is impassable but since we build a list of
                        // open nodes, we don't need this test

                        PlannerNode successorNode = new PlannerNode
                        {
                            Parent = current,
                            Position = nbp,
                            FinalCost = current.FinalCost + Vector3.Distance(nbp, newEnd)
                        };

                        PlannerNode oldNode = GetAlreadyCreatedNode(successorNode);
                        if (oldNode != null)
                        {
                            if (successorNode.FinalCost <= oldNode.FinalCost)
                            {
                                // TODO: this never gets called, why?
                                RemoveAlreadyCreatedNode(oldNode);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        PathNodes.Add(successorNode);
                    }
                }
            }

            retPath.PathNodes = new List<Vector3>();
            retPath.PathNodes.Add(finish);

            if (current != null)
            {
                PlannerNode c = current.Parent;
                while (c.Parent != null)
                {
                    retPath.PathNodes.Add(c.Position);
                    c = c.Parent;
                }
            }

            retPath.PathNodes.Add(start);
            List<Vector3> tList = new List<Vector3>(retPath.PathNodes);
            tList.Reverse();
            tList.AddRange(retPath.PathNodes);

            retPath.PathNodes = tList;

            return retPath;
        }
    }
}
