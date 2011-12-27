using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace RoboDodge
{
    public enum FollowPathType
    {
        None,
        Circular,
        Reversing,
        Terminating
    }

    // pathfinding for NPC class?
    public class NPCPath
    {
        public Vector3 StartPoint;
        public Vector3 EndPoint;

        private int _nodeIndex = 0;

        public List<Vector3> PathNodes;
        public FollowPathType FollowPathType;

        public NPCPath()
        {
        }

        public NPCPath(NPCPath path)
        {
            StartPoint = path.StartPoint;
            EndPoint = path.EndPoint;

            PathNodes = path.PathNodes;
            FollowPathType = path.FollowPathType;
        }

        static public NPCPath CreateNPCPath(XElement path)
        {
            string type = path.Attribute("type").Value;

            NPCPath newPath = new NPCPath
            {
                FollowPathType = (FollowPathType)Enum.Parse(typeof(FollowPathType), type, true)
            };

            newPath.PathNodes = new List<Vector3>();
            foreach (XElement el in path.Descendants("point"))
            {
                string valStr = el.Value;
                string[] cord = valStr.Split(new char[] { ',' });

                float x = float.Parse(cord[0]);
                float y = float.Parse(cord[1]);
                float z = float.Parse(cord[2]);
                newPath.PathNodes.Add(new Vector3(x, y, z));
            }
            newPath.StartPoint = newPath.PathNodes[0];
            newPath.EndPoint = newPath.PathNodes[newPath.PathNodes.Count() - 1];

            return newPath;
        }

        public Vector3? GetNextPoint(Vector3 CurrentPosition)
        {
            if (_nodeIndex < PathNodes.Count())
            {
                if (Vector3.Distance(CurrentPosition, PathNodes[_nodeIndex]) < 0.5f)
                {
                    _nodeIndex++;

                    if (_nodeIndex >= PathNodes.Count())
                    {
                        _nodeIndex = 0;
                    }
                }

                return PathNodes[_nodeIndex];
            }

            return null;
        }

        

    }
}
