﻿using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace LCDonald.Core.Games
{
    public class TailsSkyAdventure : LCDGameBase
    {
        public override string ShortName => "tskyadventure";
        public override string Name => "Tails' Sky Adventure (2005)";

        #region SVG Group Names
        public const string LIFE_1 = "life-1";
        public const string LIFE_2 = "life-2";
        public const string LIFE_3 = "life-3";
        public const string LEVEL_1 = "level-1";
        public const string LEVEL_2 = "level-2";
        public const string LEVEL_3 = "level-3";
        public const string ENEMY_11 = "enemy-11";
        public const string ENEMY_12 = "enemy-12";
        public const string ENEMY_13 = "enemy-13";
        public const string ENEMY_21 = "enemy-21";
        public const string ENEMY_22 = "enemy-22";
        public const string ENEMY_23 = "enemy-23";
        public const string ENEMY_31 = "enemy-31";
        public const string ENEMY_32 = "enemy-32";
        public const string ENEMY_33 = "enemy-33";
        public const string PROJECTILE_11 = "projectile-11";
        public const string PROJECTILE_12 = "projectile-12";
        public const string PROJECTILE_13 = "projectile-13";
        public const string PROJECTILE_21 = "projectile-21";
        public const string PROJECTILE_22 = "projectile-22";
        public const string PROJECTILE_23 = "projectile-23";
        public const string PROJECTILE_31 = "projectile-31";
        public const string PROJECTILE_32 = "projectile-32";
        public const string PROJECTILE_33 = "projectile-33";
        public const string TAILS_LEFT = "tails-left";
        public const string TAILS_RIGHT = "tails-right";
        public const string TAILS_CENTER = "tails-center";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>()
            {
                LIFE_1, LIFE_2, LIFE_3,   LEVEL_1, LEVEL_2, LEVEL_3,
                
                
                       ENEMY_11,  ENEMY_12,  ENEMY_13,
                 PROJECTILE_11, PROJECTILE_12, PROJECTILE_13,
                    ENEMY_21,     ENEMY_22,   ENEMY_23,
                 PROJECTILE_21, PROJECTILE_22, PROJECTILE_23,
                  ENEMY_31,       ENEMY_32,      ENEMY_33,
                 PROJECTILE_31, PROJECTILE_32, PROJECTILE_33,
                
                
                TAILS_LEFT,    TAILS_CENTER,     TAILS_RIGHT
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Left",
                    Description = "Move Tails Left",
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    Description = "Move Tails Right",
                    KeyCode = 25, // right
                },
                new LCDGameInput
                {
                    Name = "Fire",
                    Description = "Shoot a projectile",
                    KeyCode = 18, // space
                }
            };          
        }

        private int _tailsPosition;
        private int _lifeCount;
        private int _level;

        private int _enemiesHit;
        private int _enemiesMissed;

        private int _projectilePos;
        private int _enemyPos;
        private bool _spawnEnemyNextUpdate;

        protected override List<string> GetVisibleElements()
        {
            var elements = new List<string>();
            
            if (_lifeCount >= 1)
                elements.Add(LIFE_1);
            if (_lifeCount >= 2)
                elements.Add(LIFE_2);
            if (_lifeCount == 3)
                elements.Add(LIFE_3);

            if (_level >= 1)
                elements.Add(LEVEL_1);
            if (_level >= 2)
                elements.Add(LEVEL_2);
            if (_level >= 3)
                elements.Add(LEVEL_3);

            elements.Add(GetTailsElement());

            if (_projectilePos != -1)
                elements.Add("projectile-" + _projectilePos);

            if (_enemyPos != -1)
                elements.Add("enemy-" + _enemyPos);

            return elements;
        }

        private string GetTailsElement()
        {
            if (_tailsPosition == 1)            
                return TAILS_LEFT;
            else if (_tailsPosition == 2)
                return TAILS_CENTER;
            else if (_tailsPosition == 3)
                return TAILS_RIGHT;

            return "";
        }

        public override void InitializeGameState()
        {
            _tailsPosition = 2;
            _lifeCount = 3;
            _level = _isEndlessMode ? 5 : 0;

            _enemiesHit = 0;
            _enemiesMissed = 0;

            _projectilePos = -1;
            _enemyPos = -1;

            _customUpdateSpeed = _isEndlessMode ? 400 : 900;
            StartupMusic("game_start.ogg", 0);
        }        

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Left" && _tailsPosition > 1)
                {
                    _tailsPosition--;
                }
                else if (input.Name == "Right" && _tailsPosition < 3)
                {
                    _tailsPosition++;
                }
                else if (input.Name == "Fire" && _projectilePos == -1)
                {
                    QueueSound(new LCDGameSound("fire.ogg"));
                    _projectilePos = _tailsPosition + 30;
                }
            }
        }

        protected override void UpdateCore()
        {
            // +10 tolerance in case the projectile passed while the enemy moved forward
            if (_projectilePos != -1 && (_projectilePos == _enemyPos || _projectilePos + 10 == _enemyPos) )
            {
                BlinkElement("enemy-" + _enemyPos, 3);
                QueueSound(new LCDGameSound("hit.ogg"));
                _enemyPos = _projectilePos = -1;
                _enemiesHit++;
                IncrementScore();
            }

            // Move projectile forward
            if (_projectilePos > 10)
                _projectilePos -= 10;
            else 
                _projectilePos = -1;
        }

        public override void CustomUpdate()
        {
            if (_enemiesHit == 15 && !_isEndlessMode)
            {
                QueueSound(new LCDGameSound("level_up.ogg"));
                _enemiesHit = 0;
                _level++;
                BlinkElement("level-" + _level, 2);
                // Speed up
                _customUpdateSpeed -= 175;
            }

            if (_level == 4)
            {
                Victory();
                return;
            }

            if (_enemyPos != -1)
            {
                _enemyPos += 10;

                if (_enemyPos > 40 && _projectilePos != _enemyPos - 10)
                {
                    var digit = _enemyPos % 10;
                    _enemyPos = -1;
                    if (digit == _tailsPosition)
                    {
                        _lifeCount--;
                        QueueSound(new LCDGameSound("life_loss.ogg"));

                        _isInputBlocked = true;
                        // Life loss, both character and enemy blink
                        BlinkElement(GetTailsElement(), 2);
                        BlinkElement("enemy-3" + digit, 3);
                        // Wait for blinking to end
                        while (IsBlinking("enemy-3" + digit)) { }
                        _isInputBlocked = false;
                    } 
                    else
                    {
                        _enemiesMissed++;
                        if ( _enemiesMissed == 5)
                        {
                            _lifeCount--;
                            QueueSound(new LCDGameSound("life_loss.ogg"));
                            _isInputBlocked = true;
                            // Life loss, character blinks
                            var spr = GetTailsElement();
                            BlinkElement(spr, 2);
                            // Wait for blinking to end
                            while (IsBlinking(spr)) { }
                            _isInputBlocked = false;
                        } 
                        else
                            QueueSound(new LCDGameSound("miss.ogg"));
                    }

                    if (_lifeCount == 0)
                        GameOver();
                }
                else
                {
                    // Randomly shift enemy left or right
                    var shift = _rng.Next(-1, 2);
                    _enemyPos += shift;

                    // Adjust out of bounds
                    if (_enemyPos % 10 == 0)
                        _enemyPos++;

                    if (_enemyPos % 10 == 4)
                        _enemyPos--;
                }
                
            }
            else if (!_spawnEnemyNextUpdate)
                _spawnEnemyNextUpdate = true;
            else
            {
                _spawnEnemyNextUpdate = false;
                
               // Randomly spawn enemy at 11, 12 or 13
               _enemyPos = _rng.Next(11, 14);
            }
        }
        
        private void GameOver()
        {
            QueueSound(new LCDGameSound("game_over.ogg"));

            var gameOverFrame1 = new List<string> { TAILS_CENTER, ENEMY_31, ENEMY_32, ENEMY_33 };
            var gameOverFrame2 = new List<string> { TAILS_CENTER };

            // slow 4x blink
            var gameOverAnimation = new List<List<string>> { gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2,
                                                             gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame1, gameOverFrame2, gameOverFrame2, gameOverFrame2, gameOverFrame2};
            PlayAnimation(gameOverAnimation);
            _tailsPosition = -1;
            Stop();
        }

        private void Victory()
        {
            QueueSound(new LCDGameSound("game_win.ogg"));

            var victoryFrame1 = new List<string> { TAILS_CENTER, PROJECTILE_31, PROJECTILE_32, PROJECTILE_33 };
            var victoryFrame2 = new List<string> { TAILS_CENTER, PROJECTILE_21, PROJECTILE_22, PROJECTILE_23 };
            var victoryFrame3 = new List<string> { TAILS_CENTER, PROJECTILE_11, PROJECTILE_12, PROJECTILE_13 };

            // slow 4x sequence
            var victoryAnimation = new List<List<string>> { victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3,
                                                            victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame1, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame2, victoryFrame3, victoryFrame3, victoryFrame3, victoryFrame3};
            PlayAnimation(victoryAnimation);
            _tailsPosition = -1;
            Stop();
        }
    }
}
