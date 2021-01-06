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
  public class HKItemLocDataDump : Mod, ITogglableMod
  {
    public override int LoadPriority() => 9999;  // TODO: ???
    private readonly WebSocketServer _wss = new WebSocketServer(10051);
    internal static HKItemLocDataDump Instance;

    /// <summary>
    /// Creates and starts up the Websocket Server instance.
    /// </summary>
    public override void Initialize()
    {
      Instance = this;
      Log("Initializing Dreamcatcher HKDataDump...");
      _wss.AddWebSocketService<SocketServer>("/data", ss => {
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
