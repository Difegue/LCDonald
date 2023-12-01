using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class BillyGiantEgg : LCDGameBase
    {
        public override string ShortName => "bgiantegg";
        public override string Name => "Billy's Giant Egg (2005)";

        #region SVG Group Names
        public const string EGG_1 = "egg-1";
        public const string EGG_2 = "egg-2";
        public const string EGG_3 = "egg-3";
        public const string EGG_4 = "egg-4";
        public const string GET_LEFT = "get-left";
        public const string GET_RIGHT = "get-right";
        public const string BOMB_11 = "bomb-11";
        public const string BOMB_12 = "bomb-12";
        public const string BOMB_13 = "bomb-13";
        public const string BOMB_21 = "bomb-21";
        public const string BOMB_22 = "bomb-22";
        public const string BOMB_23 = "bomb-23";
        public const string BOMB_31 = "bomb-31";
        public const string BOMB_32 = "bomb-32";
        public const string BOMB_33 = "bomb-33";
        public const string FRUIT_11 = "fruit-11";
        public const string FRUIT_12 = "fruit-12";
        public const string FRUIT_13 = "fruit-13";
        public const string FRUIT_21 = "fruit-21";
        public const string FRUIT_22 = "fruit-22";
        public const string FRUIT_23 = "fruit-23";
        public const string FRUIT_31 = "fruit-31";
        public const string FRUIT_32 = "fruit-32";
        public const string FRUIT_33 = "fruit-33";
        public const string BILLYCENTER = "billy-center";
        public const string BILLYLEFT = "billy-left";
        public const string BILLYRIGHT = "billy-right";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                EGG_1, EGG_2, EGG_3, EGG_4,

                         FRUIT_11,    FRUIT_12,    FRUIT_13,
                         BOMB_11,     BOMB_12,     BOMB_13, 
                         FRUIT_21,    FRUIT_22,    FRUIT_23,
                         BOMB_21,     BOMB_22,     BOMB_23,
                         FRUIT_31,    FRUIT_32,    FRUIT_33,
                         BOMB_31,     BOMB_32,     BOMB_33,
                           GET_LEFT,         GET_RIGHT,
                   BILLYLEFT,     BILLYCENTER,       BILLYRIGHT
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Left",
                    Description = "Move Billy Left",
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    Description = "Move Billy Right",
                    KeyCode = 25, // right
                },
            };
        }

        private int _billyPosition;
        private int _fruitsCollected;
        private int _bombsHit;
        private int _level;

        private List<int> _bombPositions = new();
        private List<int> _fruitPositions = new();

        protected override List<string> GetVisibleElements()
        {
            var elements = new List<string>();

            elements.Add(GetBillyElement());

            foreach (var bombPos in ThreadSafeBombList())
                elements.Add("bomb-" + bombPos);

            foreach (var fruitPos in ThreadSafeFruitList())
                elements.Add("fruit-" + fruitPos);

            if (_fruitsCollected >= 5)
                elements.Add(EGG_1);
            if (_fruitsCollected >= 10)
                elements.Add(EGG_2);
            if (_fruitsCollected >= 15)
                elements.Add(EGG_3);

            return elements;
        }

        private string GetBillyElement()
        {
            if (_billyPosition == 1)
                return BILLYLEFT;
            else if (_billyPosition == 2)
                return BILLYCENTER;
            else if (_billyPosition == 3)
                return BILLYRIGHT;

            return "";
        }

        public override void InitializeGameState()
        {
            _billyPosition = 2;
            _fruitsCollected = 0;
            _level = _isEndlessMode ? 5 : 0;

            _bombPositions = new List<int>();
            _fruitPositions = new List<int>();

            _customUpdateSpeed = _isEndlessMode ? 250 : 950;

            StartupMusic("game_start.ogg");
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Left" && _billyPosition > 1)
                {
                    _billyPosition--;
                }
                else if (input.Name == "Right" && _billyPosition < 3)
                {
                    _billyPosition++;
                }
            }
        }

        protected override void UpdateCore()
        {
            // Fruits on 3rd line can be collected
            foreach (var ringPos in ThreadSafeFruitList().Where(r => r > 30))
            {
                var horizontalPos = ringPos % 10;

                if (horizontalPos == _billyPosition)
                {
                    _fruitPositions.Remove(ringPos);
                    IncrementScore();

                    if (horizontalPos == 1 || horizontalPos == 2)
                        BlinkElement(GET_LEFT, 1);
                    if (horizontalPos == 3 || horizontalPos == 2)
                        BlinkElement(GET_RIGHT, 1);

                    QueueSound(new LCDGameSound("hit.ogg"));
                    if (_fruitsCollected < 0)
                        _fruitsCollected = 1;
                    else
                        _fruitsCollected++;

                    // 5, 10, 15
                    if (_fruitsCollected % 5 == 0 && _fruitsCollected != 20)
                        QueueSound(new LCDGameSound("egg_up.ogg"));
                }
            }
        }

        public override void CustomUpdate()
        {
            if (_fruitsCollected >= 20 && !_isEndlessMode)
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

            // Move bombs and rings forward
            _bombPositions = ThreadSafeBombList().Select(x => x += 10).ToList();
            _fruitPositions = ThreadSafeFruitList().Select(x => x += 10).ToList();

            foreach (var bombPos in ThreadSafeBombList().Where(b => b > 40))
            {
                _bombPositions.Remove(bombPos);
                var horizontalPos = bombPos % 10;

                if (horizontalPos == _billyPosition)
                {
                    QueueSound(new LCDGameSound("miss.ogg"));
                    _bombsHit++;
                    
                    if (_bombsHit >= 2)
                    {
                        _bombsHit = 0;
                        // Reset egg state to previous level, or game over if state 0
                        if (_fruitsCollected < 5) GameOver();
                        else
                        _fruitsCollected = _fruitsCollected >= 15 ? 10 : _fruitsCollected >= 10 ? 5 : 0;
                    }
                }
            }

            foreach (var fruitPos in ThreadSafeFruitList().Where(r => r > 40))
            {
                _fruitPositions.Remove(fruitPos);
            }

            if (_fruitsCollected == -2)
            {
                GameOver();
                return;
            }

            // 60% chance to spawn at least one bomb
            if (_rng.Next(0, 100) < 60)
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

                // 50% chance to spawn fruit(s)
                if (_rng.Next(0, 100) < 50)
                {
                    SpawnFruits();
                }
            }
            else
            {
                // No bombs, spawn at least one fruit
                SpawnFruits();
            }

        }

        private void SpawnFruits()
        {
            // Bombs and fruits can't occupy the same space
            var fruitPos = _rng.Next(1, 4) + 10;

            if (!ThreadSafeBombList().Contains(fruitPos))
                _fruitPositions.Add(fruitPos);

            // 33% chance to spawn a second ring
            if (_rng.Next(0, 100) < 33)
            {
                var secondFruit = fruitPos == 11 ? 13 : 11;
                if (!ThreadSafeBombList().Contains(secondFruit))
                    _fruitPositions.Add(secondFruit);
            }
        }

        private void LevelUp()
        {
            _isInputBlocked = true;
            _fruitPositions.Clear();
            _bombPositions.Clear();
            _fruitsCollected = 0;
            _billyPosition = 2;

            BlinkElement(EGG_4, 6);
            QueueSound(new LCDGameSound("egg_hatch.ogg"));

            var levelUpFrame1 = new List<string> { BILLYCENTER, EGG_1, EGG_2, EGG_3 };
            var levelUpFrame2 = new List<string> { BILLYCENTER, EGG_4 };
            var levelUpFrame3 = new List<string> { BILLYCENTER };

            var levelUpAnimation = new List<List<string>> { levelUpFrame1, levelUpFrame1, 
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame3, levelUpFrame3,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame3, levelUpFrame3,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame3, levelUpFrame3,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame3, levelUpFrame3,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame3, levelUpFrame3,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame3, levelUpFrame3,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame3, levelUpFrame3,
                                                            levelUpFrame2, levelUpFrame2, levelUpFrame3, levelUpFrame3};
            PlayAnimation(levelUpAnimation);
            _level++;
        }

        private void GameOver()
        {
            _billyPosition = -1;
            _level = 0;
            _bombPositions.Clear();
            _fruitPositions.Clear();

            GenericGameOverAnimation(new List<string> { BILLYCENTER, BOMB_31, BOMB_32, BOMB_33 }, "game_over.ogg");
            Stop();
        }

        private void Victory()
        {
            _billyPosition = -1;
            _level = 0;
            _bombPositions.Clear();
            _fruitPositions.Clear();

            QueueSound(new LCDGameSound("game_win.ogg"));

            var victoryFrame1 = new List<string> { BILLYCENTER, EGG_1 };
            var victoryFrame2 = new List<string> { BILLYCENTER, EGG_2 };
            var victoryFrame3 = new List<string> { BILLYCENTER, EGG_3 };
            var victoryFrame4 = new List<string> { BILLYCENTER, EGG_4 };

            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame4, victoryFrame4,
                                                            victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame4, victoryFrame4,
                                                            victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame4, victoryFrame4,
                                                            victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame4, victoryFrame4,
                                                            victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame4, victoryFrame4,
                                                            victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame4, victoryFrame4,
                                                            victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame4, victoryFrame4};
            PlayAnimation(victoryAnimation);
            Stop();
        }

        private List<int> ThreadSafeBombList() => new List<int>(_bombPositions);
        private List<int> ThreadSafeFruitList() => new List<int>(_fruitPositions);
    }
}
