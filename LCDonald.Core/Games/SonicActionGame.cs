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
        public override string Name => "Sonic Action Game (2003)";

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

        private List<int> _platformPositions = new List<int>();
        private int _blockSpawnCounter;

        private Random _rng = new Random();

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

            return elements;
        }

        private string GetSonicElement() => "sonic-" + _sonicPosition;

        public override void InitializeGameState()
        {
            _sonicPosition = 1;
            _platformsDodged = 0;
            _platformsHit = 0;
            _level = 0;

            _platformPositions = new List<int>();

            _customUpdateSpeed = 800;
            QueueSound(new LCDGameSound("../common/game_start.ogg"));

            // Wait for sound to end before spawning platforms 
            // 2x custom loop is about the correct speed.
            _blockSpawnCounter = 2;
            _isInputBlocked = true;
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
            if (_platformsDodged >= 20)
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

            // Don't spawn platforms ?
            if (_blockSpawnCounter > 0)
            {
                _blockSpawnCounter--;
                _isInputBlocked = false;
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

            // TODO: Move platforms & rings forward
            _platformPositions = ThreadSafePlatformList().Select(x => x--).ToList();

            bool hasDodgedPlatform = false;
            bool hasBeenHit = false;
            foreach (var platform in ThreadSafePlatformList())
            {
                if (platform > 50)
                {
                    _platformPositions.Remove(platform);
                    var horizontalPos = platform % 10;

                    if (horizontalPos == _sonicPosition)
                    {
                        QueueSound(new LCDGameSound("../common/miss.ogg"));
                        _platformsHit++;
                        hasBeenHit = true;

                        // Show miss indicators depending on which rings were missed
                        if (horizontalPos == 1)
                            BlinkElement(HIT_1, 1);
                        else if (horizontalPos == 2)
                        {
                            BlinkElement(HIT_2, 1);
                        }
                        //else if (horizontalPos == 3)
                            //BlinkElement(HIT_RIGHT, 1);
                    }
                    else
                        hasDodgedPlatform = true;
                }
            }

            // Only count a row as dodged if the player hasn't been hit
            if (hasDodgedPlatform && !hasBeenHit)
            {
                QueueSound(new LCDGameSound("../common/hit.ogg"));
                _platformsDodged++;
            }

            if (_platformsHit == 5)
            {
                GameOver();
                return;
            }


            // Spawn new row of cars
            var firstCar = _rng.Next(11, 14);
            _platformPositions.Add(firstCar);

            // 50% chance to add a second car
            if (_rng.Next(0, 2) == 0)
            {
                var secondCar = firstCar;

                do { secondCar = _rng.Next(11, 14); } while (secondCar == firstCar);
                _platformPositions.Add(secondCar);
            }

        }

        private void GameOver()
        {
            _sonicPosition = -1;
            _level = 0;
            _platformPositions.Clear();

            QueueSound(new LCDGameSound("../common/game_over.ogg"));

            var gameOverFrame1 = new List<string> { SONIC_1, HIT_1, PLATFORM_11 };
            var gameOverFrame2 = new List<string>();

            // slow 4x blink
            var gameOverAnimation = new List<List<string>> { gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2};
            PlayAnimation(gameOverAnimation);
            Stop();
        }

        private void Victory()
        {
            _sonicPosition = -1;
            _level = 0;
            _platformPositions.Clear();

            QueueSound(new LCDGameSound("../common/game_win.ogg"));

            // 3x levels + 10x all
            var victoryFrame1 = new List<string> { LEVEL_1, LEVEL_2, LEVEL_3 };
            var victoryFrame2 = new List<string>();
            var victoryFrame3 = GetAllGameElements();

            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2};
            PlayAnimation(victoryAnimation);
            Stop();
        }

        private List<int> ThreadSafePlatformList() => new List<int>(_platformPositions);
    }
}
