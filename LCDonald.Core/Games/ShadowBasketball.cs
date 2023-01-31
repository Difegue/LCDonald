using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class ShadowBasketball : LCDGameBase
    {
        public override string ShortName => "sbasketball";
        public override string Name => "Shadow Basketball (2005)";

        #region SVG Group Names
        public const string SEGMENT_1 = "segment-1";
        public const string SEGMENT_21 = "segment-21";
        public const string SEGMENT_22 = "segment-22";
        public const string SEGMENT_23 = "segment-23";
        public const string SEGMENT_24 = "segment-24";
        public const string SEGMENT_25 = "segment-25";
        public const string SEGMENT_26 = "segment-26";
        public const string SEGMENT_27 = "segment-27";
        public const string SEGMENT_28 = "segment-28";
        public const string BALL_11 = "ball-11";
        public const string BALL_12 = "ball-12";
        public const string BALL_13 = "ball-13";
        public const string BALL_21 = "ball-21";
        public const string BALL_22 = "ball-22";
        public const string BALL_23 = "ball-23";
        public const string BALL_32 = "ball-32";
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
                                        SEGMENT_1,       SEGMENT_27,  SEGMENT_28,
                                                    SEGMENT_25,             SEGMENT_23,
                            BALL_32,                            SEGMENT_24,

                  BALL_21,  BALL_22,  BALL_23,
                 HANDS_21,  HANDS_22,  HANDS_23,
                 OMEGA_1,   OMEGA_2,   OMEGA_3,
                 HANDS_1,   HANDS_2,   HANDS_3, 
                 
                 BALL_11,   BALL_12,   BALL_13, 
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
                    Name = "Ball",
                    Description = "Shoot a Basketball",
                    KeyCode = 18, // space
                }
            };
        }

        private int _shadowPosition;
        private int _omegaPosition; // TODO must be list
        private int _ballPosition;
        private bool _handsUp;
        
        private int _goalsScored;
        private int _ballsIntercepted;
        private int _level;

        protected override List<string> GetVisibleElements()
        {
            var handsPosition = _omegaPosition + (_handsUp ? 20 : 0);

            var elements = new List<string>
            {
                "omega-" + _omegaPosition,
                "hands-" + handsPosition,
                "ball-" + _ballPosition,
                "shadow-" + _shadowPosition
            };

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
                case 2:
                    elements.Add(SEGMENT_21);
                    elements.Add(SEGMENT_22);
                    elements.Add(SEGMENT_27);
                    elements.Add(SEGMENT_28);
                    elements.Add(SEGMENT_25);
                    elements.Add(SEGMENT_24);
                    break;
                case 4:
                    elements.Add(SEGMENT_26);
                    elements.Add(SEGMENT_22);
                    elements.Add(SEGMENT_27);
                    elements.Add(SEGMENT_28);
                    elements.Add(SEGMENT_23);
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
            }

            if (_goalsScored > 8)
                elements.Add(SEGMENT_1);

            return elements;
        }

        public override void InitializeGameState()
        {
            _shadowPosition = 1;
            _omegaPosition = 1;
            _ballPosition = 0;
            _handsUp = false;
            _goalsScored = 0;
            _ballsIntercepted = 0;
            _level = 0;
            
            _customUpdateSpeed = 800;
            
            StartupMusic();
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Left" && _shadowPosition > 1)
                {
                    _shadowPosition--;
                }
                else if (input.Name == "Right" && _shadowPosition < 3)
                {
                    _shadowPosition++;
                }
                else if (input.Name == "Ball" && _ballPosition == 0)
                {
                    _ballPosition = _shadowPosition + 10;
                    // TODO sound
                }
            }
        }

        protected override void UpdateCore()
        {
            // Unused in this game
        }

        public override void CustomUpdate()
        {
            if (_goalsScored >= 30)
            {
                QueueSound(new LCDGameSound("../common/level_up.ogg"));
                _ballsIntercepted = 0;
                _goalsScored = 0;
                _level++;
                // Speed up
                _customUpdateSpeed -= 125;
            }

            if (_level == 4)
            {
                Victory();
                return;
            }

            // Move ball forward
            if (_ballPosition != 0)
                _ballPosition += 10;

            bool hasDodgedCar = false;
            bool hasBeenHit = false;
            

            // Only count a row as dodged if the player hasn't been hit
            if (hasDodgedCar && !hasBeenHit)
            {
                QueueSound(new LCDGameSound("../common/hit.ogg"));
                _goalsScored++;
            }

            if (_ballsIntercepted == 5)
            {
                GameOver();
                return;
            }

        }

        private void GameOver()
        {
            _shadowPosition = -1;
            _ballPosition = 0;
            _level = 0;

            GenericGameOverAnimation(new List<string> { SHADOW_1 });
            Stop();
        }

        private void Victory()
        {
            _shadowPosition = -1;
            _ballPosition = 0;
            _level = 0;

            GenericVictoryAnimation(new List<string> { SHADOW_1 });
            Stop();
        }
        
    }
}
