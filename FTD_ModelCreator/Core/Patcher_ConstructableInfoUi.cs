using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.Ftd.Constructs.UI;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Choices;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Numbers;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Texts;
using BrilliantSkies.Ui.Consoles.Segments;
using BrilliantSkies.Ui.Consoles.Styles;
using BrilliantSkies.Ui.Examples.Log;
using BrilliantSkies.Ui.Tips;
using FTD_ModelCreator.Model;
using HarmonyLib;
using UnityEngine;

namespace FTD_ModelCreator.Core
{

    [HarmonyPatch(typeof(GeneralTab), "Build")]
    class Patcher_ConstructableInfoUi
    {
        private static readonly string tabname = "Model";
        private static readonly string modelspawnblockname = "ModelSpawner";
        private static int currentdownsizeddecos;
        private static int currentremaindecos;
        //private static bool isoverride=false;

        static void Postfix(GeneralTab __instance)
        {
            ModelCreator creator = ModelCreator.GetModelCreator(__instance._focus.Construct.Main.GetName(), __instance._focus.Construct.UniqueId);
            ScreenSegmentStandard segment = __instance.CreateStandardSegment();
            segment.NameWhereApplicable = tabname;
            segment.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            segment.SpaceAbove = 30f;
            UpdateDecoAmounts(__instance, creator);
            ScreenSegmentStandardHorizontal horizontal = __instance.CreateStandardHorizontalSegment();
            horizontal.AddInterpretter(SubjectiveDisplay<ConstructInfo>.Quick(__instance._focus, M.m<ConstructInfo>((a) => { return $"Used:{currentdownsizeddecos}"; })));
            horizontal.AddInterpretter(SubjectiveDisplay<ConstructInfo>.Quick(__instance._focus, M.m<ConstructInfo>((a) => { return $"Remain:{currentremaindecos}"; })));
            if (creator.converted)
            {
                horizontal.AddInterpretter(SubjectiveDisplay<ConstructInfo>.Quick(__instance._focus, M.m<ConstructInfo>((a) => { return "<color=red>Already converted.</color>"; })));
            }
            else
            {
                if (creator.CanConvert(__instance._focus.Construct))
                {
                    horizontal.AddInterpretter(SubjectiveDisplay<ConstructInfo>.Quick(__instance._focus, M.m<ConstructInfo>((a) => { return "<color=green>We can convert it.</color>"; })));
                }
                else
                {
                    horizontal.AddInterpretter(SubjectiveDisplay<ConstructInfo>.Quick(__instance._focus, M.m<ConstructInfo>((a) => { return "<color=red>We cannot convert it.</color>"; })));
                }
            }
            segment.AddInterpretter<SubjectiveButton<ConstructInfo>>(SubjectiveButton<ConstructInfo>.Quick(__instance._focus, "Create Model", new ToolTip("Make model of this vehicle.You should take backup before excuting this."),
                (construct) =>
                {
                    if (creator.CanConvert(__instance._focus.Construct) && !creator.converted)
                    {
                        GUISoundManager.GetSingleton().PlaySuccess();
                        creator.Create(construct.Construct);
                    }
                    else
                    {
                        GUISoundManager.GetSingleton().PlayFailure();
                    }
                }));
            segment.AddInterpretter<SubjectiveFloatClampedWithBar<ConstructInfo>>(new SubjectiveFloatClampedWithBar<ConstructInfo>(M.m<ConstructInfo>(0), M.m<ConstructInfo>(ModelCreator.maxreducescale),
        M.m<ConstructInfo>((ConstructInfo a) => creator.ReducedScale), M.m<ConstructInfo>(0.1f), __instance._focus, M.m<ConstructInfo>("Reduced Scale"), (a, v) => { if (v >= ModelCreator.minreducescale) { creator.ReducedScale = (uint)v; } else { creator.ReducedScale = ModelCreator.minreducescale; } },
        (a, b) => "Change Reduced Scale", M.m<ConstructInfo>(new ToolTip("Change Reduced Scale"))));
#if DEBUG
            segment.AddInterpretter<SubjectiveButton<ConstructInfo>>(SubjectiveButton<ConstructInfo>.Quick(__instance._focus, "Open Log", new ToolTip("デバッグ用"),
                (construct) =>
                {
                    //AdvLogger.LogInfo(construct.Construct.AllSubConstructsPerIndex[0].AllBasicsRestricted.AliveAndDead.Blocks[0].item.Code.ClassName);
                    new LogUi(new LogFinderDirectory(new System.IO.DirectoryInfo(Get.PermanentPaths.CopiedLogsDir().ToString()), AdvLogger.CurrentLogFile)).ActivateGui(BrilliantSkies.Ui.Displayer.GuiActivateType.Force);
                }));
            segment.AddInterpretter<SubjectiveButton<ConstructInfo>>(SubjectiveButton<ConstructInfo>.Quick(__instance._focus, "Quit", new ToolTip("デバッグ用"),
                (construct) =>
                {
                    Application.Quit();
                }));
#endif

            //segment.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Override Deco Limit", new ToolTip("Override Deco Limit"),
            //  (a, b) => { isoverride = b;if (isoverride) { AllConstructDecorations._limitPerPacketManager = 99999; } else { AllConstructDecorations._limitPerPacketManager = 5000; }; }, (a) => { return isoverride; }));
            ScreenSegmentTable table = __instance.CreateTableSegment(3, 10);
            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude AI Blocks", new ToolTip("Exclude AI Blocks"),
                (a, b) => { a.excludeAITabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeAITabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Defence Blocks", new ToolTip("Exclude Defence Blocks"),
               (a, b) => { a.excludeDefenceTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeDefenceTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Air Blocks", new ToolTip("Exclude Air Blocks"),
               (a, b) => { a.excludeAirTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeAirTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude APS Blocks(Except Barrels)", new ToolTip("Exclude APS Blocks(Except Barrels and Mantlets)"),
               (a, b) => { a.excludeAPSTabBlocksExceptBarrel = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeAPSTabBlocksExceptBarrel; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude APS Barrels", new ToolTip("Exclude APS Barrels"),
               (a, b) => { a.excludeAPSBarrel = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeAPSBarrel; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude CRAM Blocks(Except Barrels)", new ToolTip("Exclude CRAM Blocks(Except Barrels)"),
               (a, b) => { a.excludeCRAMTabBlocksExceptBarrel = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeCRAMTabBlocksExceptBarrel; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude CRAM Barrels", new ToolTip("Exclude CRAM Barrels"),
               (a, b) => { a.excludeCRAMBarrel = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeCRAMBarrel; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Deco Blocks", new ToolTip("Exclude Deco Blocks"),
               (a, b) => { a.excludeDecoTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeDecoTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Fuel Engine Blocks", new ToolTip("Exclude Fuelengine Blocks"),
               (a, b) => { a.excludeFuelengineTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeFuelengineTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Steam Engine Blocks", new ToolTip("Exclude Steamengine Blocks"),
             (a, b) => { a.excludeSteamengineTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeSteamengineTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Land Blocks", new ToolTip("Exclude Land Blocks"),
               (a, b) => { a.excludeLandTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeLandTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Laser Blocks(Except Opticses)", new ToolTip("Exclude Laser Blocks(Except Opticses)"),
               (a, b) => { a.excludeLaserTabBlocksExceptOptics = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeLaserTabBlocksExceptOptics; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Laser Blocks", new ToolTip("Exclude Laser Blocks"),
               (a, b) => { a.excludeLaserOptics = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeLaserOptics; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude PAC Blocks(Except Lenses)", new ToolTip("Exclude PAC Blocks(Except Lenses)"),
               (a, b) => { a.excludePACTabBlocksExceptLens = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludePACTabBlocksExceptLens; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude PAC Lenses", new ToolTip("Exclude PAC Lenses"),
             (a, b) => { a.excludePACLens = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludePACLens; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Control Blocks", new ToolTip("Exclude Control Blocks"),
               (a, b) => { a.excludeControlTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeControlTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Misc Blocks", new ToolTip("Exclude Misc Blocks"),
             (a, b) => { a.excludeMiscTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeMiscTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Missile Blocks", new ToolTip("Exclude Missile Blocks"),
             (a, b) => { a.excludeMissileTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeMissileTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Resources Blocks", new ToolTip("Exclude Resources Blocks"),
               (a, b) => { a.excludeResourcesTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeResourcesTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Simple Weapon Blocks", new ToolTip("Exclude SimpleWeapon Blocks"),
               (a, b) => { a.excludeSimpleWeaponTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeSimpleWeaponTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Water Blocks", new ToolTip("Exclude Water Blocks"),
            (a, b) => { a.excludeWaterTabBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeWaterTabBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Blocks Blocks(Except Structures)", new ToolTip("Exclude Blocks Blocks(Except Structures)"),
               (a, b) => { a.excludeBlocksTabBlocksExceptStructure = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeBlocksTabBlocksExceptStructure; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Metals", new ToolTip("Exclude Metals"),
               (a, b) => { a.excludeMetalBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeMetalBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Woods", new ToolTip("Exclude Woods"),
                           (a, b) => { a.excludeWoodBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeWoodBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Stones", new ToolTip("Exclude Stones"),
               (a, b) => { a.excludeStoneBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeStoneBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude HAs", new ToolTip("Exclude HAs"),
               (a, b) => { a.excludeHABlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeHABlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Glasses", new ToolTip("Exclude Glasses"),
               (a, b) => { a.excludeGlassBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeGlassBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Leads", new ToolTip("Exclude Leads"),
               (a, b) => { a.excludeLeadBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeLeadBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Rubbers", new ToolTip("Exclude Rubbers"),
               (a, b) => { a.excludeRubberBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeRubberBlocks; }));

            table.AddInterpretter<SubjectiveToggle<ModelCreator>>(SubjectiveToggle<ModelCreator>.Quick(creator, "Exclude Alloys", new ToolTip("Exclude Alloys"),
               (a, b) => { a.excludeAlloyBlocks = b;/* UpdateDecoAmounts(__instance, creator);*/ }, (a) => { return a.excludeAlloyBlocks; }));
            //foreach (var item in creator.GetSimplifyZones())
            //{
            //    ScreenSegmentStandardHorizontal horizontal = __instance.CreateStandardHorizontalSegment();
            //    horizontal.AddInterpretter<SubjectiveButton<ConstructInfo>>(SubjectiveButton<ConstructInfo>.Quick(__instance._focus, "Remove This Simplify Zone"
            //  , new ToolTip("A"), (c) => { creator.RemoveSimplifyZone(item.Key);currentremaindecos=creator.CalculateDownsizedDecorationAmount(__instance._focus.Construct); currentdownsizeddecos=creator.CalculateRemainDecorationAmount(__instance._focus.Construct); }));
            //    horizontal.AddInterpretter<StringDisplay>(StringDisplay.Quick($"Start:{item.Key.Key} End:{item.Key.Value}"));       
            //}
        }

        private static void UpdateDecoAmounts(GeneralTab __instance, ModelCreator creator)
        {
            currentremaindecos = creator.CalculateRemainDecorationAmount(__instance._focus.Construct); currentdownsizeddecos = creator.CalculateDownsizedDecorationAmount(__instance._focus.Construct);
        }
    }
}