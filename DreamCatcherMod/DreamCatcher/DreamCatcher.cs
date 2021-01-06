﻿using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Modding;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;


namespace DreamCatcher
{
  internal class SocketServer : WebSocketBehavior
  {

    private string currentArea = "";

    // Import Room Mappings json for Scene : Area mappings.
    /*    private static string raw_txt = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "\\room_mappings.json"));
        private readonly Dictionary<string, string> roomMappings = JsonConvert.DeserializeObject<Dictionary<string, string>>(raw_txt);
    */

    private Dictionary<string, string> roomMappings = new Dictionary<string, string>(){
  {"Abyss_01", "Royal_Waterways"},
  {"Abyss_02", "Ancient_Basin"},
  {"Abyss_03_b", "Deepnest"},
  {"Abyss_03_c", "Kingdoms_Edge"},
  {"Abyss_03", "Ancient_Basin"},
  {"Abyss_04", "Ancient_Basin"},
  {"Abyss_05", "Ancient_Basin"},
  {"Abyss_06_Core", "Abyss"},
  {"Abyss_08", "Abyss"},
  {"Abyss_09", "Abyss"},
  {"Abyss_10", "Abyss"},
  {"Abyss_12", "Abyss"},
  {"Abyss_15", "Abyss"},
  {"Abyss_16", "Abyss"},
  {"Abyss_17", "Ancient_Basin"},
  {"Abyss_18", "Ancient_Basin"},
  {"Abyss_19", "Ancient_Basin"},
  {"Abyss_20", "Ancient_Basin"},
  {"Abyss_21", "Ancient_Basin"},
  {"Abyss_22", "Ancient_Basin"},
  {"Abyss_Lighthouse_room", "Abyss"},
  {"Cliffs_01", "Howling_Cliffs"},
  {"Cliffs_02", "Howling_Cliffs"},
  {"Cliffs_03", "Howling_Cliffs"},
  {"Cliffs_04", "Howling_Cliffs"},
  {"Cliffs_05", "Howling_Cliffs"},
  {"Cliffs_06", "Howling_Cliffs"},
  {"Crossroads_01", "Forgotten_Crossroads"},
  {"Crossroads_02", "Forgotten_Crossroads"},
  {"Crossroads_03", "Forgotten_Crossroads"},
  {"Crossroads_04", "Forgotten_Crossroads"},
  {"Crossroads_05", "Forgotten_Crossroads"},
  {"Crossroads_06", "Forgotten_Crossroads"},
  {"Crossroads_07", "Forgotten_Crossroads"},
  {"Crossroads_08", "Forgotten_Crossroads"},
  {"Crossroads_09", "Forgotten_Crossroads"},
  {"Crossroads_10", "Forgotten_Crossroads"},
  {"Crossroads_11_alt", "Forgotten_Crossroads"},
  {"Crossroads_12", "Forgotten_Crossroads"},
  {"Crossroads_13", "Forgotten_Crossroads"},
  {"Crossroads_14", "Forgotten_Crossroads"},
  {"Crossroads_15", "Forgotten_Crossroads"},
  {"Crossroads_16", "Forgotten_Crossroads"},
  {"Crossroads_18", "Forgotten_Crossroads"},
  {"Crossroads_19", "Forgotten_Crossroads"},
  {"Crossroads_21", "Forgotten_Crossroads"},
  {"Crossroads_22", "Forgotten_Crossroads"},
  {"Crossroads_25", "Forgotten_Crossroads"},
  {"Crossroads_27", "Forgotten_Crossroads"},
  {"Crossroads_30", "Forgotten_Crossroads"},
  {"Crossroads_31", "Forgotten_Crossroads"},
  {"Crossroads_33", "Forgotten_Crossroads"},
  {"Crossroads_35", "Forgotten_Crossroads"},
  {"Crossroads_36", "Forgotten_Crossroads"},
  {"Crossroads_37", "Forgotten_Crossroads"},
  {"Crossroads_38", "Forgotten_Crossroads"},
  {"Crossroads_39", "Forgotten_Crossroads"},
  {"Crossroads_40", "Forgotten_Crossroads"},
  {"Crossroads_42", "Forgotten_Crossroads"},
  {"Crossroads_43", "Forgotten_Crossroads"},
  {"Crossroads_45", "Forgotten_Crossroads"},
  {"Crossroads_46", "Forgotten_Crossroads"},
  {"Crossroads_46b", "Resting_Grounds"},
  {"Crossroads_47", "Forgotten_Crossroads"},
  {"Crossroads_48", "Forgotten_Crossroads"},
  {"Crossroads_49", "Forgotten_Crossroads"},
  {"Crossroads_49b", "City_of_Tears"},
  {"Crossroads_50", "Resting_Grounds"},
  {"Crossroads_52", "Forgotten_Crossroads"},
  {"Crossroads_ShamanTemple", "Forgotten_Crossroads"},
  {"Deepnest_01", "Fungal_Wastes"},
  {"Deepnest_01b", "Deepnest"},
  {"Deepnest_02", "Deepnest"},
  {"Deepnest_03", "Deepnest"},
  {"Deepnest_09", "Deepnest"},
  {"Deepnest_10", "Deepnest"},
  {"Deepnest_14", "Deepnest"},
  {"Deepnest_16", "Deepnest"},
  {"Deepnest_17", "Deepnest"},
  {"Deepnest_26", "Deepnest"},
  {"Deepnest_26b", "Deepnest"},
  {"Deepnest_30", "Deepnest"},
  {"Deepnest_31", "Deepnest"},
  {"Deepnest_32", "Deepnest"},
  {"Deepnest_33", "Deepnest"},
  {"Deepnest_34", "Deepnest"},
  {"Deepnest_35", "Deepnest"},
  {"Deepnest_36", "Deepnest"},
  {"Deepnest_37", "Deepnest"},
  {"Deepnest_38", "Deepnest"},
  {"Deepnest_39", "Deepnest"},
  {"Deepnest_40", "Deepnest"},
  {"Deepnest_41", "Deepnest"},
  {"Deepnest_42", "Deepnest"},
  {"Deepnest_43", "Queens_Gardens"},
  {"Deepnest_44", "Deepnest"},
  {"Deepnest_45_v02", "Deepnest"},
  {"Deepnest_East_01", "Kingdoms_Edge"},
  {"Deepnest_East_02", "Kingdoms_Edge"},
  {"Deepnest_East_03", "Kingdoms_Edge"},
  {"Deepnest_East_04", "Kingdoms_Edge"},
  {"Deepnest_East_06", "Kingdoms_Edge"},
  {"Deepnest_East_07", "Kingdoms_Edge"},
  {"Deepnest_East_08", "Kingdoms_Edge"},
  {"Deepnest_East_09", "Kingdoms_Edge"},
  {"Deepnest_East_10", "Kingdoms_Edge"},
  {"Deepnest_East_11", "Kingdoms_Edge"},
  {"Deepnest_East_12", "Kingdoms_Edge"},
  {"Deepnest_East_13", "Kingdoms_Edge"},
  {"Deepnest_East_14", "Kingdoms_Edge"},
  {"Deepnest_East_14b", "Kingdoms_Edge"},
  {"Deepnest_East_15", "Kingdoms_Edge"},
  {"Deepnest_East_16", "Kingdoms_Edge"},
  {"Deepnest_East_17", "Kingdoms_Edge"},
  {"Deepnest_East_18", "Kingdoms_Edge"},
  {"Deepnest_East_Hornet", "Kingdoms_Edge"},
  {"Deepnest_Spider_Town", "Deepnest"},
  {"Dream_NailCollection", "Resting Grounds"},
  {"Fungus1_01", "Greenpath"},
  {"Fungus1_01b", "Greenpath"},
  {"Fungus1_02", "Greenpath"},
  {"Fungus1_03", "Greenpath"},
  {"Fungus1_04", "Greenpath"},
  {"Fungus1_05", "Greenpath"},
  {"Fungus1_06", "Greenpath"},
  {"Fungus1_07", "Greenpath"},
  {"Fungus1_08", "Greenpath"},
  {"Fungus1_09", "Greenpath"},
  {"Fungus1_10", "Greenpath"},
  {"Fungus1_11", "Greenpath"},
  {"Fungus1_12", "Greenpath"},
  {"Fungus1_13", "Greenpath"},
  {"Fungus1_14", "Greenpath"},
  {"Fungus1_15", "Greenpath"},
  {"Fungus1_16_alt", "Greenpath"},
  {"Fungus1_17", "Greenpath"},
  {"Fungus1_19", "Greenpath"},
  {"Fungus1_20_v02", "Greenpath"},
  {"Fungus1_21", "Greenpath"},
  {"Fungus1_22", "Greenpath"},
  {"Fungus1_23", "Queens_Gardens"},
  {"Fungus1_24", "Queens_Gardens"},
  {"Fungus1_25", "Greenpath"},
  {"Fungus1_26", "Greenpath"},
  {"Fungus1_28", "Howling_Cliffs"},
  {"Fungus1_29", "Greenpath"},
  {"Fungus1_30", "Greenpath"},
  {"Fungus1_31", "Greenpath"},
  {"Fungus1_32", "Greenpath"},
  {"Fungus1_34", "Greenpath"},
  {"Fungus1_35", "Greenpath"},
  {"Fungus1_36", "Greenpath"},
  {"Fungus1_37", "Greenpath"},
  {"Fungus1_Slug", "Greenpath"},
  {"Fungus2_01", "Fungal_Wastes"},
  {"Fungus2_02", "Fungal_Wastes"},
  {"Fungus2_03", "Fungal_Wastes"},
  {"Fungus2_04-upper", "Fungal_Wastes"},
  {"Fungus2_04", "Fungal_Wastes"},
  {"Fungus2_05", "Fungal_Wastes"},
  {"Fungus2_06", "Fungal_Wastes"},
  {"Fungus2_07", "Fungal_Wastes"},
  {"Fungus2_08", "Fungal_Wastes"},
  {"Fungus2_09", "Fungal_Wastes"},
  {"Fungus2_10", "Fungal_Wastes"},
  {"Fungus2_11", "Fungal_Wastes"},
  {"Fungus2_12", "Fungal_Wastes"},
  {"Fungus2_13", "Fungal_Wastes"},
  {"Fungus2_14", "Fungal_Wastes"},
  {"Fungus2_15", "Fungal_Wastes"},
  {"Fungus2_17", "Fungal_Wastes"},
  {"Fungus2_18", "Fungal_Wastes"},
  {"Fungus2_19", "Fungal_Wastes"},
  {"Fungus2_20", "Fungal_Wastes"},
  {"Fungus2_21", "Fungal_Wastes"},
  {"Fungus2_23", "Fungal_Wastes"},
  {"Fungus2_25", "Deepnest"},
  {"Fungus2_26", "Fungal_Wastes"},
  {"Fungus2_28", "Fungal_Wastes"},
  {"Fungus2_29", "Fungal_Wastes"},
  {"Fungus2_30", "Fungal_Wastes"},
  {"Fungus2_31", "Fungal_Wastes"},
  {"Fungus2_32", "Fungal_Wastes"},
  {"Fungus2_33", "Fungal_Wastes"},
  {"Fungus2_34", "Fungal_Wastes"},
  {"Fungus3_01", "Fog_Canyon"},
  {"Fungus3_02", "Fog_Canyon"},
  {"Fungus3_03", "Fog_Canyon"},
  {"Fungus3_04-upper", "Queens_Gardens"},
  {"Fungus3_04", "Queens_Gardens"},
  {"Fungus3_05", "Queens_Gardens"},
  {"Fungus3_08", "Queens_Gardens"},
  {"Fungus3_10", "Queens_Gardens"},
  {"Fungus3_11", "Queens_Gardens"},
  {"Fungus3_13", "Queens_Gardens"},
  {"Fungus3_21", "Queens_Gardens"},
  {"Fungus3_22", "Queens_Gardens"},
  {"Fungus3_23", "Queens_Gardens"},
  {"Fungus3_24", "Fog_Canyon"},
  {"Fungus3_25", "Fog_Canyon"},
  {"Fungus3_25b", "Fog_Canyon"},
  {"Fungus3_26", "Fog_Canyon"},
  {"Fungus3_27", "Fog_Canyon"},
  {"Fungus3_28", "Fog_Canyon"},
  {"Fungus3_30", "Fog_Canyon"},
  {"Fungus3_34", "Queens_Gardens"},
  {"Fungus3_35", "Fog_Canyon"},
  {"Fungus3_39", "Queens_Gardens"},
  {"Fungus3_40", "Queens_Gardens"},
  {"Fungus3_44", "Fog_Canyon"},
  {"Fungus3_47", "Fog_Canyon"},
  {"Fungus3_48-upper", "Queens_Gardens"},
  {"Fungus3_48", "Queens_Gardens"},
  {"Fungus3_49", "Queens_Gardens"},
  {"Fungus3_50", "Queens_Gardens"},
  {"Fungus3_archive_02", "Fog_Canyon"},
  {"Fungus3_archive", "Fog_Canyon"},
  {"GG_Lurker", "Kingdoms_Edge"},
  {"GG_Pipeway", "Royal_Waterways"},
  {"GG_Waterways", "Royal_Waterways"},
  {"Grimm_Divine", "Dirtmouth"},
  {"Grimm_Main_Tent", "Dirtmouth"},
  {"Hive_01", "Hive"},
  {"Hive_02", "Hive"},
  {"Hive_03_c", "Hive"},
  {"Hive_03", "Hive"},
  {"Hive_04", "Hive"},
  {"Hive_05", "Hive"},
  {"Mines_01", "Crystal_Peak"},
  {"Mines_02", "Crystal_Peak"},
  {"Mines_03", "Crystal_Peak"},
  {"Mines_04", "Crystal_Peak"},
  {"Mines_05", "Crystal_Peak"},
  {"Mines_06", "Crystal_Peak"},
  {"Mines_07", "Crystal_Peak"},
  {"Mines_10", "Crystal_Peak"},
  {"Mines_11", "Crystal_Peak"},
  {"Mines_13", "Crystal_Peak"},
  {"Mines_16", "Crystal_Peak"},
  {"Mines_17", "Crystal_Peak"},
  {"Mines_18", "Crystal_Peak"},
  {"Mines_19", "Crystal_Peak"},
  {"Mines_20-lower", "Crystal_Peak"},
  {"Mines_20", "Crystal_Peak"},
  {"Mines_23", "Crystal_Peak"},
  {"Mines_24", "Crystal_Peak"},
  {"Mines_25", "Crystal_Peak"},
  {"Mines_28", "Crystal_Peak"},
  {"Mines_29", "Crystal_Peak"},
  {"Mines_30", "Crystal_Peak"},
  {"Mines_31", "Crystal_Peak"},
  {"Mines_32", "Crystal_Peak"},
  {"Mines_33", "Forgotten_Crossroads"},
  {"Mines_34", "Crystal_Peak"},
  {"Mines_35", "Crystal_Peak"},
  {"Mines_36", "Crystal_Peak"},
  {"Mines_37", "Crystal_Peak"},
  {"RestingGrounds_02", "Resting_Grounds"},
  {"RestingGrounds_04", "Resting_Grounds"},
  {"RestingGrounds_05", "Resting_Grounds"},
  {"RestingGrounds_06", "Resting_Grounds"},
  {"RestingGrounds_07", "Resting_Grounds"},
  {"RestingGrounds_08", "Resting_Grounds"},
  {"RestingGrounds_09", "Resting_Grounds"},
  {"RestingGrounds_10", "Resting_Grounds"},
  {"RestingGrounds_12", "Resting_Grounds"},
  {"RestingGrounds_17", "Resting_Grounds"},
  {"Room_Bretta", "Dirtmouth"},
  {"Room_Charm_Shop", "Forgotten_Crossroads"},
  {"Room_Colosseum_01", "Kingdoms_Edge"},
  {"Room_Colosseum_02", "Kingdoms_Edge"},
  {"Room_Colosseum_Spectate", "Kingdoms_Edge"},
  {"Room_Fungus_Shaman", "Fog_Canyon"},
  {"Room_GG_Shortcut", "Royal_Waterways"},
  {"Room_Mansion", "Resting_Grounds"},
  {"Room_mapper", "Dirtmouth"},
  {"Room_Mask_Maker", "Deepnest"},
  {"Room_Mender_House", "Forgotten_Crossroads"},
  {"Room_nailmaster_02", "Greenpath"},
  {"Room_nailmaster_03", "Kingdoms_Edge"},
  {"Room_nailmaster", "Howling_Cliffs"},
  {"Room_nailsmith", "City_of_Tears"},
  {"Room_Ouiji", "Dirtmouth"},
  {"Room_Queen", "Queens_Gardens"},
  {"Room_ruinhouse", "Forgotten Crossroads"},
  {"Room_shop", "Dirtmouth"},
  {"Room_Slug_Shrine", "Greenpath"},
  {"Room_spider_small", "Deepnest"},
  {"Room_temple", "Forgotten_Crossroads"},
  {"Room_Town_Stag_Station", "Dirtmouth"},
  {"Room_Wyrm", "Kingdoms_Edge"},
  {"Ruins_Bathhouse", "City_of_Tears"},
  {"Ruins_Elevator", "City_of_Tears"},
  {"Ruins_House_01", "City_of_Tears"},
  {"Ruins_House_02", "City_of_Tears"},
  {"Ruins_House_03", "City_of_Tears"},
  {"Ruins1_01", "City_of_Tears"},
  {"Ruins1_02", "City_of_Tears"},
  {"Ruins1_03", "City_of_Tears"},
  {"Ruins1_04", "City_of_Tears"},
  {"Ruins1_05", "City_of_Tears"},
  {"Ruins1_05b", "City_of_Tears"},
  {"Ruins1_05c", "City_of_Tears"},
  {"Ruins1_06", "City_of_Tears"},
  {"Ruins1_09", "City_of_Tears"},
  {"Ruins1_17", "City_of_Tears"},
  {"Ruins1_18", "City_of_Tears"},
  {"Ruins1_23", "City_of_Tears"},
  {"Ruins1_24-upper", "City_of_Tears"},
  {"Ruins1_24", "City_of_Tears"},
  {"Ruins1_25", "City_of_Tears"},
  {"Ruins1_27", "City_of_Tears"},
  {"Ruins1_28", "City_of_Tears"},
  {"Ruins1_29", "City_of_Tears"},
  {"Ruins1_30", "City_of_Tears"},
  {"Ruins1_31", "City_of_Tears"},
  {"Ruins1_31b", "City_of_Tears"},
  {"Ruins1_32", "City_of_Tears"},
  {"Ruins2_01_b", "City_of_Tears"},
  {"Ruins2_01", "City_of_Tears"},
  {"Ruins2_03", "City_of_Tears"},
  {"Ruins2_03b", "City_of_Tears"},
  {"Ruins2_04", "City_of_Tears"},
  {"Ruins2_05", "City_of_Tears"},
  {"Ruins2_06", "City_of_Tears"},
  {"Ruins2_07", "City_of_Tears"},
  {"Ruins2_08", "City_of_Tears"},
  {"Ruins2_09", "City_of_Tears"},
  {"Ruins2_10", "Resting_Grounds"},
  {"Ruins2_10b", "City_of_Tears"},
  {"Ruins2_11_b", "City_of_Tears"},
  {"Ruins2_11", "City_of_Tears"},
  {"Ruins2_Watcher_Room", "City_of_Tears"},
  {"Town", "Dirtmouth"},
  {"Tutorial_01", "Dirtmouth"},
  {"Waterways_01", "Royal_Waterways"},
  {"Waterways_02", "Royal_Waterways"},
  {"Waterways_03", "Royal_Waterways"},
  {"Waterways_04", "Royal_Waterways"},
  {"Waterways_04b", "Royal_Waterways"},
  {"Waterways_05", "Royal_Waterways"},
  {"Waterways_06", "Royal_Waterways"},
  {"Waterways_07", "Royal_Waterways"},
  {"Waterways_08", "Royal_Waterways"},
  {"Waterways_09", "Royal_Waterways"},
  {"Waterways_12", "Royal_Waterways"},
  {"Waterways_13", "Royal_Waterways"},
  {"Waterways_14", "Royal_Waterways"},
  {"Waterways_15", "Royal_Waterways"},
  {"White_Palace_01", "White_Palace"},
  {"White_Palace_02", "White_Palace"},
  {"White_Palace_03_hub", "White_Palace"},
  {"White_Palace_04", "White_Palace"},
  {"White_Palace_05-upper", "White_Palace"},
  {"White_Palace_05", "White_Palace"},
  {"White_Palace_06", "White_Palace"},
  {"White_Palace_07", "White_Palace"},
  {"White_Palace_08", "White_Palace"},
  {"White_Palace_09", "White_Palace"},
  {"White_Palace_11", "White_Palace"},
  {"White_Palace_12", "White_Palace"},
  {"White_Palace_13", "White_Palace"},
  {"White_Palace_14", "White_Palace"},
  {"White_Palace_15", "White_Palace"},
  {"White_Palace_16", "White_Palace"},
  {"White_Palace_17", "White_Palace"},
  {"White_Palace_18", "White_Palace"},
  {"White_Palace_19", "White_Palace"},
  {"White_Palace_20", "White_Palace"},
};

