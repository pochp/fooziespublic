using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FooziesConstants
{
    class GameplayConstants
    {
        /// <summary>
        /// GAMEPLAY CONSTANTS
        /// </summary>
        /// 

        public const int ATTACK_STARTUP = 6;
        public const int ATTACK_ACTIVE = 4;
        public const int ATTACK_FULL_EXTEND = 2;
        public const int ATTACK_RECOVERY_TOTAL = 45;
        public const int ATTACK_RECOVERY_SHORTEN = 26;
        public const int HURTBOX_WHIFF_EARLY = 2000;
        public const int HURTBOX_WHIFF_LATE = 1000;
        public const int HURTBOX_STARTUP = 500;
        public const int HURTBOX_ACTIVE = 500; //hurtbox when active is shorter than the recovery so it favorises clashes
        public const int HITBOX_ACTIVE_EARLY = 1200;
        public const int HITBOX_ACTIVE_LATE = 2200;
        public const int CHARACTER_HURTBOX_WIDTH = 500;
        public const int CHARACTER_HURTBOX_HEIGHT = 2500;
        public const int CROUCHING_HURTBOX_WIDTH = 600;
        public const int CROUCHING_HURTBOX_HEIGHT = 1000;
        public const int THROW_STARTUP = 3;
        public const int THROW_ACTIVE = 2;
        public const int THROW_RECOVERY = 30;
        public const int THROW_BREAK_WINDOW = 5;
        public const int THROW_STARTUP_HURTBOX = 400;
        public const int THROW_ACTIVE_RANGE = 800;
        public const int THROW_RECOVERY_HURTBOX = 400;
        public const int WALK_F_SPEED = 65;
        public const int WALK_B_SPEED = -50;
        public const int ATTACK_HITSTOP = 5;
        public const int ATTACK_BLOCKSTUN = 43;
        public const int ATTACK_PUSHBACK_SPEED = -120;
        public const int ATTACK_PUSHBACK_DURATION = 5;
        public const int BREAK_DURATION = 15;
        public const int BREAK_PUSHBACK = -120;
        public const int CLASH_PUSHBACK_SPEED = -150;
        public const int CLASH_PUSHBACK_DURATION = 10;
        public const int CLASH_HITSTOP = 20;
        public const int BLOCK_HITSTOP = 10;
        public const int STARTING_POSITION = 5000;
        public const int FRAMES_PER_ROUND = 1800; //30 seconds
        public const int ARENA_RADIUS = 8700;
        public const int ROUNDS_TO_WIN = 5;
         
         /// <summary>
         /// TIME CONSTANTS
         /// </summary>
        public const int FRAMES_END_ROUND_SPLASH = 120;
        public const int FRAMES_COUNTDOWN = 30;
        public const float FRAME_LENGTH = 0.016666666666f;
        public const int GAME_OVER_LENGTH = 99999;
    }
}
