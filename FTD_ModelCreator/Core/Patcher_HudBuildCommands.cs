using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.Ftd.Avatar.HUD;
using BrilliantSkies.Ui.Elements;
using BrilliantSkies.Ui.Tips;
using FTD_ModelCreator.Model;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

namespace FTD_ModelCreator.Core
{
    [HarmonyPatch(typeof(HudBuildCommands), "DisplayPrefabOptions")]
    class Patcher_HudBuildCommands
    {
        public static readonly UiDef CaptureSimpilfyZone = new UiDef("captureSimplifyZone", new Guid("5296eae2-fe30-44b9-8dba-712ce96d5303"), new ToolTip
            ("Capture Simplify Zone"));
        public static void Postfix(SavedSubObject prefab, bool displayCapture)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            cBuild build = cBuild.GetSingleton();
            if (displayCapture && !prefab.IsValid)
            {
                InputType input = CaptureSimpilfyZone.DisplayButton();
                if (input > InputType.None)
                {
                    ModelCreator creator = ModelCreator.GetModelCreator(build.C.Main.GetName(), build.C.Main.myTransform.gameObject.GetInstanceID());
                    KeyValuePair<Vector3i, Vector3i> capturedpos = build.GetPrefabCaptureBox();
                    //List<int> indexes = new List<int>();
                    //List<string> indexesstring = new List<string>();
                    //string indexlog = string.Empty;
                    Block selectedblock = build.C.AllBasics.GetClosestBlockToGlobalPoint(build.buildMarker.transform.position);
                    SubConstruct sub = selectedblock.GetConstructableOrSubConstructable() as SubConstruct;
                    MainConstruct main = selectedblock.GetConstructableOrSubConstructable() as MainConstruct;
                    //if (main == null)
                    //{
                    //    //bool ismain = sub.Parent as MainConstruct != null;
                    //    indexes.Add();
                    //    build.GetCC()
                    //    SubConstruct sub2 = sub.Parent as SubConstruct;
                    //    if (sub2 == null)
                    //    {
                    //        indexes.Add(sub2.PersistentSubConstructIndex);
                    //    }
                    //    else
                    //    {

                    //    }
                    //    while (true)
                    //    {
                    //        SubConstruct sub2 = sub.Parent as MainConstruct != null;
                    //    }
                    //}
                    //else
                    //{
                    //    indexes.Add(-1);
                    //}
                    //indexes.Add(build.C.PersistentSubConstructIndex);
                    //indexesstring.AddRange(indexes.ConvertAll<string>((index) => { return index.ToString(); }));
                    //foreach (var item in indexesstring)
                    //{
                    //    indexlog += item;
                    //}
                    int constructsindex = -1;
                    Vector3 globalpos = Vector3.zero;
                    Vector3 buildmarkerrotation = build.GetBuildMarkerLocalRotation().eulerAngles;
                    if (main == null)
                    {
                        constructsindex = sub.PersistentSubConstructIndex;
                        globalpos = sub.AllBasics.GetBlockViaLocalPosition(capturedpos.Key).GameWorldPosition;
                    }
                    else
                    {
                        globalpos = build.C.Main.AllBasics.GetBlockViaLocalPosition(capturedpos.Key).GameWorldPosition;
                    }
                    //creator.AddSimplifyZone(capturedpos, new KeyValuePair<Vector3, int>(buildmarkerrotation,constructsindex));
                    AdvLogger.LogEvent($"Captured Simplify Zone for {build.C.Main.GetName()}. Start:{capturedpos.Key.ToString()} End:{capturedpos.Value.ToString()} Index:{constructsindex} " +
                        $"Rotation:{buildmarkerrotation} StartBlockDimension:{build.C.Main.AllBasics.GetBlockViaLocalPosition(capturedpos.Key).item.SizeInfo.Dimensions}" +
                        $" GlobalPos:{globalpos}");              
                    //creator.ProcessforSimplifyZones(build.C.Main);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
   
    }
}
