using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace LCDonald.Core.Games
{
    public class TailsSkyAdventure : LCDGameBase
    {
        public override string GetAssetFolderName() => "tskyadventure";
        public override string GetGameName() => "Tails' Sky Adventure";

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
                    Description = "",
                    KeyCode = 23, // left
                },
                new LCDGameInput
                {
                    Name = "Right",
                    Description = "",
                    KeyCode = 25, // right
                },
                new LCDGameInput
                {
                    Name = "Fire",
                    Description = "",
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

        private Random _rng = new Random();

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
            _level = 0;

            _enemiesHit = 0;
            _enemiesMissed = 0;

            _projectilePos = -1;
            _enemyPos = -1;

            _customUpdateSpeed = 900;
            QueueSound(new LCDGameSound("game_start.ogg"));
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

            }

            // Move projectile forward
            if (_projectilePos > 10)
                _projectilePos -= 10;
            else 
                _projectilePos = -1;
        }

        public override void CustomUpdate()
        {
            if (_enemiesHit == 15)
            {
                QueueSound(new LCDGameSound("level_up.ogg"));
                _enemiesHit = 0;
                _level++;
                BlinkElement("level-" + _level, 2);
                _customUpdateSpeed -= 200;
            }

            if (_enemyPos != -1)
            {
                _enemyPos += 10;

                if (_enemyPos > 40)
                {
                    var digit = _enemyPos % 10;
                    if (digit == _tailsPosition)
                    {
                        _lifeCount--;
                        QueueSound(new LCDGameSound("life_loss.ogg"));

                        _isInputBlocked = true;
                        BlinkElement(GetTailsElement(), 1);
                        // Wait for blinking to end
                        while (IsBlinking(GetTailsElement())) { }
                        _isInputBlocked = false;

                        if (_lifeCount == 0) GameOverAnimation();
                    } 
                    else
                    {
                        _enemiesMissed++;
                        QueueSound(new LCDGameSound("miss.ogg"));
                        if (_enemiesMissed > 15) GameOverAnimation();
                    }

                    _enemyPos = -1;
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

        private void GameOverAnimation()
        {
            _isInputBlocked = true;
            QueueSound(new LCDGameSound("game_over.ogg"));
            //Stop();
        }

        private void VictoryAnimation()
        {
            _isInputBlocked = true;
            QueueSound(new LCDGameSound("game_win.ogg"));
            //Stop();
        }
    }
}
