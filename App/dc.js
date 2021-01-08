//TODOS:
// 1. How do we do dreamers?
// 2. White pieces?  What is this?
// 3. How do we do ore + keys?
// 4. Is there a "set state" we could work with?
// 5. Keep checking names with mini runs.
// 6. Condense datatypes.  We almost certainly don't need all these things.

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
          console.log("Getting spoiler...")
          ws.send("/getspoiler")
        };
        ws.onmessage = m => { handleMessage(m.data); }
      });
    }
  }
};

function handleMessage(m) {
  var j = JSON.parse(m)
  console.log("OKAY OKAY: ", j)

  if (relevantEvents.includes(j["item"])) {
    handleItemGetEvent(j["item"], j["value"], j["current_area"])
  } else if (Object.keys(j)[0] === "spoiler") {
    window.areaItems = j["spoiler"]
    plotItemsOnPage()
  }
}

function parseItemName(item) {
  // Remove apostrophes and spaces->underscores.
  return item.replace(/ /g, '_').replace(/'/g, '')
}

function handleItemGetEvent(itemEvent, value, current_area) {
  console.log(itemEvent, value, current_area)
  var item_ = parseItemName(eventToItem[itemEvent])
  const annoyingItems = ["Ore", "Simple_Key"]

  if (annoyingItems.includes(item_)) {
    console.log(`Current value of ${item_} is ${value}.`)
  } else {
    dimItemFound(item_, current_area)
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

    // All the image tags for the tracker.
    var trackerImages_ = ""
    for (var jdx = 0; jdx < areaItems[areas[idx]].length; jdx++) {
      var itemCleanName_ = itemNameAlphaUnderscore[areaItems[areas[idx]][jdx]];
      trackerImages_ += `
        <div class="tracker-image">
          <img class="item-image ${itemCleanName_}_${locData[areas[idx]]["display"]}" src="./images/${itemCleanName_}.png"/>
        </div>`
    }

    html_ += `
    <div class="area-pill-inner" style="${divStyle}">
      <div class="area-pill-rounded-edge" style="${circleStyle}">
        <div class="area-pill-area-title-div">
          <span class="area-pill-area-title-text">${locData[areas[idx]]["abbr"]}</span>
        </div>
      </div>
      <div class="pill-items-container">
        ${trackerImages_}
      </div>
    </div>`
    html_ += `</div>` // Closes <div class="area-pill">
  }
  $("#tracker-table").append(html_)
}


function dimItemFound(item, locWithUnderscores) {
  // Item alpha undercored, locWithUnderscores.
  console.log(`Dimming ${item} at ${locWithUnderscores}.`)
  $(`img[class~="${item}_${locWithUnderscores}"]:not(.item-found)`).first().addClass("item-found")
}

function initialize() {
  // Main method for the JS to initialize the WS.
  const wsObj = wsFactory()
  wsObj.connect("ws://localhost:10051/data")
}

$(window).on('load', function () { initialize(); })

// Constants
const locData = {
  'Abyss': { background: "#707170", border: "#242524", abbr: "Abyss", display: 'Abyss' },
  'Ancient Basin': { background: "#73747d", border: "#282a37", abbr: "AnBsn", display: 'Ancient_Basin' },
  'City of Tears': { background: "#6b89a9", border: "#1b4a7b", abbr: "CityT", display: 'City_of_Tears' },
  'Crystal Peak': { background: "#b588b0", border: "#95568f", abbr: "CryPk", display: 'Crystal_Peak' },
  'Deepnest': { background: "#666b80", border: "#141c3c", abbr: "DNest", display: 'Deepnest' },
  'Dirtmouth': { background: "#787994", border: "#2f315b", abbr: "Dirtm", display: 'Dirtmouth' },
  'Fog Canyon': { background: "#9da3bd", border: "#5b6591", abbr: "FogCn", display: 'Fog_Canyon' },
  'Forgotten Crossroads': {
    background: "#687796", border: "#202d5d", abbr: "XRoad", display: 'Forgotten_Crossroads'
  },
  'Fungal Wastes': { background: "#58747c", border: "#113945", abbr: "FungW", display: 'Fungal_Wastes' },
  'Greenpath': { background: "#679487", border: "#155b47", abbr: "GPath", display: 'Greenpath' },
  'Hive': { background: "#C17F6E", border: "#A64830", abbr: "Hive", display: 'Hive' },
  'Howling Cliffs': { background: "#75809a", border: "#3b4a6f", abbr: "HClif", display: 'Howling_Cliffs' },
  'Kingdom\'s Edge': { background: "#768384", border: "#3c4e50", abbr: "KEdge", display: 'Kingdoms_Edge' },
  'Queen\'s Gardens': { background: "#559f9d", border: "#0d7673", abbr: "QGdn", display: 'Queens_Gardens' },
  'Resting Grounds': { background: "#84799d", border: "#423169", abbr: "RestG", display: 'Resting_Grounds' },
  'Royal Waterways': { background: "#6d919d", border: "#1e5669", abbr: "RWatr", display: 'Royal_Waterways' },
}

// Remember to also have "scene" for the scene processing.

// Collector's map?
const eventToItemData = {
  'scene': '',
  'simpleKeys': "Simple_Key",
  'ore': "Ore",
  'hasDash': "Mothwing_Cloak",
  'hasWalljump': "Mantis_Claw",
  'hasSuperDash': "Crystal_Heart",
  'hasShadowDash': "Mothwing_Cloak",
  'hasAcidArmour': "Ismas_Tear",
  'hasDoubleJump': "Monarch_Wings",
  'hasHowlingWraiths': "Howling_Wraiths",
  'hasAbyssShriek': "Howling_Wraiths",
  'hasDesolateDive': "Desolate_Dive",
  'hasDescendingDark': "Desolate_Dive",
  'hasLantern': "Lumafly_Lantern",
  'hasTramPass': "Tram_Pass",
  'hasCityKey': "City_Crest",
  'hasSlykey': "Shopkeeper_Key",
  'hasWhiteKey': "Elegant_Key",
  'hasLoveKey': "Love_Key",
  'hasKingsBrand': "Kings_Brand",
  'hasVengefulSpirit': "Vengeful_Spirit",
  'hasShadeSoul': "Vengeful_Spirit",
  'hasCyclone': "Cyclone_Slash",
  'hasDashSlash': "Dash_Slash", // wait what?
  'hasUpwardSlash': "Dash_Slash", // why is this dash slash?
  'hasDreamNail': "Dream_Nail",
  'hasDreamGate': "Dream_Nail",
  'dreamNailUpgraded': "Dream_Nail",
  'gotCharm36': "" // what is this?  white piece?
}


// Here we make it so that the tracker shows the base version of the spell.
const itemNameAlphaUnderscore = {
  "Abyss Shriek": "Howling_Wraiths",
  "Awoken Dream Nail": "Dream_Nail",
  "City Crest": "City_Crest",
  "Collector's Map": "Grub",
  "Crystal Heart": "Crystal_Heart",
  "Cyclone Slash": "Cyclone_Slash",
  "Dash Slash": "Dash_Slash",
  "Descending Dark": "Desolate_Dive",
  "Desolate Dive": "Desolate_Dive",
  "Dream Gate": "Dream_Gate",
  "Dream Nail": "Dream_Nail",
  "Elegant Key": "Elegant_Key",
  "Great Slash": "Great_Slash",
  "Grimmchild": "Grimmchild",
  "Herrah": "Herrah",
  "Howling Wraiths": "Howling_Wraiths",
  "Isma's Tear": "Ismas_Tear",
  "King Fragment": "Charm_KingSoul_Left",
  "King's Brand": "Kings_Brand",
  "Love Key": "Love_Key",
  "Lumafly Lantern": "Lumafly_Lantern",
  "Lurien": "Lurien",
  "Mantis Claw": "Mantis_Claw",
  "Monarch Wings": "Monarch_Wings",
  "Monomon": "Monomon",
  "Mothwing Cloak": "Mothwing_Cloak",
  "Pale Ore-Basin": "Pale_Ore",
  "Pale Ore-Colosseum": "Pale_Ore",
  "Pale Ore-Crystal Peak": "Pale_Ore",
  "Pale Ore-Grubs": "Pale_Ore",
  "Pale Ore-Nosk": "Pale_Ore",
  "Pale Ore-Seer": "Pale_Ore",
  "Queen Fragment": "Charm_KingSoul_Right",
  "Shade Cloak": "Mothwing_Cloak",
  "Shade Soul": "Vengeful_Spirit",
  "Shopkeeper's Key": "Shopkeepers_Key",
  "Simple Key-Basin": "Simple_Key",
  "Simple Key-City": "Simple_Key",
  "Simple Key-Lurker": "Simple_Key",
  "Simple Key-Sly": "Simple_Key",
  "Tram Pass": "Tram_Pass",
  "Vengeful Spirit": "Vengeful_Spirit",
  "Void Heart": "Void_Heart",
}


// function _makeItemsToTrackArray() {
//   var itemsToTrack = []
//   if (window.spoilerItemsWanted.includes("dreamers")) { itemsToTrack = itemsToTrack.concat(dreamers) }
//   if (window.spoilerItemsWanted.includes("majorItems")) { itemsToTrack = itemsToTrack.concat(majorItems) }
//   if (window.spoilerItemsWanted.includes("minorItems")) { itemsToTrack = itemsToTrack.concat(minorItems) }
//   return itemsToTrack;
// }

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

const minorItems = ["City Crest", "Collector's Map", "Cyclone Slash", "Dash Slash", "Elegant Key", "Great Slash", "Grimmchild", "King Fragment", "King's Brand", "Love Key", "Lumafly Lantern", "Pale Ore-Basin", "Pale Ore-Colosseum", "Pale Ore-Crystal Peak", "Pale Ore-Grubs", "Pale Ore-Nosk", "Pale Ore-Seer", "Queen Fragment", "Shopkeeper's Key", "Simple Key-Basin", "Simple Key-City", "Simple Key-Lurker", "Simple Key-Sly", "Tram Pass", "Void Heart",]


