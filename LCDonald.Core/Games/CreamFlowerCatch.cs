using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    /// <summary>
    /// This game is basically a rebrand of abanana, except that the game doesn't pause for a cycle when flowers are missed.
    /// </summary>
    public class CreamFlowerCatch : LCDGameBase
    {
        public override string ShortName => "cflower";
        public override string Name => "Cream Flower Catch (2004)";

        #region SVG Group Names
        public const string LEVEL_1 = "level-1";
        public const string LEVEL_2 = "level-2";
        public const string LEVEL_3 = "level-3";
        public const string FLOWER_11 = "flower-11";
        public const string FLOWER_12 = "flower-12";
        public const string FLOWER_13 = "flower-13";
        public const string FLOWER_21 = "flower-21";
        public const string FLOWER_22 = "flower-22";
        public const string FLOWER_23 = "flower-23";
        public const string FLOWER_31 = "flower-31";
        public const string FLOWER_32 = "flower-32";
        public const string FLOWER_33 = "flower-33";
        public const string FLOWER_41 = "flower-41";
        public const string FLOWER_42 = "flower-42";
        public const string FLOWER_43 = "flower-43";
        public const string CREAM_LEFT = "cream-left";
        public const string CREAM_RIGHT = "cream-right";
        public const string CREAM_CENTER = "cream-center";
        public const string MISS_LEFT = "miss-left";
        public const string MISS_RIGHT = "miss-right";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                LEVEL_1, LEVEL_2, LEVEL_3,


                FLOWER_11,  FLOWER_12,  FLOWER_13,
                FLOWER_21,  FLOWER_22,  FLOWER_23,
                FLOWER_31,  FLOWER_32,  FLOWER_33,
                FLOWER_41,  FLOWER_42,  FLOWER_43,

                    MISS_LEFT,      MISS_RIGHT,
               CREAM_LEFT,  CREAM_CENTER,  CREAM_RIGHT
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Left",
                    Description = "Move Cream Left",
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    Description = "Move Cream Right",
                    KeyCode = 25, // right
                }
            };          
        }

        private int _creamPosition;
        private int _flowersCollected;
        private int _flowersMissed;
        private int _level;

        private List<int> _flowerPositions = new List<int>();

        protected override List<string> GetVisibleElements()
        {
            var elements = new List<string>();

            if (_level >= 1)
                elements.Add(LEVEL_1);
            if (_level >= 2)
                elements.Add(LEVEL_2);
            if (_level >= 3)
                elements.Add(LEVEL_3);

            elements.Add(GetCreamElement());

            foreach (var ringPos in ThreadSafeFlowerList())
                elements.Add("flower-" + ringPos);

            return elements;
        }

        private string GetCreamElement()
        {
            if (_creamPosition == 1)
                return CREAM_LEFT;
            else if (_creamPosition == 2)
                return CREAM_CENTER;
            else if (_creamPosition == 3)
                return CREAM_RIGHT;

            return "";
        }

        public override void InitializeGameState()
        {
            _creamPosition = 2;
            _flowersCollected = 0;
            _flowersMissed = 0;
            _level = _isEndlessMode ? 5 : 0;

            _flowerPositions = new List<int>();
            _customUpdateSpeed = _isEndlessMode ? 600 : 900;

            StartupMusic();
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Left" && _creamPosition > 1)
                {
                    _creamPosition--;
                }
                else if (input.Name == "Right" && _creamPosition < 3)
                {
                    _creamPosition++;
                }
            }
        }

        protected override void UpdateCore()
        {
            // Collect flowers if cream is in front
            foreach (var fPos in ThreadSafeFlowerList())
            {
                var digit = fPos % 10;

                if (digit == _creamPosition && fPos > 40)
                {
                    QueueSound(new LCDGameSound("../common/hit.ogg"));
                    _flowerPositions.Remove(fPos);
                    _flowersCollected++;
                }
            }

        }

        public override void CustomUpdate()
        {
            if (_flowersCollected >= 30 && !_isEndlessMode)
            {
                QueueSound(new LCDGameSound("../common/level_up.ogg"));
                _flowersMissed = 0;
                _flowersCollected = 0;
                _level++;
                // Speed up
                _customUpdateSpeed -= 175;
            }

            if (_level == 4)
            {
                Victory();
                return;
            }


            // Move flowers forward
            _flowerPositions = ThreadSafeFlowerList().Select(x => x += 10).ToList();

            foreach (var ringPos in ThreadSafeFlowerList())
            {
                if (ringPos > 50)
                {
                    QueueSound(new LCDGameSound("../common/miss.ogg"));
                    _flowerPositions.Remove(ringPos);
                    _flowersMissed++;

                    var verticalPos = ringPos % 10;

                    // Show miss indicators depending on which flowers were missed
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

            if (_flowersMissed == 10)
            {
                GameOver();
                return;
            }

            // Spawn new row of flowers
            var firstFlower = _rng.Next(11, 14);
            _flowerPositions.Add(firstFlower);

            // 50% chance to add a second flower
            if (_rng.Next(0, 2) == 0)
            {
                var secondFlower = firstFlower;

                do { secondFlower = _rng.Next(11, 14); } while (secondFlower == firstFlower);
                _flowerPositions.Add(secondFlower);
            }

        }

        private void GameOver()
        {
            _creamPosition = -1;
            _level = 0;
            _flowerPositions.Clear();

            GenericGameOverAnimation(new List<string> { FLOWER_11, FLOWER_12, FLOWER_13, FLOWER_21, FLOWER_22, FLOWER_23, FLOWER_31, FLOWER_32, FLOWER_33, FLOWER_41, FLOWER_42, FLOWER_43 });
            Stop();
        }

        private void Victory()
        {
            _creamPosition = -1;
            _level = 0;
            _flowerPositions.Clear();

            GenericVictoryAnimation(new List<string> { LEVEL_1, LEVEL_2, LEVEL_3 });
            Stop();
        }

        private List<int> ThreadSafeFlowerList() => new List<int>(_flowerPositions);
    }
}
