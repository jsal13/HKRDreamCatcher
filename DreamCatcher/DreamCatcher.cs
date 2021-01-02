using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    protected override void OnMessage(MessageEventArgs e) => Send(e.Data);
    protected override void OnError(ErrorEventArgs e) => Send(e.Message);


    /// If the API returns a bool, go to MessageBool.  Otherwise, go to MessageInt.
    /// https://radiance.host/apidocs/Hooks.html
    public void MessageBool(string item, bool value) => Send($"{item}: {value}");
    public void MessageInt(string item, int value) => Send($"{item}: {value}");
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