    private bool gotUserSpoiler = false;

    public SocketServer() => IgnoreExtensions = true;
    public void Broadcast(string s) => Sessions.Broadcast(s);

    protected override void OnMessage(WebSocketSharp.MessageEventArgs e) => Send(e.Data);
    protected override void OnError(WebSocketSharp.ErrorEventArgs e) => Send(e.Message);

    /// If the API returns a bool, go to MessageBool.  Otherwise, go to MessageInt.
    /// https://radiance.host/apidocs/Hooks.html

    public void MessageBool(string item, bool value)
    {
      if (State != WebSocketState.Open) { return; }
      var lowercaseBool = value ? "true" : "false";
      Send($"{{\"{item}\": {lowercaseBool}, \"current_area\": \"{this.currentArea}\"}}");
    }
    public void MessageInt(string item, int value)
    {
      if (State != WebSocketState.Open) { return; }
      Send($"{{\"{item}\": {value}, \"current_area\": \"{this.currentArea}\"}}");
    }
    public void MessageSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
      if (State != WebSocketState.Open) { return; }
          // Reads the user Rando Spoilerlog.
      Thread.Sleep(200);
      if (!this.gotUserSpoiler)
      {
        string userDataPath = System.IO.Path.Combine(Application.persistentDataPath, "RandomizerSpoilerLog.txt");
        string spoilerLog = System.IO.File.ReadAllText(userDataPath);
        this.gotUserSpoiler = true;
        Send($"{{\"spoiler\": \"{spoilerLog}\"}}");
      }

      this.currentArea = this.roomMappings[scene.name];
      Send($"{{\"scene\": \"{scene.name}\", \"scene_parsed\": \"{this.currentArea}\"}}");

    }
  }

  public class HKDataDump : Mod, ITogglableMod
  {
    public override int LoadPriority() => 9999;  // TODO: ???
    private readonly WebSocketServer _wss = new WebSocketServer(10051);
    internal static HKDataDump Instance;

    /// <summary>
    /// Creates and starts up the Websocket Server instance.
    /// </summary>
    public override void Initialize()
    {
      Instance = this;
      Log("Initializing Dreamcatcher HKDataDump...");
      _wss.AddWebSocketService<SocketServer>("/data", ss=> {
        ModHooks.Instance.SetPlayerBoolHook += ss.MessageBool;
        ModHooks.Instance.SetPlayerIntHook += ss.MessageInt;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += ss.MessageSceneLoaded;
      });
      _wss.Start();
      Log("Initialized Dreamcatcher HKDataDump!");
    }

    /// <summary>
    /// Called when the mod is disabled; stops webserver and removes socket.
    /// </summary>
    public void Unload()
    {
      _wss.Stop();
      _wss.RemoveWebSocketService("/data");
    }

  }
}
