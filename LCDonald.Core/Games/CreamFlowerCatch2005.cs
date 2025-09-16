using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{

    public class CreamFlowerCatch2005 : LCDGameBase
    {


#if BURGER
        public override string ShortName => "heartmooncatch";
        public override string Name => "Honey's Heart-Moon Catch";
#else
        public override string ShortName => "cflower2";
        public override string Name => "Cream Flower Catch (2005)";
#endif

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
        public const string HIT_LEFT = "hit-left";
        public const string HIT_CENTER = "hit-center";
        public const string HIT_RIGHT = "hit-right";
        public const string CREAM_LEFT = "cream-left";
        public const string CREAM_RIGHT = "cream-right";
        public const string CREAM_CENTER = "cream-center";
        public const string MISS_LEFT = "miss-left";
        public const string MISS_RIGHT = "miss-right";
        public const string MISS_1 = "miss-1";
        public const string MISS_2 = "miss-2";
        public const string MISS_3 = "miss-3";
        public const string MISS_4 = "miss-4";
        public const string MISS_5 = "miss-5";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                LEVEL_1, LEVEL_2, LEVEL_3,


                FLOWER_11,  FLOWER_12,  FLOWER_13,
                FLOWER_21,  FLOWER_22,  FLOWER_23,
                FLOWER_31,  FLOWER_32,  FLOWER_33,
 FLOWER_41, HIT_LEFT, HIT_CENTER, FLOWER_42, HIT_RIGHT, FLOWER_43,

               CREAM_LEFT,  CREAM_CENTER,  CREAM_RIGHT,
     MISS_LEFT, MISS_1, MISS_2, MISS_3, MISS_4, MISS_5, MISS_RIGHT
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

            // Show miss-x elements depending on the amount of flowers missed
            if (_flowersMissed >= 1)
                elements.Add(MISS_1);
            if (_flowersMissed >= 2)
                elements.Add(MISS_2);
            if (_flowersMissed >= 3)
                elements.Add(MISS_3);
            if (_flowersMissed >= 4)
                elements.Add(MISS_4);
            if (_flowersMissed >= 5)
                elements.Add(MISS_5);

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
            _flowersMissed = _isEndlessMode ? 4 : 0;
            _level = _isEndlessMode ? 5 : 0;

            _flowerPositions = new List<int>();
            _customUpdateSpeed = _isEndlessMode ? 600 : 900;

            StartupMusic("game_start.ogg");
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
                    QueueSound(new LCDGameSound("hit.ogg"));
                    _flowerPositions.Remove(fPos);
                    _flowersCollected++;
                    IncrementScore();

                    // blink the matching hit bubble
                    switch (digit)
                    {
                        case 1:
                            BlinkElement(HIT_LEFT, 1);
                            break;
                        case 2:
                            BlinkElement(HIT_CENTER, 1);
                            break;
                        case 3:
                            BlinkElement(HIT_RIGHT, 1);
                            break;
                    }
                }
            }

        }

        public override void CustomUpdate()
        {

            if (_flowersCollected >= 30 && !_isEndlessMode)
            {
                QueueSound(new LCDGameSound("level_up.ogg"));
                _flowersMissed = 0;
                _flowersCollected = 0;
                _level++;

                // blink levels and pause spawn
                if (_level >= 1)
                    BlinkElement(LEVEL_1, 3);
                if (_level >= 2)
                    BlinkElement(LEVEL_2, 3);
                if (_level >= 3)
                    BlinkElement(LEVEL_3, 3);

                // Speed up
                _customUpdateSpeed -= 175;

                BlockCustomUpdates(1);
                return;
            }

            if (_level == 4)
            {
                Victory();
                return;
            }


            // Move flowers forward
            _flowerPositions = ThreadSafeFlowerList().Select(x => x += 10).ToList();

            foreach (var flowerPos in ThreadSafeFlowerList())
            {
                if (flowerPos > 50)
                {
                    QueueSound(new LCDGameSound("miss.ogg"));
                    _flowerPositions.Remove(flowerPos);
                    _flowersMissed++;

                    var verticalPos = flowerPos % 10;

                    // Show miss indicators 
                    BlinkElement(MISS_LEFT, 1);
                    BlinkElement(MISS_RIGHT, 1);
                }
            }

            if (_flowersMissed == 5)
            {
                GameOver();
                return;
            }

            // Spawn new row of flowers
            var firstFlower = _rng.Next(11, 14);
            _flowerPositions.Add(firstFlower);

            // 50% chance to add a second flower if on level 2 and up
            if (_rng.Next(0, 2) == 0 && _level >= 2)
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

            GenericGameOverAnimation(new List<string> { MISS_LEFT, MISS_1, MISS_2, MISS_3, MISS_4, MISS_5, MISS_RIGHT }, "game_over.ogg");
            Stop();
        }

        private void Victory()
        {
            _creamPosition = -1;
            _level = 0;
            _flowerPositions.Clear();

            // Rising flowers + cream leftcenterright, 3x 
            QueueSound(new LCDGameSound("game_win.ogg"));

            var victoryFrame1l = new List<string> { FLOWER_41, FLOWER_42, FLOWER_43, CREAM_LEFT };
            var victoryFrame2l = new List<string> { FLOWER_31, FLOWER_32, FLOWER_33, CREAM_LEFT };
            var victoryFrame3l = new List<string> { FLOWER_21, FLOWER_22, FLOWER_23, CREAM_LEFT };
            var victoryFrame4l = new List<string> { FLOWER_11, FLOWER_12, FLOWER_13, CREAM_LEFT };
            var victoryFrame1c = new List<string> { FLOWER_41, FLOWER_42, FLOWER_43, CREAM_CENTER };
            var victoryFrame2c = new List<string> { FLOWER_31, FLOWER_32, FLOWER_33, CREAM_CENTER };
            var victoryFrame3c = new List<string> { FLOWER_21, FLOWER_22, FLOWER_23, CREAM_CENTER };
            var victoryFrame4c = new List<string> { FLOWER_11, FLOWER_12, FLOWER_13, CREAM_CENTER };
            var victoryFrame1r = new List<string> { FLOWER_41, FLOWER_42, FLOWER_43, CREAM_RIGHT };
            var victoryFrame2r = new List<string> { FLOWER_31, FLOWER_32, FLOWER_33, CREAM_RIGHT };
            var victoryFrame3r = new List<string> { FLOWER_21, FLOWER_22, FLOWER_23, CREAM_RIGHT };
            var victoryFrame4r = new List<string> { FLOWER_11, FLOWER_12, FLOWER_13, CREAM_RIGHT };


            var victoryAnimation = new List<List<string>> { victoryFrame1l, victoryFrame1l, victoryFrame1c, victoryFrame1c, victoryFrame2r, victoryFrame2r, victoryFrame2l, victoryFrame2l, 
                                                            victoryFrame3c, victoryFrame3c, victoryFrame3r, victoryFrame3r, victoryFrame4l, victoryFrame4l, victoryFrame4c, victoryFrame4c,
                                                            victoryFrame1r, victoryFrame1r, victoryFrame1l, victoryFrame1l, victoryFrame2c, victoryFrame2c, victoryFrame2r, victoryFrame2r, 
                                                            victoryFrame3l, victoryFrame3l, victoryFrame3c, victoryFrame3c, victoryFrame4r, victoryFrame4r, victoryFrame4l, victoryFrame4l,
                                                            victoryFrame1c, victoryFrame1c, victoryFrame1r, victoryFrame1r, victoryFrame2l, victoryFrame2l, victoryFrame2c, victoryFrame2c, 
                                                            victoryFrame3r, victoryFrame3r, victoryFrame3l, victoryFrame3l, victoryFrame4c, victoryFrame4c, victoryFrame4r,victoryFrame4r };
            PlayAnimation(victoryAnimation);
            Stop();
        }

        private List<int> ThreadSafeFlowerList() => new List<int>(_flowerPositions);
    }
}
