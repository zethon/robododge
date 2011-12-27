using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace RoboDodge
{
    public class SoundManager
    {
        public bool Mute = false;

        private static SoundManager _instance;

        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SoundManager();
                }

                return _instance;
            }
        }

        private Dictionary<string, SoundEffect> _sounds;

        public void Add(string strAssetName,ContentManager c)
        {
            if (!_sounds.ContainsKey(strAssetName))
            {
                _sounds.Add(strAssetName, c.Load<SoundEffect>(strAssetName));
            }
        }

        public void Play(string strAssetName)
        {
            if (Mute)
            {
                return;
            }

            if (_sounds.ContainsKey(strAssetName))
            {
                SoundEffect s = _sounds[strAssetName];
                s.Play();
            }
        }

        private SoundManager()
        {
            _sounds = new Dictionary<string, SoundEffect>();
        }
    }
}
