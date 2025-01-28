using System.Collections.Generic;
using UnityEngine;

public static class PlayerStats {
    public static float TimeElapsed = 0f;
    public static int BossesKilled = 0; // not displayed, but used for deciding rendering logic
    public static int DamageDealt = 0;
    public static int DamageTaken = 0;
    public static int TimesDodged = 0;
    public static int FrenziesUnleashed = 0;

    public static bool PiercingUpgrade = true;
    public static float AttackCooldownTime = .5f;
    public static float ProjectileSpeed = 5f;
    public static float DodgeCooldownTime = 1f;
    public static int MaxHealth = 10;
    public static int FrenzyThreshold = 50;

}
