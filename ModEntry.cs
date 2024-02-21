using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley.Pathfinding;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.GameData.Shops;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using xTile.Dimensions;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using StardewModdingAPI.Events;
using StardewValley.BellsAndWhistles;



namespace SweetPolyamory
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        internal static IMonitor ModMonitor { get; set; }

        internal static IModHelper ModHelper { get; set; }
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static Mod context;
        public static Multiplayer mp;
        public static Random myRand;
        public static string farmHelperSpouse = null;
        internal static NPC tempOfficialSpouse;
        public static int bedSleepOffset = 76;

        public static string spouseToDivorce = null;
        public static int divorceHeartsLost;

        public static Dictionary<long, Dictionary<string, NPC>> currentSpouses = new Dictionary<long, Dictionary<string, NPC>>();
        public static Dictionary<long, Dictionary<string, NPC>> currentUnofficialSpouses = new Dictionary<long, Dictionary<string, NPC>>();



        public override void Entry(IModHelper helper)
        {

            instance = this;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.PatchAll();


           

        /*
            harmony.Patch(
           original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(bool) }),
           prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
        );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(PathFindController.endBehavior) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(PathFindController.endBehavior), typeof(int) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
        */


            Config = Helper.ReadConfig<ModConfig>();
            context = this;
            if (!Config.EnableMod)
                return;

            ModMonitor = Monitor;
            SHelper = helper;

            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            myRand = new Random();

            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            helper.Events.Content.AssetRequested += Content_AssetRequested;

            PathFindControllerPatches.Initialize(Monitor, helper);
            Divorce.Initialize(Monitor, Config, helper);
            NPCPatches.Initialize(Monitor, Config, helper);
            Game1Patches.Initialize(Monitor);
            LocationPatches.Initialize(Monitor, Config, helper);
            FarmerPatches.Initialize(Monitor, Config, helper);
            UIPatches.Initialize(Monitor, Config, helper);
            EventPatches.Initialize(Monitor, Config, helper);

            


            // npc patches

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_marriageDuties_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.getSpouse)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_getSpouse_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isRoommate)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isRoommate_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarried)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarried_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarriedOrEngaged)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarriedOrEngaged_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Prefix)),
               transpiler: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "engagementResponse"),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.spouseObstacleCheck)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_spouseObstacleCheck_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.setUpForOutdoorPatioActivity)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_setUpForOutdoorPatioActivity_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.playSleepingAnimation)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_playSleepingAnimation_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_playSleepingAnimation_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.GetDispositionModifiedString)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_GetDispositionModifiedString_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_GetDispositionModifiedString_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "loadCurrentDialogue"),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_loadCurrentDialogue_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_loadCurrentDialogue_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToRetrieveDialogue)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToRetrieveDialogue_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_checkAction_Prefix))
            );


            // Child patches

            harmony.Patch(
               original: typeof(Character).GetProperty("displayName").GetMethod,
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Character_displayName_Getter_Postfix))
            );

            // Path patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(PathFindController.endBehavior) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(PathFindController.endBehavior), typeof(int) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );


            // Location patches

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.GetSpouseBed)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_GetSpouseBed_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.getSpouseBedSpot)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_getSpouseBedSpot_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Beach), nameof(Beach.checkAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Beach), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_resetLocalState_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "checkEventPrecondition", new Type[] { typeof(string), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_checkEventPrecondition_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(ManorHouse), nameof(ManorHouse.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(Location) }),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.ManorHouse_performAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(ManorHouse), nameof(ManorHouse.answerDialogueAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.ManorHouse_answerDialogueAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_answerDialogueAction_Prefix))
            );


            // pregnancy patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.pickPersonalFarmEvent)),
               prefix: new HarmonyMethod(typeof(Mod), nameof(Mod.Utility_pickPersonalFarmEvent_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(QuestionEvent), nameof(QuestionEvent.setUp)),
               prefix: new HarmonyMethod(typeof(Mod), nameof(Mod.QuestionEvent_setUp_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.setUp)),
               prefix: new HarmonyMethod(typeof(Mod), nameof(Mod.BirthingEvent_setUp_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.tickUpdate)),
               prefix: new HarmonyMethod(typeof(Mod), nameof(Mod.BirthingEvent_tickUpdate_Prefix))
            );


            // Farmer patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doDivorce)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_doDivorce_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.isMarriedOrRoommates)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_isMarried_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getSpouse)),
               postfix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_getSpouse_Postfix))
            );
            harmony.Patch(
               original: AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.spouse)),
               postfix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_spouse_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetSpouseFriendship)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_GetSpouseFriendship_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.checkAction)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getChildren)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_getChildren_Prefix))
            );


            // UI patches

            harmony.Patch(
               original: AccessTools.Method(typeof(SocialPage), "drawNPCSlot"),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawNPCSlot_prefix)),
               transpiler: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawSlot_transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(SocialPage), "drawFarmerSlot"),
               transpiler: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawSlot_transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(SocialPage.SocialEntry), nameof(SocialPage.SocialEntry.IsMarriedToAnyone)),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_isMarriedToAnyone_Prefix))
            );

            harmony.Patch(
               original: typeof(DialogueBox).GetConstructor(new Type[] { typeof(List<string>) }),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.DialogueBox_Prefix))
            );


            // Event patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.answerDialogueQuestion)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_answerDialogueQuestion_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.LoadActors)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_loadActors_Prefix)),
               postfix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_loadActors_Postfix))
            );


            // Game1 patches

            harmony.Patch(
               original: AccessTools.GetDeclaredMethods(typeof(Game1)).Where(m => m.Name == "getCharacterFromName" && m.ReturnType == typeof(NPC)).First(),
               prefix: new HarmonyMethod(typeof(Game1Patches), nameof(Game1Patches.getCharacterFromName_Prefix))
            );





















        }




   















        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, ShopData>();
                    try
                    {
                        for (int i = 0; i < dict.Data["DesertTrade"].Items.Count; i++)
                        {
                            if (dict.Data["DesertTrade"].Items[i].ItemId == "(O)808")
                                dict.Data["DesertTrade"].Items[i].Condition = "PLAYER_FARMHOUSE_UPGRADE Current 1, !PLAYER_HAS_ITEM Current 808";
                        }
                    }
                    catch
                    {

                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/HaleyHouse"))
            {
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;

                    string key = "195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019";
                    if (data.TryGetValue(key, out string value))
                    {
                        data[key] = Regex.Replace(value, "(pause 1000/speak Maru \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 3", "");
                        //data["91740942"] = data["195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019"];
                    }
                    key = "195012/f Olivia 2500/f Sophia 2500/f Claire 2500/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019";
                    if (data.TryGetValue(key, out value))
                    {
                        data[key] = Regex.Replace(value, "(pause 1000/speak Maru \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 3", "");
                        //data["91740942"] = data["195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019"];
                    }

                    if (data.TryGetValue("choseToExplain", out value))
                    {
                        data["choseToExplain"] = Regex.Replace(value, "(pause 1000/speak Maru \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 4", "");
                    }
                    if (data.TryGetValue("lifestyleChoice", out value))
                    {
                        data["lifestyleChoice"] = Regex.Replace(value, "(pause 1000/speak Maru \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 4", "");
                    }

                });

            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Saloon"))
            {
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                    string key = "195013/f Shane 2500/f Sebastian 2500/f Sam 2500/f Harvey 2500/f Alex 2500/f Elliott 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 911526/e 528052/e 9581348/e 43/e 384882/e 233104/k 195099";
                    if (!data.TryGetValue(key, out string value))
                    {
                        Monitor.Log("Missing event key for Saloon!");
                        return;
                    }
                    data[key] = Regex.Replace(value, "(pause 1000/speak Sam \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 3", "");
                    //data["19501342"] = Regex.Replace(aData, "(pause 1000/speak Sam \\\")[^$]+.a\\\"",$"$1{SHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 3", "");
                    if (data.TryGetValue("choseToExplain", out value))
                    {
                        data["choseToExplain"] = Regex.Replace(value, "(pause 1000/speak Sam \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 4", "");
                    }
                    if (data.TryGetValue("crying", out value))
                    {
                        data["crying"] = Regex.Replace(value, "(pause 1000/speak Sam \\\")[^$]+.a\\\"", $"$1{SHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 4", "");
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/StringsFromCSFiles"))
            {
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                    data["NPC.cs.3985"] = Regex.Replace(data["NPC.cs.3985"], @"\.\.\.\$s.+", $"$n#$b#$c 0.5#{data["ResourceCollectionQuest.cs.13681"]}#{data["ResourceCollectionQuest.cs.13683"]}");
                    Monitor.Log($"NPC.cs.3985 is set to \"{data["NPC.cs.3985"]}\"");
                });

            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/animationDescriptions"))
            {
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                    List<string> sleepKeys = data.Keys.ToList().FindAll((s) => s.EndsWith("_Sleep"));
                    foreach (string key in sleepKeys)
                    {
                        if (!data.ContainsKey(key.ToLower()))
                        {
                            Monitor.Log($"adding {key.ToLower()} to animationDescriptions");
                            data.Add(key.ToLower(), data[key]);
                        }
                    }
                });

            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/EngagementDialogue"))
            {
                if (!Config.RomanceAllVillagers)
                    return;
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                    Farmer f = Game1.player;
                    if (f == null)
                    {
                        return;
                    }
                    foreach (string friend in f.friendshipData.Keys)
                    {
                        if (!data.ContainsKey(friend + "0"))
                        {
                            data[friend + "0"] = "";
                        }
                        if (!data.ContainsKey(friend + "1"))
                        {
                            data[friend + "1"] = "";
                        }
                    }
                });
            }
            else if (Config.RomanceAllVillagers && (e.NameWithoutLocale.BaseName.StartsWith("Characters/schedules/") || e.NameWithoutLocale.BaseName.StartsWith("Characters\\schedules\\")))
            {
                try
                {
                    string name = e.NameWithoutLocale.BaseName.Replace("Characters/schedules/", "").Replace("Characters\\schedules\\", "");
                    NPC npc = Game1.getCharacterFromName(name);
                    if (npc != null && npc.Age < 2 && !(npc is Child))
                    {

                        if (Game1.characterData[npc.Name].CanBeRomanced)
                        {
                            Monitor.Log($"can edit schedule for {name}");
                            e.Edit(delegate (IAssetData idata)
                            {
                                IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                                List<string> keys = new List<string>(data.Keys);
                                foreach (string key in keys)
                                {
                                    if (!data.ContainsKey($"marriage_{key}"))
                                        data[$"marriage_{key}"] = data[key];
                                }
                            });
                        }
                    }
                }
                catch
                {
                }


            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/Locations"))
            {
                e.Edit(delegate (IAssetData idata)
                {
                    IDictionary<string, string> data = idata.AsDictionary<string, string>().Data;
                    data["Beach_Mariner_PlayerBuyItem_AnswerYes"] = data["Beach_Mariner_PlayerBuyItem_AnswerYes"].Replace("5000", Config.PendantPrice + "");
                });
            }
        }



        //  From HelpfulEvents.cs



        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Points To Marry",
                getValue: () => Config.MinPointsToMarry,
                setValue: value => Config.MinPointsToMarry = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Points To Date",
                getValue: () => Config.MinPointsToDate,
                setValue: value => Config.MinPointsToDate = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Prevent Hostile Divorces",
                getValue: () => Config.PreventHostileDivorces,
                setValue: value => Config.PreventHostileDivorces = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Complex Divorces",
                getValue: () => Config.ComplexDivorce,
                setValue: value => Config.ComplexDivorce = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Roommate Romance",
                getValue: () => Config.RoommateRomance,
                setValue: value => Config.RoommateRomance = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max children",
                getValue: () => Config.MaxChildren,
                setValue: value => Config.MaxChildren = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Parent Names",
                getValue: () => Config.ShowParentNames,
                setValue: value => Config.ShowParentNames = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Buy Pendants Anytime",
                getValue: () => Config.BuyPendantsAnytime,
                setValue: value => Config.BuyPendantsAnytime = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Pendant Price",
                getValue: () => Config.PendantPrice,
                setValue: value => Config.PendantPrice = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Percent Chance For In Bed",
                getValue: () => Config.PercentChanceForSpouseInBed,
                setValue: value => Config.PercentChanceForSpouseInBed = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Chance For In Kitchen",
                getValue: () => Config.PercentChanceForSpouseInKitchen,
                setValue: value => Config.PercentChanceForSpouseInKitchen = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Chance For In Patio",
                getValue: () => Config.PercentChanceForSpouseAtPatio,
                setValue: value => Config.PercentChanceForSpouseAtPatio = value,
                min: 0,
                max: 100
            );

            //LoadModApis(); loads Integrations.cs, which is not needed
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            currentSpouses.Clear();
            currentUnofficialSpouses.Clear();
        }
        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            SetAllNPCsDatable();
            ResetSpouses(Game1.player);
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            ResetDivorces();
            ResetSpouses(Game1.player);


            foreach (GameLocation location in Game1.locations)
            {
                if (ReferenceEquals(location.GetType(), typeof(FarmHouse)))
                {
                    PlaceSpousesInFarmhouse(location as FarmHouse);
                }
            }
            if (Game1.IsMasterGame)
            {
                Game1.getFarm().addSpouseOutdoorArea(Game1.player.spouse == null ? "" : Game1.player.spouse);
                farmHelperSpouse = GetRandomSpouse(Game1.MasterPlayer);
            }
            foreach (Farmer f in Game1.getAllFarmers())
            {
                var spouses = GetSpouses(f, true).Keys;
                foreach (string s in spouses)
                {
                    ModMonitor.Log($"{f.Name} is married to {s}");
                }
            }
        }


        public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            foreach (GameLocation location in Game1.locations)
            {

                if (location is FarmHouse)
                {
                    FarmHouse fh = location as FarmHouse;
                    if (fh.owner == null)
                        continue;

                    List<string> allSpouses = GetSpouses(fh.owner, true).Keys.ToList();
                    List<string> bedSpouses = ReorderSpousesForSleeping(allSpouses.FindAll((s) => Config.RoommateRomance || !fh.owner.friendshipData[s].RoommateMarriage));

                    using (IEnumerator<NPC> characters = fh.characters.GetEnumerator())
                    {
                        while (characters.MoveNext())
                        {
                            var character = characters.Current;
                            if (!(character.currentLocation == fh))
                            {
                                character.farmerPassesThrough = false;
                                character.HideShadow = false;
                                character.isSleeping.Value = false;
                                continue;
                            }

                            if (allSpouses.Contains(character.Name))
                            {

                                if (IsInBed(fh, character.GetBoundingBox()))
                                {
                                    character.farmerPassesThrough = true;

                                    if (!character.isMoving() )
                                    {
                                        Vector2 bedPos = GetSpouseBedPosition(fh, character.Name);
                                        if (Game1.timeOfDay >= 2000 || Game1.timeOfDay <= 600)
                                        {
                                            character.position.Value = bedPos;

                                            if (Game1.timeOfDay >= 2200)
                                            {
                                                character.ignoreScheduleToday = true;
                                            }
                                            if (!character.isSleeping.Value)
                                            {
                                                character.isSleeping.Value = true;

                                            }
                                            if (character.Sprite.CurrentAnimation == null)
                                            {
                                                if (!HasSleepingAnimation(character.Name))
                                                {
                                                    character.Sprite.StopAnimation();
                                                    character.faceDirection(0);
                                                }
                                                else
                                                {
                                                    character.playSleepingAnimation();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            character.faceDirection(3);
                                            character.isSleeping.Value = false;
                                        }
                                    }
                                    else
                                    {
                                        character.isSleeping.Value = false;
                                    }
                                    character.HideShadow = true;
                                }
                                else if (Game1.timeOfDay < 2000 && Game1.timeOfDay > 600)
                                {
                                    character.farmerPassesThrough = false;
                                    character.HideShadow = false;
                                    character.isSleeping.Value = false;
                                }
                            }
                        }
                    }
                }
            }
        }


















        //Free Love Code - Misc.CS Class

        private static Dictionary<string, int> topOfHeadOffsets = new Dictionary<string, int>();

        public static void ReloadSpouses(Farmer farmer)
        {
            currentSpouses[farmer.UniqueMultiplayerID] = new Dictionary<string, NPC>();
            currentUnofficialSpouses[farmer.UniqueMultiplayerID] = new Dictionary<string, NPC>();
            string ospouse = farmer.spouse;
            if (ospouse != null)
            {
                var npc = Game1.getCharacterFromName(ospouse);
                if (npc is not null)
                {
                    currentSpouses[farmer.UniqueMultiplayerID][ospouse] = npc;
                }
            }
            ModMonitor.Log($"Checking for extra spouses in {farmer.friendshipData.Count()} friends");
            foreach (string friend in farmer.friendshipData.Keys)
            {
                if (farmer.friendshipData[friend].IsMarried() && friend != farmer.spouse)
                {
                    var npc = Game1.getCharacterFromName(friend, true);
                    if (npc != null)
                    {
                        currentSpouses[farmer.UniqueMultiplayerID][friend] = npc;
                        currentUnofficialSpouses[farmer.UniqueMultiplayerID][friend] = npc;
                    }
                }
            }
            if (farmer.spouse is null && currentSpouses[farmer.UniqueMultiplayerID].Any())
                farmer.spouse = currentSpouses[farmer.UniqueMultiplayerID].First().Key;
            ModMonitor.Log($"reloaded {currentSpouses[farmer.UniqueMultiplayerID].Count} spouses for {farmer.Name} {farmer.UniqueMultiplayerID}");
        }
        public static Dictionary<string, NPC> GetSpouses(Farmer farmer, bool all)
        {
            if (!currentSpouses.ContainsKey(farmer.UniqueMultiplayerID) || ((currentSpouses[farmer.UniqueMultiplayerID].Count == 0 && farmer.spouse != null)))
            {
                ReloadSpouses(farmer);
            }
            if (farmer.spouse == null && currentSpouses[farmer.UniqueMultiplayerID].Count > 0)
            {
                farmer.spouse = currentSpouses[farmer.UniqueMultiplayerID].First().Key;
            }
            return all ? currentSpouses[farmer.UniqueMultiplayerID] : currentUnofficialSpouses[farmer.UniqueMultiplayerID];
        }

        internal static void ResetDivorces()
        {
            if (!Config.PreventHostileDivorces)
                return;
            List<string> friends = Game1.player.friendshipData.Keys.ToList();
            foreach (string f in friends)
            {
                if (Game1.player.friendshipData[f].Status == FriendshipStatus.Divorced)
                {
                    ModMonitor.Log($"Wiping divorce for {f}");
                    if (Game1.player.friendshipData[f].Points < 8 * 250)
                        Game1.player.friendshipData[f].Status = FriendshipStatus.Friendly;
                    else
                        Game1.player.friendshipData[f].Status = FriendshipStatus.Dating;
                }
            }
        }

        public static string GetRandomSpouse(Farmer f)
        {
            var spouses = GetSpouses(f, true);
            if (spouses.Count == 0)
                return null;
            ShuffleDic(ref spouses);
            return spouses.Keys.ToArray()[0];
        }

        public static void PlaceSpousesInFarmhouse(FarmHouse farmHouse)
        {
            Farmer farmer = farmHouse.owner;

            if (farmer == null)
                return;

            List<NPC> allSpouses = GetSpouses(farmer, true).Values.ToList();

            if (allSpouses.Count == 0)
            {
                ModMonitor.Log("no spouses");
                return;
            }

            ShuffleList(ref allSpouses);

            List<string> bedSpouses = new List<string>();
            string kitchenSpouse = null;

            foreach (NPC spouse in allSpouses)
            {
                if (spouse is null)
                    continue;
                if (!farmHouse.Equals(spouse.currentLocation))
                {
                    ModMonitor.Log($"{spouse.Name} is not in farm house ({spouse.currentLocation.Name})");
                    continue;
                }
                int type = myRand.Next(0, 100);

                ModMonitor.Log($"spouse rand {type}, bed: {Config.PercentChanceForSpouseInBed} kitchen {Config.PercentChanceForSpouseInKitchen}");

                if (type < Config.PercentChanceForSpouseInBed)
                {
                    if (bedSpouses.Count < 1 && (Config.RoommateRomance || !farmer.friendshipData[spouse.Name].IsRoommate()) && HasSleepingAnimation(spouse.Name))
                    {
                        ModMonitor.Log("made bed spouse: " + spouse.Name);
                        bedSpouses.Add(spouse.Name);
                    }

                }
                else if (type < Config.PercentChanceForSpouseInBed + Config.PercentChanceForSpouseInKitchen)
                {
                    if (kitchenSpouse == null)
                    {
                        ModMonitor.Log("made kitchen spouse: " + spouse.Name);
                        kitchenSpouse = spouse.Name;
                    }
                }
                else if (type < Config.PercentChanceForSpouseInBed + Config.PercentChanceForSpouseInKitchen + Config.PercentChanceForSpouseAtPatio)
                {
                    if (!Game1.isRaining && !Game1.IsWinter && !Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && !spouse.Name.Equals("Krobus") && spouse.Schedule == null)
                    {
                        ModMonitor.Log("made patio spouse: " + spouse.Name);
                        spouse.setUpForOutdoorPatioActivity();
                        ModMonitor.Log($"{spouse.Name} at {spouse.currentLocation.Name} {spouse.TilePoint}");
                    }
                }
            }

            foreach (NPC spouse in allSpouses)
            {
                if (spouse is null)
                    continue;
                ModMonitor.Log("placing " + spouse.Name);

                Point spouseRoomSpot = new Point(-1, -1);

               
                if (spouseRoomSpot.X < 0 && farmer.spouse == spouse.Name)
                {
                    spouseRoomSpot = farmHouse.GetSpouseRoomSpot();
                    ModMonitor.Log($"Using default spouse spot {spouseRoomSpot}");
                }

                if (!farmHouse.Equals(spouse.currentLocation))
                {
                    ModMonitor.Log($"{spouse.Name} is not in farm house ({spouse.currentLocation.Name})");
                    continue;
                }

                ModMonitor.Log("in farm house");
                spouse.shouldPlaySpousePatioAnimation.Value = false;

                Vector2 bedPos = GetSpouseBedPosition(farmHouse, spouse.Name);

                if (bedSpouses.Count > 0 && bedSpouses.Contains(spouse.Name) && bedPos != Vector2.Zero)
                {
                    ModMonitor.Log($"putting {spouse.Name} in bed");
                    spouse.position.Value = GetSpouseBedPosition(farmHouse, spouse.Name);
                }
                else if (kitchenSpouse == spouse.Name && !IsTileOccupied(farmHouse, farmHouse.getKitchenStandingSpot(), spouse.Name))
                {
                    ModMonitor.Log($"{spouse.Name} is in kitchen");

                    spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                    spouse.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
                }
                else if (spouseRoomSpot.X > -1 && !IsTileOccupied(farmHouse, spouseRoomSpot, spouse.Name))
                {
                    ModMonitor.Log($"{spouse.Name} is in spouse room");
                    spouse.setTilePosition(spouseRoomSpot);
                    spouse.setSpouseRoomMarriageDialogue();
                }
                else
                {
                    spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(myRand));
                    spouse.faceDirection(myRand.Next(0, 4));
                    ModMonitor.Log($"{spouse.Name} spouse random loc {spouse.TilePoint}");
                    spouse.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
                }
            }
        }

        private static bool IsTileOccupied(GameLocation location, Point tileLocation, string characterToIgnore)
        {
            Microsoft.Xna.Framework.Rectangle tileLocationRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64 + 1, tileLocation.Y * 64 + 1, 62, 62);

            for (int i = 0; i < location.characters.Count; i++)
            {
                if (location.characters[i] != null && !location.characters[i].Name.Equals(characterToIgnore) && location.characters[i].GetBoundingBox().Intersects(tileLocationRect))
                {
                    ModMonitor.Log($"Tile {tileLocation} is occupied by {location.characters[i].Name}");

                    return true;
                }
            }
            return false;
        }

        public static Point GetSpouseBedEndPoint(FarmHouse fh, string name)
        {
            var bedSpouses = GetBedSpouses(fh);

            Point bedStart = fh.GetSpouseBed().GetBedSpot();
            int bedWidth = GetBedWidth(Game1.MasterPlayer);   //this may fuck up Multiplayer a little, will return whoever is the main farmer's number of spouses
            bool up = fh.upgradeLevel > 1;

            int x = (int)(bedSpouses.IndexOf(name) / (float)(bedSpouses.Count) * (bedWidth - 1));
            if (x < 0)
                return Point.Zero;
            return new Point(bedStart.X + x, bedStart.Y);
        }
        public static Vector2 GetSpouseBedPosition(FarmHouse fh, string name)
        {
            var allBedmates = GetBedSpouses(fh);

            Point bedStart = GetBedStart(fh);
            int x = 64 + (int)((allBedmates.IndexOf(name) + 1) / (float)(allBedmates.Count + 1) * (GetBedWidth(Game1.MasterPlayer) - 1) * 64);
            return new Vector2(bedStart.X * 64 + x, bedStart.Y * 64 + bedSleepOffset - (GetTopOfHeadSleepOffset(name) * 4));
        }

        public static Point GetBedStart(FarmHouse fh)
        {
            if (fh?.GetSpouseBed()?.GetBedSpot() == null)
                return Point.Zero;
            return new Point(fh.GetSpouseBed().GetBedSpot().X - 1, fh.GetSpouseBed().GetBedSpot().Y - 1);
        }

        public static bool IsInBed(FarmHouse fh, Microsoft.Xna.Framework.Rectangle box)
        {
            int bedWidth = GetBedWidth(Game1.MasterPlayer);
            Point bedStart = GetBedStart(fh);
            Microsoft.Xna.Framework.Rectangle bed = new Microsoft.Xna.Framework.Rectangle(bedStart.X * 64, bedStart.Y * 64, bedWidth * 64, 3 * 64);

            if (box.Intersects(bed))
            {
                return true;
            }
            return false;
        }
        public static int GetBedWidth(Farmer f)
        {
            
            var spouses = GetSpouses(f, true);
            if (spouses.Count <= 2)
            {
                return 3;

            }


            else
            {
                return spouses.Count;
            }
        }
        public static List<string> GetBedSpouses(FarmHouse fh)
        {
            if (Config.RoommateRomance)
                return GetSpouses(fh.owner, true).Keys.ToList();

            return GetSpouses(fh.owner, true).Keys.ToList().FindAll(s => !fh.owner.friendshipData[s].RoommateMarriage);
        }

        public static List<string> ReorderSpousesForSleeping(List<string> sleepSpouses)
        {
            List<string> configSpouses = Config.SpouseSleepOrder.Split(',').Where(s => s.Length > 0).ToList();
            List<string> spouses = new List<string>();
            foreach (string s in configSpouses)
            {
                if (sleepSpouses.Contains(s))
                    spouses.Add(s);
            }

            foreach (string s in sleepSpouses)
            {
                if (!spouses.Contains(s))
                {
                    spouses.Add(s);
                    configSpouses.Add(s);
                }
            }
            string configString = string.Join(",", configSpouses);
            if (configString != Config.SpouseSleepOrder)
            {
                Config.SpouseSleepOrder = configString;
                SHelper.WriteConfig(Config);
            }

            return spouses;
        }


        public static int GetTopOfHeadSleepOffset(string name)
        {
            if (topOfHeadOffsets.ContainsKey(name))
            {
                return topOfHeadOffsets[name];
            }
            //SMonitor.Log($"dont yet have offset for {name}");
            int top = 0;

            if (name == "Krobus")
                return 8;

            Texture2D tex = Game1.content.Load<Texture2D>($"Characters\\{name}");

            int sleepidx;
            string sleepAnim = SleepAnimation(name);
            if (sleepAnim == null || !int.TryParse(sleepAnim.Split('/')[0], out sleepidx))
                sleepidx = 8;

            if ((sleepidx * 16) / 64 * 32 >= tex.Height)
            {
                sleepidx = 8;
            }


            Color[] colors = new Color[tex.Width * tex.Height];
            tex.GetData(colors);

            //SMonitor.Log($"sleep index for {name} {sleepidx}");

            int startx = (sleepidx * 16) % 64;
            int starty = (sleepidx * 16) / 64 * 32;

            //SMonitor.Log($"start {startx},{starty}");

            for (int i = 0; i < 16 * 32; i++)
            {
                int idx = startx + (i % 16) + (starty + i / 16) * 64;
                if (idx >= colors.Length)
                {
                    ModMonitor.Log($"Sleep pos couldn't get pixel at {startx + i % 16},{starty + i / 16} ");
                    break;
                }
                Color c = colors[idx];
                if (c != Color.Transparent)
                {
                    top = i / 16;
                    break;
                }
            }
            topOfHeadOffsets.Add(name, top);
            return top;
        }


        public static bool HasSleepingAnimation(string name)
        {
            string sleepAnim = SleepAnimation(name);
            if (sleepAnim == null || !sleepAnim.Contains("/"))
                return false;

            if (!int.TryParse(sleepAnim.Split('/')[0], out int sleepidx))
                return false;

            Texture2D tex = SHelper.GameContent.Load<Texture2D>($"Characters/{name}");
            //SMonitor.Log($"tex height for {name}: {tex.Height}");

            if (sleepidx / 4 * 32 >= tex.Height)
            {
                return false;
            }
            return true;
        }

        private static string SleepAnimation(string name)
        {
            string anim = null;
            if (Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions").ContainsKey(name.ToLower() + "_sleep"))
            {
                anim = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions")[name.ToLower() + "_sleep"];
            }
            else if (Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions").ContainsKey(name + "_Sleep"))
            {
                anim = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions")[name + "_Sleep"];
            }
            return anim;
        }


        internal static void NPCDoAnimation(NPC npc, string npcAnimation)
        {
            Dictionary<string, string> animationDescriptions = SHelper.GameContent.Load<Dictionary<string, string>>("Data\\animationDescriptions");
            if (!animationDescriptions.ContainsKey(npcAnimation))
                return;

            string[] rawData = animationDescriptions[npcAnimation].Split('/');
            var animFrames = Utility.parseStringToIntArray(rawData[1], ' ');

            List<FarmerSprite.AnimationFrame> anim = new List<FarmerSprite.AnimationFrame>();
            for (int i = 0; i < animFrames.Length; i++)
            {
                anim.Add(new FarmerSprite.AnimationFrame(animFrames[i], 100, 0, false, false, null, false, 0));
            }
            ModMonitor.Log($"playing animation {npcAnimation} for {npc.Name}");
            npc.Sprite.setCurrentAnimation(anim);
        }

        public static void ResetSpouses(Farmer f, bool force = false)
        {
            if (force)
            {
                currentSpouses.Remove(f.UniqueMultiplayerID);
                currentUnofficialSpouses.Remove(f.UniqueMultiplayerID);
            }
            Dictionary<string, NPC> spouses = GetSpouses(f, true);
            if (f.spouse == null)
            {
                if (spouses.Count > 0)
                {
                    ModMonitor.Log("No official spouse, setting official spouse to: " + spouses.First().Key);
                    f.spouse = spouses.First().Key;
                }
            }

            foreach (string name in f.friendshipData.Keys)
            {
                if (f.friendshipData[name].IsEngaged())
                {
                    ModMonitor.Log($"{f.Name} is engaged to: {name} {f.friendshipData[name].CountdownToWedding} days until wedding");
                    if (f.friendshipData[name].WeddingDate.TotalDays < new WorldDate(Game1.Date).TotalDays)
                    {
                        ModMonitor.Log("invalid engagement: " + name);
                        f.friendshipData[name].WeddingDate.TotalDays = new WorldDate(Game1.Date).TotalDays + 1;
                    }
                    if (f.spouse != name)
                    {
                        ModMonitor.Log("setting spouse to engagee: " + name);
                        f.spouse = name;
                    }
                }
                if (f.friendshipData[name].IsMarried() && f.spouse != name)
                {
                    //SMonitor.Log($"{f.Name} is married to: {name}");
                    if (f.spouse != null && f.friendshipData[f.spouse] != null && !f.friendshipData[f.spouse].IsMarried() && !f.friendshipData[f.spouse].IsEngaged())
                    {
                        ModMonitor.Log("invalid ospouse, setting ospouse to " + name);
                        f.spouse = name;
                    }
                    if (f.spouse == null)
                    {
                        ModMonitor.Log("null ospouse, setting ospouse to " + name);
                        f.spouse = name;
                    }
                }
            }
            ReloadSpouses(f);
        }
        public static void SetAllNPCsDatable()
        {
            if (!Config.RomanceAllVillagers)
                return;
            Farmer f = Game1.player;
            if (f == null)
            {
                return;
            }
            foreach (string friend in f.friendshipData.Keys)
            {
                NPC npc = Game1.getCharacterFromName(friend);
                if (npc != null && !npc.datable.Value && npc is NPC && !(npc is Child) && (npc.Age == 0 || npc.Age == 1))
                {
                    ModMonitor.Log($"Making {npc.Name} datable.");
                    npc.datable.Value = true;
                }
            }
        }


        public static void ShuffleList<T>(ref List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = myRand.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void ShuffleDic<T1, T2>(ref Dictionary<T1, T2> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = myRand.Next(n + 1);
                var value = list[list.Keys.ToArray()[k]];
                list[list.Keys.ToArray()[k]] = list[list.Keys.ToArray()[n]];
                list[list.Keys.ToArray()[n]] = value;
            }
        }


        // from Pregnancy.cs


        public static bool Utility_pickPersonalFarmEvent_Prefix(ref FarmEvent __result)
        {
            if (!Config.EnableMod)
                return true;
            ModMonitor.Log("picking event");
            if (Game1.weddingToday)
            {
                __result = null;
                return false;
            }



            List<NPC> allSpouses = GetSpouses(Game1.player, true).Values.ToList();

            ShuffleList(ref allSpouses);

            foreach (NPC spouse in allSpouses)
            {
                if (spouse == null)
                {
                    ModMonitor.Log($"Utility_pickPersonalFarmEvent_Prefix spouse is null");
                    continue;
                }
                Farmer f = spouse.getSpouse();

                Friendship friendship = f.friendshipData[spouse.Name];

                if (friendship.DaysUntilBirthing <= 0 && friendship.NextBirthingDate != null)
                {
                    lastPregnantSpouse = null;
                    lastBirthingSpouse = spouse;
                    __result = new BirthingEvent();
                    return false;
                }
            }

   

            lastBirthingSpouse = null;
            lastPregnantSpouse = null;

            foreach (NPC spouse in allSpouses)
            {
                if (spouse == null)
                    continue;
                Farmer f = spouse.getSpouse();
                if (!Config.RoommateRomance && f.friendshipData[spouse.Name].RoommateMarriage)
                    continue;

                int heartsWithSpouse = f.getFriendshipHeartLevelForNPC(spouse.Name);
                Friendship friendship = f.friendshipData[spouse.Name];
                List<Child> kids = f.getChildren();
                int maxChildren =  Config.MaxChildren;
                FarmHouse fh = Utility.getHomeOfFarmer(f);
                bool can = spouse.daysAfterLastBirth <= 0 && fh.cribStyle.Value > 0 && fh.upgradeLevel >= 2 && friendship.DaysUntilBirthing < 0 && heartsWithSpouse >= 10 && friendship.DaysMarried >= 7 && (kids.Count < maxChildren);
                ModMonitor.Log($"Checking ability to get pregnant: {spouse.Name} {can}:{(fh.cribStyle.Value > 0 ? $" no crib" : "")}{(Utility.getHomeOfFarmer(f).upgradeLevel < 2 ? $" house level too low {Utility.getHomeOfFarmer(f).upgradeLevel}" : "")}{(friendship.DaysMarried < 7 ? $", not married long enough {friendship.DaysMarried}" : "")}{(friendship.DaysUntilBirthing >= 0 ? $", already pregnant (gives birth in: {friendship.DaysUntilBirthing})" : "")}");
                if (can && Game1.player.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation.Value) && myRand.NextDouble() < 0.05)
                {
                    ModMonitor.Log("Requesting a baby!");
                    lastPregnantSpouse = spouse;
                    __result = new QuestionEvent(1);
                    return false;
                }
            }
            return true;
        }

        public static NPC lastPregnantSpouse;
        private static NPC lastBirthingSpouse;

        public static bool QuestionEvent_setUp_Prefix(int ___whichQuestion, ref bool __result)
        {
            if (Config.EnableMod && ___whichQuestion == 1)
            {
                if (lastPregnantSpouse == null)
                {
                    __result = true;
                    return false;
                }
                Response[] answers = new Response[]
                {
                    new Response("Yes", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_Yes")),
                    new Response("Not", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_No"))
                };

                if (!lastPregnantSpouse.isAdoptionSpouse() || Config.GayPregnancies)
                {
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion", lastPregnantSpouse.Name), answers, new GameLocation.afterQuestionBehavior(answerPregnancyQuestion), lastPregnantSpouse);
                }
                else
                {
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion_Adoption", lastPregnantSpouse.Name), answers, new GameLocation.afterQuestionBehavior(answerPregnancyQuestion), lastPregnantSpouse);
                }
                Game1.messagePause = true;
                __result = false;
                return false;
            }
            return true;
        }

        public static bool BirthingEvent_tickUpdate_Prefix(GameTime time, BirthingEvent __instance, ref bool __result, ref int ___timer, string ___soundName, ref bool ___playedSound, string ___message, ref bool ___naming, bool ___getBabyName, bool ___isMale, string ___babyName)
        {
            if (!Config.EnableMod || !___getBabyName)
                return true;

            Game1.player.CanMove = false;
            ___timer += time.ElapsedGameTime.Milliseconds;
            Game1.fadeToBlackAlpha = 1f;

            if (!___naming)
            {
                Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(__instance.returnBabyName), Game1.content.LoadString(___isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
                ___naming = true;
            }
            if (___babyName != null && ___babyName != "" && ___babyName.Length > 0)
            {
                double chance = (lastBirthingSpouse.Name.Equals("Maru") || lastBirthingSpouse.Name.Equals("Krobus")) ? 0.5 : 0.0;
                chance += (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
                bool isDarkSkinned = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed).NextDouble() < chance;
                string newBabyName = ___babyName;
                List<NPC> all_characters = Utility.getAllCharacters();
                bool collision_found = false;
                do
                {
                    collision_found = false;
                    using (List<NPC>.Enumerator enumerator = all_characters.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.Name.Equals(newBabyName))
                            {
                                newBabyName += " ";
                                collision_found = true;
                                break;
                            }
                        }
                    }
                }
                while (collision_found);
                Child baby = new Child(newBabyName, ___isMale, isDarkSkinned, Game1.player)
                {
                    Age = 0,
                    Position = new Vector2(16f, 4f) * 64f + new Vector2(0f + myRand.Next(-64, 48), -24f + myRand.Next(-24, 24)),
                };
                baby.modData["ApryllForever.SweetPolyamory/OtherParent"] = lastBirthingSpouse.Name;

                Utility.getHomeOfFarmer(Game1.player).characters.Add(baby);
                Game1.playSound("smallSelect");
                Game1.getCharacterFromName(lastBirthingSpouse.Name).daysAfterLastBirth = 5;
                Game1.player.friendshipData[lastBirthingSpouse.Name].NextBirthingDate = null;
                if (Game1.player.getChildrenCount() == 2)
                {
                    Game1.getCharacterFromName(lastBirthingSpouse.Name).shouldSayMarriageDialogue.Value = true;
                    Game1.getCharacterFromName(lastBirthingSpouse.Name).currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_SecondChild" + myRand.Next(1, 3), true, new string[0]));
                    Game1.getSteamAchievement("Achievement_FullHouse");
                }
                else if (lastBirthingSpouse.isAdoptionSpouse() && !Config.GayPregnancies)
                {
                    Game1.getCharacterFromName(lastBirthingSpouse.Name).currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_Adoption", true, new string[]
                    {
                        ___babyName
                    }));
                }
                else
                {
                    Game1.getCharacterFromName(lastBirthingSpouse.Name).currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_FirstChild", true, new string[]
                    {
                        ___babyName
                    }));
                }
                Game1.morningQueue.Enqueue(delegate
                {
                    mp.globalChatInfoMessage("Baby", new string[]
                    {
                        Lexicon.capitalize(Game1.player.Name),
                        Game1.player.spouse,
                        Lexicon.getGenderedChildTerm(___isMale),
                        Lexicon.getPronoun(___isMale),
                        baby.displayName
                    });
                });
                if (Game1.keyboardDispatcher != null)
                {
                    Game1.keyboardDispatcher.Subscriber = null;
                }
                Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).getBedSpot()) * 64f;
                Game1.globalFadeToClear(null, 0.02f);
                lastBirthingSpouse = null;
                __result = true;
                return false;
            }
            __result = false;
            return false;
        }
        public static bool BirthingEvent_setUp_Prefix(ref bool ___isMale, ref string ___message, ref bool __result)
        {
            if (!Config.EnableMod)
                return true;
            if (lastBirthingSpouse == null)
            {
                __result = true;
                return false;
            }
            NPC spouse = lastBirthingSpouse;
            Game1.player.CanMove = false;
            ___isMale = myRand.NextDouble() > 0.5f;
            if (spouse.isAdoptionSpouse())
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(___isMale));
            }
            else if (spouse.Gender == Gender.Male)
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(___isMale));
            }
            else
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(___isMale), spouse.displayName);
            }
            __result = false;
            return false;
        }

        public static void answerPregnancyQuestion(Farmer who, string answer)
        {
            if (answer == "Yes" && who is not null && lastPregnantSpouse is not null && who.friendshipData.ContainsKey(lastPregnantSpouse.Name))
            {
                WorldDate birthingDate = new WorldDate(Game1.Date);
                birthingDate.TotalDays += 14;
                who.friendshipData[lastPregnantSpouse.Name].NextBirthingDate = birthingDate;
                lastPregnantSpouse.isAdoptionSpouse();
            }
        }




























    }
    }
