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
      "charmSlotsFilled",
      "charmSlotsFilled",
      "charmsOwned",
      "corn_crossroadsLeft",
      "damagedBlue",
      "disablePause",
      "elderbugSpeechFinalBossDoor",
      "enteredTutorialFirstTime",
      "equippedCharm_2",
      "geo",
      "hasGodfinder",
      "hazardRespawnFacingRight",
      "health",
      "healthBlue",
      "isInvincible",
      "joniHealthBlue",
      "journalEntriesCompleted",
      "journalEntriesTotal",
      "journalNotesCompleted",
      "killsBlocker",
      "killsClimber",
      "maxHealth",
      "MPCharge",
      "MPReserve",
      "openedMapperShop",
      "openedSlyShop",
      "prevHealth",
      "previousDarkness",
      "quirrelCityEncountered",
      "quirrelLeftEggTemple",
      "RandomizerMod.Bool.Godtuner.Sly",
      "respawnFacingRight",
      "respawnType",
      "shamanPillar",
      "slyRescued"
    };

    protected override void OnMessage(MessageEventArgs e) => Send(e.Data);
    protected override void OnError(ErrorEventArgs e) => Send(e.Message);

    /// If the API returns a bool, go to MessageBool.  Otherwise, go to MessageInt.
    /// https://radiance.host/apidocs/Hooks.html
    public void MessageBool(string item, bool value)
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
    }
    public void MessageSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
      Thread.Sleep(100);
      var sceneNameParsed = SceneToAreaMapping(scene.name);
      Send($"\"scene\": {scene.name}, \"scene_parsed\": {sceneNameParsed}");
    }

    public string SceneToAreaMapping(string sceneName)
    {
      if (sceneName.Contains("RestingGrounds")) { return "Resting Grounds"; }
      else { return "Unknown!"; }
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

    private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
      throw new System.NotImplementedException();
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

