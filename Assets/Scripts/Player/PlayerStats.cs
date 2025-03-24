using System.Collections.Generic;
using UnityEngine;

public static class PlayerStats {
    public static float TimeElapsed = 0f;
    public static int BossesKilled = 0; // not displayed, but used for deciding rendering logic
    public static int DamageDealt = 0;
    public static int DamageTaken = 0;
    public static int TimesDodged = 0;
    public static int FrenziesUnleashed = 0;

    public static bool PiercingUpgrade = false;
    public static float AttackCooldownTime = .3f; // upgraded .15f, original .3f
    public static float ProjectileSpeed = 5f; // upgraded 8f, original 5f
    public static float DodgeCooldownTime = 1f; // upgraded .5f, original 1f
    public static int MaxHealth = 10; // upgraded 15, original 10
    public static int FrenzyThreshold = 50; // upgraded 25, original 50

    public static bool Victory = false;

}
