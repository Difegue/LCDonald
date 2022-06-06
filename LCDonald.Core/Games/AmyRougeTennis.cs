using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class AmyRougeTennis : LCDGameBase
    {
        public override string ShortName => "artennis";
        public override string Name => "Amy & Rouge Tennis (2005)";

        #region SVG Group Names
        public const string SCORE_CENTER = "score-center";
        public const string SCORE_A1 = "score-a1";
        public const string SCORE_A2 = "score-a2";
        public const string SCORE_A3 = "score-a3";
        public const string SCORE_R1 = "score-r1";
        public const string SCORE_R2 = "score-r2";
        public const string SCORE_R3 = "score-r3";
        public const string BALL_11 = "ball-11";
        public const string BALL_12 = "ball-12";
        public const string BALL_21 = "ball-21";
        public const string BALL_22 = "ball-22";
        public const string BALL_23 = "ball-23";
        public const string BALL_31 = "ball-31";
        public const string BALL_32 = "ball-32";
        public const string BALL_33 = "ball-33";
        public const string BALL_41 = "ball-41";
        public const string BALL_42 = "ball-42";
        public const string BALL_43 = "ball-43";
        public const string ROUGE_1 = "rouge-1";
        public const string ROUGE_2 = "rouge-2";
        public const string AMY_1 = "amy-1";
        public const string AMY_2 = "amy-2";
        public const string AMY_3 = "amy-3";
        public const string TENNIS_1 = "tennis-1";
        public const string TENNIS_2 = "tennis-2";
        public const string TENNIS_3 = "tennis-3";
        public const string MISS = "miss";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
              SCORE_A3, SCORE_A2, SCORE_A1, SCORE_CENTER, SCORE_R1, SCORE_R2, SCORE_R3,

                            ROUGE_1,                        ROUGE_2,
                            BALL_11,                        BALL_12,
                       BALL_21,             BALL_22,           BALL_23,
                                              MISS,
                    BALL_31,                BALL_32,             BALL_33,
                  BALL_41,                  BALL_42,                BALL_43,
               TENNIS_1, AMY_1,         TENNIS_2, AMY_2,        TENNIS_3, AMY_3,
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Left",
                    Description = "Move Amy Left",
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    Description = "Move Amy Right",
                    KeyCode = 25, // right
                },
                new LCDGameInput
                {
                    Name = "Tennis",
                    Description = "Hit the ball",
                    KeyCode = 18, // space
                }
            };
        }

        private int _amyPosition;
        private int _rougePosition;
        private int _ballPosition;
        private int _level;
        private int _currentScore;

        private int _aiResistance;

        private bool _batEngaged;
        private bool _ballDirection; // false = default, true = reversed

        protected override List<string> GetVisibleElements()
        {
            var elements = _currentScore switch
            {
                0 => new List<string>() { SCORE_CENTER, },
                1 => new List<string>() { SCORE_CENTER, SCORE_A1 },
                2 => new List<string>() { SCORE_CENTER, SCORE_A1, SCORE_A2 },
                3 => new List<string>() { SCORE_CENTER, SCORE_A1, SCORE_A2, SCORE_A3 },
                -1 => new List<string>() { SCORE_CENTER, SCORE_R1 },
                -2 => new List<string>() { SCORE_CENTER, SCORE_R1, SCORE_R2 },
                -3 => new List<string>() { SCORE_CENTER, SCORE_R1, SCORE_R2, SCORE_R3 },
                _ => new List<string>() { SCORE_CENTER, SCORE_A1, SCORE_A2, SCORE_A3, SCORE_R1, SCORE_R2, SCORE_R3 },
            };

            elements.Add(GetAmyElement());
            elements.Add("rouge-" + _rougePosition);
            elements.Add("ball-" + _ballPosition);

            if (_batEngaged)
                elements.Add("tennis-" + _amyPosition);

            return elements;
        }

        private string GetAmyElement() => "amy-" + _amyPosition;

        public override void InitializeGameState()
        {
            _amyPosition = 2;
            _ballPosition = -1;
            _rougePosition = _rng.Next(1, 3); // TODO record this initial spawn so it can be mirrored for each new ball
            _level = 0;

            _customUpdateSpeed = 500;

            StartupMusic();
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Left" && _amyPosition > 1)
                {
                    _amyPosition--;
                }
                else if (input.Name == "Right" && _amyPosition < 3)
                {
                    _amyPosition++;
                }
                else if (input.Name == "Tennis")
                {
                    _batEngaged = true;
                }
            }
        }

        protected override void UpdateCore()
        {
            // Bounce ball back 
            if (_ballPosition > 40 && _ballPosition % 10 == _amyPosition && _batEngaged)
            {
                _ballDirection = true;
                // TODO sound
            }
            _batEngaged = false;
        }

        public override void CustomUpdate()
        {
            // Spawn ball at rougepos
            if (_ballPosition == -1)
            {
                _ballPosition = _rougePosition + 10;
                _ballDirection = false;

                // As the level goes up, the AI requires more counterhits to score
                _aiResistance = _level switch
                {
                    0 => _rng.Next(1, 4),
                    1 => _rng.Next(1, 6),
                    2 => _rng.Next(1, 9),
                    3 => _rng.Next(1, 10),
                    4 => _rng.Next(1, 19),
                    _ => _aiResistance
                };
            }
            else
            {
                // Move ball depending on direction
                // TODO deviation
                if (_ballDirection)
                {
                    _ballPosition -= 10;
                }
                else
                {
                    _ballPosition += 10;
                }
            }

            if (_ballPosition > 50)
            {
                _currentScore--;
                _ballPosition = -1;
                BlinkElement(MISS, 1);
                QueueSound(new LCDGameSound("../common/miss.ogg"));
            }

            if (_ballPosition < 20 && _ballPosition != -1 && _aiResistance > 0)
            {
                
                _aiResistance--;
                // TODO move rouge in front of ball + sound
                _ballDirection = false;
            }

            if (_ballPosition < 10 && _ballPosition != -1 && _aiResistance == 0)
            {
                _currentScore++;
                _ballPosition = -1;
                QueueSound(new LCDGameSound("../common/hit.ogg"));
            }

            if (_currentScore == 4)
            {
                QueueSound(new LCDGameSound("../common/level_up.ogg"));
                _currentScore = 0;
                
                //TODO animation
                _level++;

                // Speed up
                _customUpdateSpeed -= 75;
            }

            if (_level == 5)
            {
                Victory();
                return;
            }

            if (_currentScore == -3)
            {
                GameOver();
                return;
            }
        }

        private void GameOver()
        {
            _amyPosition = -1;
            _level = 0;
            _rougePosition = -1;
            _ballPosition = -1;
            _currentScore = 0;

            // hit blink + advantage bar animation

            //GenericGameOverAnimation(new List<string> { SONIC_1, HIT_1, BALL_12 });
            Stop();
        }

        private void Victory()
        {
            _amyPosition = -1;
            _level = 0;
            _rougePosition = -1;
            _ballPosition = -1;
            _currentScore = 0;

            // amy blink + advantage bar animation

            //GenericVictoryAnimation(new List<string> { LEVEL_1, LEVEL_2, LEVEL_3 });
            Stop();
        }
    }
}
