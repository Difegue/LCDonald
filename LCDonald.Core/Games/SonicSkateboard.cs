using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class SonicSkateboard : LCDGameBase
    {
        public override string ShortName => "sskateboard";
        public override string Name => "Sonic Skateboard (2005)";

        #region SVG Group Names
        public const string LIFE_1 = "life-1";
        public const string LIFE_2 = "life-2";
        public const string LIFE_3 = "life-3";
        public const string LEVEL_1 = "level-1";
        public const string LEVEL_2 = "level-2";
        public const string LEVEL_3 = "level-3";
        public const string BIGRING_1 = "bigring-1";
        public const string BIGRING_2 = "bigring-2";
        public const string BIGRING_3 = "bigring-3";
        public const string BOMB_11 = "bomb-11";
        public const string BOMB_12 = "bomb-12";
        public const string BOMB_13 = "bomb-13";
        public const string BOMB_21 = "bomb-21";
        public const string BOMB_22 = "bomb-22";
        public const string BOMB_23 = "bomb-23";
        public const string BOMB_31 = "bomb-31";
        public const string BOMB_32 = "bomb-32";
        public const string BOMB_33 = "bomb-33";
        public const string RING_11 = "ring-11";
        public const string RING_13 = "ring-13";
        public const string RING_21 = "ring-21";
        public const string RING_23 = "ring-23";
        public const string RING_31 = "ring-31";
        public const string RING_33 = "ring-33";
        public const string SONIC_CENTER = "sonic-center";
        public const string SONIC_LEFT = "sonic-left";
        public const string SONIC_RIGHT = "sonic-right";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                LIFE_1, LIFE_2, LIFE_3,     LEVEL_1, LEVEL_2, LEVEL_3,

                                     BIGRING_1,
                         RING_11,    BIGRING_2,    RING_13,
                         BOMB_11,     BOMB_12,     BOMB_13,
                         RING_21,    BIGRING_3,    RING_23,
                         BOMB_21,     BOMB_22,     BOMB_23,
                         RING_31,                  RING_33,
                         BOMB_31,     BOMB_32,     BOMB_33,

                   SONIC_LEFT,     SONIC_CENTER,       SONIC_RIGHT
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
        private int _ringsCollected;
        private int _bombsHit;
        private int _lives;
        private int _level;

        private List<int> _bombPositions = new List<int>();
        private List<int> _ringPositions = new List<int>();
        private int _blockSpawnCounter;

        private Random _rng = new Random();

        protected override List<string> GetVisibleElements()
        {
            var elements = GetLivesAndLevelElements();

            elements.Add(GetSonicElement());

            foreach (var bombPos in ThreadSafeBombList())
                elements.Add("bomb-" + bombPos);

            foreach (var ringPos in ThreadSafeRingList())
                elements.Add("ring-" + ringPos);

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

        private List<string> GetLivesAndLevelElements()
        {
            var elements = new List<string>();

            if (_level >= 1)
                elements.Add(LEVEL_1);
            if (_level >= 2)
                elements.Add(LEVEL_2);
            if (_level >= 3)
                elements.Add(LEVEL_3);

            if (_lives >= 1)
                elements.Add(LIFE_1);
            if (_lives >= 2)
                elements.Add(LIFE_2);
            if (_lives >= 3)
                elements.Add(LIFE_3);

            return elements;
        }

        public override void InitializeGameState()
        {
            _sonicPosition = 2;
            _ringsCollected = 0;
            _bombsHit = 0;
            _level = 0;
            _lives = 3;

            _bombPositions = new List<int>();
            _ringPositions = new List<int>();

            _customUpdateSpeed = 950;
            QueueSound(new LCDGameSound("../common/game_start.ogg"));

            // Wait for sound to end before spawning anything 
            // 2x custom loop is about the correct speed.
            _blockSpawnCounter = 2;
            _isInputBlocked = true;
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
            // Rings on 3rd line can be collected
            foreach (var ringPos in ThreadSafeRingList().Where(r => r > 30))
            {
                var horizontalPos = ringPos % 10;

                if (horizontalPos == _sonicPosition)
                {
                    _ringPositions.Remove(ringPos);
                    QueueSound(new LCDGameSound("../common/hit.ogg"));
                    _ringsCollected++;
                }
            }

            if (_blockSpawnCounter == 0)
            {
                _isInputBlocked = false;
            }
        }

        public override void CustomUpdate()
        {
            if (_ringsCollected >= 30)
            {
                if (_level == 3)
                {
                    Victory();
                    return;
                }
                else
                {
                    LevelUp();
                    // Speed up
                    _customUpdateSpeed -= 150;
                }

            }

            if (_blockSpawnCounter > 0)
            {
                _blockSpawnCounter--;
            }
            else
            {
                // Move bombs and rings forward
                _bombPositions = ThreadSafeBombList().Select(x => x += 10).ToList();
                _ringPositions = ThreadSafeRingList().Select(x => x += 10).ToList();

                foreach (var bombPos in ThreadSafeBombList().Where(b => b > 40))
                {
                    _bombPositions.Remove(bombPos);
                    var horizontalPos = bombPos % 10;

                    if (horizontalPos == _sonicPosition)
                    {
                        QueueSound(new LCDGameSound("../common/miss.ogg"));
                        _bombsHit++;
                        _isInputBlocked = true;
                        BlinkElement(GetSonicElement(), 1);
                    }
                }

                foreach (var ringPos in ThreadSafeRingList().Where(r => r > 40))
                {
                    _ringPositions.Remove(ringPos);
                }

                if (_bombsHit == 5)
                {
                    _lives--;
                    if (_lives == 0)
                        GameOver();
                    return;
                }

                // 90% chance to spawn at least one bomb
                if (_rng.Next(0, 100) < 90)
                {
                    var firstBomb = _rng.Next(11, 14);
                    _bombPositions.Add(firstBomb);

                    // 33% chance to spawn a second bomb
                    if (_rng.Next(0, 100) < 33)
                    {
                        var secondBomb = firstBomb;

                        do { secondBomb = _rng.Next(11, 14); } while (secondBomb == firstBomb);
                        _bombPositions.Add(secondBomb);
                    }

                    // 33% chance to spawn ring(s)
                    if (_rng.Next(0, 100) < 33)
                    {
                        SpawnRings();
                    }
                }
                else
                {
                    // No bombs, spawn at least one ring
                    SpawnRings();
                }
            }
        }

        private void SpawnRings()
        {
            // Rings only have two starting positions in this game
            // Also, Bombs and rings can't occupy the same space
            var ringPos = _rng.Next(0, 2) == 0 ? 11 : 13;

            if (!ThreadSafeBombList().Contains(ringPos))
                _ringPositions.Add(ringPos);

            // 33% chance to spawn a second ring
            if (_rng.Next(0, 100) < 33)
            {
                var secondRing = ringPos == 11 ? 13 : 11;
                if (!ThreadSafeBombList().Contains(secondRing))
                    _ringPositions.Add(secondRing);
            }
        }

        private void LevelUp()
        {
            _isInputBlocked = true;
            _ringPositions.Clear();
            _bombPositions.Clear();
            _bombsHit = 0;
            _ringsCollected = 0;
            _sonicPosition = 2;
            
            var levelUpFrame1 = new List<string>(GetLivesAndLevelElements()) { SONIC_CENTER, BIGRING_1 };
            var levelUpFrame2 = new List<string>(GetLivesAndLevelElements()) { SONIC_CENTER, BIGRING_2 };
            var levelUpFrame3 = new List<string>(GetLivesAndLevelElements()) { SONIC_CENTER, BIGRING_3 };

            var levelUpAnimation = new List<List<string>> { levelUpFrame1, levelUpFrame1, levelUpFrame1, levelUpFrame1, levelUpFrame1, levelUpFrame1, levelUpFrame1,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame2, levelUpFrame2, levelUpFrame2, levelUpFrame2, levelUpFrame2,
                                                            levelUpFrame3, levelUpFrame3, levelUpFrame3, levelUpFrame3, levelUpFrame3, levelUpFrame3, levelUpFrame3, };
            PlayAnimation(levelUpAnimation);
            QueueSound(new LCDGameSound("../common/level_up.ogg"));
            _level++;
        }

        private void GameOver()
        {
            _sonicPosition = -1;
            _level = 0;
            _bombPositions.Clear();
            _ringPositions.Clear();

            QueueSound(new LCDGameSound("../common/game_over.ogg"));

            var gameOverFrame1 = new List<string> { SONIC_CENTER, BOMB_11, BOMB_12, BOMB_13, BOMB_21, BOMB_22, BOMB_23, BOMB_31, BOMB_32, BOMB_33 };
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
            _lives = 0;
            _level = 0;
            _bombPositions.Clear();
            _ringPositions.Clear();

            QueueSound(new LCDGameSound("../common/game_win_short.ogg"));

            // 10x bigring animation + sonic-center blink (god)
            var victoryFrame1 = new List<string> { BIGRING_1 };
            var victoryFrame2 = new List<string> { BIGRING_2 };
            var victoryFrame3 = new List<string> { BIGRING_3 };
            var victoryFrame1s = new List<string> { BIGRING_1, SONIC_CENTER };
            var victoryFrame2s = new List<string> { BIGRING_2, SONIC_CENTER };
            var victoryFrame3s = new List<string> { BIGRING_3, SONIC_CENTER };

            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2s, victoryFrame2s, victoryFrame2s, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame1s, victoryFrame1s, victoryFrame1s, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3s, victoryFrame3s, victoryFrame3s,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2s, victoryFrame2s, victoryFrame2s, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame1s, victoryFrame1s, victoryFrame1s, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3s, victoryFrame3s, victoryFrame3s,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2s, victoryFrame2s, victoryFrame2s, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame1s, victoryFrame1s, victoryFrame1s, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3s, victoryFrame3s, victoryFrame3s,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2s, victoryFrame2s, victoryFrame2s, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame1s, victoryFrame1s, victoryFrame1s, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3s, victoryFrame3s, victoryFrame3s,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2s, victoryFrame2s, victoryFrame2s, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame1s, victoryFrame1s, victoryFrame1s, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3s, victoryFrame3s, victoryFrame3s,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2s, victoryFrame2s, victoryFrame2s, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame1s, victoryFrame1s, victoryFrame1s, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3s, victoryFrame3s, victoryFrame3s};
            PlayAnimation(victoryAnimation);
            Stop();
        }

        private List<int> ThreadSafeBombList() => new List<int>(_bombPositions);
        private List<int> ThreadSafeRingList() => new List<int>(_ringPositions);
    }
}
