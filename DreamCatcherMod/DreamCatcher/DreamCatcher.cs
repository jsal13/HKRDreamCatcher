using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Modding;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System;

namespace DreamCatcher
{
  internal class SocketServer : WebSocketBehavior
  {

    private string currentArea = "";
    private bool gotUserSpoiler = false;

    //  Import Json for Scene:Area map.
    private static readonly string p_ = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "room_mappings.json");
    public readonly Dictionary<string, string> roomMappings = JsonConvert.DeserializeObject<LocationJson>(File.ReadAllText(p_)).locations;

    public class LocationJson
    {
      [JsonProperty("locations")]
      public Dictionary<string, string> locations;
    }

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

