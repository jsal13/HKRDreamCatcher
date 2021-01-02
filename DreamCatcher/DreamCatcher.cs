using System.Linq;
using System.Threading;
using Modding;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;


namespace DreamCatcher
{
  internal class SocketServer : WebSocketBehavior
  {
    public SocketServer() => IgnoreExtensions = true;
    public void Broadcast(string s) => Sessions.Broadcast(s);

    private static readonly string[] itemsToIgnore = {
"atBench",
"atMapPrompt",
"backerCredits",
"bankerTheftCheck",
"beamDamage",
"blockerHits",
"charmBenchMsg",
"charmSlots",
"charmSlotsFilled",
"charmSlotsFilled",
"charmsOwned",
"corn_abyssLeft",
"corn_cityLeft",
"corn_cliffsLeft",
"corn_deepnestLeft",
"corn_fogCanyonLeft",
"corn_fungalWastesLeft",
"corn_greenpathLeft",
"corn_minesLeft",
"corn_outskirtsLeft",
"corn_royalGardensLeft",
"corn_waterwaysLeft",
"corniferAtHome",
"currentArea",
"currentInvPane",
"damagedBlue",
"disablePause",
"elderbugFirstCall",
"elderbugHistory1",
"elderbugSpeechFinalBossDoor",
"elderbugSpeechMapShop",
"enteredTutorialFirstTime",
"environmentType",
"environmentTypeDefault",
"equippedCharm_2",
"geo",
"hasGodfinder",
"hasMap",
"hasQuill",
"hazardRespawnFacingRight",
"health",
"healthBlue",
"isFirstGame",
"isInvincible",
"joniHealthBlue",
"journalEntriesCompleted",
"journalEntriesTotal",
"journalNotesCompleted",
"killedBuzzer",
"killsBlocker",
"killsClimber",
"mapAbyss",
"mapAllRooms",
"mapCity",
"mapCliffs",
"mapCrossroads",
"mapDeepnest",
"mapDirtmouth",
"mapFogCanyon",
"mapFungalWastes",
"mapGreenpath",
"mapMines",
"mapOutskirts",
"mapRestingGrounds",
"mapRoyalGardens",
"mapWaterways",
"maxHealth",
"metElderbug",
"MPCharge",
"MPReserve",
"newDataBuzzer",
"openedMapperShop",
"openedSlyShop",
"openingCreditsPlayed",
"overcharmed",
"permadeathMode",
"prevHealth",
"previousDarkness",
"quirrelCityEncountered",
"quirrelLeftEggTemple",
"RandomizerMod.Bool.Godtuner.Sly",
"respawnFacingRight",
"respawnType",
"royalCharmState",
"shamanPillar",
"slyRescued",
"visitedDirtmouth",
    };

    protected override void OnMessage(MessageEventArgs e) => Send(e.Data);
    protected override void OnError(ErrorEventArgs e) => Send(e.Message);

    /// If the API returns a bool, go to MessageBool.  Otherwise, go to MessageInt.
    /// https://radiance.host/apidocs/Hooks.html
/*    public void MessageBool(string item, bool value)
    {
      if (itemsToIgnore.Contains(item))
      {
        return;
      }
      Send($"\"{item}\": {value}");
    }
    public void MessageInt(string item, int value)
    {
      if (itemsToIgnore.Contains(item))
      {
        return;
      }

      Send($"\"{item}\": {value}");
    }*/
    public void MessageSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
      Thread.Sleep(100);
      var sceneNameParsed = SceneToAreaMapping(scene.name);
      Send($"\"scene\": {scene.name}, \"scene_parsed\": {sceneNameParsed}");
    }

    public string SceneToAreaMapping(string sceneName)
    {
      if (sceneName.Contains("RestingGrounds")) { return "Resting Grounds"; }
      else if (sceneName.Contains("Fungus1")) { return "Greenpath"; }
      else if (sceneName.Contains("Fungus2")) { return "Fungal Wastes"; }
      else if (sceneName.Contains("Ruins1")) { return "City of Tears"; }
      else if (sceneName.Contains("Waterway")) { return "Royal Waterway"; }
      else if (sceneName.Contains("Deepnest")) { return "Deepnest"; }
      else if (sceneName.Contains("Abyss")) { return "Abyss"; }
      else if (sceneName.Contains("Deepnest_East")) { return "Kingdom's Edge"; }
      else if (sceneName.Contains("Mines")) { return "Crystal Peaks"; }
      else if (sceneName.Contains("Fungus3")) { return "Queen's Gardens"; }
      // else if (sceneName.Contains("Fungus1_") || sceneName.Contains('White_Palace') { return "Greenpath"; }
      else if (sceneName.Contains("Cliffs")) { return "Howling Cliffs"; }
      else { return "Unknown! Please Record This Area!"; }
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
/*        ModHooks.Instance.SetPlayerBoolHook += ss.MessageBool;
        ModHooks.Instance.SetPlayerIntHook += ss.MessageInt;*/
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

