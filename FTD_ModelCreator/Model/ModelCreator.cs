using BrilliantSkies.Blocks.Decorative;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Ftd.Constructs.Modules.All.Decorations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using BrilliantSkies.Common.Controls;
namespace FTD_ModelCreator.Model
{
    class ModelCreator
    {
        private static readonly string modelspawnblockname = "ModelSpawner";
        private static readonly Guid spawnerguid = new Guid("f978b3f0-54f7-49ed-b43e-515ffc297233");
        private static readonly int maxdecorationperblock = 32;
        private static readonly int maxdecorationpervehicle = 5000;
        private readonly string name;
        private readonly int instanceid;
        private static Stopwatch stopwatch = new Stopwatch();
        public static readonly uint maxreducescale = 20;
        public static readonly uint minreducescale = 10;

        public bool excludeAITabBlocks = false;
        public bool excludeDefenceTabBlocks = false;
        public bool excludeAirTabBlocks = false;
        public bool excludeAPSTabBlocksExceptBarrel = false;
        public bool excludeCRAMTabBlocksExceptBarrel = false;
        public bool excludeDecoTabBlocks = false;
        public bool excludeFuelengineTabBlocks = false;
        public bool excludeLandTabBlocks = false;
        public bool excludeLaserTabBlocksExceptOptics = false;
        public bool excludePACTabBlocksExceptLens = false;
        public bool excludeControlTabBlocks = false;
        public bool excludeMiscTabBlocks = false;
        public bool excludeMissileTabBlocks = false;
        public bool excludeResourcesTabBlocks = false;
        public bool excludeSimpleWeaponTabBlocks = false;
        public bool excludeSteamengineTabBlocks = false;
        public bool excludeWaterTabBlocks = false;
        public bool excludeBlocksTabBlocksExceptStructure = false;
        public bool excludeStoneBlocks = false;
        public bool excludeHABlocks = false;
        public bool excludeMetalBlocks = false;
        public bool excludeAlloyBlocks = false;
        public bool excludeRubberBlocks = false;
        public bool excludeGlassBlocks = false;
        public bool excludeLeadBlocks = false;
        public bool excludeWoodBlocks = false;
        public bool excludeAPSBarrel = false;
        public bool excludeAPSMantlet = false;
        public bool excludeCRAMBarrel = false;
        public bool excludePACLens = false;
        public bool excludeLaserOptics = false;

        private static List<ModelCreator> instances = new List<ModelCreator>();
        //private List<KeyValuePair<KeyValuePair<Vector3i, Vector3i>, KeyValuePair<Vector3, int>>> CapturedPositions { get; set; }
        public uint ReducedScale { get; set; }
        private ModelCreator(string vehiclename, int mainconstructinstanceID)
        {
            //CapturedPositions = new List<KeyValuePair<KeyValuePair<Vector3i, Vector3i>, KeyValuePair<Vector3, int>>>();
            name = vehiclename;
            instanceid = mainconstructinstanceID;
        }
        /// <summary>
        ///     ModelCreatorのインスタンスを取得
        /// </summary>
        /// <param name="vehiclename">ビークル名</param>
        /// <param name="mainconstructuniqueID">インスタンスID(メインコンストラクト)</param>
        /// <returns></returns>
        public static ModelCreator GetModelCreator(string vehiclename, int mainconstructuniqueID)
        {
            if (instances.Exists((vehicles) => { return vehicles.name == vehiclename && vehicles.instanceid == mainconstructuniqueID; }))
            {
                return instances.Find((vehicles) => { return vehicles.name == vehiclename; });
            }
            ModelCreator newinstance = new ModelCreator(vehiclename, mainconstructuniqueID);
            instances.Add(newinstance);
            AdvLogger.LogEvent($"Created ModelCreator instance for {vehiclename} InstanceID:{mainconstructuniqueID}");
            newinstance.ReducedScale = 10;
            return newinstance;
        }

