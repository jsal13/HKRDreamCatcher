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
        ws.onmessage = m => { handleMessage(m.data); }
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
  var tParsed = spoilerText
    .replace(/\r/g, "") // Weird Windows thing.
    .match(/ALL ITEMS[\s\S]*/)[0] // Only look at ALL ITEMS.
    .replace(/ALL ITEMS/, "") // Remove ALL ITEMS heading.
    .replace(/ ?\(\d+\) ?/g, "") // Remove progression numbers.
    .replace(/ ?\(Key\) ?/g, "") // Remove Sly Key items.
    .replace(/ \[.+?\]/g, "") // Only for new spoilerlogs.
    .replace(/<---at--->.*/g, "") // Remove "at" endings.
    .replace(/SETTINGS[\s\S]*/, "") // Remove settings part at the end. 

  //TODO: Okay, this is REAL GROSS.
  const smArea2LgArea = Object.keys(smallAreaToLargeArea)
  for (var idx = 0; idx < smArea2LgArea.length; idx++) {
    var replace = smArea2LgArea[idx];
    var re = new RegExp(replace, "g");

    tParsed = tParsed.replace(re, smallAreaToLargeArea[smArea2LgArea[idx]])
  }
  // END-REAL-GROSS

  tParsed = tParsed.split("\n\n").filter(s => { return s !== "" })

  // This takes a string like "areaname: item1, item2, item3..." and turns it into
  // [areaname, [item1, item2, item3]], and does so for all areas in the above parsed text.  The locations are NOT yet unique.
  window.spoilerMap = tParsed.map(area => {
    const splitArea = area.split(":")
    return [splitArea[0], splitArea[1].split("\n").filter(s => { return s !== "" })]
  })

  window.spoilerMap = window.spoilerMap.reduce((acc, row) => {
    if (typeof acc[row[0]] === "undefined") {
      acc[row[0]] = row[1]
    } else {
      acc[row[0]] = acc[row[0]].concat(row[1])
    }
    return acc
  }, {})

  _filterAndMapAreaItems() // makes window.areaItems
}

function _filterAndMapAreaItems() {
  const itemsToTrack = _makeItemsToTrackArray()
  var areas = Object.keys(window.spoilerMap)

  // Creates {area: [item1, item2, ...], ...}
  window.areaItems = {}
  for (var idx = 0; idx < areas.length; idx++) {

    var area = areas[idx]
    var items = window.spoilerMap[area]
    var importantItemList = items.filter(t => { return itemsToTrack.includes(t) })

    if (importantItemList.length > 0) { window.areaItems[area] = importantItemList }
  }

  plotItemsOnPage()
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
  console.log("RAW: ", m)
  var j = JSON.parse(m)
  if (relevantEvents.includes(Object.keys(j)[0])) {
    console.log(j)
  }
}


// HTML-Side JS
function plotItemsOnPage() {
  var html_ = ""
  const areas = Object.keys(window.areaItems)
  for (var idx = 0; idx < areas.length; idx++) {
    html_ += '<div class="area-pill">'
    var divStyle = `background: ${locData[areas[idx]]['background']}; 
        border: 6px solid ${locData[areas[idx]]['border']};`
    var circleStyle = `background: ${locData[areas[idx]]['border']}; 
        border: 2px solid ${locData[areas[idx]]['border']};`

    html_ += `<div class="loc-div" style="${divStyle}">`
    html_ += `<div class="loc-circle" style="${circleStyle}">`
    html_ += `<div class="loc-title-div"> <span class="loc-title-text">${locData[areas[idx]]["abbr"]}</span></div>`
    html_ += '</div>'
    html_ += '</div>'


    html_ += `<div class="item-div"> <div class="flex-container">`
    for (var jdx = 0; jdx < areaItems[areas[idx]].length; jdx++) {
      html_ += `<div class="tracker-image"><img class="item-image" src="./images/${itemToImageNameMapping[areaItems[areas[idx]][jdx]]}"/></div>`
    }
    html_ += `</div></div >`
    html_ += '</div>'
  }

  $("#tracker-table").append(html_)
}


function initialize() {

  // Create the Checkbox functionality for picking spoiler item types.
  $('input').on('click', function () {
    window.spoilerItemsWanted = [];
    $('.spoiler-items-wanted input:checked').each(function () {
      window.spoilerItemsWanted.push($(this).val());

      // Re-calculate what we need on the list.
    });
    if (typeof window.spoilerMap !== 'undefined') { _filterAndMapAreaItems(window.spoilerMap) }
  });

  // Main method for the JS to initialize the WS.
  // const wsObj = wsFactory()
  // wsObj.connect("ws://localhost:10051/data")
}

$(window).on('load', function () { initialize(); })


