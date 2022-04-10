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

        public override List<string> GetVisibleGameElements()
        {
            // TODO
            return new List<string>()
            {
                LIFE_1,LIFE_2,(LIFE_3),(LEVEL_1),(LEVEL_2),
                (LEVEL_3),
                (ENEMY_11),
                (ENEMY_12),
                (ENEMY_13),
                (TAILS_CENTER)
            };
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
                Console.WriteLine(input.Name);
            // TODO
        }

        public override void InitializeGameState()
        {
            // TODO
        }

        public override void UpdateGameState()
        {
            // TODO
        }
        
    }
}
