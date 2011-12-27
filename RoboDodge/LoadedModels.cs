using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace RoboDodge
{
    class LoadedModels
    {
        public ContentManager Content = null;

        private Dictionary<string, Model> _models;
        public Dictionary<string, Model> Models
        {
            get { return _models; }
        }

        static private LoadedModels _instance;
        static public LoadedModels Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoadedModels();
                }

                return _instance;
            }
        }

        private LoadedModels()
        {
            _models = new Dictionary<string, Model>();            
        }

        public void Add(string key)
        {
            _models.Add(key,Content.Load<Model>(key));
        }

        public Model this[string key]
        {
            get
            {
                return _models[key];
            }
        }
    }
}
