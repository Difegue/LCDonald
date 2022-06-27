using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCDonald.Core.Games
{
    public class ShadowGrinder : LCDGameBase
    {
        public override string ShortName => "sgrinder";
        public override string Name => "Shadow Grinder (2003)";

        #region SVG Group Names
        public const string LEVEL_1 = "level-1";
        public const string LEVEL_2 = "level-2";
        public const string LEVEL_3 = "level-3";
        public const string RAIL_0 = "rail-0";
        public const string RAIL_1 = "rail-1";
        public const string RAIL_2 = "rail-2";
        public const string RAIL_3 = "rail-3";
        public const string RAIL_4 = "rail-4";
        public const string RAIL_5 = "rail-5";
        public const string RAIL_6 = "rail-6";
        public const string RAIL_7 = "rail-7";
        public const string MISS = "miss";
        public const string SHADOW_JUMP = "shadow-2";
        public const string SHADOW = "shadow-1";
        public const string SHADOW_FALL = "shadow-0";
        #endregion SVG Group Names

        public override List<string> GetAllGameElements()
        {
            return new List<string>() 
            {
                SHADOW_JUMP,                LEVEL_1, LEVEL_2, LEVEL_3,

                SHADOW,     
                RAIL_0, RAIL_1,
                                RAIL_2, RAIL_3,
                SHADOW_FALL,                    RAIL_4, RAIL_5,
                MISS,                                           RAIL_6, RAIL_7
            };
        }

        public override List<LCDGameInput> GetAvailableInputs()
        {
            return new List<LCDGameInput>()
            {
                new LCDGameInput
                {
                    Name = "Jump",
                    Description = "Jump over gaps",
                    KeyCode = 18, // space
                }
            };
        }

        private int _shadowPosition;
        private int _gapsDodged;
        private int _misses;
        private int _level;
        private bool _spawnFirstGap;

        private List<int> _railPositions = new List<int>();

        protected override List<string> GetVisibleElements()
        {
            var elements = new List<string>();

            if (_level >= 1)
                elements.Add(LEVEL_1);
            if (_level >= 2)
                elements.Add(LEVEL_2);
            if (_level >= 3)
                elements.Add(LEVEL_3);

            elements.Add(GetShadowPosition());

            if (_shadowPosition == 0)
                elements.Add(MISS);

            foreach (var rail in ThreadSafeRailList())
                elements.Add("rail-" + rail);

            return elements;
        }

        private string GetShadowPosition() => "shadow-" + _shadowPosition;

        public override void InitializeGameState()
        {
            _shadowPosition = 1;
            _gapsDodged = 0;
            _misses = 0;
            _level = 0;
            _spawnFirstGap = true;

            _railPositions = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7};
            _customUpdateSpeed = 800;

            StartupMusic();
        }

        public override void HandleInputs(List<LCDGameInput> pressedInputs)
        {
            foreach (var input in pressedInputs)
            {
                if (input == null) continue;

                if (input.Name == "Jump" && _shadowPosition == 1)
                {
                    _shadowPosition = 2;
                }
            }
        }

        protected override void UpdateCore()
        {
            // Unused in this game
        }

        public override void CustomUpdate()
        {
            if (_gapsDodged >= 15)
            {
                QueueSound(new LCDGameSound("../common/level_up.ogg"));
                _misses = 0;
                _gapsDodged = 0;
                _level++;
                
                // Speed up
                _customUpdateSpeed -= 125;
            }

            if (_level == 4)
            {
                Victory();
                return;
            }

            // Move rails forward
            _railPositions = ThreadSafeRailList().Select(x => x-= 1).ToList();

            if (!_railPositions.Contains(0))
            {
                switch (_shadowPosition)
                {
                    case 1:
                        _shadowPosition = 0;
                        QueueSound(new LCDGameSound("../common/miss.ogg"));
                        _misses++;
                        break;
                    case 2:
                        QueueSound(new LCDGameSound("../common/hit.ogg"));
                        _gapsDodged++;
                        break;
                } 
            } 

            // Apply gravity to shadow - unless he's currently jumping over a gap
            if (_shadowPosition != 1 && _railPositions.Contains(0))
            {
                _shadowPosition = 1;
            }

            if (_misses == 10)
            {
                GameOver();
                return;
            }

            // Always spawn a gap on first cycle
            if (_spawnFirstGap)
            {
                _spawnFirstGap = false;
            }
            else
            {
                // 30% chance of not spawning a new rail_7, creating a 1 rail-wide gap
                // Note: The original game seems to loop through predefined sets of gaps instead, but it's easier to just rng it..
                if (_rng.Next(1, 3) != 1)
                {
                    _railPositions.Add(7);
                } else if (!_railPositions.Contains(6))
                {
                    _railPositions.Add(7); // Prevent 2rail-wide gaps
                }
            }
        }

        private void GameOver()
        {
            _shadowPosition = -1;
            _level = 0;
            _railPositions.Clear();

            GenericGameOverAnimation(new List<string> { MISS, SHADOW_FALL, RAIL_1, RAIL_2, RAIL_3, RAIL_4, RAIL_5, RAIL_6, RAIL_7 });
            Stop();
        }

        private void Victory()
        {
            _shadowPosition = -1;
            _level = 0;
            _railPositions.Clear();

            GenericVictoryAnimation(new List<string> { LEVEL_1, LEVEL_2, LEVEL_3 });
            Stop();
        }

        private List<int> ThreadSafeRailList() => new List<int>(_railPositions);
    }
}
