using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using System.Threading.Tasks;
using Mutagen.Bethesda.Plugins;
using BreakdownRecipeGenerator.Config;
using Noggog;
using Mutagen.Bethesda.Plugins.Cache;

namespace BreakdownRecipeGenerator
{
    public class Program
    {
        public static Lazy<Settings> Config = null!;
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetAutogeneratedSettings("Mod Settings", "settings.json", out Config)
                .SetTypicalOpen(GameRelease.SkyrimSE, "CraftingBreakdown.esp")
                .Run(args);
        }

        // A struct to hold the tiers of diverse crafting components.
        struct TierEntry
        {
            public IFormLinkGetter<IItemGetter> component;
            public short componentTier;

            public TierEntry(IFormLinkGetter<IItemGetter> item, short tier)
            {
                component = item; componentTier = tier;
            }
        };

        // The list of crafting component tiers; anything not listed has a tier of 0, so less important than anything listed.
        static List<TierEntry> IngredientTiers { get; } = new()
        {
            new TierEntry(Skyrim.Ingredient.BoneMeal, 1),
            new TierEntry(Skyrim.MiscItem.Leather01, 1),
            new TierEntry(Skyrim.MiscItem.IngotIron, 1),
            new TierEntry(Skyrim.MiscItem.IngotSteel, 2),
            new TierEntry(Skyrim.MiscItem.ingotSilver, 2),
            new TierEntry(Skyrim.MiscItem.IngotCorundum, 2),
            new TierEntry(Skyrim.MiscItem.IngotDwarven, 3),
            new TierEntry(Skyrim.MiscItem.IngotIMoonstone, 3),
            new TierEntry(Skyrim.MiscItem.IngotQuicksilver, 3),
            // new TierEntry(Skyrim.MiscItem.IngotGold, 3), // Not listed as higher tier so gems or gold can each be extracted.
            new TierEntry(Skyrim.MiscItem.IngotOrichalcum, 3),
            new TierEntry(Skyrim.MiscItem.IngotMalachite, 3),
            new TierEntry(Skyrim.MiscItem.IngotEbony, 4),
            new TierEntry(Skyrim.MiscItem.DragonBone, 4),
            new TierEntry(Skyrim.MiscItem.DragonScales, 4),
            new TierEntry(Dragonborn.MiscItem.DLC2OreStalhrim, 4)
        };

        // Returns the tier of the given crafting component.
        static short GetComponentTier(IFormLinkGetter<IItemGetter> component)
        {
            return IngredientTiers.Find(comp => comp.component.Equals(component)).componentTier;
        }

        // Returns whether a crafting recipe is empty or not.
        static bool IsRecipeEmpty(IConstructibleObjectGetter recipeGetter)
        {
            return recipeGetter.Items is null || recipeGetter.Items.Count == 0;
        }

