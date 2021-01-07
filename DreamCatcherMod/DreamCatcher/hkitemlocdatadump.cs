using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using Modding;
using UnityEngine;
using WebSocketSharp.Server;


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
    public static string GetSpoilerLog()
    {
      string userDataPath = System.IO.Path.Combine(Application.persistentDataPath, "RandomizerSpoilerLog.txt");
      return System.IO.File.ReadAllText(userDataPath);
    }

    public static string ParseSpoilerLog(string spoilerText)
    {
      List<string> relevantItems = new List<string>() {
        "Herrah", "Lurien", "Monomon", "Abyss Shriek", "Awoken Dream Nail", "Crystal Heart", "Descending Dark",
        "Desolate Dive", "Dream Gate", "Dream Nail", "Howling Wraiths", "Isma's Tear", "Mantis Claw",
        "Monarch Wings", "Mothwing Cloak", "Shade Cloak", "Shade Soul", "Vengeful Spirit", "City Crest",
        "Collector's Map", "Cyclone Slash", "Dash Slash", "Elegant Key", "Great Slash", "Grimmchild", "King Fragment",
        "King's Brand", "Love Key", "Lumafly Lantern", "Pale Ore-Basin", "Pale Ore-Colosseum", "Pale Ore-Crystal Peak",
        "Pale Ore-Grubs", "Pale Ore-Nosk", "Pale Ore-Seer", "Queen Fragment", "Shopkeeper's Key", "Simple Key-Basin",
        "Simple Key-City", "Simple Key-Lurker", "Simple Key-Sly", "Tram Pass", "Void Heart"
      };
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

      // FORMATTING SPOILER LOG INTO USEFUL FORMAT.
      // Regex Replacements for the Spoiler to make it nice.
      spoilerText = Regex.Replace(spoilerText, @"\r", "", RegexOptions.Multiline); // Weird windows thing.
      spoilerText = Regex.Match(spoilerText, @"ALL ITEMS[\s\S]*", RegexOptions.Multiline).Value;  // Only take the ALL ITEMS part and beyond.
      spoilerText = Regex.Replace(spoilerText, @"ALL ITEMS", "", RegexOptions.Multiline); // Remove ALL ITEMS text.
      spoilerText = Regex.Replace(spoilerText, @" ?\(\d+\) ?", "", RegexOptions.Multiline); // Remove progression values.
      spoilerText = Regex.Replace(spoilerText, @" ?\(Key\) ?", "", RegexOptions.Multiline); // Remove "Key" text.
      spoilerText = Regex.Replace(spoilerText, @" \[.+?\]", "", RegexOptions.Multiline); // Remove cost information.
      spoilerText = Regex.Replace(spoilerText, @"<---at--->.*", "", RegexOptions.Multiline); // Remove "At" and everything after the at.  We know the loc already.
      spoilerText = Regex.Replace(spoilerText, @"SETTINGS[\s\S]*", "", RegexOptions.Multiline); // Remove Settings.

      foreach (KeyValuePair<string, string> kvp in smallAreaToGeneralArea)
      {
        spoilerText = Regex.Replace(spoilerText, kvp.Key, kvp.Value, RegexOptions.Multiline);
      }

      // Splitting up the spoiler so it looks like: "areaname: item1, item2, item3, ..." 
      var spoilerArray = spoilerText.Split(new[] { "\n\n" }, StringSplitOptions.None).ToList();
      spoilerArray = spoilerArray.Where(x => !String.IsNullOrWhiteSpace(x)).ToList(); // take out blank lines.

      Dictionary<string, List<string>> areaItemDict = new Dictionary<string, List<string>>();
      for (var idx = 0; idx < spoilerArray.Count(); idx++)
      {
        var splitstr = spoilerArray[idx].Split('\n').ToList();
        var area_ = splitstr[0].TrimEnd(':');

        if (areaItemDict.ContainsKey(area_)) { continue; }
        else areaItemDict[area_] = new List<string>();

        // Make a list and append to it.
        for (var jdx = idx; jdx < spoilerArray.Count(); jdx++)
        {
          // Check the other areas after the current to see if there are any more when we convert "small" areas to general areas.
          var splitstr2 = spoilerArray[jdx].Split('\n').ToList();
          if (splitstr2[0].TrimEnd(':') != area_) continue;

          var items_ = splitstr2.Skip(1).ToList();
          foreach (string item in items_) if (relevantItems.Contains(item)) areaItemDict[area_].Add(item);
        }
      }

      Dictionary<string, List<string>> areaItemDictCleaned = areaItemDict.Where(x => x.Value.Count > 0).ToDictionary(x => x.Key, x => x.Value);
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
