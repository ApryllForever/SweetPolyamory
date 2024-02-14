using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SweetPolyamory
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        internal static IMonitor ModMonitor { get; set; }

        internal static IModHelper ModHelper { get; set; }

        public override void Entry(IModHelper helper)
        {

            instance = this;






            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.PatchAll();


        }




        }
    }
