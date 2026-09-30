using System;
using UnityEngine;

namespace LastGod.Weapons
{
    /// <summary>
    /// Authoritative contract for all equipable weapons in The Last God.
    /// </summary>
    public interface IWeapon
    {
        string WeaponName { get; }
        int CurrentAmmo { get; }
        int MagazineCapacity { get; }
        bool IsReloading { get; }
        bool CanFire { get; }

        event Action<int, int> OnAmmoChanged; // (current, max)
        event Action OnFired;
        event Action OnReloadStarted;
        event Action OnReloadFinished;
        event Action OnDryFired;

        void Aim(Vector2 direction);
        bool Fire();
        void Reload();
    }
}
