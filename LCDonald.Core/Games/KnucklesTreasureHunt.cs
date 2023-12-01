using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class KnucklesTreasureHunt : LCDGameBase
    {
        public override string ShortName => "ktreasure";
        public override string Name => "Knuckles Treasure Hunt (2005)";

        #region SVG Group Names
        public const string EMERALD_1 = "emerald-1";
        public const string EMERALD_2 = "emerald-2";
        public const string EMERALD_3 = "emerald-3";
        public const string EMERALD_4 = "emerald-4";
        public const string EMERALD_5 = "emerald-5";
        public const string PLATFORM_2 = "platform-2";
        public const string PLATFORM_3 = "platform-3";
        public const string PLATFORM_12 = "platform-12";
        public const string PLATFORM_13 = "platform-13";
        public const string PLATFORM_22 = "platform-22";
        public const string PLATFORM_23 = "platform-23";
        public const string PLATFORM_32 = "platform-32";
        public const string PLATFORM_33 = "platform-33";
        public const string KNUCKLES_11 = "knux-11";
        public const string KNUCKLES_12 = "knux-12";
        public const string KNUCKLES_13 = "knux-13";
        public const string KNUCKLES_14 = "knux-14";
        public const string KNUCKLES_21 = "knux-21";
        public const string KNUCKLES_22 = "knux-22";
        public const string KNUCKLES_23 = "knux-23";
        public const string KNUCKLES_24 = "knux-24";
        public const string KNUCKLES_31 = "knux-31";
        public const string KNUCKLES_32 = "knux-32";
        public const string KNUCKLES_33 = "knux-33";
        public const string KNUCKLES_34 = "knux-34";
        public const string FALL_UP = "fall-up";
        public const string FALL_DOWN = "fall-down";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                                            FALL_UP,

                                     PLATFORM_2, PLATFORM_3,

             EMERALD_1, KNUCKLES_11, KNUCKLES_12, KNUCKLES_13, KNUCKLES_14, EMERALD_2,
                                     PLATFORM_12, PLATFORM_13,

             EMERALD_3, KNUCKLES_21, KNUCKLES_22, KNUCKLES_23, KNUCKLES_24, EMERALD_4,
                                     PLATFORM_22, PLATFORM_23,

                        KNUCKLES_31, KNUCKLES_32, KNUCKLES_33, KNUCKLES_34, EMERALD_5,
                                     PLATFORM_32, PLATFORM_33,

                                            FALL_DOWN,
            };
        }

        // Spawnlists gleaned by looking at the real game. Platforms spawn following this looping pattern.
        public static List<bool> SPAWNMAP_LEFT = new List<bool> {true, false, true, false, true, false, true, true, false, true, false, true, false, false };
        public static List<bool> SPAWNMAP_RIGHT = new List<bool> {true, false, false, false, true, false, true, false, true, false, false, true };

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Left",
                    Description = "Move Knuckles Left",
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    Description = "Move Knuckles Right",
                    KeyCode = 25, // right
                },
            };
        }
        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                _hasKnucklesMoved = true;
                if (input.Name == "Left" && _knucklesPosition % 10 > 1)
                {
                    _knucklesPosition--;
                    QueueSound(new LCDGameSound("move.ogg"));
                }
                else if (input.Name == "Right" && _knucklesPosition % 10 < 4)
                {
                    _knucklesPosition++;
                    QueueSound(new LCDGameSound("move.ogg"));
                }
            }
        }

        private int _knucklesPosition;
        private int _lifeCount;
        private int _level;

        private int _emeraldsCollected;
        private int _emeraldTimeLimit;
        private List<int> _platformPositions;
        private List<int> _emeraldPositions;
        
        private int _platformsToMoveNextUpdate;
        private int _currentLeftSpawn;
        private int _currentRightSpawn;
        private bool _isFalling;
        private bool _isFirstEmerald;
        private bool _hasKnucklesMoved;

        protected override List<string> GetVisibleElements()
        {
            var elements = GetKnucklesAndPlatformElements();

            foreach (var emerald in ThreadSafeEmeraldList())
                elements.Add("emerald-" + emerald);

            return elements;
        }

        private List<string> GetKnucklesAndPlatformElements() => new List<string>(GetPlatformElements()) { GetKnucklesElement() };

        private List<string> GetPlatformElements()
        {
            var elements = new List<string>();
            foreach (var ptf in ThreadSafePlatformList())
                elements.Add("platform-" + ptf);
            return elements;
        }

        private string GetKnucklesElement()
        {
            if (_knucklesPosition != -1)
                return "knux-" + _knucklesPosition;

            return "";
        }

        public override void InitializeGameState()
        {
            _lifeCount = 3;
            _level = _isEndlessMode ? 7 : 1;

            _emeraldsCollected = 0;
            _isFirstEmerald = true;

            _knucklesPosition = 31;
            _platformsToMoveNextUpdate = 2;
            _currentLeftSpawn = 0;
            _currentRightSpawn = 0;
            _platformPositions = new List<int>();
            _emeraldPositions = new List<int>();

            _customUpdateSpeed = _isEndlessMode ? 400 : 1000;
            StartupMusic("game_start.ogg", 2);
        }

        private void EmeraldSpawnAnimation()
        {
            _isInputBlocked = true;
            _hasKnucklesMoved = false;

            // Spawn as many emeralds as the current level
            if (_level >= 5)
            {
                _emeraldPositions = new List<int> { 1, 2, 3, 4, 5 };
            }
            else for (var i = 0; i < _level; i++)
            {
                var emeraldPosition = GetRandomEmeraldPosition();
                _emeraldPositions.Add(emeraldPosition);
            }

            // Play animation, emerald goes from 1 to 5, shows actual positions, then disappears
            var spawnFrame1 = new List<string>(GetKnucklesAndPlatformElements()) { EMERALD_3 };
            var spawnFrame2 = new List<string>(GetKnucklesAndPlatformElements()) { EMERALD_1 };
            var spawnFrame3 = new List<string>(GetKnucklesAndPlatformElements()) { EMERALD_2 };
            var spawnFrame4 = new List<string>(GetKnucklesAndPlatformElements()) { EMERALD_4 };
            var spawnFrame5 = new List<string>(GetKnucklesAndPlatformElements()) { EMERALD_5 };
            var spawnFrame6 = new List<string>(GetVisibleElements());
            var spawnFrame7 = new List<string>(GetKnucklesAndPlatformElements());

            var spawnAnimation = new List<List<string>> { spawnFrame1, spawnFrame1, spawnFrame1, spawnFrame1, spawnFrame1, spawnFrame1,
                                                          spawnFrame2, spawnFrame2, spawnFrame2, spawnFrame2, spawnFrame2, spawnFrame2,
                                                          spawnFrame3, spawnFrame3, spawnFrame3, spawnFrame3, spawnFrame3, spawnFrame3,
                                                          spawnFrame4, spawnFrame4, spawnFrame4, spawnFrame4, spawnFrame4, spawnFrame4,
                                                          spawnFrame5, spawnFrame5, spawnFrame5, spawnFrame5, spawnFrame5, spawnFrame5,
                                                          spawnFrame6, spawnFrame6, spawnFrame6, spawnFrame6, spawnFrame6, spawnFrame6,
                                                          spawnFrame7, spawnFrame7, spawnFrame7, spawnFrame7, spawnFrame7, spawnFrame7 };
            QueueSound(new LCDGameSound("emerald_spawn.ogg"));
            PlayAnimation(spawnAnimation);

            // 10 + level cycles before emeralds start blinking
            _emeraldTimeLimit = 10 + _level;

            _isInputBlocked = false;
        }

        private int GetRandomEmeraldPosition()
        {
            if (ThreadSafeEmeraldList().Count == 5)
                return -1;

            if (_isFirstEmerald)
            {
                // First emerald is always at position 4
                _isFirstEmerald = false;
                return 4;
            }

            int position = 0;
            while (position == 0 || ThreadSafeEmeraldList().Contains(position))
            {
                position = _rng.Next(1, 6);
            }

            return position;
        }

        private void FallAnimation()
        {
            if (_isFalling) return;

            _isFalling = true;
            _isInputBlocked = true;
            _emeraldPositions.Clear();

            
            var fallFrame1 = new List<string>(GetPlatformElements()) { FALL_DOWN };
            if (_knucklesPosition == 2)
            {
                // Special fallup case, just blink FALL_UP twice
                fallFrame1 = new List<string>(GetPlatformElements()) { FALL_UP };
            }
            else
            {
                // Center knuckles in case he's in the border zones
                if (_knucklesPosition % 10 == 1)
                    _knucklesPosition++;
                if (_knucklesPosition % 10 == 4)
                    _knucklesPosition--;

                // Dynamic amount of frames depending on the current player position
                // Fall all the way to the bottom, then blink FALL_DOWN twice
                while (_knucklesPosition <= 33)
                {
                    QueueSound(new LCDGameSound("fall.ogg"));

                    var fallFrame = GetKnucklesAndPlatformElements();
                    PlayAnimation(new List<List<string>> { fallFrame, fallFrame, fallFrame, fallFrame, fallFrame, fallFrame });

                    _knucklesPosition += 10;
                }
            }

            var fallFrame2 = new List<string>(GetPlatformElements());

            var endAnimation = new List<List<string>> { fallFrame1, fallFrame1, fallFrame1, fallFrame1, 
                                                        fallFrame2, fallFrame2, fallFrame2, fallFrame2, 
                                                        fallFrame1, fallFrame1, fallFrame1, fallFrame1, 
                                                        fallFrame2, fallFrame2, fallFrame2, fallFrame2};
            QueueSound(new LCDGameSound("fall_end.ogg"));
            PlayAnimation(endAnimation);

            _lifeCount--;
            if (_lifeCount == 0)
                GameOver();

            _knucklesPosition = 31;
            _emeraldPositions.Clear();
            _isFalling = false;
        }

        protected override void UpdateCore()
        {
            // Check if knuckles is on top of a platform, otherwise trigger fall anim
            var digit = _knucklesPosition % 10;
            if ((digit == 2 || digit == 3) && !ThreadSafePlatformList().Contains(_knucklesPosition))
                FallAnimation();

            // Out of bounds
            if (_knucklesPosition == 2 || _knucklesPosition == 43)
                FallAnimation();

            // Check for emeralds
            foreach (var emerald in ThreadSafeEmeraldList())
            {
                // Don't collect an emerald if it spawned right on top of the current player position,
                // Make him do one movement at least (even if it's to the same position)
                if (IsInFrontOfEmerald(emerald) && _hasKnucklesMoved)
                {
                    _emeraldsCollected++;
                    IncrementScore();
                    QueueSound(new LCDGameSound("emerald_get.ogg"));
                    _emeraldPositions.Remove(emerald);
                }

                if (ThreadSafeEmeraldList().Count == 0)
                {
                    _isInputBlocked = true;
                    BlockCustomUpdates(1); // Block one custom update so that emerald spawn doesn't happen instantly
                }
            }

        }

        private bool IsInFrontOfEmerald(int emerald)
        {
            return (_knucklesPosition == 11 && emerald == 1) ||
                   (_knucklesPosition == 14 && emerald == 2) ||
                   (_knucklesPosition == 21 && emerald == 3) ||
                   (_knucklesPosition == 24 && emerald == 4) ||
                   (_knucklesPosition == 34 && emerald == 5);
        }

        public override void CustomUpdate()
        {
            if (_emeraldsCollected == 5 * _level)
            {
                LevelUp();
            }

            // If no emeralds, start spawn
            if (ThreadSafeEmeraldList().Count == 0 && !_isFalling)
            {
                EmeraldSpawnAnimation();
            }

            if (_level == 6)
            {
                Victory();
                return;
            }

            // Check emerald time limit
            _emeraldTimeLimit--;
            if (_emeraldTimeLimit < (-5 - _level)) // After 5 more cycles + level, force a life loss
            {
                FallAnimation();
            }
            else if (_emeraldTimeLimit < 0)
            {
                QueueSound(new LCDGameSound("emerald_blink.ogg"));
                foreach (var emerald in ThreadSafeEmeraldList())
                {
                    BlinkElement("emerald-" + emerald, 1);
                }
            }
            
            // Platform spawn and movement -- should be on a separate timer to be truly PCB-accurate

            // Remove platforms that scrolled past the end of the screen
            _platformPositions.Remove(2);
            _platformPositions.Remove(43);

            // Platforms move the left column first, then the right
            foreach (var platform in ThreadSafePlatformList().Where(p => p % 10 == _platformsToMoveNextUpdate))
            {
                _platformPositions.Remove(platform);
                if (_platformsToMoveNextUpdate == 2)
                {
                    _platformPositions.Add(platform - 10);
                    if (_knucklesPosition == platform)
                        _knucklesPosition -= 10;
                }
                if (_platformsToMoveNextUpdate == 3)
                {
                    _platformPositions.Add(platform + 10);
                    if (_knucklesPosition == platform)
                        _knucklesPosition += 10;
                }
            }

            // Spawn platforms, according to the spawnlists
            if (_platformsToMoveNextUpdate == 2)
            {
                _platformsToMoveNextUpdate = 3;

                if (SPAWNMAP_LEFT[_currentLeftSpawn])
                {
                    _platformPositions.Add(32);
                }
                _currentLeftSpawn++;

                if (_currentLeftSpawn == SPAWNMAP_LEFT.Count())
                    _currentLeftSpawn = 0;
            }
            else
            {
                _platformsToMoveNextUpdate = 2;

                if (SPAWNMAP_RIGHT[_currentRightSpawn])
                {
                    _platformPositions.Add(3);
                }
                _currentRightSpawn++;

                if (_currentRightSpawn == SPAWNMAP_RIGHT.Count())
                    _currentRightSpawn = 0;
            }
        }

        private void LevelUp()
        {
            // Blink all emeralds 2x, reset knux pos
            _isInputBlocked = true;

            var levelUpFrame1 = new List<string>(GetKnucklesAndPlatformElements()) { EMERALD_1, EMERALD_2, EMERALD_3, EMERALD_4, EMERALD_5 };
            var levelUpFrame2 = new List<string>(GetKnucklesAndPlatformElements());

            var levelUpAnimation = new List<List<string>> { levelUpFrame1, levelUpFrame1, levelUpFrame1, levelUpFrame1, levelUpFrame1, levelUpFrame1,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame2, levelUpFrame2, levelUpFrame2, levelUpFrame2,
                                                            levelUpFrame1, levelUpFrame1, levelUpFrame1, levelUpFrame1, levelUpFrame1, levelUpFrame1,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame2, levelUpFrame2, levelUpFrame2, levelUpFrame2,};
            QueueSound(new LCDGameSound("level_up.ogg"));
            PlayAnimation(levelUpAnimation);
            _level++;
            _knucklesPosition = 31;
            _emeraldsCollected = 0;
            _lifeCount = 3;

            // Speed up
            _customUpdateSpeed -= 75;
        }

        private void GameOver()
        {
            QueueSound(new LCDGameSound("game_over.ogg"));

            // fall up -> fall down slow blink
            var gameOverFrame1 = new List<string> { FALL_UP };
            var gameOverFrame2 = new List<string> { FALL_DOWN };

            var gameOverAnimation = new List<List<string>> { gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1,
                                                             gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, 
                                                             gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1,
                                                             gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1,
                                                             gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2};
            PlayAnimation(gameOverAnimation);
            _knucklesPosition = -1;
            Stop();
        }

        private void Victory()
        {
            QueueSound(new LCDGameSound("game_win.ogg"));

            // knuckles21 solid + all emeralds blink
            var victoryFrame1 = new List<string> { KNUCKLES_21 };
            var victoryFrame2 = new List<string> { KNUCKLES_21, EMERALD_1, EMERALD_2, EMERALD_3, EMERALD_4, EMERALD_5 };

            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, 
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, 
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, 
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, 
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, 
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, 
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, 
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, 
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, };
            PlayAnimation(victoryAnimation);
            _knucklesPosition = -1;
            Stop();
        }

        private List<int> ThreadSafePlatformList() => new List<int>(_platformPositions);
        private List<int> ThreadSafeEmeraldList() => new List<int>(_emeraldPositions);
    }
}
