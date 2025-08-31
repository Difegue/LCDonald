using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class AmyRougeVolleyball : LCDGameBase
    {
        public override string ShortName => "arvolleyball";
#if BURGER
        public override string Name => "Curly Frenchie Volleyball (Fry Friends Volleyball)";
#else
        public override string Name => "Amy & Rouge Volleyball (2004)";
#endif

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
                    Name = "Volley",
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

        private int _aiResistance = int.MaxValue;
        private int _nextBallSpawn;

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
            _rougePosition = -1;
            
            // record this initial spawn so it can be mirrored for each new ball
            _nextBallSpawn = _rng.Next(1, 3);

            _level = _isEndlessMode ? 6 : 0;
            _currentScore = _isEndlessMode ? -3 : 0;
            _customUpdateSpeed = _isEndlessMode ? 200 : 500;

            StartupMusic("game_start.ogg");
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
                else if (input.Name == "Volley")
                {
                    _batEngaged = true;
                }
            }
        }

        protected override void UpdateCore()
        {
            // Bounce ball back 
            if (_ballPosition > 40 && _ballPosition % 10 == _amyPosition && _batEngaged && !_ballDirection)
            {
                _ballDirection = true;
                QueueSound(new LCDGameSound("hit_confirm.ogg"));
            } 
            else if (_batEngaged)
            {
                // hit noball sound
                QueueSound(new LCDGameSound("hit.ogg"));
            }
            _batEngaged = false;
        }

        public override void CustomUpdate()
        {
            // Spawn ball at rougepos
            if (_ballPosition == -1)
            {
                _rougePosition = _nextBallSpawn;
                _nextBallSpawn = _rougePosition == 1 ? 2 : 1;
                
                _ballPosition = _rougePosition + 10;
                _ballDirection = false;

                // As the level goes up, the """AI""" requires more counterhits to score
                _aiResistance = _level switch
                {
                    0 => _rng.Next(1, 2),
                    1 => _rng.Next(1, 6),
                    2 => _rng.Next(2, 9),
                    3 => _rng.Next(3, 10),
                    4 => _rng.Next(6, 19),
                    _ => _aiResistance
                };
            }
            else
            {
                // Move ball depending on direction
                if (_ballDirection)
                {
                    _ballPosition -= 10;
                }
                else
                {
                    _ballPosition += 10;
                }

                // Randomly shift ball left or right
                if (_ballPosition > 20 && _ballPosition < 40) {
                    var shift = _rng.Next(-1, 2);
                    _ballPosition += shift;

                    // Adjust out of bounds
                    if (_ballPosition % 10 == 0)
                        _ballPosition++;

                    if (_ballPosition % 10 == 4)
                        _ballPosition--;
                }

                // impossible
                if (_ballPosition == 13)
                    _ballPosition = 12;
            }

            if (_ballPosition > 50)
            {
                _currentScore--;
                _ballPosition = -1;
                BlinkElement(MISS, 1);
                QueueSound(new LCDGameSound("miss.ogg"));
            }

            if (_ballPosition < 20 && _ballPosition != -1 && _aiResistance > 0 && _ballDirection)
            {
                _aiResistance--;
                IncrementScore();
                QueueSound(new LCDGameSound("hit_return.ogg"));
                _ballDirection = false;

                // move rouge in front of ball 
                _rougePosition = _ballPosition % 10;
            }

            if (_ballPosition < 10 && _ballPosition != -1 && _aiResistance == 0)
            {
                _currentScore++;
                _ballPosition = -1;
                QueueSound(new LCDGameSound("score.ogg"));
            }

            if (_currentScore == 4)
            {
                LevelUpAnimation();

                // Speed up
                _customUpdateSpeed -= 60;
            }

            if (_level == 5)
            {
                Victory();
                return;
            }

            if (_currentScore == -4)
            {
                GameOver();
                return;
            }
        }

        private void LevelUpAnimation()
        {
            _isInputBlocked = true;
            // Wait for previous sound to finish playing
            System.Threading.Thread.Sleep(200);
            _currentScore = 0;

            var levelUpFrame0 = new List<string>(GetVisibleGameElements()) { SCORE_CENTER };
            var levelUpFrame1 = new List<string>(GetVisibleGameElements()) { SCORE_A1 };
            var levelUpFrame2 = new List<string>(GetVisibleGameElements()) { SCORE_A2 };
            var levelUpFrame3 = new List<string>(GetVisibleGameElements()) { SCORE_A3 };
            
            var levelUpAnimation = new List<List<string>> { levelUpFrame0, levelUpFrame1,  levelUpFrame2, levelUpFrame3, };
            QueueSound(new LCDGameSound("level_up.ogg"));
            PlayAnimation(levelUpAnimation);
            _level++;
        }

        private void GameOver()
        {
            _amyPosition = -1;
            _level = 0;
            _rougePosition = -1;
            _ballPosition = -1;
            _currentScore = 0;

            QueueSound(new LCDGameSound("game_over.ogg"));

            // miss blink + advantage bar animation
            var victoryFrame1 = new List<string> { SCORE_CENTER };
            var victoryFrame2 = new List<string> { SCORE_R1 };
            var victoryFrame3 = new List<string> { SCORE_R2 };
            var victoryFrame4 = new List<string> { SCORE_R3 };
            var victoryFrame1s = new List<string> { SCORE_CENTER, MISS };
            var victoryFrame2s = new List<string> { SCORE_R1, MISS };
            var victoryFrame3s = new List<string> { SCORE_R2, MISS };
            var victoryFrame4s = new List<string> { SCORE_R3, MISS };


            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,};
            PlayAnimation(victoryAnimation);
            Stop();
        }

        private void Victory()
        {
            _amyPosition = -1;
            _level = 0;
            _rougePosition = -1;
            _ballPosition = -1;
            _currentScore = 0;

            QueueSound(new LCDGameSound("game_win.ogg"));

            // all amy blink + advantage bar animation
            var victoryFrame1 = new List<string> { SCORE_CENTER };
            var victoryFrame2 = new List<string> { SCORE_A1 };
            var victoryFrame3 = new List<string> { SCORE_A2 };
            var victoryFrame4 = new List<string> { SCORE_A3 };
            var victoryFrame1s = new List<string> { SCORE_CENTER, AMY_1, AMY_2, AMY_3, TENNIS_2 };
            var victoryFrame2s = new List<string> { SCORE_A1, AMY_1, AMY_2, AMY_3, TENNIS_2 };
            var victoryFrame3s = new List<string> { SCORE_A2, AMY_1, AMY_2, AMY_3, TENNIS_2 };
            var victoryFrame4s = new List<string> { SCORE_A3, AMY_1, AMY_2, AMY_3, TENNIS_2 };


            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,
                                                            victoryFrame1, victoryFrame2, victoryFrame3, victoryFrame4, victoryFrame1s, victoryFrame2s, victoryFrame3s, victoryFrame4s,};
            PlayAnimation(victoryAnimation);
            Stop();
        }
    }
}
