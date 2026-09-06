using UnityEngine;

namespace LastGod.ThirdPerson.FutureEntities
{
    public enum DivineEntityDomain
    {
        Wrath,
        Love,
        Time,
        Faith,
        Control,
        Creation,
        Void
    }

    public interface IEntityBoss
    {
        DivineEntityDomain Domain { get; }
        string EntityName { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDefeated { get; }

        void InitializeBoss(EntityData data);
        void TriggerBossPhase(int phaseIndex);
        void OnAeronAbsorbEnergy();
    }

    [CreateAssetMenu(fileName = "NewEntityData", menuName = "LastGod/Entities/EntityData")]
    public class EntityData : ScriptableObject
    {
        public DivineEntityDomain domain;
        public string entityName;
        [TextArea(2, 4)]
        public string loreDescription;
        public float maxHealth = 600f;
        public int phaseCount = 3;
        public AudioClip bossThemeMusic;
        public AudioClip deathSequenceMusic;
        public GameObject arenaVFXPrefab;
        public GameObject absorbedPowerPrefab;
    }
}
