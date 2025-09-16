using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class SonicSpeedway : LCDGameBase
    {
#if BURGER
        public override string ShortName => "speedway";
        public override string Name => "Burger Bard Speedway Racing";
#else
        public override string ShortName => "sspeedway";
        public override string Name => "Sonic Speedway (2003)";
#endif

        #region SVG Group Names
        public const string HIT_LEFT = "hit-left";
        public const string HIT_RIGHT = "hit-right";
        public const string LEVEL_1 = "level-1";
        public const string LEVEL_2 = "level-2";
        public const string LEVEL_3 = "level-3";
        public const string CAR_11 = "car-11";
        public const string CAR_12 = "car-12";
        public const string CAR_13 = "car-13";
        public const string CAR_21 = "car-21";
        public const string CAR_22 = "car-22";
        public const string CAR_23 = "car-23";
        public const string CAR_31 = "car-31";
        public const string CAR_32 = "car-32";
        public const string CAR_33 = "car-33";
        public const string CAR_41 = "car-41";
        public const string CAR_42 = "car-42";
        public const string CAR_43 = "car-43";
        public const string SONIC_CENTER = "sonic-center";
        public const string SONIC_LEFT = "sonic-left";
        public const string SONIC_RIGHT = "sonic-right";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                LEVEL_1, LEVEL_2, LEVEL_3,

                                        CAR_11, CAR_12, CAR_13,
                                CAR_21,   CAR_22,   CAR_23,
                        CAR_31,     CAR_32,     CAR_33,
                    CAR_41,       CAR_42,       CAR_43,
                        HIT_LEFT,       HIT_RIGHT,
                SONIC_LEFT,  SONIC_CENTER,  SONIC_RIGHT
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Left",
                    Description = "Move Sonic Left",
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    Description = "Move Sonic Right",
                    KeyCode = 25, // right
                },
            };
        }

        private int _sonicPosition;
        private int _carRowsDodged;
        private int _carsHit;
        private int _level;

        private List<int> _carPositions = new List<int>();

        protected override List<string> GetVisibleElements()
        {
            var elements = new List<string>();

            if (_level >= 1)
                elements.Add(LEVEL_1);
            if (_level >= 2)
                elements.Add(LEVEL_2);
            if (_level >= 3)
                elements.Add(LEVEL_3);

            elements.Add(GetSonicElement());

            foreach (var carPos in ThreadSafeCarList())
                elements.Add("car-" + carPos);

            return elements;
        }

        private string GetSonicElement()
        {
            if (_sonicPosition == 1)
                return SONIC_LEFT;
            else if (_sonicPosition == 2)
                return SONIC_CENTER;
            else if (_sonicPosition == 3)
                return SONIC_RIGHT;

            return "";
        }

        public override void InitializeGameState()
        {
            _sonicPosition = 2;
            _carRowsDodged = 0;
            _carsHit = 0;
            _level = _isEndlessMode ? 5 : 0;

            _carPositions = new List<int>();
            _customUpdateSpeed = _isEndlessMode ? 300 : 800;
            
            StartupMusic();
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Left" && _sonicPosition > 1)
                {
                    _sonicPosition--;
                }
                else if (input.Name == "Right" && _sonicPosition < 3)
                {
                    _sonicPosition++;
                }
            }
        }

        protected override void UpdateCore()
        {
            // Unused in this game
        }

        public override void CustomUpdate()
        {
            if (_carRowsDodged >= 30 && !_isEndlessMode)
            {
                QueueSound(new LCDGameSound("../common/level_up.ogg"));
                _carsHit = 0;
                _carRowsDodged = 0;
                _level++;
                // Speed up
                _customUpdateSpeed -= 125;
            }

            if (_level == 4)
            {
                Victory();
                return;
            }

            // Move rings forward
            _carPositions = ThreadSafeCarList().Select(x => x += 10).ToList();

            bool hasDodgedCar = false;
            bool hasBeenHit = false;
            foreach (var carPos in ThreadSafeCarList())
            {
                if (carPos > 50)
                {
                    _carPositions.Remove(carPos);
                    var horizontalPos = carPos % 10;

                    if (horizontalPos == _sonicPosition)
                    {
                        QueueSound(new LCDGameSound("../common/miss.ogg"));
                        _carsHit++;
                        hasBeenHit = true;

                        // Show miss indicators depending on which rings were missed
                        if (horizontalPos == 1)
                            BlinkElement(HIT_LEFT, 1);
                        else if (horizontalPos == 2)
                        {
                            BlinkElement(HIT_LEFT, 1);
                            BlinkElement(HIT_RIGHT, 1);

                        }
                        else if (horizontalPos == 3)
                            BlinkElement(HIT_RIGHT, 1);
                    }
                    else
                        hasDodgedCar = true;
                }
            }

            // Only count a row as dodged if the player hasn't been hit
            if (hasDodgedCar && !hasBeenHit)
            {
                QueueSound(new LCDGameSound("../common/hit.ogg"));
                _carRowsDodged++;
                IncrementScore();
            }

            if (_carsHit == 5)
            {
                GameOver();
                return;
            }


            // Spawn new row of cars
            var firstCar = _rng.Next(11, 14);
            _carPositions.Add(firstCar);

            // 50% chance to add a second car
            if (_rng.Next(0, 2) == 0)
            {
                var secondCar = firstCar;

                do { secondCar = _rng.Next(11, 14); } while (secondCar == firstCar);
                _carPositions.Add(secondCar);
            }

        }

        private void GameOver()
        {
            _sonicPosition = -1;
            _level = 0;
            _carPositions.Clear();

            GenericGameOverAnimation(new List<string> { SONIC_CENTER, CAR_42, HIT_LEFT, HIT_RIGHT });
            Stop();
        }

        private void Victory()
        {
            _sonicPosition = -1;
            _level = 0;
            _carPositions.Clear();

            GenericVictoryAnimation(new List<string> { LEVEL_1, LEVEL_2, LEVEL_3 });
            Stop();
        }

        private List<int> ThreadSafeCarList() => new List<int>(_carPositions);
    }
}
