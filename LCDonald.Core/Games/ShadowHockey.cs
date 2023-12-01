using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class ShadowHockey : LCDGameBase
    {
        public override string ShortName => "shockey"; 
        public override string Name => "Shadow Hockey (2004)";

        #region SVG Group Names
        public const string SEGMENT_21 = "segment-21";
        public const string SEGMENT_22 = "segment-22";
        public const string SEGMENT_23 = "segment-23";
        public const string SEGMENT_24 = "segment-24";
        public const string SEGMENT_25 = "segment-25";
        public const string SEGMENT_26 = "segment-26";
        public const string SEGMENT_27 = "segment-27";
        public const string SEGMENT_28 = "segment-28";
        public const string PUCK_11 = "puck-11";
        public const string PUCK_12 = "puck-12";
        public const string PUCK_13 = "puck-13";
        public const string PUCK_21 = "puck-21";
        public const string PUCK_22 = "puck-22";
        public const string PUCK_23 = "puck-23";
        public const string PUCK_32 = "puck-32";
        public const string HIT = "hit";
        public const string SHADOW_1 = "shadow-1";
        public const string SHADOW_2 = "shadow-2";
        public const string SHADOW_3 = "shadow-3";
        public const string OMEGA_1 = "omega-1";
        public const string OMEGA_2 = "omega-2";
        public const string OMEGA_3 = "omega-3";
        public const string HANDS_1 = "hands-1";
        public const string HANDS_2 = "hands-2";
        public const string HANDS_3 = "hands-3";
        public const string HANDS_21 = "hands-21";
        public const string HANDS_22 = "hands-22";
        public const string HANDS_23 = "hands-23";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {                                                   SEGMENT_21,
                                                    SEGMENT_26,             SEGMENT_22,
                                                         SEGMENT_27,  SEGMENT_28,
                              HIT,                  SEGMENT_25,             SEGMENT_23,
                            PUCK_32,                            SEGMENT_24,

                  PUCK_21,  PUCK_22,  PUCK_23,
                 HANDS_21,  HANDS_22,  HANDS_23,
                 OMEGA_1,   OMEGA_2,   OMEGA_3,
                 HANDS_1,   HANDS_2,   HANDS_3, 
                 
                 PUCK_11,   PUCK_12,   PUCK_13, 
                SHADOW_1,  SHADOW_2,  SHADOW_3
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Left",
                    Description = "Move Shadow Left",
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    Description = "Move Shadow Right",
                    KeyCode = 25, // right
                },
                new LCDGameInput
                {
                    Name = "Hockey",
                    Description = "Shoot a puck",
                    KeyCode = 18, // space
                }
            };
        }

        private int _shadowPosition;
        private List<int> _omegaPositions = new();
        private int _ballPosition;
        private List<int> _handPositions = new();

        private int _goalsScored;
        private int _ballsIntercepted;
        private int _level;

        protected override List<string> GetVisibleElements()
        {
            var elements = new List<string>
            {
                "puck-" + _ballPosition,
                "shadow-" + _shadowPosition
            };

            if (_ballPosition == 32)
            {
                elements.Add("hit");
            }

            // Threadsafe list copies
            foreach (var pos in new List<int>(_omegaPositions))
                elements.Add("omega-" + pos);

            foreach (var pos in new List<int>(_handPositions))
                elements.Add("hands-" + pos);

            // 8-segment display
            switch (_goalsScored % 10)
            {
                case 0:
                    elements.Add(SEGMENT_21);
                    elements.Add(SEGMENT_22);
                    elements.Add(SEGMENT_23);
                    elements.Add(SEGMENT_24);
                    elements.Add(SEGMENT_25);
                    elements.Add(SEGMENT_26);
                    break;
                case 1:
                    elements.Add(SEGMENT_22);
                    elements.Add(SEGMENT_23);
                    break;
                case 2:
                    elements.Add(SEGMENT_21);
                    elements.Add(SEGMENT_22);
                    elements.Add(SEGMENT_27);
                    elements.Add(SEGMENT_28);
                    elements.Add(SEGMENT_25);
                    elements.Add(SEGMENT_24);
                    break;
                case 3:
                    elements.Add(SEGMENT_21);
                    elements.Add(SEGMENT_22);
                    elements.Add(SEGMENT_27);
                    elements.Add(SEGMENT_28);
                    elements.Add(SEGMENT_23);
                    elements.Add(SEGMENT_24);
                    break;
                case 4:
                    elements.Add(SEGMENT_26);
                    elements.Add(SEGMENT_22);
                    elements.Add(SEGMENT_27);
                    elements.Add(SEGMENT_28);
                    elements.Add(SEGMENT_23);
                    break;
                case 5:
                    elements.Add(SEGMENT_21);
                    elements.Add(SEGMENT_26);
                    elements.Add(SEGMENT_27);
                    elements.Add(SEGMENT_28);
                    elements.Add(SEGMENT_23);
                    elements.Add(SEGMENT_24);
                    break;
                case 6:
                    elements.Add(SEGMENT_21);
                    elements.Add(SEGMENT_26);
                    elements.Add(SEGMENT_27);
                    elements.Add(SEGMENT_28);
                    elements.Add(SEGMENT_23);
                    elements.Add(SEGMENT_25);
                    elements.Add(SEGMENT_24);
                    break;
                case 7:
                    elements.Add(SEGMENT_21);
                    elements.Add(SEGMENT_22);
                    elements.Add(SEGMENT_23);
                    break;
                case 8:
                    elements.Add(SEGMENT_21);
                    elements.Add(SEGMENT_22);
                    elements.Add(SEGMENT_23);
                    elements.Add(SEGMENT_24);
                    elements.Add(SEGMENT_25);
                    elements.Add(SEGMENT_26);
                    elements.Add(SEGMENT_27);
                    elements.Add(SEGMENT_28);
                    break;
                case 9:
                    elements.Add(SEGMENT_21);
                    elements.Add(SEGMENT_22);
                    elements.Add(SEGMENT_23);
                    elements.Add(SEGMENT_24);
                    elements.Add(SEGMENT_26);
                    elements.Add(SEGMENT_27);
                    elements.Add(SEGMENT_28);
                    break;
            }

            return elements;
        }

        public override void InitializeGameState()
        {
            _shadowPosition = 2;
            _omegaPositions = new() { 2 };
            _ballPosition = 0;
            _handPositions = new() { 2 };
            _goalsScored = 0;
            _ballsIntercepted = 0;
            _level = _isEndlessMode ? 6 : 0;

            _customUpdateSpeed = _isEndlessMode ? 200 : 800;

            if (_isEndlessMode)
            {
                AddOmega();
                AddOmega();
            }

            StartupMusic("game_start.ogg");
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Left" && _shadowPosition > 1)
                {
                    _shadowPosition--;
                    QueueSound(new LCDGameSound("move.ogg"));
                }
                else if (input.Name == "Right" && _shadowPosition < 3)
                {
                    _shadowPosition++;
                    QueueSound(new LCDGameSound("move.ogg"));
                }
                else if (input.Name == "Hockey" && _ballPosition == 0)
                {
                    _ballPosition = _shadowPosition + 10;
                    SetBallSlowdown(true);
                    QueueSound(new LCDGameSound("hit.ogg"));
                }
            }
        }

        private void SetBallSlowdown(bool slowdown)
        {
            // This simulates a weird behavior in the OG game where the defenders move more slowly when a ball is out.
            if (slowdown)
                _customUpdateSpeed += 150;
            else
                _customUpdateSpeed -= 150;
        }

        private bool _ballHalfStep = false;
        protected override void UpdateCore()
        {
            if (_ballPosition == 0) return;

            // Move ball forward -- takes 2 update cycles to move 1 space
            if (_ballHalfStep)
            {
                _ballHalfStep = false;
                _ballPosition += 10;
            }
            else
                _ballHalfStep = true;


            if (_ballPosition > 40)
            {
                QueueSound(new LCDGameSound("score.ogg"));
                _goalsScored++;
                IncrementScore();

                // hack to make the ball unshootable for 2 cycles
                _ballPosition = -10; 
                _ballHalfStep = true;

                SetBallSlowdown(false);
                return;
            }

            if (_ballPosition > 23)
                _ballPosition = 32;

            if (_handPositions.Contains(_ballPosition))
            {
                _ballsIntercepted++;
                _isInputBlocked = true;
                QueueSound(new LCDGameSound("miss.ogg"));

                BlinkElement("omega-" + _ballPosition % 10, 2);
                BlinkElement("hands-" + _ballPosition, 2);
                BlinkElement("ball-" + _ballPosition, 2);

                _ballPosition = 0;
                SetBallSlowdown(false);
            }
        }

        public override void CustomUpdate()
        {
            _isInputBlocked = false;

            if (_goalsScored >= 9 && !_isEndlessMode)
            {
                _isInputBlocked = true;
                QueueSound(new LCDGameSound("level_up.ogg"));
                _ballsIntercepted = 0;
                _goalsScored = 0;
                _level++;

                // Speed up
                _customUpdateSpeed -= 100;

                _omegaPositions.Clear();
                _handPositions.Clear();

                // Add various amount of defenders depending on the level
                // Level 1-2: One defender
                // Level 3-4: Two defenders
                // Level 5: Three defenders
                var pos = _rng.Next(1, 4);
                _omegaPositions.Add(pos);
                _handPositions.Add(pos);

                if (_level >= 2)
                    AddOmega();

                if (_level == 4)
                    AddOmega();

                return;
            }

            if (_level == 5)
            {
                Victory();
                return;
            }

            // Move defenders
            if (_omegaPositions.Count < 3)
                for (var i = 0; i < _omegaPositions.Count; i++)
                {
                    var pos = _omegaPositions[i];

                    // Randomly shift left/right if the position isn't already taken
                    var newPos = pos + _rng.Next(-1, 2);

                    // Check boundaries
                    if (newPos == 0)
                        newPos = 3;

                    if (newPos == 4)
                        newPos = 1;

                    // If the spot is already taken, use the other only left one (neither pos nor newPos)
                    if (newPos != pos && _omegaPositions.Contains(newPos))
                    {
                        var l = new List<int> { 1, 2, 3 };
                        l.Remove(pos);
                        l.Remove(newPos);
                        newPos = l.First();
                    }

                    _omegaPositions[i] = newPos;
                    _handPositions[i] = newPos;
                }

            // 33% chance to raise hands (+20 to pos so it matches ball positioning)
            if (_ballPosition == 0)
                for (var i = 0; i < _handPositions.Count; i++)
                    _handPositions[i] = _rng.Next(0, 3) == 0 ? _handPositions[i] % 10 + 20 : _handPositions[i] % 10;

            if (_ballsIntercepted >= 5)
            {
                GameOver();
                return;
            }

        }

        private void AddOmega()
        {
            var pos = 1;
            // Incredibly unefficient
            while (_omegaPositions.Contains(pos))
                pos = _rng.Next(1, 4);

            _omegaPositions.Add(pos);
            _handPositions.Add(pos);
        }

        private void GameOver()
        {
            _shadowPosition = -1;
            _omegaPositions.Clear();
            _handPositions.Clear();
            _goalsScored = -1;
            _ballPosition = 0;
            _level = 0;

            GenericGameOverAnimation(new List<string> { SHADOW_2, OMEGA_2, HANDS_22 }, "game_over.ogg");
            Stop();
        }

        private void Victory()
        {
            _shadowPosition = -1;
            _omegaPositions.Clear();
            _handPositions.Clear();
            _goalsScored = -1;
            _ballPosition = 0;
            _level = 0;

            QueueSound(new LCDGameSound("game_win.ogg"));

            // 2x move across screen left to right scoring
            var victoryFrame1 = new List<string> { SHADOW_1, PUCK_11 };
            var victoryFrame2 = new List<string> { SHADOW_1, PUCK_21 };
            var victoryFrame3 = new List<string> { SHADOW_1, PUCK_32 };
            var victoryFrame4 = new List<string> { SHADOW_2, PUCK_12 };
            var victoryFrame5 = new List<string> { SHADOW_2, PUCK_22 };
            var victoryFrame6 = new List<string> { SHADOW_2, PUCK_32 };
            var victoryFrame7 = new List<string> { SHADOW_3, PUCK_13 };
            var victoryFrame8 = new List<string> { SHADOW_3, PUCK_23 };
            var victoryFrame9 = new List<string> { SHADOW_3, PUCK_32 };


            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4,
                                                            victoryFrame5, victoryFrame5, victoryFrame5, victoryFrame5, victoryFrame6, victoryFrame6, victoryFrame6, victoryFrame6,
                                                            victoryFrame7, victoryFrame7, victoryFrame7, victoryFrame7, victoryFrame8, victoryFrame8, victoryFrame8, victoryFrame8,
                                                            victoryFrame9, victoryFrame9, victoryFrame9, victoryFrame9, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1,
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame5, victoryFrame5, victoryFrame5, victoryFrame5,
                                                            victoryFrame6, victoryFrame6, victoryFrame6, victoryFrame6, victoryFrame7, victoryFrame7, victoryFrame7, victoryFrame7,
                                                            victoryFrame8, victoryFrame8, victoryFrame8, victoryFrame8, victoryFrame9, victoryFrame9, victoryFrame9, victoryFrame9};

            PlayAnimation(victoryAnimation);
            Stop();
        }

    }
}