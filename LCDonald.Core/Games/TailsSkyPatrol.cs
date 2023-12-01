using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class TailsSkyPatrol : LCDGameBase
    {
        public override string ShortName => "tskypatrol";
        public override string Name => "Tails' Sky Patrol (2003)";

        #region SVG Group Names
        public const string MISS_CENTER = "miss-center";
        public const string MISS_BOTTOM = "miss-bottom";
        public const string LEVEL_1 = "level-1";
        public const string LEVEL_2 = "level-2";
        public const string LEVEL_3 = "level-3";
        public const string RING_11 = "ring-11";
        public const string RING_12 = "ring-12";
        public const string RING_13 = "ring-13";
        public const string RING_21 = "ring-21";
        public const string RING_22 = "ring-22";
        public const string RING_23 = "ring-23";
        public const string RING_31 = "ring-31";
        public const string RING_32 = "ring-32";
        public const string RING_33 = "ring-33";
        public const string RING_41 = "ring-41";
        public const string RING_42 = "ring-42";
        public const string RING_43 = "ring-43";
        public const string TAILS_TOP = "tails-top";
        public const string TAILS_BOTTOM = "tails-bottom";
        public const string TAILS_CENTER = "tails-center";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                LEVEL_1, LEVEL_2, LEVEL_3,

                RING_11,  RING_21,  RING_31, RING_41,    TAILS_TOP,
                                                MISS_CENTER,
                RING_12,  RING_22,  RING_32, RING_42,    TAILS_CENTER,
                                                MISS_BOTTOM,
                RING_13,  RING_23,  RING_33, RING_43,    TAILS_BOTTOM
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Up",
                    Description = "Move Tails Up",
                    KeyCode = 24, // up
                },
                new LCDGameInput
                {
                    Name = "Down",
                    Description = "Move Tails Down",
                    KeyCode = 26, // down
                }
            };
        }

        private int _tailsPosition;
        private int _ringsCollected;
        private int _ringsMissed;
        private int _level;

        private List<int> _ringPositions = new List<int>();

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

            foreach (var ringPos in ThreadSafeRingList())
                elements.Add("ring-" + ringPos);

            return elements;
        }

        private string GetTailsElement()
        {
            if (_tailsPosition == 1)
                return TAILS_TOP;
            else if (_tailsPosition == 2)
                return TAILS_CENTER;
            else if (_tailsPosition == 3)
                return TAILS_BOTTOM;

            return "";
        }

        public override void InitializeGameState()
        {
            _tailsPosition = 2;
            _ringsCollected = 0;
            _ringsMissed = 0;
            _level = _isEndlessMode ? 5 : 0;

            _ringPositions = new List<int>();
            _customUpdateSpeed = _isEndlessMode ? 400 : 900;

            StartupMusic();
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Up" && _tailsPosition > 1)
                {
                    _tailsPosition--;
                }
                else if (input.Name == "Down" && _tailsPosition < 3)
                {
                    _tailsPosition++;
                }
            }
        }

        protected override void UpdateCore()
        {
            // Collect rings if tails is in front
            foreach (var ringPos in ThreadSafeRingList())
            {
                var digit = ringPos % 10;

                if (digit == _tailsPosition && ringPos > 40)
                {
                    QueueSound(new LCDGameSound("../common/hit.ogg"));
                    _ringPositions.Remove(ringPos);
                    _ringsCollected++;
                    IncrementScore();
                }
            }

        }

        public override void CustomUpdate()
        {
            if (_ringsCollected >= 30 && !_isEndlessMode)
            {
                QueueSound(new LCDGameSound("../common/level_up.ogg"));
                _ringsMissed = 0;
                _ringsCollected = 0;
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
            _ringPositions = ThreadSafeRingList().Select(x => x += 10).ToList();

            foreach (var ringPos in ThreadSafeRingList())
            {
                if (ringPos > 50)
                {
                    QueueSound(new LCDGameSound("../common/miss.ogg"));
                    _ringPositions.Remove(ringPos);
                    _ringsMissed++;

                    // Pause ring spawn/advance if there was a miss
                    BlockCustomUpdates(1);

                    var verticalPos = ringPos % 10;

                    // Show miss indicators depending on which rings were missed
                    if (verticalPos == 1)
                        BlinkElement(MISS_CENTER, 1);
                    else if (verticalPos == 2)
                    {
                        BlinkElement(MISS_CENTER, 1);
                        BlinkElement(MISS_BOTTOM, 1);

                    }
                    else if (verticalPos == 3)
                        BlinkElement(MISS_BOTTOM, 1);
                }
            }

            if (_ringsMissed == 10)
            {
                GameOver();
                return;
            }

            // If rings were missed, cancel the previous forward move
            if (_blockedCustomUpdates > 0)
                _ringPositions = ThreadSafeRingList().Select(x => x -= 10).ToList();
            else
            {
                // Spawn new row of rings
                var firstRing = _rng.Next(11, 14);
                _ringPositions.Add(firstRing);

                // 50% chance to add a second ring
                if (_rng.Next(0, 2) == 0)
                {
                    var secondRing = firstRing;

                    do { secondRing = _rng.Next(11, 14); } while (secondRing == firstRing);
                    _ringPositions.Add(secondRing);
                }
            }

        }

        private void GameOver()
        {
            _tailsPosition = -1;
            _level = 0;
            _ringPositions.Clear();

            GenericGameOverAnimation(new List<string> { RING_11, RING_12, RING_13, RING_21, RING_22, RING_23, RING_31, RING_32, RING_33, RING_41, RING_42, RING_43 });
            Stop();
        }

        private void Victory()
        {
            _tailsPosition = -1;
            _level = 0;
            _ringPositions.Clear();

            GenericVictoryAnimation(new List<string> { LEVEL_1, LEVEL_2, LEVEL_3 });
            Stop();
        }

        private List<int> ThreadSafeRingList() => new List<int>(_ringPositions);
    }
}