// Constants
const locData = {
  'Abyss': {
    background: "#707170",
    border: "#242524",
    abbr: "Abyss",
    displayName: 'Abyss',
  },
  'Ancient Basin': {
    background: "#73747d",
    border: "#282a37",
    abbr: "AnBsn",
    displayName: 'Ancient Basin',
  },
  'City of Tears': {
    background: "#6b89a9",
    border: "#1b4a7b",
    abbr: "CityT",
    displayName: 'City of Tears',
  },
  'Crystal Peak': {
    background: "#b588b0",
    border: "#95568f",
    abbr: "CryPk",
    displayName: 'Crystal Peak',
  },
  'Deepnest': {
    background: "#666b80",
    border: "#141c3c",
    abbr: "DNest",
    displayName: 'Deepnest',
  },
  'Dirtmouth': {
    background: "#787994",
    border: "#2f315b",
    abbr: "Dirtm",
    displayName: 'Dirtmouth',
  },
  'Fog Canyon': {
    background: "#9da3bd",
    border: "#5b6591",
    abbr: "FogCn",
    displayName: 'Fog Canyon',
  },
  'Forgotten Crossroads': {
    background: "#687796",
    border: "#202d5d",
    abbr: "FxRds",
    displayName: 'Forgotten Crossroads',
  },
  'Fungal Wastes': {
    background: "#58747c",
    border: "#113945",
    abbr: "FungW",
    displayName: 'Fungal Wastes',
  },
  'Greenpath': {
    background: "#679487",
    border: "#155b47",
    abbr: "GPath",
    displayName: 'Greenpath',
  },
  'Hive': {
    background: "#C17F6E",
    border: "#A64830",
    abbr: "Hive",
    displayName: 'Hive',
  },
  'Howling Cliffs': {
    background: "#75809a",
    border: "#3b4a6f",
    abbr: "HClif",
    displayName: 'Howling Cliffs',
  },
  'Kingdom\'s Edge': {
    background: "#768384",
    border: "#3c4e50",
    abbr: "KEdge",
    displayName: 'Kingdom\'s Edge',
  },
  'Queen\'s Gardens': {
    background: "#559f9d",
    border: "#0d7673",
    abbr: "QGdn",
    displayName: 'Queen\'s Garden',
  },
  'Resting Grounds': {
    background: "#84799d",
    border: "#423169",
    abbr: "RestG",
    displayName: 'Resting Grounds',
  },
  'Royal Waterways': {
    background: "#6d919d",
    border: "#1e5669",
    abbr: "RWatr",
    displayName: 'Royal Waterways',
  },
}

//TODO: some number of events need revamp: hasDesolateDive?, 
// Remember to also have "scene" for the scene processing.
const relevantEvents = [
  'scene',
  'simpleKeys',
  'ore',
  'hasDash',
  'hasWallJump',
  'hasSuperDash',
  'hasShadowDash',
  'hasAcidArmour',
  'hasDoubleJump',
  'hasLantern',
  'hasTramPass',
  'hasCityKey',
  'hasSlykey',
  'hasWhiteKey',
  'hasMenderKey',
  'hasWaterwaysKey',
  'hasSpaKey',
  'hasLoveKey',
  'hasKingsBrand',
  'fireballLevel',
  'quakeLevel',
  'screamLevel',
  'hasCyclone',
  'hasDashSlash',
  'hasUpwardSlash',
  'hasDreamNail',
  'hasDreamGate',
  'dreamNailUpgraded',
  'royalCharmState',
  'gotShadeCharm',
]

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

const smallAreaToLargeArea = {
  "Ancestral Mound": "Forgotten Crossroads",
  "Beast\'s Den": "Deepnest",
  "Black Egg Temple": "Forgotten Crossroads",
  "Blue Lake": "Resting Grounds",
  "Cast Off Shell": "Kingdom\'s Edge",
  "Colosseum": "Kingdom's Edge",
  "Crystallized Mound": "Crystal Peak",
  "Distant Village": "Deepnest",
  "Failed Tramway": "Deepnest",
  "Fungal Core": "Fungal Wastes",
  "Hallownest\'s Crown": "Crystal Peak",
  "Iselda": "Dirtmouth",
  "Isma\'s Grove": "Royal Waterways",
  "Junk Pit": "Royal Waterways",
  "King\'s Pass": "Dirtmouth",
  "King\'s Station": "City of Tears",
  "Lake of Unn": "Greenpath",
  "Leg Eater": "Fungal Wastes",
  "Mantis Village": "Fungal Wastes",
  "Overgrown Mound": "Fog Canyon",
  "Palace Grounds": "Ancient Basin",
  "Pleasure House": "City of Tears",
  "Queen\'s Station": "Fungal Wastes",
  "Salubra": "Forgotten Crossroads",
  "Sly (Key)": "Dirtmouth",
  "Sly": "Dirtmouth",
  "Soul Sanctum": "City of Tears",
  "Spirit\'s Glade": "Resting Grounds",
  "Stag Nest": "Howling Cliffs",
  "Stone Sanctuary": "Greenpath",
  "Teacher\'s Archives": "Fog Canyon",
  "Tower of Love": "City of Tears",
  "Weaver\'s Den": "Deepnest",
}