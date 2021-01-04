function wsFactory() {
  return {
    tryCount: 3,
    connect: function (url) {
      var ctx = this,
        ws = new WebSocket(url);

      return new Promise(function (v, x) {
        ws.onerror = e => {
          console.log(`WS connection attempt ${4 - ctx.tryCount} -> Unsuccessful`);
          e.target.readyState === 3 && --ctx.tryCount;
          if (ctx.tryCount > 0) setTimeout(() => v(ctx.connect(url)), 1000);
          else x(new Error("3 unsuccessfull connection attempts"));
        };
        ws.onopen = e => {
          console.log(`WS connection Status: ${e.target.readyState}`);
          v(ws);
        };
        ws.onmessage = m => { handleMessage(m.data); console.log(m) }
      });
    }
  }
};

function handleFileRead(input) {
  const file_ = input.files[0];
  const reader = new FileReader();
  reader.readAsText(file_);
  reader.onload = function () {
    parseSpoilerLog(reader.result, window.spoilerItemsWanted);
  };
  reader.onerror = function () {
    console.log(reader.error);
  };
}

function parseSpoilerLog(spoilerText) {
  const tParsed = spoilerText
    .replace(/\r/g, "") // Weird Windows thing.
    .match(/ALL ITEMS[\s\S]*/)[0] // Only look at ALL ITEMS.
    .replace(/ALL ITEMS/, "") // Remove ALL ITEMS heading.
    .replace(/\(\d+\) /g, "") // Remove progression numbers.
    .replace(/ \[.+?\]/g, "") // Only for new spoilerlogs.
    .replace(/<---at--->.*/g, "") // Remove "at" endings.
    .replace(/SETTINGS[\s\S]*/, "") // Remove settings part at the end. 
    .split("\n\n").filter(s => { return s !== "" })

  // This takes a string like "areaname: item1, item2, item3..." and turns it into
  // [areaname, [item1, item2, item3]], and does so for all areas in the above parsed text.
  window.spoilerMap = tParsed.map(area => {
    const splitArea = area.split(":")
    return [splitArea[0], splitArea[1].split("\n").filter(s => { return s !== "" })]
  })
  _filterAndMapAreaItems() // makes window.areaItems
  console.log(window.areaItems)
}

function _filterAndMapAreaItems() {
  const itemsToTrack = _makeItemsToTrackArray()

  // Creates {area: [item1, item2, ...], ...}
  window.areaItems = {}
  for (var idx = 0; idx < window.spoilerMap.length; idx++) {
    var importantItemList = window.spoilerMap[idx][1].filter(t => { return itemsToTrack.includes(t) })
    if (importantItemList.length > 0) { window.areaItems[window.spoilerMap[idx][0]] = importantItemList }
  }

  plotItemsOnPage() // Done on the HTML side.
}

function _isSpoilerUploaded() {
  return typeof lastname !== "undefined"
}

function _makeItemsToTrackArray() {
  var itemsToTrack = []
  if (window.spoilerItemsWanted.includes("dreamers")) { itemsToTrack = itemsToTrack.concat(dreamers) }
  if (window.spoilerItemsWanted.includes("majorItems")) { itemsToTrack = itemsToTrack.concat(majorItems) }
  if (window.spoilerItemsWanted.includes("minorItems")) { itemsToTrack = itemsToTrack.concat(minorItems) }
  return itemsToTrack;
}

function handleMessage(m) {
  return 0
}

// Make this depend on the things we're doing.

const dreamers = [
  "Herrah",
  "Lurien",
  "Monomon",
]

const majorItems = [
  "Abyss Shriek",
  "Awoken Dream Nail",
  "Crystal Heart",
  "Descending Dark",
  "Desolate Dive",
  "Dream Gate",
  "Dream Nail",
  "Howling Wraiths",
  "Isma's Tear",
  "Mantis Claw",
  "Monarch Wings",
  "Mothwing Cloak",
  "Shade Cloak",
  "Shade Soul",
  "Vengeful Spirit",
]


const minorItems = [
  "City Crest",
  "Collector's Map",
  "Cyclone Slash",
  "Dash Slash",
  "Elegant Key",
  "Great Slash",
  "Grimmchild",
  "King Fragment",
  "King's Brand",
  "Love Key",
  "Lumafly Lantern",
  "Pale Ore-Basin",
  "Pale Ore-Colosseum",
  "Pale Ore-Crystal Peak",
  "Pale Ore-Grubs",
  "Pale Ore-Nosk",
  "Pale Ore-Seer",
  "Queen Fragment",
  "Shopkeeper's Key",
  "Simple Key-Basin",
  "Simple Key-City",
  "Simple Key-Lurker",
  "Simple Key-Sly",
  "Tram Pass",
  "Void Heart",
]

const itemToImageNameMapping = {
  "Abyss Shriek": "Abyss_Shriek.png",
  "Awoken Dream Nail": "Awoken_Dream_Nail.png",
  "City Crest": "City_Crest.png",
  "Collector's Map": "Map.png",
  "Crystal Heart": "Crystal_Heart.png",
  "Cyclone Slash": "Cyclone_Slash.png",
  "Dash Slash": "Dash_Slash.png",
  "Descending Dark": "Descending_Dark.png",
  "Desolate Dive": "Desolate_Dive.png",
  "Dream Gate": "Dream_Gate.png",
  "Dream Nail": "Dream_Nail.png",
  "Elegant Key": "Elegant_Key.png",
  "Great Slash": "Great_Slash.png",
  "Grimmchild": "Grimmchild.png",
  "Herrah": "Herrah.png",
  "Howling Wraiths": "Howling_Wraiths.png",
  "Isma's Tear": "Ismas_Tear.png",
  "King Fragment": "Charm_KingSoul_Left.png",
  "King's Brand": "Kings_Brand.png",
  "Love Key": "Love_Key.png",
  "Lumafly Lantern": "Lumafly_Lantern.png",
  "Lurien": "Lurien.png",
  "Mantis Claw": "Mantis_Claw.png",
  "Monarch Wings": "Monarch_Wings.png",
  "Monomon": "Monomon.png",
  "Mothwing Cloak": "Mothwing_Cloak.png",
  "Pale Ore-Basin": "Pale_Ore.png",
  "Pale Ore-Colosseum": "Pale_Ore.png",
  "Pale Ore-Crystal Peak": "Pale_Ore.png",
  "Pale Ore-Grubs": "Pale_Ore.png",
  "Pale Ore-Nosk": "Pale_Ore.png",
  "Pale Ore-Seer": "Pale_Ore.png",
  "Queen Fragment": "Charm_KingSoul_Right.png",
  "Shade Cloak": "Shade_Cloak.png",
  "Shade Soul": "Shade_Soul.png",
  "Shopkeeper's Key": "Shopkeepers_Key.png",
  "Simple Key-Basin": "Simple_Key.png",
  "Simple Key-City": "Simple_Key.png",
  "Simple Key-Lurker": "Simple_Key.png",
  "Simple Key-Sly": "Simple_Key.png",
  "Tram Pass": "Tram_Pass.png",
  "Vengeful Spirit": "Vengeful_Spirit.png",
  "Void Heart": "Void_Heart.png",

}




function initialize() {
  // Main method for the JS to initialize the WS.
  // const wsObj = wsFactory()
  // wsObj.connect("ws://localhost:10051/data")
  //   .then(m => { console.log(m) })
  //   .catch(console.log);
}

initialize();