using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace LCDonald.Core.Games
{
    public class TailsSoccer : LCDGameBase
    {

#if BURGER
        public override string ShortName => "ksoccer";
        public override string Name => "Kola Bear's Soccer";
#else
        public override string ShortName => "tsoccer";
        public override string Name => "Tails Soccer (2004)";
#endif

        #region SVG Group Names
        public const string LEVEL_1 = "level-1";
        public const string LEVEL_2 = "level-2";
        public const string LEVEL_3 = "level-3";
        public const string BALL_11 = "ball-11";
        public const string BALL_12 = "ball-12";
        public const string BALL_13 = "ball-13";
        public const string BALL_21 = "ball-21";
        public const string BALL_22 = "ball-22";
        public const string BALL_23 = "ball-23";
        public const string BALL_31 = "ball-31";
        public const string BALL_32 = "ball-32";
        public const string BALL_33 = "ball-33";
        public const string BALL_41 = "ball-41";
        public const string BALL_42 = "ball-42";
        public const string BALL_43 = "ball-43";
        public const string TAILS_LEFT = "tails-left";
        public const string TAILS_RIGHT = "tails-right";
        public const string TAILS_CENTER = "tails-center";
        public const string BALL_MISS = "ball-miss";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                LEVEL_1, LEVEL_2, LEVEL_3,


                       BALL_11,  BALL_12,  BALL_13,
                    BALL_21,     BALL_22,   BALL_23,
                  BALL_31,       BALL_32,      BALL_33,
                BALL_41,         BALL_42,       BALL_43,

               TAILS_LEFT,  TAILS_CENTER,  TAILS_RIGHT,
                                BALL_MISS, 
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Left",
                    Description = "Move Tails Left",
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    Description = "Move Tails Right",
                    KeyCode = 25, // right
                }
            };          
        }

        private int _tailsPosition;
        private int _level;

        private int _ballsIntercepted;
        private int _ballsMissed;

        private int _ballPos;

        protected override List<string> GetVisibleElements()
        {
            var elements = new List<string>();

            if (_level >= 1)
                elements.Add(LEVEL_1);
            if (_level >= 2)
                elements.Add(LEVEL_2);
            if (_level >= 3)
                elements.Add(LEVEL_3);

            elements.Add(GetTailsElement());

            if (_ballPos != -1)
                elements.Add("ball-" + _ballPos);

            return elements;
        }

        private string GetTailsElement()
        {
            if (_tailsPosition == 1)            
                return TAILS_LEFT;
            else if (_tailsPosition == 2)
                return TAILS_CENTER;
            else if (_tailsPosition == 3)
                return TAILS_RIGHT;

            return "";
        }

        public override void InitializeGameState()
        {
            _tailsPosition = 2;
            _level = _isEndlessMode ? 5 : 0;

            _ballsIntercepted = 0;
            _ballsMissed = 0;

            _ballPos = -1;

            _customUpdateSpeed = _isEndlessMode ? 150 : 500;
            StartupMusic();
        }        

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Left" && _tailsPosition > 1)
                {
                    _tailsPosition--;
                }
                else if (input.Name == "Right" && _tailsPosition < 3)
                {
                    _tailsPosition++;
                }
            }
        }

        protected override void UpdateCore()
        {
            // Unused in this game
        }

        public override void CustomUpdate()
        {
            if (_ballsIntercepted == 15 && !_isEndlessMode)
            {
                QueueSound(new LCDGameSound("../common/level_up.ogg"));
                _ballsIntercepted = 0;
                _ballsMissed = 0;
                _level++;
                // Speed up
                _customUpdateSpeed -= 75; 
            }

            if (_level == 4)
            {
                Victory();
                return;
            }

            if (_ballPos != -1)
            {
                _ballPos += 10;

                if (_ballPos > 50)
                {
                    var digit = _ballPos % 10;
                    _ballPos = -1;
                    if (digit == _tailsPosition)
                    {
                        _ballsIntercepted++;
                        IncrementScore();
                        QueueSound(new LCDGameSound("../common/hit.ogg"));
                    } 
                    else
                    {
                        _ballsMissed++;
#if BURGER
                        // More time for miss animation
                        BlinkElement(BALL_MISS, 3);
#else
                        BlinkElement(BALL_MISS, 1);
#endif
                        QueueSound(new LCDGameSound("../common/miss.ogg"));
                    }

                    if (_ballsMissed == 5)
                        GameOver();
                }
                else if (_ballPos < 40)
                {
                    // Randomly shift ball left or right
                    var shift = _rng.Next(-1, 2);
                    _ballPos += shift;
                    
                    // Adjust out of bounds
                    if (_ballPos % 10 == 0)
                        _ballPos++;

                    if (_ballPos % 10 == 4)
                        _ballPos--;
                }
                
            }
            else 
            {
                // Randomly spawn ball at 11, 12 or 13
                _ballPos = _rng.Next(11, 14);
            }
        }
        
        private void GameOver()
        {
            _tailsPosition = -1;
            _ballPos = -1;

            GenericGameOverAnimation(new List<string> { BALL_MISS, TAILS_CENTER });
            Stop();
        }

        private void Victory()
        {
            _tailsPosition = -1;
            _ballPos = -1;
            
            GenericVictoryAnimation(new List<string> { LEVEL_1, LEVEL_2, LEVEL_3 });
            Stop();
        }
    }
}
