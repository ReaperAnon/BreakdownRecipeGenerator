using System.Collections.Generic;
using System;
using System.Linq;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.WPF.Reflection.Attributes;
using Mutagen.Bethesda.FormKeys.SkyrimSE;

namespace BreakdownRecipeGenerator.Config
{
    public class CustomGenericEntry
    {
        public IFormLink<IItemGetter>? RequiredItem { get; set; }
        public int RequiredAmount { get; set; }
        public IFormLink<IItemGetter>? ResultingItem { get; set; }
        public int ResultYield { get; set; }
        public bool TannersRackEntry { get; set; }
        public List<IFormLink<IPerkGetter>>? RequiredPerks { get; set; }

        public CustomGenericEntry(CustomWeaponEntry newEntry) { RequiredItem = newEntry.RequiredItem; RequiredAmount = newEntry.RequiredAmount; ResultingItem = newEntry.ResultingItem; ResultYield = newEntry.ResultYield; RequiredPerks = newEntry.RequiredPerks; TannersRackEntry = newEntry.TannersRackEntry; }
        public CustomGenericEntry(CustomArmorEntry newEntry) { RequiredItem = newEntry.RequiredItem; RequiredAmount = newEntry.RequiredAmount; ResultingItem = newEntry.ResultingItem; ResultYield = newEntry.ResultYield; RequiredPerks = newEntry.RequiredPerks; TannersRackEntry = newEntry.TannersRackEntry; }
        public CustomGenericEntry(CustomMiscEntry newEntry) { RequiredItem = newEntry.RequiredItem; RequiredAmount = newEntry.RequiredAmount; ResultingItem = newEntry.ResultingItem; ResultYield = newEntry.ResultYield; RequiredPerks = newEntry.RequiredPerks; TannersRackEntry = newEntry.TannersRackEntry; }
    }

    [ObjectNameMember(nameof(RequiredItem))]
    public class CustomWeaponEntry
    {
        public IFormLink<IWeaponGetter>? RequiredItem { get; set; }
        public int RequiredAmount { get; set; }
        public IFormLink<IMiscItemGetter>? ResultingItem { get; set; }
        public int ResultYield { get; set; }
        public bool TannersRackEntry { get; set; }
        public List<IFormLink<IPerkGetter>>? RequiredPerks { get; set; }

        public static implicit operator CustomGenericEntry(CustomWeaponEntry entry) => new CustomGenericEntry(entry);

        public CustomWeaponEntry() { RequiredItem = null; RequiredAmount = 0; ResultingItem = null; ResultYield = 0; RequiredPerks = null; TannersRackEntry = false; }
        public CustomWeaponEntry(IFormLink<IWeaponGetter> reqItem, int reqAmount, IFormLink<IMiscItemGetter> resItem, int resAmount, List<IFormLink<IPerkGetter>>? reqPerk = null, bool isTanning = false) { RequiredItem = reqItem; RequiredAmount = reqAmount; ResultingItem = resItem; ResultYield = resAmount; RequiredPerks = reqPerk; TannersRackEntry = isTanning; }
    }

    [ObjectNameMember(nameof(RequiredItem))]
    public class CustomArmorEntry
    {
        public IFormLink<IArmorGetter>? RequiredItem { get; set; }
        public int RequiredAmount { get; set; }
        public IFormLink<IMiscItemGetter>? ResultingItem { get; set; }
        public int ResultYield { get; set; }
        public bool TannersRackEntry { get; set; }
        public List<IFormLink<IPerkGetter>>? RequiredPerks { get; set; }

        public static implicit operator CustomGenericEntry(CustomArmorEntry entry) => new CustomGenericEntry(entry);

        public CustomArmorEntry() { RequiredItem = null; RequiredAmount = 0; ResultingItem = null; ResultYield = 0; RequiredPerks = null; TannersRackEntry = false; }
        public CustomArmorEntry(IFormLink<IArmorGetter> reqItem, int reqAmount, IFormLink<IMiscItemGetter> resItem, int resAmount, List<IFormLink<IPerkGetter>>? reqPerk = null, bool isTanning = false) { RequiredItem = reqItem; RequiredAmount = reqAmount; ResultingItem = resItem; ResultYield = resAmount; RequiredPerks = reqPerk; TannersRackEntry = isTanning; }
    }

    [ObjectNameMember(nameof(RequiredItem))]
    public class CustomMiscEntry
    {
        public IFormLink<IMiscItemGetter>? RequiredItem { get; set; }
        public int RequiredAmount { get; set; }
        public IFormLink<IMiscItemGetter>? ResultingItem { get; set; }
        public int ResultYield { get; set; }
        public bool TannersRackEntry { get; set; }
        public List<IFormLink<IPerkGetter>>? RequiredPerks { get; set; }

