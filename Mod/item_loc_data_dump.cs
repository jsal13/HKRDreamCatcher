﻿using Modding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
//using RandomizerMod;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using WebSocketSharp.Server;

namespace DreamCatcher
{
  public class HKItemLocDataDump : Mod, ITogglableMod
  {
    public override int LoadPriority() => 9999;  // TODO: ???
    private readonly WebSocketServer _wss = new WebSocketServer(10051);
    internal static HKItemLocDataDump Instance;

    public override void Initialize()
    {
      Instance = this;

      Log("[...] Initializing Dreamcatcher HKItemLocDataDump Socket.");
      _wss.AddWebSocketService<SocketServer>("/data", ss =>
      {
        ModHooks.Instance.SavegameLoadHook += ss.LoadSave;
        ModHooks.Instance.ApplicationQuitHook += ss.OnQuit;

        ModHooks.Instance.SetPlayerBoolHook += ss.MessageBool;
        ModHooks.Instance.SetPlayerIntHook += ss.MessageInt;
        // UnityEngine.SceneManagement.SceneManager.sceneLoaded += ss.MessageSceneLoaded;
        On.GameManager.BeginSceneTransition += ss.ManageTransitions;
      });

      _wss.Start();
      Log("[OK] Initialized Dreamcatcher HKItemLocDataDump Socket.");
    }
    public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    public static string GetSpoilerLog()
    {
      string userDataPath = System.IO.Path.Combine(Application.persistentDataPath, "RandomizerSpoilerLog.txt");
      return System.IO.File.ReadAllText(userDataPath);
    }

    public static string ParseSpoilerLog(string rawSpoilerText)
    {

      Dictionary<string, string> smallAreaToGeneralArea = new Dictionary<string, string>(){
        {"Ancestral Mound", "Forgotten Crossroads"},
        {"Beast\'s Den", "Deepnest"},
        {"Black Egg Temple", "Forgotten Crossroads"},
        {"Blue Lake", "Resting Grounds"},
        {"Cast Off Shell", "Kingdom\'s Edge"},
        {"Colosseum", "Kingdom's Edge"},
        {"Crystallized Mound", "Crystal Peak"},
        {"Distant Village", "Deepnest"},
        {"Failed Tramway", "Deepnest"},
        {"Fungal Core", "Fungal Wastes"},
        {"Hallownest\'s Crown", "Crystal Peak"},
        {"Iselda", "Dirtmouth"},
        {"Isma\'s Grove", "Royal Waterways"},
        {"Junk Pit", "Royal Waterways"},
        {"King\'s Pass", "Dirtmouth"},
        {"King\'s Station", "City of Tears"},
        {"Lake of Unn", "Greenpath"},
        {"Leg Eater", "Fungal Wastes"},
        {"Mantis Village", "Fungal Wastes"},
        {"Overgrown Mound", "Fog Canyon"},
        {"Palace Grounds", "Ancient Basin"},
        {"Pleasure House", "City of Tears"},
        {"Queen\'s Station", "Fungal Wastes"},
        {"Salubra", "Forgotten Crossroads"},
        {"Sly (Key)", "Dirtmouth"},
        {"Sly", "Dirtmouth"},
        {"Soul Sanctum", "City of Tears"},
        {"Spirit\'s Glade", "Resting Grounds"},
        {"Stag Nest", "Howling Cliffs"},
        {"Stone Sanctuary", "Greenpath"},
        {"Teacher\'s Archives", "Fog Canyon"},
        {"Tower of Love", "City of Tears"},
        {"Weaver\'s Den", "Deepnest"}
      };

      // SPOILER PARSING.
      var spoilerText = Regex.Replace(rawSpoilerText, @"\r", "", RegexOptions.Multiline); // Weird windows thing.
      spoilerText = Regex.Match(spoilerText, @"ALL ITEMS[\s\S]*", RegexOptions.Multiline).Value;  // Only take the ALL ITEMS part and beyond.
      spoilerText = Regex.Replace(spoilerText, @"ALL ITEMS", "", RegexOptions.Multiline); // Remove ALL ITEMS text.
      spoilerText = Regex.Replace(spoilerText, @" ?\(\d+\) ?", "", RegexOptions.Multiline); // Remove progression values.
      spoilerText = Regex.Replace(spoilerText, @" ?\(Key\) ?", "", RegexOptions.Multiline); // Remove "Key" text.
      spoilerText = Regex.Replace(spoilerText, @" \[.+?\]", "", RegexOptions.Multiline); // Remove cost information.
      spoilerText = Regex.Replace(spoilerText, @"<---at--->.*", "", RegexOptions.Multiline); // Remove "At" and everything after the at.  We know the loc already.
      spoilerText = Regex.Replace(spoilerText, @"SETTINGS[\s\S]*", "", RegexOptions.Multiline); // Remove Settings.

      // Replace the specific area with general areas above.
      foreach (KeyValuePair<string, string> kvp in smallAreaToGeneralArea) spoilerText = Regex.Replace(spoilerText, kvp.Key, kvp.Value, RegexOptions.Multiline);

      // Splitting up the spoiler so it looks like: "areaname: item1, item2, item3, ..." 
      var spoilerArray = spoilerText
        .Split(new[] { "\n\n" }, StringSplitOptions.None)
        .Where(x => !String.IsNullOrEmpty(x))
        .ToList();

      // Creates the area-to-items dictionary.
      var acc = 0;
      Dictionary<string, List<string>> areaItemDict = new Dictionary<string, List<string>>();
      foreach (string row in spoilerArray)
      {
        var splitstr = row.Split('\n'); // toList was here before.
        var area_ = splitstr[0].TrimEnd(':');

        // If the area is already in there, don't add it again.
        if (areaItemDict.ContainsKey(area_)) { continue; }
        else areaItemDict[area_] = new List<string>();

        // Go through every string (after the current area) and put in the items that correspond to a row for that area.
        // Note the area rows are NOT unique (there's more than one row with the same area) as we regex'd
        // from specific area to general area above.
        for (var jdx = acc; jdx < spoilerArray.Count(); jdx++)
        {
          var splitstr2 = spoilerArray[jdx].Split('\n'); //.ToList();
          if (splitstr2[0].TrimEnd(':') != area_) continue;

          // It looks like [area, item1, item2, ...] so we skip 1 to exclude area:
          foreach (string item in splitstr2.Skip(1)) areaItemDict[area_].Add(item);
        }
        acc += 1;
      }

      // Sort the areas alphabetically.
      SortedDictionary<string, List<string>> areaItemDictCleaned = new SortedDictionary<string, List<string>>(
        areaItemDict.Where(x => x.Value.Count > 0).ToDictionary(x => x.Key, x => x.Value)
      );
      var areaItemJson = JsonConvert.SerializeObject(areaItemDictCleaned);
      return areaItemJson;
    }

    public static string GetAndParseSpoilerLog()
    // TODO: Should I make the two methods above one?
    {
      var spoilerText = GetSpoilerLog();
      return ParseSpoilerLog(spoilerText);
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