        ///// <summary>
        ///// SimplifyZoneを追加
        ///// </summary>
        ///// <param name="pos">始点と終点の座標</param>
        ///// <param name="rotationandsubindex">ビルドマーカーのローカル回転とコンストラクトのIndex</param>
        //public void AddSimplifyZone(KeyValuePair<Vector3i, Vector3i> pos, KeyValuePair<Vector3, int> rotationandsubindex)
        //{
        //    if (!CapturedPositions.Exists((cap) => { return cap.Key.Key == pos.Key && cap.Key.Value == pos.Value; }))
        //    {
        //        CapturedPositions.Add(new KeyValuePair<KeyValuePair<Vector3i, Vector3i>, KeyValuePair<Vector3, int>>(pos, rotationandsubindex));
        //    }
        //    else
        //    {
        //        return;
        //    }
        //}
        //public void RemoveSimplifyZone(KeyValuePair<Vector3i, Vector3i> pos) => CapturedPositions.RemoveAll((a) => { return a.Key.Value == pos.Value && a.Key.Key == pos.Key; });
        //public List<KeyValuePair<KeyValuePair<Vector3i, Vector3i>, KeyValuePair<Vector3, int>>> GetSimplifyZones() => CapturedPositionst;
        //1:メインコンストラクトのBlockの配列からTurretとSpinblockを取得
        //2:AllBasicsRestricted.GetSubConstructableViaLocalPositionでSubConstructsを取得
        public void Create(AllConstruct construct)
        {
            stopwatch.Restart();
            List<Block> blocks = construct.Main.AllBasics.AliveAndDead.Blocks.FindAll((a) => { return a.Name.IndexOf("invisible") == -1; });
            List<Decoration> originaldecorations = (List<Decoration>)construct.Main.Decorations.DecorationList;
            ExcludeBlocks(ref blocks);
            Block spanwner = blocks.Find((c) => { return c.Name == modelspawnblockname; });
            if (spanwner == null)
            {
                AdvLogger.LogInfo("spawner not found");
                return;
            }
            int tetherblockcount = -1;
            int zshift = -1;
            Decoration[] downsizeddecorations = new Decoration[originaldecorations.Count];
            Decoration[] downsizedblockdecorations = new Decoration[blocks.Count];
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] != null)
                {
                    Mimic mimic = blocks[i] as Mimic;
                    ShiftThetherPoint(ref tetherblockcount, ref zshift, i);
                    Vector3 thether = GetThetherPoint(tetherblockcount, zshift);
                    if (mimic != null)
                    {
                        //           AddDeco(construct, SetUpDecoration(construct.Main, spanwner.LocalPosition, mimic.Data.MeshGuid, mimic.Data.Scaling, mimic.color, blocks[i].LocalPosition + mimic.Data.Positioning.Us
                        //, mimic.Data.Orientation.Us + blocks[i].LocalRotation.eulerAngles, thether, thether));
                    }
                    else
                    {
                        AddDeco(construct, SetUpDecoration(construct.Main, spanwner.LocalPosition, blocks[i].item.ComponentId.Guid, new Vector3(1, 1, 1), blocks[i].color, blocks[i].LocalPosition,
                            blocks[i].LocalRotation.eulerAngles, thether, thether));
                    }

                }
            }
            for (int i = 0; i < downsizeddecorations.Length; i++)
            {
                Vector3 thether = GetThetherPoint(tetherblockcount, zshift);
                ShiftThetherPoint(ref tetherblockcount, ref zshift, i);
                AddDeco(construct, SetUpDecoration(construct.Main, spanwner.LocalPosition, originaldecorations[i].TetherPoint.Us, originaldecorations[i], thether, thether));
            }
            foreach (var item in construct.AllBasicsRestricted.SubConstructList)
            {
                CreationProcessinSubConstruct(construct.Main, spanwner, ref tetherblockcount, ref zshift, item);
            }
            stopwatch.Stop();
            AdvLogger.LogEvent($"Converted. Time:{stopwatch.ElapsedMilliseconds}[ms]({stopwatch.Elapsed.TotalSeconds}[s])");
        }

        private static Vector3i GetThetherPoint(int tetherblockcount, int zshift)
        {
            return Vector3i.back * (tetherblockcount - 10 * zshift) + Vector3i.down * zshift;
        }

        private void CreationProcessinSubConstruct(MainConstruct construct, Block spanwner, ref int tetherblockcount, ref int zshift, SubConstruct item)
        {
            PrecisionSpinBlock spinBlock = null;
            Turret360Precision turret = null;
            float azimuth = 0;
            float elevation = 0;
            List<Block> subblocks = item.AllBasicsRestricted.AliveAndDead.Blocks.FindAll((a) => { return a.Name.IndexOf("invisible") == -1; });
            List<Decoration> suboriginaldecorations = (List<Decoration>)item.Decorations.DecorationList;
            ExcludeBlocks(ref subblocks);
            enumSpinBlockMode mode = enumSpinBlockMode.continuous;
            for (int i = 0; i < subblocks.Count; i++)
            {
                spinBlock = subblocks[i] as PrecisionSpinBlock;
                turret = subblocks[i] as Turret360Precision;
                if (subblocks[i].LocalPosition == Vector3.zero)
                {
                    if (spinBlock != null)
                    {
                        if (spinBlock.Overlap.CurrentAzimuth <= 180)
                        {
                            azimuth = spinBlock.Overlap.CurrentAzimuth;
                            elevation = 0;
                        }
                        else
                        {
                            azimuth = spinBlock.Overlap.CurrentAzimuth - 360;
                            elevation = 0;
                        }
                        mode = spinBlock.P.Mode.Us;
                        subblocks[i] = subblocks[0];
                        subblocks[0] = spinBlock;
                    }
                    if (turret != null)
                    {
                        if (turret.overlap.CurrentAzimuth <= 180)
                        {
                            azimuth = turret.overlap.CurrentAzimuth;
                            elevation = -turret.overlap.CurrentElevation;
                        }
                        else
                        {
                            azimuth = turret.overlap.CurrentAzimuth - 360;
                            elevation = turret.overlap.CurrentElevation - 360;
                        }
                        subblocks[i] = subblocks[0];
                        subblocks[0] = turret;
                    }
                    break;
                }
            }
            AdvLogger.LogInfo(subblocks.Count + "Safeup" + item.orientation + "LocalRotation" + azimuth + "AZ " + elevation + "EL");
            for (int i = 0; i < subblocks.Count; i++)
            {
                Vector3 thether = GetThetherPoint(tetherblockcount, zshift);
                Decoration deco = SetUpDecoration(construct, spanwner.LocalPosition, subblocks[i].item.ComponentId.Guid, new Vector3(1, 1, 1), subblocks[i].color, subblocks[i].LocalPositionInMainConstruct,
                    subblocks[i].LocalRotation.eulerAngles, thether, thether);
                AdvLogger.LogInfo(item.RootLocalRotation.eulerAngles + "RootLocalRotation" + subblocks[i].LocalRotation.eulerAngles + "LocalRotation" + item.SafeLocalRotation.eulerAngles + "SafeRotation" + azimuth + "AZ " + elevation + "EL");
                RotateForAzimuthandElevation(item, azimuth, elevation, subblocks[i], deco);
                AddDeco(construct, deco);
                ShiftThetherPoint(ref tetherblockcount, ref zshift, i);
            }
            //for (int i = 0; i < suboriginaldecorations.Count; i++)
            //{
            //    Vector3 thether = GetThetherPoint(tetherblockcount, zshift);
            //    ShiftThetherPoint(ref tetherblockcount, ref zshift, i);
            //    Decoration decoration = SetUpDecorationForConvertDecoOnSub(construct.Main, item, spanwner.LocalPosition, suboriginaldecorations[i], thether, thether, azimuth);
            //    RotateForAzimuthandElevation(item, azimuth, elevation, decoration, suboriginaldecorations[i]);
            //    AddDeco(construct, decoration);
            //}
            foreach (var childsubconstruct in item.AllBasicsRestricted.SubConstructList)
            {
                CreationProcessinSubConstruct(construct, spanwner, ref tetherblockcount, ref zshift, childsubconstruct);
            }
        }

        private static void RotateForAzimuthandElevation(SubConstruct item, float azimuth, float elevation, Decoration deco, Decoration originaldeco)
        {
            Block tetherpointblock = null;
            originaldeco.GetBlock(out tetherpointblock);
            if (item.orientation == Orientations.Up)
            {
                ProcessForUpandDown(item, azimuth, elevation, deco, (uint)(tetherpointblock.LocalRotation.eulerAngles.y + originaldeco.Orientation.y), (uint)(tetherpointblock.LocalRotation.eulerAngles.x + originaldeco.Orientation.x));
            }
        }
        private static void RotateForAzimuthandElevation(SubConstruct item, float azimuth, float elevation, Block subblocks, Decoration deco)
        {
            if (item.orientation == Orientations.Up /*|| item.orientation == Orientations.Down*/)
            {
                ProcessForUpandDown(item, azimuth, elevation, deco, (uint)subblocks.LocalRotation.eulerAngles.y, (uint)subblocks.LocalRotation.eulerAngles.x);
            }
        }

        private static void ProcessForUpandDown(SubConstruct item, float azimuth, float elevation, Decoration deco, uint yaxislocalrotaion, uint xaxislocalrotation)
        {
            if (yaxislocalrotaion <= 45)
            {
                deco.Orientation.Us += new Vector3(-elevation, azimuth + item.RootLocalRotation.eulerAngles.y, 0);
            }
            else if (yaxislocalrotaion <= 135)
            {
                deco.Orientation.Us += new Vector3(-deco.Orientation.Us.x, azimuth + item.RootLocalRotation.eulerAngles.y, -elevation);
                if (xaxislocalrotation == 270)
                {
                    deco.Orientation.Us -= new Vector3(90, 0, 0);
                }
            }
            else if (yaxislocalrotaion <= 225)
            {
                deco.Orientation.Us += new Vector3(elevation, azimuth + item.RootLocalRotation.eulerAngles.y, 0);
            }
            else
            {
                deco.Orientation.Us += new Vector3(-deco.Orientation.Us.x, azimuth + item.RootLocalRotation.eulerAngles.y, elevation);
                if (xaxislocalrotation == 270)
                {
                    deco.Orientation.Us -= new Vector3(90, 0, 0);
                }
            }
            //switch (subblocks.LocalRotation.eulerAngles.y)
            //{
            //    case 0:
            //        {
            //            deco.Orientation.Us += new Vector3(-elevation, azimuth + item.RootLocalRotation.eulerAngles.y, 0);
            //        }
            //        break;
            //    case 90:
            //        {
            //            deco.Orientation.Us += new Vector3(-deco.Orientation.Us.x, azimuth + item.RootLocalRotation.eulerAngles.y, -elevation);
            //            if (subblocks.LocalRotation.eulerAngles.x == 270)
            //            {
            //                deco.Orientation.Us -= new Vector3(90, 0, 0);
            //            }
            //        }
            //        break;
            //    case 270:
            //        {
            //            deco.Orientation.Us += new Vector3(-deco.Orientation.Us.x, azimuth + item.RootLocalRotation.eulerAngles.y, elevation);
            //            if (subblocks.LocalRotation.eulerAngles.x == 270)
            //            {
            //                deco.Orientation.Us -= new Vector3(90, 0, 0);
            //            }
            //        }
            //        break;
            //    case 180:
            //        {
            //            deco.Orientation.Us += new Vector3(elevation, azimuth + item.RootLocalRotation.eulerAngles.y, 0);
            //        }
            //        break;
            //}
        }

        public int CalculateDownsizedDecorationAmount(MainConstruct construct)
        {
            int count = 0;
            List<Block> blocklist = new List<Block>();
            foreach (var item in construct.Main.AllBasicsRestricted.AliveAndDead.Blocks)
            {
                blocklist.Add(item);
            }
            ExcludeBlocks(ref blocklist);
            count += blocklist.Count + construct.Main.DecorationsRestricted.DecorationCount;
            blocklist.Clear();
            foreach (var item in construct.AllBasicsRestricted.SubConstructList)
            {
                foreach (var block in item.AllBasicsRestricted.AliveAndDead.Blocks)
                {
                    blocklist.Add(block);
                }
                ExcludeBlocks(ref blocklist);
                count += blocklist.Count + item.DecorationsRestricted.DecorationCount;
                blocklist.Clear();
            }
            return count;
        }

        public int CalculateRemainDecorationAmount(MainConstruct construct)
        {
            return maxdecorationpervehicle - CalculateDownsizedDecorationAmount(construct);
        }

        public bool CanConvert(MainConstruct construct)
        {
            if (CalculateRemainDecorationAmount(construct) >= 0)
            {
                return true;
            }
            return false;
        }
        private static void AddDeco(AllConstruct construct, Decoration deco)
        {
            Block block = null;
            deco.GetBlock(out block);
            if (block?.Name != modelspawnblockname || block == null)
            {
                MultiDecorationEditor b = construct.Main.Decorations.AddOrEditDecorations(deco);
                b.DeactivateGui();
            }
        }

        private static void ShiftThetherPoint(ref int tetherblockcount, ref int yshift, int i)
        {
            if ((i % maxdecorationperblock == 0))
            {
                tetherblockcount++;
                if (tetherblockcount % 10 == 0)
                {
                    yshift++;
                }
            }
        }


        private Decoration SetUpDecoration(MainConstruct construct, Vector3 tetherpos, Guid meshguid, Vector3 scale, int color, Vector3 localpos, Vector3 localrotation, Vector3 thethershift, Vector3 posshift)
        {
            Decoration decoration = construct.Decorations.NewDecoration((Vector3i)(tetherpos + thethershift), true);
            decoration.MeshGuid.Us = meshguid;
            decoration.Color.Us = color;
            decoration.Positioning.Us = OriginalVector3ToModelVector3(localpos - posshift * ReducedScale);
            decoration.Scaling.Us = OriginalVector3ToModelVector3(scale);
            decoration.Orientation.Us = localrotation;
            decoration.CallbackToChangeSyncroniser.WeHaveChanged(decoration);
            return decoration;
        }
        //TODO:Atan(z/x)で角の大きさを求める
        //加法定理を用いて+X[deg]した時の位置を求める
        private Decoration SetUpDecoration(MainConstruct construct, Vector3 tetherpos, Vector3 originallocalpos, Decoration deco, Vector3 thethershift, Vector3 posshift)
        {
            Decoration decoration = construct.Decorations.NewDecoration((Vector3i)(tetherpos + thethershift), true);
            decoration.MeshGuid.Us = deco.MeshGuid;
            decoration.Color.Us = deco.Color;
            decoration.Scaling.Us = OriginalVector3ToModelVector3(deco.Scaling);
            decoration.Orientation.Us = deco.Orientation.Us;
            decoration.Positioning.Us = OriginalVector3ToModelVector3(originallocalpos + deco.Positioning.Us - posshift * ReducedScale);
            decoration.CallbackToChangeSyncroniser.WeHaveChanged(decoration);
            return decoration;
        }
        private Decoration SetUpDecorationForConvertDecoOnSub(MainConstruct construct, SubConstruct sub, Vector3 tetherpos, Decoration deco, Vector3 thethershift, Vector3 posshift, float azimuth)
        {
            Block block = null;
            deco.GetBlock(out block);
            //float azimuthrad = azimuth * Mathf.Deg2Rad;
            Decoration decoration = construct.Decorations.NewDecoration((Vector3i)(tetherpos + thethershift), true);
            //float z = decoration.Positioning.Us.z + block.LocalPosition.z;
            //float x = decoration.Positioning.Us.x + block.LocalPosition.x;
            //float decorad = Mathf.Atan2(z, x);
            //float radius = Mathf.Sign(Mathf.Pow(z, 2) + Mathf.Pow(x, 2));
            decoration.MeshGuid.Us = deco.MeshGuid;
            decoration.Color.Us = deco.Color;
            decoration.Scaling.Us = OriginalVector3ToModelVector3(deco.Scaling);
            decoration.Orientation.Us = deco.Orientation.Us;

            //Vector3 rotatedpos = new Vector3(radius * Mathf.Sin(decorad + azimuthrad), decoration.Positioning.y,
            //    radius * Mathf.Cos(decorad + azimuthrad));
            decoration.Positioning.Us = OriginalVector3ToModelVector3(sub.RootLocalPosition + deco.TetherPoint.Us + deco.Positioning.Us - posshift * ReducedScale);

            decoration.CallbackToChangeSyncroniser.WeHaveChanged(decoration);
            return decoration;
        }

        private void ExcludeBlocks(ref List<Block> blocks)
        {
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.AI, excludeAITabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Air, excludeAirTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.APSBarrel, excludeAPSBarrel);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.APSExceptBarrel, excludeAPSTabBlocksExceptBarrel, GenreEnum.APSBarrel);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.BlockExceptStructure, excludeBlocksTabBlocksExceptStructure);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Stone, excludeStoneBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.HA, excludeHABlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Glass, excludeGlassBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Metal, excludeMetalBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Alloy, excludeAlloyBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Wood, excludeWoodBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Rubber, excludeRubberBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Control, excludeControlTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.CRAMBarrel, excludeCRAMBarrel);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.CRAMExceptBarrel, excludeCRAMTabBlocksExceptBarrel, GenreEnum.CRAMBarrel);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Deco, excludeDecoTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Defence, excludeDefenceTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.FuelEngine, excludeFuelengineTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Land, excludeLandTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.LaserExceptOptics, excludeLaserTabBlocksExceptOptics, GenreEnum.LaserOptics);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.LaserOptics, excludeLaserOptics);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Misc, excludeMiscTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.PACExceptLens, excludePACTabBlocksExceptLens, GenreEnum.PACLens);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.PACLens, excludePACLens);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Missile, excludeMissileTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.SteamEngine, excludeSteamengineTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.SimpleWeapon, excludeSimpleWeaponTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Resources, excludeResourcesTabBlocks);
            ExcludeBlocksbyGenreEnum(ref blocks, GenreEnum.Water, excludeWaterTabBlocks);
        }
        /// <summary>
        /// List<Block>からGenreEnumで指定されたブロックを削除
        /// </summary>
        /// <param name="original">削除するList<Block></param>
        /// <param name="blockgenre">削除するGUIDのGenreEnum</param>
        /// <param name="excludeflag"></param>
        /// <param name="includefromgenre">blockgenreから除外するGUIDのGenreEnum</param>
        private void ExcludeBlocksbyGenreEnum(ref List<Block> original, GenreEnum blockgenre, bool excludeflag = true, GenreEnum includefromgenre = GenreEnum.None)
        {
            if (excludeflag)
            {
                FieldInfo[] info = typeof(InventoryTabsandBlocksGuid).GetFields();
                List<FieldInfo> propertylist = info.ToList().FindAll((a) => { return a.Name.IndexOf(blockgenre.ToString()) != -1; });
                List<FieldInfo> includefrompropertylist = info.ToList().FindAll((a) => { return a.Name.IndexOf(includefromgenre.ToString()) != -1; });
                List<Guid> guidlist = new List<Guid>();
                List<Guid> includefromguidlist = new List<Guid>();
                guidlist = propertylist.ConvertAll<Guid>((a) => { return (Guid)a.GetValue(null); });
                List<int> excludeindex = new List<int>();
                if (includefromgenre != GenreEnum.None)
                {
                    includefromguidlist = includefrompropertylist.ConvertAll<Guid>((a) => { return (Guid)a.GetValue(null); });
                }
                for (int i = 0; i < original.Count; i++)
                {
                    bool isexclude = guidlist.Contains(original[i].item.InventoryTabOrVariantId.Reference.Guid) || guidlist.Contains(original[i].item.ComponentId.Guid) && original[i].Name != modelspawnblockname;
                    if (isexclude && !includefromguidlist.Contains(original[i].item.ComponentId.Guid))
                    {
                        excludeindex.Add(i);
                    }
                }
                for (int i = 0; i < excludeindex.Count; i++)
                {
                    RemoveFromArray(ref original, excludeindex[i] - i);
                }
            }
        }
        private void RemoveFromArray(ref List<Block> array, int index)
        {
            for (int i = index; i < array.Count; i++)
            {
                if (array.Count - 1 != i)
                {
                    array[i] = array[i + 1];
                }
                else
                {
                    array.RemoveAt(i);
                    array.Capacity--;
                }
            }
        }

        //public void ProcessforSimplifyZones(MainConstruct construct)
        //{
        //    Block simplifywith = new Block();
        //    Vector3 simplifydim = Vector3i.one;
        //    Vector3 zonedim = Vector3i.one;
        //    Vector3 scaledim = Vector3.one;
        //    //foreach (var item in CapturedPositions)
        //    //{//Value==-1(メインコンストラクト) それ以外はサブコンストラクト
        //    var item = CapturedPositions.Last();
        //    if (item.Value.Value == -1)
        //    {
        //        simplifywith = construct.Main.AllBasics.GetBlockViaLocalPosition(item.Key.Key);
        //    }
        //    else
        //    {
        //        simplifywith = construct.AllSubConstructsPerIndex[item.Value.Value - 1].AllBasics.GetBlockViaLocalPosition(item.Key.Key);
        //    }

        //    simplifydim = RotateVector3(simplifywith.item.SizeInfo.Dimensions, simplifywith.LocalRotation.eulerAngles);
        //    zonedim = new Vector3(Math.Abs(item.Key.Key.x - item.Key.Value.x) + 1, Math.Abs(item.Key.Key.y - item.Key.Value.y) + 1, Math.Abs(item.Key.Key.z - item.Key.Value.z) + 1);
        //    //zonedim = RotateVector3(zonedim, simplifywith.LocalRotation.eulerAngles);
        //    scaledim = RotateVector3(new Vector3(zonedim.x / simplifydim.x, zonedim.y / simplifydim.y, zonedim.z / simplifydim.z), simplifywith.LocalRotation.eulerAngles);
        //    AdvLogger.LogInfo($"simplifywithRotation:{simplifywith.LocalRotation.eulerAngles} simplifydim:{simplifydim} zonedim:{zonedim} scaledim:{scaledim}");
        //    Decoration decoration = new Decoration();
        //    if (item.Value.Value == -1)
        //    {
        //        decoration = PlaceNewDecoration(construct, simplifywith, scaledim, item);
        //    }
        //    else
        //    {
        //        decoration = construct.AllSubConstructsPerIndex[item.Value.Value - 1].Decorations.NewDecoration(item.Key.Key, true);
        //        decoration.Scaling.Us = scaledim;
        //        decoration.Color.Us = simplifywith.color;
        //        decoration.MeshGuid.Us = simplifywith.item.ComponentId.Guid;
        //        decoration.Positioning.Us = new Vector3((scaledim.x - 1) * 0.5f, (scaledim.y - 1) * 0.5f, (scaledim.z - 1) * 0.5f);
        //        construct.AllSubConstructsPerIndex[item.Value.Value - 1].Decorations.AddOrEditDecorations(decoration);
        //    }
        //    //}
        //    //Block simplifywith = new Block();
        //    //Vector3i simplifydim = Vector3i.one;
        //    //Vector3i zonedim = Vector3i.one;
        //    //Vector3 scaledim = Vector3.one;
        //    //foreach (var item in CapturedPositions)
        //    //{
        //    //    simplifydim = simplifywith.item.SizeInfo.Dimensions;
        //    //    zonedim = new Vector3i(Math.Abs(item.Key.Key.x - item.Key.Value.x) + 1, Math.Abs(item.Key.Key.y - item.Key.Value.y) + 1, Math.Abs(item.Key.Key.z - item.Key.Value.z) + 1);
        //    //    scaledim = new Vector3(zonedim.x / simplifydim.x, zonedim.y / simplifydim.y, zonedim.z / simplifydim.z);
        //    //    Decoration decoration = new Decoration();
        //    //    if (item.Value == -1)
        //    //    {
        //    //        simplifywith = construct.AllBasics.GetBlockViaLocalPosition(item.Key.Key);
        //    //        decoration = PlaceNewDecoration(construct, simplifywith, scaledim, item);
        //    //    }
        //    //    else
        //    //    {
        //    //        //simplifywith = construct.AllSubConstructsPerIndex[item.Value];
        //    //        //SubConstruct sub = GetBranchSubConstruct(construct, item.Value);
        //    //        //simplifywith = sub.AllBasics.GetBlockViaLocalPosition(item.Key.Key);
        //    //        //decoration = PlaceNewDecoration(sub, simplifywith, scaledim, item);
        //    //    }
        //    //}
        //}

        //private Vector3 RotateVector3(Vector3 original, Vector3 rotation)
        //{
        //    // Yaw=Y Pitch=X Roll=Z(FTD内)
        //    //メモ：軸を同時に動かすと失敗
        //    float newxrot = original.x;
        //    float newyrot = original.y;
        //    float newzrot = original.z;

        //    if (rotation.z % 90 == 0 && rotation.z % 180 != 0)
        //    {
        //        //SwapFloats(ref newzrot, ref newxrot);
        //        SwapFloats(ref newyrot, ref newxrot);
        //        return new Vector3(newxrot, newyrot, newzrot);
        //    }
        //    if (rotation.y % 90 == 0 && rotation.y % 180 != 0)
        //    {
        //        //SwapFloats(ref newyrot, ref newzrot);
        //        SwapFloats(ref newxrot, ref newzrot);
        //        return new Vector3(newxrot, newyrot, newzrot);
        //    }
        //    if (rotation.x % 90 == 0 && rotation.x % 180 != 0)
        //    {
        //        //SwapFloats(ref newxrot, ref newyrot);
        //        SwapFloats(ref newzrot, ref newyrot);
        //        return new Vector3(newxrot, newyrot, newzrot);
        //    }           
        //    return Vector3.zero;
        //}
        //private void SwapFloats(ref float f1, ref float f2)
        //{
        //    float f1backup = f1;
        //    f1 = f2;
        //    f2 = f1backup;
        //}    

        //private Decoration PlaceNewDecoration(AllConstruct construct, Block simplifywith, Vector3 scaledim, KeyValuePair<KeyValuePair<Vector3i, Vector3i>, KeyValuePair<Vector3, int>> item)
        //{
        //    Decoration decoration = construct.Decorations.NewDecoration(item.Key.Key, true);
        //    decoration.Scaling.Us = scaledim;
        //    decoration.MeshGuid.Us = simplifywith.item.ComponentId.Guid;
        //    decoration.Color.Us = simplifywith.color;
        //    //decoration.Positioning.Us = new Vector3((scaledim.x - 1) * 0.5f, (scaledim.y - 1) * 0.5f, (scaledim.z - 1) * 0.5f);
        //    decoration.Orientation.Us = RotateVector3(simplifywith.LocalRotation.eulerAngles, simplifywith.LocalRotation.eulerAngles);
        //    decoration.Positioning.Us = RotateVector3(new Vector3((scaledim.x - 1) * 0.5f, (scaledim.y - 1) * 0.5f, (scaledim.z - 1) * 0.5f), simplifywith.LocalRotation.eulerAngles);
        //    construct.Decorations.AddOrEditDecorations(decoration);
        //    return decoration;
        //}

        private Vector3 OriginalVector3ToModelVector3(Vector3 original)
        {
            return new Vector3(original.x / ReducedScale, original.y / ReducedScale, original.z / ReducedScale);
        }

        //private AllConstruct SimplifyConstructs(AllConstruct construct)
        //{
        //    foreach (var item in GetSimplifyZones())
        //    {
        //        if (item.Value[0] > -1)
        //        {
        //            ExcludeBlocksFromBP, item.Key.Key, item.Key.Value);
        //        }
        //        else
        //        {
        //            ExcludeBlocksFromBP(construct,item.Key.Key, item.Key.Value);
        //        }
        //    }
        //}
        //private AllConstruct ExcludeBlocksFromBP(AllConstruct excludefrom, Vector3i start, Vector3i end)
        //{

        //}
    }
}