        public static implicit operator CustomGenericEntry(CustomMiscEntry entry) => new CustomGenericEntry(entry);

        public CustomMiscEntry() { RequiredItem = null; RequiredAmount = 0; ResultingItem = null; ResultYield = 0; RequiredPerks = null; TannersRackEntry = false; }
        public CustomMiscEntry(IFormLink<IMiscItemGetter> reqItem, int reqAmount, IFormLink<IMiscItemGetter> resItem, int resAmount, List<IFormLink<IPerkGetter>>? reqPerk = null, bool isTanning = false) { RequiredItem = reqItem; RequiredAmount = reqAmount; ResultingItem = resItem; ResultYield = resAmount; RequiredPerks = reqPerk; TannersRackEntry = isTanning; }
    }

    public class Settings
    {
        [Tooltip("The percentage of the original material requirements the breakdown recipe will give back (decimal values are rounded down).")]
        public int YieldPercentage { get; set; } = 80;


        [Tooltip("Makes the patcher generate a recipe for each component of the crafting recipe instead of only for the highest tier ones.")]
        public bool GenerateRecipeForEachComponent { get; set; } = false;


        [Tooltip("A list of crafting station keywords to exclude. All crafting stations that have tempering recipes should be excluded here.")]
        public List<FormLink<IKeywordGetter>> ExcludedCraftingStations = new()
        {
            Skyrim.Keyword.CraftingSmithingArmorTable,
            Skyrim.Keyword.CraftingSmithingSharpeningWheel
        };


        [Tooltip("A custom list of items that get a breakdown recipe generated yielding the chosen components.")]
        public List<CustomMiscEntry> MiscRecipes { get; set; } = new()
        {
            new CustomMiscEntry(Skyrim.MiscItem.IronMaceBrokenHandle, 1, Skyrim.MiscItem.IngotIron, 1),
            new CustomMiscEntry(Skyrim.MiscItem.IronMaceBrokenTop, 1, Skyrim.MiscItem.IngotIron, 1),
            new CustomMiscEntry(Skyrim.MiscItem.IronSwordBrokenHandle, 1, Skyrim.MiscItem.IngotIron, 1),
            new CustomMiscEntry(Skyrim.MiscItem.IronSwordBrokenTop, 1, Skyrim.MiscItem.IngotIron, 1),
            new CustomMiscEntry(Skyrim.MiscItem.IronWarAxeBrokenHandle, 1, Skyrim.MiscItem.IngotIron, 1),
            new CustomMiscEntry(Skyrim.MiscItem.IronWarAxeBrokenTop, 1, Skyrim.MiscItem.IngotIron, 1),
            new CustomMiscEntry(Skyrim.MiscItem.Gold001, 100, Skyrim.MiscItem.IngotGold, 1),
            new CustomMiscEntry(Skyrim.MiscItem.Firewood01, 1, Skyrim.MiscItem.Coal01, 3),
            new CustomMiscEntry(Skyrim.MiscItem.Firewood01, 1, Skyrim.MiscItem.Charcoal, 6),
            new CustomMiscEntry(Skyrim.MiscItem.DwarvenBowl01, 2, Skyrim.MiscItem.IngotDwarven, 1, new() {Skyrim.Perk.DwarvenSmithing}),
            new CustomMiscEntry(Skyrim.MiscItem.DwarvenBowl02, 2, Skyrim.MiscItem.IngotDwarven, 1, new() {Skyrim.Perk.DwarvenSmithing}),
            new CustomMiscEntry(Skyrim.MiscItem.DwarvenBowl03, 2, Skyrim.MiscItem.IngotDwarven, 1, new() {Skyrim.Perk.DwarvenSmithing}),
            new CustomMiscEntry(Skyrim.MiscItem.DwarvenCog, 3, Skyrim.MiscItem.IngotDwarven, 1, new() {Skyrim.Perk.DwarvenSmithing}),
            new CustomMiscEntry(Skyrim.MiscItem.DwarvenGear, 3, Skyrim.MiscItem.IngotDwarven, 1, new() {Skyrim.Perk.DwarvenSmithing}),
            new CustomMiscEntry(Skyrim.MiscItem.DwarvenScrapMetal, 2, Skyrim.MiscItem.IngotDwarven, 1, new() {Skyrim.Perk.DwarvenSmithing})
        };


        [Tooltip("A custom list of items that get a breakdown recipe generated yielding the chosen components.")]
        public List<CustomArmorEntry> ArmorRecipes { get; set; } = new();


        [Tooltip("A custom list of items that get a breakdown recipe generated yielding the chosen components.")]
        public List<CustomWeaponEntry> WeaponRecipes { get; set; } = new();
    }
}
