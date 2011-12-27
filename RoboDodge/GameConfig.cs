using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using log4net;

namespace RoboDodge
{
    public class GameConfig
    {
        static ILog log = LogManager.GetLogger(typeof(GameConfig));

        [XmlElement]
        public int Height
        { get; set; }

        [XmlElement]
        public int Width
        { get; set; }

        [XmlElement]
        public bool FullScreen
        { get; set; }

        [XmlElement]
        public int StartLevel
        { get; set; }

        [XmlElement]
        public bool Mute
        { get; set; }

        [XmlElement]
        public bool BGMute
        { get; set; }

        [XmlElement]
        public int PlayerHealth
        { get; set; }

        [XmlElement]
        public bool NPCAggro
        { get; set; }

        [XmlElement]
        public bool FreeCamera
        { get; set; }

        [XmlElement]
        public bool ShowDebugInfo
        { get; set; }

        static private GameConfig _instance = null;
        static public GameConfig Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new GameConfig();
                }

                return _instance; 
            }
        }

        private GameConfig()
        {
        }

        static public void LoadConfig(string fileName)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(GameConfig));
            TextReader textReader = new StreamReader(fileName);
            _instance = (GameConfig)deserializer.Deserialize(textReader);
            textReader.Close();

            log.InfoFormat("Config loaded from '{0}'", fileName);
        }

        static public void SaveConfig(GameConfig config, string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GameConfig));
            TextWriter textWriter = new StreamWriter(fileName);
            serializer.Serialize(textWriter, config);
            textWriter.Close();
        }
    }
}
