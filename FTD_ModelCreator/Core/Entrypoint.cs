using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using BrilliantSkies.Modding;
using System.Reflection;
using System.IO;

namespace FTD_ModelCreator.Core
{
    class Entrypoint : GamePlugin_PostLoad
    {
        public Entrypoint()
        {
            //Assembly.LoadFrom(Path.Combine(Assembly.GetExecutingAssembly().Location, "0Harmony.dll"));
        }

        public string name => "Punyo_ModelCreator";

        public Version version { get; }

        public bool AfterAllPluginsLoaded()
        {
            return true;
        }

        public void OnLoad()
        {
            Harmony h = new Harmony("com.punyo.modelcreator");
            h.PatchAll();
        }

        public void OnSave()
        {
        }
    }
}
