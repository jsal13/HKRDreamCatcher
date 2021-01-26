using Modding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DreamCatcher
{
  internal class SocketServer : WebSocketBehavior
  {
    private bool messagedHerrah = false;
    private bool messagedMonomon = false;
    private bool messagedLurien = false;

    private static readonly StreamReader srRoomMapper = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("DreamCatcher.room_mappings.csv"));
    private static readonly StreamReader srEventsToIgnore = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("DreamCatcher.events_to_ignore.csv"));
    private readonly Dictionary<string, string> roomMappings;
    private readonly List<string> ignoreEvent;

    private readonly string dreamCatcherPath = Path.Combine(Application.persistentDataPath, "DreamCatcherSpoilerLog.csv");

    //private readonly Stream debugFile = File.Create(@"C:\Users\jimmy\Desktop\debugDC.txt");


    public SocketServer()
    {
      IgnoreExtensions = true;

      // Load the roomMappings and eventsToIgnore.
      string[] stringSeps = new string[] { "\r\n" };
      using (srRoomMapper) this.roomMappings = srRoomMapper
        .ReadToEnd().Split(stringSeps, StringSplitOptions.None).Select(s => s.Split(',')).ToDictionary(x => x[0], x => x[1]);
      using (srEventsToIgnore) this.ignoreEvent = srEventsToIgnore.ReadToEnd().Split(stringSeps, StringSplitOptions.None).ToList();

    }

    public string MakeEvent(string eventName, string eventType, string eventValue, string eventLocation = "") =>
      JsonConvert.SerializeObject(new Dictionary<string, string>() {
        { "event_key", eventName },
        { "event_type", eventType },
        { "event_value", eventValue },
        { "event_location", eventLocation }
    });

    public void Broadcast(string s) => Sessions.Broadcast(s);
    protected override void OnOpen()
    {
      //TextWriterTraceListener myTextListener = new TextWriterTraceListener(this.debugFile);
      //Trace.Listeners.Add(myTextListener);
      Send(MakeEvent("websocket", "websocket_status", "open"));
    }
    protected override void OnError(WebSocketSharp.ErrorEventArgs e) => Send(e.Message);
    public void LoadSave(int _slot) => Send(MakeEvent("game_level_event", "game_level_event", "load_game"));
    public void OnQuit() => Send(MakeEvent("game_level_event", "game_level_event", "quit_game"));

    protected override void OnClose(CloseEventArgs e)
    {
      Send(MakeEvent("websocket", "websocket_status", "closed"));
      ModHooks.Instance.SavegameLoadHook -= LoadSave;
      ModHooks.Instance.SetPlayerBoolHook -= MessageBool;
      ModHooks.Instance.SetPlayerIntHook -= MessageInt;
      ModHooks.Instance.ApplicationQuitHook -= OnQuit;
      On.GameManager.BeginSceneTransition -= ManageTransitions;
      base.OnClose(e);
    }

    protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
    {
      switch (e.Data)
      {
        case "/get-spoiler-log":
          Send(MakeEvent("ping_event", "spoiler", HKItemLocDataDump.GetAndParseSpoilerLog()));
          break;

        case "/get-scene":
          // Uses scenehook.
          UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
          break;

        case "/ping-dreamers":
          // If buying from sly...?  Doesn't work?  ​
          // event_type: "RandomizerMod.Dreamer.Herrah.Leg_Eater"​

          if (PlayerData.instance.monomonDefeated && !messagedMonomon) { Send(MakeEvent("ping_event", "dreamer", "Monomon")); messagedMonomon = true; }
          if (PlayerData.instance.lurienDefeated && !messagedLurien) { Send(MakeEvent("ping_event", "dreamer", "Lurien")); messagedLurien = true; }
          if (PlayerData.instance.hegemolDefeated && !messagedHerrah) { Send(MakeEvent("ping_event", "dreamer", "Herrah")); messagedHerrah = true; }
          break;

        case "/refresh-dc-log":
          var lines = System.IO.File.ReadAllLines(dreamCatcherPath).Where(x => !string.IsNullOrEmpty(x)).ToArray();
          string joinedLines = String.Join(", ", lines.ToArray());
          Send(MakeEvent("ping_event", "found_items", $"[{joinedLines}]"));
          break;

        case "/recreate-dc-log":
          File.Delete(this.dreamCatcherPath);
          File.Create(this.dreamCatcherPath).Close();
          break;

        default:
          if (e.Data.StartsWith("/add-to-dc-log"))
          {
            using (StreamWriter outputFile = new StreamWriter(this.dreamCatcherPath, true))
            { outputFile.WriteLine($"{e.Data.Remove(0, 15)}"); }
          }
          break;
      }
    }

    /// If the API returns a bool, go to MessageBool.  Otherwise, go to MessageInt.
    /// https://radiance.host/apidocs/Hooks.html
    public void MessageBool(string item, bool value)
    {
      try
      {
        if (this.ignoreEvent.Contains(item)) { return; }
        //if (!(item.StartsWith("killed") || item.StartsWith("kills") || item.StartsWith("newData") || item.StartsWith("opened"))) {

        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var lowercaseBool = value ? "true" : "false";
        string roomName = (string)this.roomMappings[scene.name];
        Send(MakeEvent("item", item, lowercaseBool, roomName));
      } catch { return; }
    }

    public void MessageInt(string item, int value)
    {
      try
      {
        if (this.ignoreEvent.Contains(item)) { return; }

        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string roomName = (string)this.roomMappings[scene.name];
        Send(MakeEvent("item", item, value.ToString(), roomName));
      } catch { return; }
    }

    public void ManageTransitions(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
    {
      if (info.SceneName == "GG_Entrance_Cutscene") Send(MakeEvent("game_level_event", "game_level_event", "new_game"));
      // GET CURRENT ROOMNAME HERE.
      orig(self, info);
    }
  }
}