        // Returns whether the recipes we are looking at have the correct objects we can make breakdown recipes for.
        static bool IsRecipeCorrect(IConstructibleObjectGetter recipeGetter, ILinkCache linkCache)
        {
            foreach(var excludedKey in Config.Value.ExcludedCraftingStations) if (recipeGetter.WorkbenchKeyword.Equals(excludedKey))
                return false;
            if (!recipeGetter.CreatedObject.TryResolve(linkCache, out var resolvedObject))
                return false;

            return resolvedObject is IArmorGetter || resolvedObject is IWeaponGetter || resolvedObject is IAmmunitionGetter;
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Constructs a crafting recipe.
            void ConstructRecipe(IConstructibleObjectGetter originalRecipe, ContainerEntry requiredItems, IFormLinkNullable<IConstructibleGetter> producedItem, int yield, int idx)
            {
                ConstructibleObject newRecipe = new(state.PatchMod, "Breakdown" + originalRecipe.EditorID + idx);
                newRecipe.Items = new(); newRecipe.Items.Add(requiredItems);
                newRecipe.CreatedObject = producedItem;
                newRecipe.CreatedObjectCount = (ushort)yield;

                // Set the keyword based on the component it produces.
                if (producedItem.Equals(Skyrim.MiscItem.Leather01) || producedItem.Equals(Skyrim.MiscItem.LeatherStrips))
                    newRecipe.WorkbenchKeyword = Skyrim.Keyword.CraftingTanningRack.AsNullable();
                else
                    newRecipe.WorkbenchKeyword = Skyrim.Keyword.CraftingSmelter.AsNullable();

                // Add a condition for having to have the item to be deconstructed on you to reduce clutter.
                ConditionFloat hasItemCondition = new();
                FunctionConditionData functionCondition = new();
                functionCondition.Function = Condition.Function.GetItemCount;
                functionCondition.ParameterOneRecord = requiredItems.Item.Item;
                hasItemCondition.Data = functionCondition;
                hasItemCondition.CompareOperator = CompareOperator.GreaterThan;
                hasItemCondition.ComparisonValue = 0;
                newRecipe.Conditions.Add(hasItemCondition);

                // Add any perk conditions items might have so you can't melt down items you don't have the perks to craft.
                foreach(var condition in originalRecipe.Conditions)
                {
                    if (condition is not IConditionFloatGetter || condition.Data is not IFunctionConditionDataGetter) continue;
                    if ((condition.Data as IFunctionConditionDataGetter)!.Function != Condition.Function.HasPerk) continue;
                    newRecipe.Conditions.Add(condition.DeepCopy());
                }

                state.PatchMod.ConstructibleObjects.Add(newRecipe);
            }

            foreach (var recipeGetter in state.LoadOrder.PriorityOrder.ConstructibleObject().WinningOverrides().ToList())
            {
                if (recipeGetter.EditorID is null) continue;
                if (IsRecipeEmpty(recipeGetter)) continue;
                if (!IsRecipeCorrect(recipeGetter, state.LinkCache)) continue;

                // Set the list of components for which recipes will be generated to the highest tier ones or to all, based on the selected option.
                int idx = 0;
                int highestTier = recipeGetter.Items!.Max(recipe => GetComponentTier(recipe.Item.Item));
                IEnumerable<IContainerEntryGetter> componentList = Config.Value.GenerateRecipeForEachComponent ? recipeGetter.Items! : recipeGetter.Items!.Where(item => GetComponentTier(item.Item.Item) == highestTier);
                foreach (var itemEntry in componentList)
                {
                    ContainerItem contItem = new();
                    ContainerEntry contEntry = new();
                    contItem.Item = recipeGetter.CreatedObject.AsNullable();
                    contItem.Count = recipeGetter.CreatedObjectCount ?? 1;
                    contEntry.Item = contItem;

                    // Determine the yield of the breakdown recipe.
                    int recipeYield = (int)Math.Floor(itemEntry.Item.Count * Config.Value.YieldPercentage / 100f);
                    if(recipeYield < 1)
                    {
                        recipeYield = 1;
                        contEntry.Item.Count = (int)Math.Ceiling(contEntry.Item.Count * (2 - Config.Value.YieldPercentage / 100f));
                    }

                    // Pass the function an index so there won't be any editor ID conflicts for items with multiple recipes.
                    ConstructRecipe(recipeGetter, contEntry, itemEntry.Item.Item.Cast<IConstructibleGetter>().AsNullable(), recipeYield, ++idx);
                }
            }

            // Merge the lists into one for ease of use.
            List<CustomGenericEntry> customRecipes = new();
            foreach (var entry in Config.Value.ArmorRecipes) customRecipes.Add(entry);
            foreach (var entry in Config.Value.MiscRecipes) customRecipes.Add(entry);
            foreach (var entry in Config.Value.WeaponRecipes) customRecipes.Add(entry);
            foreach (var customEntry in customRecipes.EmptyIfNull())
            {
                // Give it a unique ID based on the contained items to avoid conflicting editor IDs.
                if (customEntry.RequiredItem is null || customEntry.ResultingItem is null) continue;
                ConstructibleObject newRecipe = new(mod: state.PatchMod, editorID: "Breakdown" + customEntry.RequiredItem + "-" + customEntry.ResultingItem);

                ContainerItem contItem = new();
                ContainerEntry contEntry = new();
                contItem.Item = customEntry.RequiredItem;
                contItem.Count = customEntry.RequiredAmount;
                contEntry.Item = contItem;

                newRecipe.Items = new(); newRecipe.Items.Add(contEntry);
                newRecipe.CreatedObject = customEntry.ResultingItem.Cast<IConstructibleGetter>().AsNullable();
                newRecipe.CreatedObjectCount = (ushort)customEntry.ResultYield;
                newRecipe.WorkbenchKeyword = customEntry.TannersRackEntry ? Skyrim.Keyword.CraftingTanningRack.AsNullable() : Skyrim.Keyword.CraftingSmelter.AsNullable();

                // Add a condition for having to have the item on you to reduce clutter.
                ConditionFloat hasItemCondition = new();
                FunctionConditionData itemFunctionCondition = new();
                itemFunctionCondition.Function = Condition.Function.GetItemCount;
                itemFunctionCondition.ParameterOneRecord = customEntry.RequiredItem;
                hasItemCondition.Data = itemFunctionCondition;
                hasItemCondition.CompareOperator = CompareOperator.GreaterThan;
                hasItemCondition.ComparisonValue = 0;
                newRecipe.Conditions.Add(hasItemCondition);

                // Add any perk requirements specified in the settings.
                foreach(var perkReq in customEntry.RequiredPerks.EmptyIfNull())
                {
                    ConditionFloat hasPerkCondition = new();
                    FunctionConditionData perkFunctionCondition = new();
                    perkFunctionCondition.Function = Condition.Function.HasPerk;
                    perkFunctionCondition.ParameterOneRecord = perkReq;
                    hasPerkCondition.Data = perkFunctionCondition;
                    hasPerkCondition.CompareOperator = CompareOperator.EqualTo;
                    hasPerkCondition.ComparisonValue = 1;
                    newRecipe.Conditions.Add(hasPerkCondition);
                }

                state.PatchMod.ConstructibleObjects.Add(newRecipe);
            }
        }
    }
}