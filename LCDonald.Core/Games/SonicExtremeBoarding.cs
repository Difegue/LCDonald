using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class SonicExtremeBoarding : LCDGameBase
    {
        
#if BURGER
        public override string ShortName => "extremeboard";
        public override string Name => "Vibes Extreme Skateboarding";
#else
        public override string ShortName => "sxtremeboard";
        public override string Name => "Sonic Extreme Boarding (2004)";
#endif

        #region SVG Group Names
        public const string HIT_1 = "hit-1";
        public const string HIT_2 = "hit-2";
        public const string HIT_3 = "hit-3";
        public const string HIT_4 = "hit-4";
        public const string HIT_5 = "hit-5";
        public const string HIT_6 = "hit-6";
        public const string RING_0 = "ring-0";
        public const string RING_11 = "ring-11";
        public const string RING_21 = "ring-21";
        public const string RING_31 = "ring-31";
        public const string RING_17 = "ring-17";
        public const string RING_27 = "ring-27";
        public const string RING_37 = "ring-37";
        public const string BOMB_12 = "bomb-12";
        public const string BOMB_22 = "bomb-22";
        public const string BOMB_32 = "bomb-32";
        public const string BOMB_14 = "bomb-14";
        public const string BOMB_24 = "bomb-24";
        public const string BOMB_34 = "bomb-34";
        public const string BOMB_16 = "bomb-16";
        public const string BOMB_26 = "bomb-26";
        public const string BOMB_36 = "bomb-36";
        public const string SONIC_1 = "sonic-1";
        public const string SONIC_2 = "sonic-2";
        public const string SONIC_3 = "sonic-3";
        public const string SONIC_4 = "sonic-4";
        public const string SONIC_5 = "sonic-5";
        public const string SONIC_6 = "sonic-6";
        public const string SONIC_7 = "sonic-7";
        public const string COOLRING = "cool";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
               SONIC_1, RING_31,   COOLRING,  RING_37, SONIC_7,
                         RING_21,            RING_27,
                          RING_11, RING_0, RING_17,
                          BOMB_12, BOMB_14, BOMB_16,
              HIT_1,     BOMB_22,            BOMB_26,    HIT_6,
               SONIC_2, BOMB_32,   BOMB_24,   BOMB_36, SONIC_6,
                  HIT_2,                              HIT_5,
                    SONIC_3,       BOMB_34,         SONIC_5,
                             HIT_3,        HIT_4,
                                   SONIC_4
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Left",
                    #if BURGER
                    Description = "Move Vibes Left",
#else
                    Description = "Move Sonic Left",
                    #endif
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    #if BURGER
                    Description = "Move Vibes Right",
#else
                    Description = "Move Sonic Right",
                    #endif
                    KeyCode = 25, // right
                },
            };
        }

        private int _sonicPosition;
        private int _ringsCollected;
        private int _bombsHit;
        private int _level;
        
        private int _ringSpawn;
        private int _ringPosition;
        private bool _showRingSpawnIndicator;

        private List<int> _bombPositions = new();

        protected override List<string> GetVisibleElements()
        {
            var elements = new List<string>();

            elements.Add(GetSonicElement());

            if (_showRingSpawnIndicator)
                elements.Add(RING_0);
            
            elements.Add("ring-" + _ringPosition);

            foreach (var bombPos in ThreadSafeBombList())
                elements.Add("bomb-" + bombPos);

            return elements;
        }

        private string GetSonicElement() => "sonic-" + _sonicPosition;

        public override void InitializeGameState()
        {
            _sonicPosition = 4;
            _ringsCollected = 0;
            _bombsHit = 0;
            _level = _isEndlessMode ? 5 : 0;

            _bombPositions = new List<int>();
            _ringPosition = -1;
            _ringSpawn = 11;

            // Special gamestart case: don't show the spawn indicator right away, it'll show on first customupdate
            _showRingSpawnIndicator = false;

            _customUpdateSpeed = _isEndlessMode ? 400 : 950;

            StartupMusic("game_start.ogg", 2);
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
                else if (input.Name == "Right" && _sonicPosition < 7)
                {
                    _sonicPosition++;
                }
            }
        }

        protected override void UpdateCore()
        {
            // Rings on 3rd line can be collected
            if (_ringPosition > 30)
            {
                var horizontalPos = _ringPosition % 10;

                if (horizontalPos == _sonicPosition)
                {
                    _ringPosition = 0;
                    QueueSound(new LCDGameSound("ring_hit.ogg"));
                    _ringsCollected++;
                    IncrementScore();

                    // swap ring spawn point
                    _ringSpawn = _ringSpawn == 11 ? 17 : 11;
                }
            }

#if !BURGER
            // If sonic is at position 1 or 7, bring him down to 2/6
            if (_sonicPosition == 1 || _sonicPosition == 7)
            {
                _sonicPosition = _sonicPosition == 1 ? 2 : 6;
            }
#endif

        }

        public override void CustomUpdate()
        {
            if (_ringPosition < 0)
            {
                _ringPosition = 0;
                _showRingSpawnIndicator = true;
                return;
            }

            if (_ringsCollected >= 20 && !_isEndlessMode) 
            {
                if (_level == 4)
                {
                    Victory();
                    return;
                }
                else
                {
                    LevelUp();
                    // Speed up
                    _customUpdateSpeed -= 125;
                }

            }

            // Move bombs forward
            _bombPositions = ThreadSafeBombList().Select(x => x += 10).ToList();

            foreach (var bombPos in ThreadSafeBombList().Where(b => b > 40))
            {
                _bombPositions.Remove(bombPos);
                var horizontalPos = bombPos % 10;

                if (horizontalPos == _sonicPosition)
                {
                    QueueSound(new LCDGameSound("bomb_hit.ogg"));
                    _bombsHit++;
                    
                    BlinkElement(GetSonicElement(), 1);
                    if (_sonicPosition == 2)
                    {
                        BlinkElement(HIT_1, 1);
                        BlinkElement(HIT_2, 1);
                    } else if (_sonicPosition == 4)
                    {
                        BlinkElement(HIT_3, 1);
                        BlinkElement(HIT_4, 1);
                    } else if (_sonicPosition == 6)
                    {
                        BlinkElement(HIT_5, 1);
                        BlinkElement(HIT_6, 1);
                    }
                }
            }

            // Move ring forward if there's one 
            if (_ringPosition > 0)
                _ringPosition = _ringPosition + 10;

            // Show spawn indicator if the current ring is about to go off screen
            if (_ringPosition > 30)
                _showRingSpawnIndicator = true;
            else
                _showRingSpawnIndicator = false;

            // Spawn ring if there's none
            if (_ringPosition > 40 || _ringPosition == 0)
            {
                // Simulate a weird edge case from the OG circuit - If the ring respawns on the same line due to the player not collecting it,
                // The spawn indicator remains for one extra cycle.
                if (_ringPosition % 10 != _ringSpawn % 10)
                    _showRingSpawnIndicator = false;

                _ringPosition = _ringSpawn;
            }

            if (_bombsHit == 5)
            {
                GameOver();
                return;
            }

            var bombsOnPreviousRow = ThreadSafeBombList().Where(r => r > 20 && r < 30).Count();

            // 80% chance to spawn at least one bomb
            if (bombsOnPreviousRow != 3 && _rng.Next(0, 100) < 80)
            {
                var firstBomb = SpawnBomb();
                _bombPositions.Add(firstBomb);

                // If we're on the same row as a ring, there's only one bomb max
                if (_ringPosition == 11 || _ringPosition == 17)
                    return;

                // If the previous row only has one bomb, 50% chance to spawn a second bomb
                if (bombsOnPreviousRow < 2 && _rng.Next(0, 100) < 33)
                {
                    var secondBomb = SpawnBomb();
                    _bombPositions.Add(secondBomb);
                }

                // If the previous row was empty, 33% chance to spawn the third bomb
                if (bombsOnPreviousRow == 0 && _rng.Next(0, 100) < 33)
                {
                    var thirdBomb = SpawnBomb();
                    _bombPositions.Add(thirdBomb);
                }
            }

#if BURGER
            // In Burger Bard version, the hang position is updated in the main cycle instead of being on a single frame
            // If sonic is at position 1 or 7, bring him down to 2/6
            if (_sonicPosition == 1 || _sonicPosition == 7)
            {
                _sonicPosition = _sonicPosition == 1 ? 2 : 6;
            }
#endif
        }

        private int SpawnBomb()
        {
            var bombPos = _rng.Next(12, 18);
            if (bombPos == 13) bombPos = 12;
            if (bombPos == 15) bombPos = 14;
            if (bombPos == 17) bombPos = 16;

            // bomb can't be in the same lane as the previous bomb row
            if (ThreadSafeBombList().Contains(bombPos + 10) || ThreadSafeBombList().Contains(bombPos))
                return SpawnBomb();

            // bomb can't be next to the current ring
            if (bombPos == 12 && _ringPosition == 11)
                return SpawnBomb();
            if (bombPos == 16 && _ringPosition == 17)
                return SpawnBomb();

            return bombPos;
        }

        private void LevelUp()
        {
            _isInputBlocked = true;
            _bombsHit = 0;
            _ringsCollected = 0;

            //3x coolring blink
            var levelUpFrame1 = new List<string>(GetVisibleGameElements()) { COOLRING };
            var levelUpFrame2 = new List<string>(GetVisibleGameElements());

            var levelUpAnimation = new List<List<string>> { levelUpFrame1, levelUpFrame1, levelUpFrame1,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame2, 
                                                            levelUpFrame1, levelUpFrame1, levelUpFrame1, 
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame2, 
                                                            levelUpFrame1, levelUpFrame1, levelUpFrame1, 
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame2,};
            QueueSound(new LCDGameSound("level_up.ogg"));
            PlayAnimation(levelUpAnimation);
            _level++;
        }

        private void GameOver()
        {
            _sonicPosition = -1;
            _level = 0;
            _bombPositions.Clear();
            _ringPosition = -1;

            GenericGameOverAnimation(new List<string> { SONIC_4, BOMB_12, BOMB_14, BOMB_16, BOMB_22, BOMB_24, BOMB_26, BOMB_32, BOMB_34, BOMB_36 },
                                     "game_over.ogg");
            Stop();
        }

        private void Victory()
        {
            _sonicPosition = -1;
            _level = 0;
            _bombPositions.Clear();
            _ringPosition = -1;

            QueueSound(new LCDGameSound("game_win.ogg"));

            //5x slow blink sonic_4 + coolring
            var victoryFrame1 = new List<string> {  };
            var victoryFrame2 = new List<string> { SONIC_4, COOLRING };

#if BURGER
            victoryFrame2.Add("win");
#endif

            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1,
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1,
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1,
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1,
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1,
                                                            victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2};
            PlayAnimation(victoryAnimation);
            Stop();
        }

        private List<int> ThreadSafeBombList() => [.. _bombPositions];
    }
}
