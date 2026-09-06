namespace LastGod.Player
{
    /// <summary>
    /// Enum defining all actions and powers for the Prototype Character.
    /// Basic actions remain active while divine special powers are locked after exiting the stasis chamber.
    /// </summary>
    public enum PrototypePowerType
    {
        // ─── Unlocked Basic Physical Actions ───
        BasicMovement,       // Run, Jump, Ladder Climb
        BasicAttack,         // 3-Hit Melee Combo
        DodgeRoll,           // Dash / Roll
        ShieldBlock,         // Guard / Block

        // ─── Locked Special Prototype Powers (Post-Chamber Suppression) ───
        EnergyWave,          // Ranged Energy Slash / Wave
        Teleport,            // Spatial Teleportation / Phase Shift
        MagicShield,         // Mana / Energy Forcefield Barrier
        TimeSlow,            // Chrono Dilation / Time Slow
        StealthMode,         // Cloaking / Camouflage
        PowerBoost,          // Overdrive Damage Buff
        WindPower,           // Aerial Wind Surge
        EnergyCharge,        // Stasis Energy Absorption
        LightEmission,       // Stun Flash / Light Emission
        SummonAlly,          // Phantom Projection
        HealthRegeneration,  // Nanite Healing / Potion Drink
        Hacking,             // Lab System Control Hack
        Resurrection,        // Phoenix Protocol
        AerialCharge,        // Aerial Dash Strike
        BulletDodge          // Matrix Bullet Evasive Maneuver
    }
}
