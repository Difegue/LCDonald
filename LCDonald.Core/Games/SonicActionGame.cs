using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class SonicActionGame : LCDGameBase
    {
        public override string ShortName => "sagame";
#if BURGER
        public override string Name => "Kola Bear's Platform Adventure";
#else
        public override string Name => "Sonic Action Game (2003)";
#endif

        #region SVG Group Names
        public const string LEVEL_1 = "level-1";
        public const string LEVEL_2 = "level-2";
        public const string LEVEL_3 = "level-3";
        public const string PLATFORM_11 = "platform-11";
        public const string PLATFORM_12 = "platform-12";
        public const string PLATFORM_13 = "platform-13";
        public const string PLATFORM_14 = "platform-14";
        public const string PLATFORM_21 = "platform-21";
        public const string PLATFORM_22 = "platform-22";
        public const string PLATFORM_23 = "platform-23";
        public const string PLATFORM_24 = "platform-24";
        public const string RING_1 = "ring-1";
        public const string RING_2 = "ring-2";
        public const string RING_3 = "ring-3";
        public const string HIT_1 = "hit-1";
        public const string HIT_2 = "hit-2";
        public const string SONIC_1 = "sonic-1";
        public const string SONIC_2 = "sonic-2";
        public const string SONIC_3 = "sonic-3";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                                        LEVEL_1, LEVEL_2, LEVEL_3,

                  SONIC_3,     RING_1,      RING_2,      RING_3,
                PLATFORM_21, PLATFORM_22, PLATFORM_23, PLATFORM_24,
                  SONIC_2,     HIT_2,
                PLATFORM_11, PLATFORM_12, PLATFORM_13, PLATFORM_14,
                  SONIC_1,     HIT_1
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Jump",
                    Description = "Jump on platforms",
                    KeyCode = 18, // space
                }
            };
        }

        private int _sonicPosition;
        private int _platformsDodged;
        private int _platformsHit;
        private int _level;
        private int _blockSpawnCounter;

        private List<int> _platformPositions = new List<int>();
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

            elements.Add(GetSonicElement());

            foreach (var platform in ThreadSafePlatformList())
                elements.Add("platform-" + platform);

            foreach (var ring in _ringPositions)
                elements.Add("ring-" + ring);

            return elements;
        }

        private string GetSonicElement() => "sonic-" + _sonicPosition;

        public override void InitializeGameState()
        {
            _sonicPosition = 1;
            _platformsDodged = 0;
            _platformsHit = 0;
            _level = _isEndlessMode ? 5 : 0;

            _platformPositions = new List<int>();
            _ringPositions = new List<int>();

            _customUpdateSpeed = _isEndlessMode ? 200 : 800;

            StartupMusic();
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Jump")
                {
                    // can only double jump if there's a bottom platform in front of sonic
                    // (meaning platform-12 is on)
                    if (_sonicPosition == 2 && _platformPositions.Contains(12))
                    {
                        _sonicPosition = 3;
                    }
                    else if (_sonicPosition == 1)
                    {
                        _sonicPosition = 2;
                    }
                }
            }
        }

        protected override void UpdateCore()
        {
            // Unused in this game
        }

        public override void CustomUpdate()
        {
            if (_platformsDodged >= 20 && !_isEndlessMode)
            {
                QueueSound(new LCDGameSound("../common/level_up.ogg"));
                _platformsHit = 0;
                _platformsDodged = 0;
                _level++;
                // Speed up
                _customUpdateSpeed -= 125;
            }

            if (_level == 4)
            {
                Victory();
                return;
            }

            // Move platforms & rings forward
            bool hasDodgedPlatform = false;
            bool hasBeenHit = false;
            _platformPositions = ThreadSafePlatformList().Select(x => x-= 1).ToList();
            _ringPositions = _ringPositions.Select(x => x-= 1).ToList();

            if (_ringPositions.Contains(0) && _sonicPosition == 3)
            {
                _ringPositions.Remove(0);
                hasDodgedPlatform = true;
            }

            var platformListSnapshot = ThreadSafePlatformList();
            foreach (var platform in platformListSnapshot)
            {
                if (platform == 10 || platform == 20)
                    _platformPositions.Remove(platform);

                if (platform == 11 && _sonicPosition == 1)
                {
                    hasBeenHit = true;
                    BlinkElement(HIT_1, 1);
                }

                if (platform == 21 && _sonicPosition == 2)
                {
                    hasBeenHit = true;
                    BlinkElement(HIT_2, 1);
                }

                if (platform == 11 && _sonicPosition > 1)
                {
                    // If this is not a L-block platform, count this as a dodged platform
                    if (!platformListSnapshot.Contains(22))
                        hasDodgedPlatform = true;
                }
            }

            // Apply gravity to sonic - unless there's a top platform he's standing on
            if (_sonicPosition > 1 && !_platformPositions.Contains(21))
            {
                // Land on platform if there's one
                if (_platformPositions.Contains(11))
                    _sonicPosition = 2;
                else // Back to the ground
                    _sonicPosition = 1;
            }

            if (hasBeenHit)
            {
                QueueSound(new LCDGameSound("../common/miss.ogg"));
                _platformPositions.Remove(11);
                _platformPositions.Remove(12);
                _platformPositions.Remove(21);
                _platformPositions.Remove(22);

                _ringPositions.Remove(1);
                _platformsHit++;
            }
            else if (hasDodgedPlatform)
            {
                QueueSound(new LCDGameSound("../common/hit.ogg"));
                _platformsDodged++;
                IncrementScore();
            }

            if (_platformsHit == 5)
            {
                GameOver();
                return;
            }

            // Don't spawn platforms ?
            if (_blockSpawnCounter > 0)
            {
                _blockSpawnCounter--;
            }
            else
            {
                // Spawn platform: 3 random possibilities
                // Note: The original game seems to loop through predefined sets of platforms instead, but it's easier to just rng it..
                switch (_rng.Next(1, 4))
                {
                    // Single platform
                    case 1:
                        _platformPositions.Add(14);
                        // Wait one cycle before spawning another platform
                        _blockSpawnCounter = 1;
                        break;
                    // L-block platform set
                    case 2:
                        _platformPositions.Add(14);
                        _platformPositions.Add(15);
                        _platformPositions.Add(25);
                        _ringPositions.Add(4);
                        // Wait two cycles before spawning another platform
                        _blockSpawnCounter = 2;
                        break;
                    // No platform
                    case 3:
                        // Wait one cycle before spawning another platform
                        _blockSpawnCounter = 1;
                        break;
                }
            }
        }

        private void GameOver()
        {
            _sonicPosition = -1;
            _level = 0;
            _platformPositions.Clear();
            _ringPositions.Clear();

            GenericGameOverAnimation(new List<string> { SONIC_1, HIT_1, PLATFORM_12 });
            Stop();
        }

        private void Victory()
        {
            _sonicPosition = -1;
            _level = 0;
            _platformPositions.Clear();
            _ringPositions.Clear();

            GenericVictoryAnimation(new List<string> { LEVEL_1, LEVEL_2, LEVEL_3 });
            Stop();
        }

        private List<int> ThreadSafePlatformList() => new List<int>(_platformPositions);
    }
}
