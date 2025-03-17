#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Sortify
{
    public class SortifyExampleUseAttribute : MonoBehaviour
    {
#if SORTIFY_ATTRIBUTES

        [ButtonGroup]
        public PlayerTab SelectedTab;

        public enum PlayerTab
        {
            Stats,
            Equipment,
            Abilities,
            Growth,
            Misc
        }

        [ShowIf("SelectedTab", PlayerTab.Stats)]
        [ClassDrawer("Player Stats")]
        public PlayerStats Stats = new PlayerStats();

        [ShowIf("SelectedTab", PlayerTab.Equipment)]
        [ClassDrawer("Player Equipment")]
        public PlayerEquipment Equipment = new PlayerEquipment();

        [ShowIf("SelectedTab", PlayerTab.Abilities)]
        [ClassDrawer("Player Abilities")]
        public PlayerAbilities Abilities = new PlayerAbilities();

        [ShowIf("SelectedTab", PlayerTab.Growth)]
        [ClassDrawer("Player Growth")]
        public PlayerGrowth Growth = new PlayerGrowth();

        [ShowIf("SelectedTab", PlayerTab.Misc)]
        [ClassDrawer("Miscellaneous")]
        public PlayerMisc Misc = new PlayerMisc();

        [Button("Reset All Stats")]
        public void ResetAllStats()
        {
            Stats.Health = 100f;
            Stats.Mana = 100f;
            Stats.Stamina = 100f;
            Growth.Experience = 0;
            Growth.SkillPoints = 0;
            Debug.Log("All stats reset!");
        }

        [System.Serializable]
        public class PlayerStats
        {
            [ProgressBar(0, 100, true)]
            public float Health = 75f;

            [ProgressBar(0, 100)]
            public float Mana = 50f;

            [ProgressBar(0, 100)]
            public float Stamina = 85f;

            [Space(5)]
            [MinMaxSlider(1, 20)]
            public Vector2 DamageRange = new Vector2(5, 15);

            [RangeStep(0, 1, 0.05f)]
            public float CriticalHitChance = 0.25f;

            [Unit("kg")]
            public float Weight = 72.5f;

            [Unit("m")]
            public float Height = 1.8f;
        }

        [System.Serializable]
        public class PlayerEquipment
        {
            public enum ItemType
            {
                MeleeWeapon,
                RangedWeapon,
                MagicWeapon,
                Shield,
                Ammunition,
                Helmet,
                ChestArmor,
                LegArmor,
                Boots,
                Gloves,
                Cape,
                Consumable,
                CraftingMaterial,
                Tool,
                Key,
                Potion,
                Scroll,
                Gem,
                Artifact,
                Currency,
                RawMaterial,
                EnergyResource,
                Map,
                Letter,
                SpecialKey,
                Furniture,
                CosmeticItem,
                Trophy,
                Gadget,
                Device,
                Parts,
                Badge,
                BuffItem,
                DebuffItem,
                SummonItem,
                SkillBook,
                Blueprint,
                AchievementUnlocker
            }

#if SORTIFY_COLLECTIONS
            public SDictionary<ItemType, List<Item>> Items = new SDictionary<ItemType, List<Item>>();
#endif
            [System.Serializable]
            public class Item
            {
                public string Name;
                public string Description;
            }

            [Dropdown("GetWeapons")]
            public string EquippedWeapon;

            [Dropdown("GetArmors")]
            public string EquippedArmor;

            [ButtonGroup]
            public EquipmentType SelectedEquipmentType;

            public enum EquipmentType
            {
                Weapon,
                Armor,
                Accessory
            }

            [ShowIf("Equipment.SelectedEquipmentType", EquipmentType.Weapon)]
            [ClassDrawer("Weapon Settings")]
            public WeaponSettings Weapon = new WeaponSettings();

            [ShowIf("Equipment.SelectedEquipmentType", EquipmentType.Armor)]
            [ClassDrawer("Armor Settings")]
            public ArmorSettings Armor = new ArmorSettings();

            [ShowIf("Equipment.SelectedEquipmentType", EquipmentType.Accessory)]
            [ClassDrawer("Accessory Settings")]
            public AccessorySettings Accessory = new AccessorySettings();

            [System.Serializable]
            public class WeaponSettings
            {
                [Dropdown("GetWeapons")]
                public string WeaponType;

                [ProgressBar(0, 100)]
                public float Durability;

                [Editable]
                public ScriptableObject Data;

                [Editable]
                public GameObject WeaponPrefab;
                public List<string> WeaponEffects = new List<string>();
            }

            [System.Serializable]
            public class ArmorSettings
            {
                [Dropdown("GetArmors")]
                public string ArmorType;

                [ProgressBar(0, 100)]
                public float DefenseRating;

                [Editable]
                public ScriptableObject Data;

                [Editable]
                public GameObject ArmorPrefab;
                public List<string> ArmorEffects = new List<string>();
            }

            [System.Serializable]
            public class AccessorySettings
            {
                [Dropdown("GetAccessories")]
                public string AccessoryType;

                [ProgressBar(0, 100)]
                public float BonusEffectStrength;

                [Editable]
                public ScriptableObject Data;

                [Editable]
                public GameObject AccessoryPrefab;
                public List<string> AccessoryEffects = new List<string>();
            }
        }

        [System.Serializable]
        public class PlayerAbilities
        {
            [Toggle]
            public bool CanDoubleJump;

            [Space(5)]
            [Toggle]
            public bool CanUseMagic;

            [ShowIf("Abilities.CanUseMagic")]
            [Dropdown("GetMagicSpells")]
            public string SelectedSpell;

            [ShowIf("Abilities.CanUseMagic")]
            [ClassDrawer("Magic Settings")]
            public MagicSettings Magic = new MagicSettings();

            [System.Serializable]
            public class MagicSettings
            {
                [Dropdown("GetMagicSchools")]
                public string MagicSchool;

                [RangeStep(1, 10, 0.5f)]
                public float PowerLevel;

                [Unit("s")]
                public float CastTime;
            }
        }

        [System.Serializable]
        public class PlayerGrowth
        {
            [ProgressBar(0f, 100f)]
            public float Experience = 75f;

            [ProgressBar(0f, 100f)]
            public float SkillPoints = 15f;

            [Dropdown("GetSkillCategories")]
            public string SelectedSkillCategory;

            [ShowIf("Growth.SelectedSkillCategory", "Magic")]
            [ClassDrawer("Magic Skill Details")]
            public SkillDetails MagicSkillDetails = new SkillDetails();

            [ShowIf("Growth.SelectedSkillCategory", "Combat")]
            [ClassDrawer("Combat Skill Details")]
            public SkillDetails CombatSkillDetails = new SkillDetails();

            [System.Serializable]
            public class SkillDetails
            {
                [BetterHeader("Skill Progression")]
                [ProgressBar(0, 100)]
                public float Progress;

                [Editable]
                public ScriptableObject SkillData;
            }
        }

        [System.Serializable]
        public class PlayerMisc
        {
            [Tag]
            public string PlayerTag;

            [Layer]
            public int PlayerLayer;

            [Editable]
            public ScriptableObject CustomData;

            [Editable]
            public Material CustomMaterial;
        }

        private IEnumerable<string> GetWeapons()
        {
            return new List<string> { "Sword", "Axe", "Bow", "Dagger" };
        }

        private IEnumerable<string> GetArmors()
        {
            return new List<string> { "Light Armor", "Heavy Armor", "Magic Robes" };
        }

        private IEnumerable<string> GetAccessories()
        {
            return new List<string> { "Ring", "Amulet", "Charm" };
        }

        private IEnumerable<string> GetMagicSpells()
        {
            return new List<string> { "Fireball", "Ice Shard", "Heal", "Teleport" };
        }

        private IEnumerable<string> GetMagicSchools()
        {
            return new List<string> { "Fire", "Ice", "Lightning", "Earth" };
        }

        private IEnumerable<string> GetSkillCategories()
        {
            return new List<string> { "Combat", "Magic", "Crafting", "Stealth" };
        }
#endif
        }
    }
#endif
