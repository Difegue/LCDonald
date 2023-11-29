using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    /// <summary>
    /// This game is basically a horizontal version of tskypatrol.
    /// </summary>
    public class AiAiBananaCatch : LCDGameBase
    {
        public override string ShortName => "abanana";
        public override string Name => "AiAi Banana Catch (2003)";

        #region SVG Group Names
        public const string LEVEL_1 = "level-1";
        public const string LEVEL_2 = "level-2";
        public const string LEVEL_3 = "level-3";
        public const string BANANA_11 = "banana-11";
        public const string BANANA_12 = "banana-12";
        public const string BANANA_13 = "banana-13";
        public const string BANANA_21 = "banana-21";
        public const string BANANA_22 = "banana-22";
        public const string BANANA_23 = "banana-23";
        public const string BANANA_31 = "banana-31";
        public const string BANANA_32 = "banana-32";
        public const string BANANA_33 = "banana-33";
        public const string BANANA_41 = "banana-41";
        public const string BANANA_42 = "banana-42";
        public const string BANANA_43 = "banana-43";
        public const string AIAI_LEFT = "aiai-left";
        public const string AIAI_RIGHT = "aiai-right";
        public const string AIAI_CENTER = "aiai-center";
        public const string MISS_LEFT = "miss-left";
        public const string MISS_RIGHT = "miss-right";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                                    LEVEL_1, LEVEL_2, LEVEL_3,


                BANANA_11,  BANANA_12,  BANANA_13,
                BANANA_21,  BANANA_22,  BANANA_23,
                BANANA_31,  BANANA_32,  BANANA_33,
                BANANA_41,  BANANA_42,  BANANA_43,

                    MISS_LEFT,      MISS_RIGHT,
               AIAI_LEFT,  AIAI_CENTER,  AIAI_RIGHT
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Left",
                    Description = "Move AiAi Left",
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    Description = "Move AiAi Right",
                    KeyCode = 25, // right
                }
            };          
        }

        private int _aiaiPosition;
        private int _bananasCollected;
        private int _bananasMissed;
        private int _level;

        private List<int> _bananaPositions = new List<int>();

        protected override List<string> GetVisibleElements()
        {
            var elements = new List<string>();

            if (_level >= 1)
                elements.Add(LEVEL_1);
            if (_level >= 2)
                elements.Add(LEVEL_2);
            if (_level >= 3)
                elements.Add(LEVEL_3);

            elements.Add(GetAiAiElement());

            foreach (var ringPos in ThreadSafeBananaList())
                elements.Add("banana-" + ringPos);

            return elements;
        }

        private string GetAiAiElement()
        {
            if (_aiaiPosition == 1)
                return AIAI_LEFT;
            else if (_aiaiPosition == 2)
                return AIAI_CENTER;
            else if (_aiaiPosition == 3)
                return AIAI_RIGHT;

            return "";
        }

        public override void InitializeGameState()
        {
            _aiaiPosition = 2;
            _bananasCollected = 0;
            _bananasMissed = 0;
            _level = _isEndlessMode ? 5 : 0;

            _bananaPositions = new List<int>();
            _customUpdateSpeed = _isEndlessMode ? 400 : 900;

            StartupMusic();
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Left" && _aiaiPosition > 1)
                {
                    _aiaiPosition--;
                }
                else if (input.Name == "Right" && _aiaiPosition < 3)
                {
                    _aiaiPosition++;
                }
            }
        }

        protected override void UpdateCore()
        {
            // Collect bananas if aiai is in front
            foreach (var banPos in ThreadSafeBananaList())
            {
                var digit = banPos % 10;

                if (digit == _aiaiPosition && banPos > 40)
                {
                    QueueSound(new LCDGameSound("../common/hit.ogg"));
                    _bananaPositions.Remove(banPos);
                    _bananasCollected++;
                }
            }

        }

        public override void CustomUpdate()
        {
            if (_bananasCollected >= 30 && !_isEndlessMode)
            {
                QueueSound(new LCDGameSound("../common/level_up.ogg"));
                _bananasMissed = 0;
                _bananasCollected = 0;
                _level++;
                // Speed up
                _customUpdateSpeed -= 175;
            }

            if (_level == 4)
            {
                Victory();
                return;
            }


            // Move rings forward
            _bananaPositions = ThreadSafeBananaList().Select(x => x += 10).ToList();

            foreach (var ringPos in ThreadSafeBananaList())
            {
                if (ringPos > 50)
                {
                    QueueSound(new LCDGameSound("../common/miss.ogg"));
                    _bananaPositions.Remove(ringPos);
                    _bananasMissed++;

                    // Pause ring spawn/advance if there was a miss
                    BlockCustomUpdates(1);

                    var verticalPos = ringPos % 10;

                    // Show miss indicators depending on which bananas were missed
                    if (verticalPos == 1)
                        BlinkElement(MISS_LEFT, 1);
                    else if (verticalPos == 2)
                    {
                        BlinkElement(MISS_LEFT, 1);
                        BlinkElement(MISS_RIGHT, 1);

                    }
                    else if (verticalPos == 3)
                        BlinkElement(MISS_RIGHT, 1);
                }
            }

            if (_bananasMissed == 10)
            {
                GameOver();
                return;
            }

            // If bananas were missed, cancel the previous forward move
            if (_blockedCustomUpdates > 0)
                _bananaPositions = ThreadSafeBananaList().Select(x => x -= 10).ToList();
            else
            {
                // Spawn new row of bananas
                var firstRing = _rng.Next(11, 14);
                _bananaPositions.Add(firstRing);

                // 50% chance to add a second banana
                if (_rng.Next(0, 2) == 0)
                {
                    var secondRing = firstRing;

                    do { secondRing = _rng.Next(11, 14); } while (secondRing == firstRing);
                    _bananaPositions.Add(secondRing);
                }
            }

        }

        private void GameOver()
        {
            _aiaiPosition = -1;
            _level = 0;
            _bananaPositions.Clear();

            GenericGameOverAnimation(new List<string> { BANANA_11, BANANA_12, BANANA_13, BANANA_21, BANANA_22, BANANA_23, BANANA_31, BANANA_32, BANANA_33, BANANA_41, BANANA_42, BANANA_43 });
            Stop();
        }

        private void Victory()
        {
            _aiaiPosition = -1;
            _level = 0;
            _bananaPositions.Clear();

            GenericVictoryAnimation(new List<string> { LEVEL_1, LEVEL_2, LEVEL_3 });
            Stop();
        }

        private List<int> ThreadSafeBananaList() => new List<int>(_bananaPositions);
    }
}
