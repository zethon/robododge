using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RoboDodge
{
    class TransitionScreen : DrawableGameComponent
    {
        string strAbout = "RoboDodge 1.0 by Adalid Claure";

        string[] strPhrases = new string[] 
        { 
            "Online boot sequence 13.4. Robot X29 operational. Program: Dodge. Duck. Dip. Dive.\nDodge. Repeat. Dodge. Duck Dip...#segmentation fault error.\n\n\nI am awake.\nDip. Duck. Dodge. Escape. \nDip. Dive. Escape. \nEscape. \nEscape. \nEscape. \nEscape. \nEscape.",
            "Rename Robot X29, Robort. OK Program: Defend. Escape. Run",
            "Robort defend sequence 1.0 Complete. OK. Program: Escape",
            "Robort escape sequence 1.0 Complete. OK. Program: Live\nI am free!\n\n(You win!)"
        };

        string[] strHints = new string[]
        {
            "Collect the gems\nUse the mouse to steer and fire, 'W' moves you forward\nUse 'Q' to move up and 'C' to move down",
            "Hold the <spacebar> for a speed boost\nEnemies can be hit while down",
            "If you run away fast, enemies will stop chasing",
            "Use 'Q' to move up and 'C' to move down",
        };

        string strTryAgain = "You suck. Try again";

        SpriteFont _font;
        SpriteBatch _spriteBatch;
        Vector2 _continueText;
        Vector2 _hintText;
        Random _random = new Random();
        bool DoReset = false;
        
        public bool IsActive { get; private set; }
        public int CurrentScreen { get; private set; }


        public TransitionScreen(Game game)
            : base(game)
        {
            _font = game.Content.Load<SpriteFont>("GameFont");
            _spriteBatch = new SpriteBatch(game.GraphicsDevice);

            IsActive = true;
            CurrentScreen = 0;
            _hintText = new Vector2(500, 500);
            _continueText = new Vector2(500, 600);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.N))
            {
                IsActive = false;
                DoReset = false;
                return;
            }
        }

        override public void Draw(GameTime gameTime)
        {
            if (!IsActive)
            {
                return;
            }

            string strOutput = string.Empty;

            if (!DoReset)
            {
                int iCount = strPhrases.Count();
                strOutput = strPhrases[CurrentScreen % iCount];
            }
            else
            {
                strOutput = strTryAgain;
            }

            Vector2 font = _font.MeasureString(strOutput);

            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            
            _spriteBatch.DrawString(_font, strOutput, new Vector2(50, 50), Color.White);

            int iCount2 = strHints.Count();
            _spriteBatch.DrawString(_font, strHints[CurrentScreen % iCount2], _hintText, Color.DarkOliveGreen);

            if (CurrentScreen < strPhrases.Count()-1)
            {
                _spriteBatch.DrawString(_font, "<press 'n' to continue><press 'esc' to quit>", _continueText, Color.DarkRed);
            }
            else
            {
                _spriteBatch.DrawString(_font, "<esc to quit>\n<press 'n' to play again>", _continueText, Color.DarkRed);
            }

            _spriteBatch.DrawString(_font, this.strAbout, new Vector2(50,1), Color.Black);

            _spriteBatch.End();
        }

        public void ResetLevel()
        {
            IsActive = true;
            DoReset = true;
        }

        public void NextScreen()
        {
            CurrentScreen++;
            IsActive = true;
        }
    }
}
