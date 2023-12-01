using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace LCDonald.Core.Games
{
    public class BigFishing : LCDGameBase
    {
        public override string ShortName => "bfishing";
        public override string Name => "Big's Fishing (2004)";

        #region SVG Group Names
        public const string LEVEL_1 = "level-1";
        public const string LEVEL_2 = "level-2";
        public const string LEVEL_3 = "level-3";
        public const string LEVEL_UP = "level-up";
        public const string FISH_11 = "fish-11";
        public const string FISH_12 = "fish-12";
        public const string FISH_13 = "fish-13";
        public const string FISH_21 = "fish-21";
        public const string FISH_22 = "fish-22";
        public const string FISH_23 = "fish-23";
        public const string FISH_31 = "fish-31";
        public const string FISH_32 = "fish-32";
        public const string FISH_33 = "fish-33";
        public const string FISH_41 = "fish-41";
        public const string FISH_42 = "fish-42";
        public const string FISH_43 = "fish-43";
        public const string FISH_CATCH = "fish-catch";
        public const string ROD_1 = "rod-1";
        public const string ROD_2 = "rod-2";
        public const string ROD_3 = "rod-3";
        public const string LURE_1 = "lure-1";
        public const string LURE_2 = "lure-2";
        public const string LURE_3 = "lure-3";
        public const string BIG_HANDS = "big-hands";
        public const string BIG_CATCH = "big-catch";
        public const string HIT = "hit";
        public const string MISS = "miss";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                LEVEL_1, LEVEL_2, LEVEL_3,  HIT, BIG_CATCH,
                                    LEVEL_UP,            BIG_HANDS,
                                                FISH_CATCH,
                                                    ROD_1,
                    FISH_11,        FISH_21,    FISH_31, LURE_1,   FISH_41,
                                                    ROD_2,
                    FISH_12,        FISH_22,    FISH_32, LURE_2,   FISH_42,  MISS,
                                                    ROD_3,
                    FISH_13,        FISH_23,    FISH_33, LURE_3,   FISH_43
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Up",
                    Description = "Move Lure Up",
                    KeyCode = 24, // up
                },
                new LCDGameInput
                {
                    Name = "Down",
                    Description = "Move Lure Down",
                    KeyCode = 26, // down
                },
                new LCDGameInput
                {
                    Name = "Reel",
                    Description = "Reel in Fish",
                    KeyCode = 18, // space
                },
            };
        }

        private int _rodPosition;
        private int _level;

        private int _fishCaught;
        private int _fishMissed;

        private int _fishPos;
        private bool _isFishCaught;
        private bool _hasLure;
        private bool _triggerReelAnimation;

        protected override List<string> GetVisibleElements()
        {
            var elements = GetLevels();

            elements.Add(BIG_HANDS);

            if (_isFishCaught)
                elements.Add(HIT);

            if (_rodPosition >= 1)
                elements.Add(ROD_1);

            if (_rodPosition >= 2)
                elements.Add(ROD_2);

            if (_rodPosition >= 3)
                elements.Add(ROD_3);

            if (_hasLure)
                elements.Add("lure-" + _rodPosition);

            if (_fishPos != -1)
                elements.Add("fish-" + _fishPos);

            return elements;
        }

        private List<string> GetLevels()
        {
            var elements = new List<string>();
            
            if (_level >= 1)
                elements.Add(LEVEL_1);
            if (_level >= 2)
                elements.Add(LEVEL_2);
            if (_level >= 3)
                elements.Add(LEVEL_3);

            return elements;
        }

        public override void InitializeGameState()
        {
            _rodPosition = 0;
            _level = _isEndlessMode ? 5 : 0;

            _fishCaught = 0;
            _fishMissed = 0;

            _hasLure = true;
            _fishPos = -1;

            _customUpdateSpeed = _isEndlessMode ? 450 : 850;
            StartupMusic("game_start.ogg");
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Up" && _rodPosition > 0)
                {
                    _isFishCaught = false;
                    _rodPosition--;
                    QueueSound(new LCDGameSound("rod_up.ogg"));
                }
                else if (input.Name == "Down" && _rodPosition < 3)
                {
                    _isFishCaught = false;
                    _rodPosition++;
                    QueueSound(new LCDGameSound("rod_down.ogg"));
                }
                else if (input.Name == "Reel" && _isFishCaught)
                {
                    // We can't trigger animations in input polling as they're blocking,
                    // So we set a flag so that UpdateCore will play the animation for us.
                    // (Funny how this is the only game where this limitation of the engine props up and it's the last one)
                    _triggerReelAnimation = true;
                }
            }
        }

        protected override void UpdateCore()
        {

            // If the fish is within catching range, check 
            if (_fishPos > 30 && _fishPos < 40)
            {
                var digit = _fishPos % 10;
                if (digit == _rodPosition && !_isFishCaught && !IsInputBlocked())
                {
                    _isFishCaught = true;
                    _hasLure = false;
                    QueueSound(new LCDGameSound("hit.ogg"));
                }
            }

            if (_triggerReelAnimation)
            {
                _isFishCaught = false;
                _triggerReelAnimation = false;
                ReelUpAnimation();
            }
                
        }

        public override void CustomUpdate()
        {

            if (_fishCaught == 10 && !_isEndlessMode)
            {
                QueueSound(new LCDGameSound("level_up.ogg"));
                _fishCaught = 0;
                _fishMissed = 0;
                _level++;

                BlinkElement(LEVEL_UP, 2);

                // Speed up
                _customUpdateSpeed -= 75;
            }

            if (_level == 4)
            {
                Victory();
                return;
            }

            if (_fishPos != -1)
            {
                _fishPos += 10;
                _isFishCaught = false;

                if (_fishPos < 30)
                {
                    // Randomly shift fish up or down 
                    var shift = _rng.Next(-1, 2);
                    _fishPos += shift;

                    // Adjust out of bounds
                    if (_fishPos % 10 == 0)
                        _fishPos++;

                    if (_fishPos % 10 == 4)
                        _fishPos--;
                }

                // If we didn't reel in the fish, reset the rodPosition so we can have a lure again
                if (_fishPos > 40 && !_hasLure)
                {
                    _rodPosition = 0;
                    _hasLure = true;
                }

                if (_fishPos > 50)
                {
                    _fishPos = -1;

                    _fishMissed++;
                    BlinkElement(MISS, 2);
                    QueueSound(new LCDGameSound("miss.ogg"));

                    if (_fishMissed == 5)
                        GameOver();
                }
            }
            else
            {
                // Randomly spawn fish at 11, 12 or 13
                _fishPos = _rng.Next(11, 14);
            }
        }

        private void ReelUpAnimation()
        {
            QueueSound(new LCDGameSound("fish_caught.ogg"));

            // Dynamic amount of frames depending on the current fish position
            while (_fishPos > 30)
            {
                var fishFrame = GetVisibleElements();
                PlayAnimation(new List<List<string>> { fishFrame, fishFrame, fishFrame });

                _fishPos--;
                _rodPosition--;
            }

            var reelFrame1 = new List<string>(GetLevels()) { FISH_CATCH, BIG_HANDS };
            var reelFrame2 = new List<string>(GetLevels()) { BIG_CATCH };
            var reelFrame3 = new List<string>(GetLevels()) { };

            var reelAnimation = new List<List<string>> { reelFrame1, reelFrame1, reelFrame1, 
                                                        reelFrame2, reelFrame3, reelFrame2, reelFrame3, reelFrame2, reelFrame3 };
            PlayAnimation(reelAnimation);

            _fishCaught++;
            IncrementScore();
            _hasLure = true;

            _rodPosition = 0;
            _fishPos = -1;
        }

        private void GameOver()
        {
            _rodPosition = -1;
            _fishPos = -1;

            GenericGameOverAnimation(new List<string> { MISS, FISH_11, FISH_12, FISH_13, FISH_21, FISH_22, FISH_23, FISH_31, FISH_32, FISH_33, FISH_41, FISH_42, FISH_43 },
                "game_over.ogg");
            Stop();
        }

        private void Victory()
        {
            _rodPosition = -1;
            _fishPos = -1;

            QueueSound(new LCDGameSound("game_win.ogg"));

            // hand catch level up serie, 2x
            var victoryFrame1 = new List<string> { BIG_HANDS };
            var victoryFrame2 = new List<string> { BIG_CATCH };
            var victoryFrame3 = new List<string> { LEVEL_UP };
            var victoryFrame4 = new List<string> { }; 


            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1,
                                                            victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4,
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1,
                                                            victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4,
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4, victoryFrame4 };
            PlayAnimation(victoryAnimation);
            Stop();
        }
    }
}
