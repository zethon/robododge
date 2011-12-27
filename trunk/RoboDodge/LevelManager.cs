using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using log4net;

namespace RoboDodge
{
    class LevelManager
    {
        static ILog log = LogManager.GetLogger(typeof(RDGame));

        private Game _game;

        public IList<LevelConfig> Levels;

        public int CurrentLevelIndex
        {
            get;
            private set;
        }
        
        private LevelConfig _currentLevel;
        public LevelConfig CurrentLevel
        {
            get { return _currentLevel; }
        }

        public void SetCurrentLevel(int iIndex)
        {
            if (iIndex < Levels.Count())
            {
                CurrentLevelIndex = iIndex;
                _currentLevel = Levels[iIndex];
            }
        }
        
        public LevelManager(Game game, string strXmlFile)
        {
            _game = game;
            Levels = new List<LevelConfig>();

            XDocument doc = XDocument.Load(strXmlFile);

            foreach (XElement lvl in doc.Descendants(@"level"))
            {
                Levels.Add(LevelConfig.CreateConfig(lvl));
            }

            log.DebugFormat("Total Levels loaded: {0}", Levels.Count());

            var levels = (from l in Levels
                          orderby l.Order
                          select l);

            Levels = new List<LevelConfig>(levels.ToArray());
            SetCurrentLevel(0);

            if (CurrentLevel == null)
            {
                throw new ApplicationException("Could not load a first level");
            }
        }
    }
}
