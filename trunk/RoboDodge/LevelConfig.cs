using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using log4net;

namespace RoboDodge
{
    public struct OrbInitInfo
    {
        public Type Type;
        public NPCPath Path;
    }

    public struct GemInitInfo
    {
        public Vector3 Position;
    }

    public class LevelConfig
    {
        static ILog log = LogManager.GetLogger(typeof(LevelConfig));

        public int Length;
        public int Width;
        public char[,] FloorPlan;
        public IDictionary<string, string> Textures;
        public int Order;

        public Vector2 StartVector;
        public Vector2 EndVector;
        public List<OrbInitInfo> NPCs;
        public List<NPCPath> Paths;
        public List<GemInitInfo> Gems; // TODO: deprecate
        public int GemCount;

        static public LevelConfig CreateConfig(XElement node)
        {
            LevelConfig retLevel = new LevelConfig();

            #region load floor plan
            retLevel.Gems = new List<GemInitInfo>();

            string floorPlan = (string)node.Descendants(@"map").FirstOrDefault();
            if (floorPlan != null && floorPlan != string.Empty)
            {
                string[] lines = floorPlan.Split(new char[]{'\n','\r'},StringSplitOptions.RemoveEmptyEntries);

                if (lines.Count() > 0)
                {
                    retLevel.Length = lines.Count();
                    retLevel.Width = (from l in lines
                             select (l.Length)).Max();

                    retLevel.FloorPlan = new char[retLevel.Width, retLevel.Length];

                    // fill the FloorPlan with empty tiles
                    
                    string line = string.Empty;
                    for (int y = 0; y < lines.Count(); y++ )
                    {
                        line = lines[y];
                        for (int x = 0; x < retLevel.Width; x++)
                        {
                            if (x >= line.Length)
                            {
                                retLevel.FloorPlan[x, y] = ' ';
                            }
                            else
                            {
                                retLevel.FloorPlan[x, y] = line[x];
                            }

                            if (retLevel.FloorPlan[x, y] == 'A')
                            {
                                retLevel.StartVector = new Vector2((float)x + 0.5f, (float)y + 0.5f);
                            }
                            if (retLevel.FloorPlan[x, y] == 'E')
                            {
                                retLevel.EndVector = new Vector2((float)x + 0.5f, (float)y + 0.5f);
                            }

                            if (retLevel.FloorPlan[x, y] == '*')
                            {
                                GemInitInfo i = new GemInitInfo();
                                i.Position = new Vector3((float)x + 0.5f, 0.5f, -(float)y - 0.5f);

                                retLevel.Gems.Add(i);
                            }
                        }
                    }
                }
            }
            #endregion

            // parse level order
            retLevel.Order = 0;
            if (node.Attribute("order") != null)
            {
                string strOrder = node.Attribute("order").Value;

                int.TryParse(strOrder, out retLevel.Order);
            }
            
            // create the texture hash
            retLevel.Textures = new Dictionary<string, string>();
            foreach (XElement ele in node.Descendants("texture"))
            {
                retLevel.Textures.Add(ele.Attribute("type").Value ,ele.Attribute("value").Value);
            }

            // create the npc list
            retLevel.NPCs = new List<OrbInitInfo>();
            foreach (XElement npcEl in node.Descendants("npc"))
            {
                OrbInitInfo i = new OrbInitInfo();
                i.Type = typeof(NPCOrb);

                XElement[] pathNodes = npcEl.Descendants("path").ToArray();
                if (pathNodes.Count() > 0)
                {
                    i.Path = NPCPath.CreateNPCPath(pathNodes[0]);
                }

                retLevel.NPCs.Add(i);
            }


            // create the paths
            retLevel.Paths = new List<NPCPath>();
            foreach (XElement path in node.Descendants("path"))
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

                retLevel.Paths.Add(newPath);
            }
            
            XElement gems = node.Descendants("gems").FirstOrDefault();
            if (gems != null && gems.Attribute("count") != null)
            {
                retLevel.GemCount = int.Parse(gems.Attribute("count").Value);
            }
            else
            {
                retLevel.GemCount = 0;
            }
            
            return retLevel;
        }
    }
}
