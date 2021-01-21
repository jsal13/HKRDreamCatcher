//TODOS:
// 2. White pieces?  What is this?
// 3. How do we do ore + keys?
// 4. Is there a "set state" we could work with?
// 5. Keep checking names with mini runs.
// 6. Condense datatypes.  We almost certainly don't need all these things.
// Double abilities do not work; kill them off.

// ===============================
// WEBSOCKET SERVER AND PAGE INIT 
// ===============================

function wsConnect() {
  ws = new WebSocket("ws://localhost:10051/data")
  ws.onmessage = m => { handleMessage(m.data); }
  ws.onerror = e => { console.log("[ERROR] Cannot connect to websocket.  Is Hollow Knight running?"); };
  ws.onclose = () => { setTimeout(() => { wsConnect(); }, 1000) };
}

$(window).on('load', function () {
  console.log("[OK] Connecting to websocket...")
  wsConnect();
})


// ================
// MESSAGE HANDLING
// ================

function handleMessage(m) {
  // Handles incoming messages depending on the first key sent.
  try {
    var message = JSON.parse(m)
    console.log(message)

    var messageType = Object.keys(message)[0]
    var messageVal = message[messageType]

    switch (messageType) {
      // TODO: This is so gross.  Change the backend pls.
      case "spoiler":
        plotItemsOnPage(messageVal)
        break;

      case "found-items":
        const foundItemArray = messageVal
        for (var idx = 0; idx < foundItemArray.length; idx++) {
          // Send a ping to the server to dim the item, but don't send an "add-to-log".
          dimItemFound(foundItemArray[idx]['item'], foundItemArray[idx]['current_area'])
        }
        break;

      case "event":  // TODO: Should make a thing to handle events...
        if (messageVal === "load_save") {
          console.log("[OK] Loading spoilers...")
          ws.send("/get-spoiler-log")

        }
        else if (messageVal === "new_game") {
          console.log("[OK] New Game Detected...")
          console.log("[OK] Removing old DreamCatcher Log...")
          ws.send("/recreate-dc-log")
          console.log("[OK] Loading spoilers...")
          ws.send("/get-spoiler-log")

        } else if (messageVal === "websocket_open") {
          console.log("[OK] Websocket connected.")
          console.log("[OK] Loading spoilers...")
          ws.send("/get-spoiler-log")

          intervalTasks = window.setInterval(() => {
            try {
              ws.send("/ping-dreamers")
              ws.send("/refresh-dc-log")
              ws.send("/get-scene")
            } catch (DOMException) {
              clearInterval(intervalTasks)
            }
          }, 10000)

        } else if (messageVal === "websocket_closed") {
          clearInterval(intervalGetScene)
          clearInterval(intervalPingDreamers)
          clearInterval(intervalRefreshItems)
        }
        break;

      case "scene": // TODO: Do I need to send these?
        break;

      case "dreamer": // Combine this with items below.
        ws.send(`/add-to-dc-log {"item": "${messageVal}", "current_area": "${message["current_area"]}"}`)
        dimItemFound(messageVal, message["current_area"])
        break;

      case "item": // we parse the item...
        if (eventsToTrack.includes(messageVal) && message["current_area"] !== '') {
          ws.send(`/add-to-dc-log {"item": "${messageVal}", "current_area": "${message["current_area"]}"}`)
          dimItemFound(messageVal, message["current_area"])
        }
        break;

      default:
        break;
    }
  } catch (e) {
    console.log("[ERROR] Didn't parse:", m, e)
  }
}

// ===============
// HTML FUNCTIONS
// ===============

function makeDivCSS(area) {
  var divStyle = `background: ${locData[area]['background']}; 
        border: 6px solid ${locData[area]['border']};`
  var circleStyle = `background: ${locData[area]['border']}; 
        border: 2px solid ${locData[area]['border']};`
  return [divStyle, circleStyle]
}

function plotItemsOnPage(areaItems) {
  $("#tracker-table").empty()

  var html_ = ""
  const areas = Object.keys(areaItems)
  for (var idx = 0; idx < areas.length; idx++) {
    const thisAreaItems = areaItems[areas[idx]]

    // All the image tags for the tracker.
    const [divStyle, circleStyle] = makeDivCSS(areas[idx])
    var trackerImages_ = ""
    for (var jdx = 0; jdx < thisAreaItems.length; jdx++) {
      var _event = itemToBaseEvent[thisAreaItems[jdx]];
      if (eventsToTrack.includes(_event)) {
        trackerImages_ += `
        <div class="tracker-image">
          <img class="item-image ${_event}_${locData[areas[idx]]["display"]}" src="./images/${eventToBaseItem[_event]}.png"/>
        </div>`
      }
    }

    if (trackerImages_ !== "") {
      html_ += `
    <div class="area-pill">
      <div class="area-pill-inner" style="${divStyle}">
        <div class="area-pill-rounded-edge" style="${circleStyle}">
          <div class="area-pill-area-title-div">
            <span class="area-pill-area-title-text">${locData[areas[idx]]["abbr"]}</span>
          </div>
        </div>
        <div class="pill-items-container">
          ${trackerImages_}
        </div>
      </div>
    </div>`
    }
  }
  $("#tracker-table").append(html_)
}

function dimItemFound(itemEvent, locWithUnderscores) {
  // Due to the 10 second lag and the uniqueness of the dreamers, we should just look for their name.
  console.log("Okay, dimming:", itemEvent, locWithUnderscores)
  var isDreamerItem = ["Monomon", "Lurien", "Herrah"].includes(itemEvent)
  var _loc = isDreamerItem ? "" : `_${locWithUnderscores}`

  const selector = $(`img[class*="${itemEvent}${_loc}"]:not(.item-found)`)
  selector.first().addClass("item-found")

}

// =========
// CONSTANTS
// =========
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


// Collector's map?  
// This goes to the base item.

// Ore, Simplekeys is weird.  Ore has a += 1 sort of thing going on when you collect it, so it has to be treated differently.
eventToBaseItem = {
  'scene': '',
  'Herrah': 'Herrah',
  'Monomon': 'Monomon',
  'Lurien': 'Lurien',
  'simpleKeys': "Simple_Key",
  'ore': "Ore",
  'hasLantern': "Lumafly Lantern",
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
  'hasDashSlash': "Great_Slash", // [sic]
  'hasUpwardSlash': "Dash_Slash",
  'hasDreamNail': "Dream_Nail",
  'hasDreamGate': "Dream_Nail",
  'dreamNailUpgraded': "Dream_Nail",
  'hasPinGrub': "Grub",
  // 'gotCharm_25': 'Unbreakable_Strength',
  'gotCharm_19': 'Shaman_Stone',
  'gotCharm_20': 'Soul_Catcher',
  'gotCharm_31': 'Dashmaster',
}

//Here we make it so that the tracker shows the base version of the spell.
// This parses the spoiler.
itemToBaseEvent = {
  "Abyss Shriek": "hasHowlingWraiths",
  "Awoken Dream Nail": "hasDreamNail",
  // "City Crest": "City_Crest",
  "Collector's Map": "hasPinGrub",
  "Crystal Heart": "hasSuperDash",
  "Cyclone Slash": "hasCyclone",
  "Dash Slash": "hasUpwardSlash",
  "Descending Dark": "hasDesolateDive",
  "Desolate Dive": "hasDesolateDive",
  "Dream Gate": "hasDreamNail",
  "Dream Nail": "hasDreamNail",
  "Elegant Key": "hasWhiteKey",
  "Great Slash": "hasDashSlash",  // [sic]
  // "Grimmchild": "Grimmchild",
  "Herrah": "Herrah",
  "Howling Wraiths": "hasHowlingWraiths",
  "Isma's Tear": "hasAcidArmour",
  // "King Fragment": "Charm_KingSoul_Left",
  "King's Brand": "hasKingsBrand",
  "Love Key": "hasLoveKey",
  "Lumafly Lantern": "hasLantern",
  "Lurien": "Lurien",
  "Mantis Claw": "hasWalljump",
  "Monarch Wings": "hasDoubleJump",
  "Monomon": "Monomon",
  "Mothwing Cloak": "hasDash",
  "Pale Ore-Basin": "Pale_Ore",
  "Pale Ore-Colosseum": "Pale_Ore",
  "Pale Ore-Crystal Peak": "Pale_Ore",
  "Pale Ore-Grubs": "Pale_Ore",
  "Pale Ore-Nosk": "Pale_Ore",
  "Pale Ore-Seer": "Pale_Ore",
  // "Queen Fragment": "Charm_KingSoul_Right",
  "Shade Cloak": "hasDash",
  "Shade Soul": "hasVengefulSpirit",
  // "Shopkeeper's Key": "hasSlykey",
  // // "Simple Key-Basin": "Simple_Key",
  // // "Simple Key-City": "Simple_Key",
  // // "Simple Key-Lurker": "Simple_Key",
  // // "Simple Key-Sly": "Simple_Key",
  "Tram Pass": "hasTramPass",
  "Vengeful Spirit": "hasVengefulSpirit",
  // "Void Heart": "Void_Heart",
  // 'Unbreakable Strength': 'gotCharm_25',
  'Shaman Stone': 'gotCharm_19',
  'Soul Catcher': 'gotCharm_20',
  'Dashmaster': 'gotCharm_31',
}

itemsToTrack = [
  "Herrah",
  "Lurien",
  "Monomon",
  "Collector's Map",
  "Abyss Shriek",
  "Awoken Dream Nail",
  "Crystal Heart",
  "Descending Dark",
  "Desolate Dive",
  "Dream Gate",
  "Dream Nail",
  "Howling Wraiths",
  "Isma's Tear",
  "Lumafly Lantern",
  "Mantis Claw",
  "Monarch Wings",
  "Mothwing Cloak",
  "Shade Cloak",
  "Shade Soul",
  "Vengeful Spirit",
  'Shaman Stone',
  'Soul Catcher',
  'Dashmaster',
  "Tram Pass",
]

eventsToTrack = itemsToTrack.map(x => itemToBaseEvent[x])




// const dreamers = [
//   "Herrah",
//   "Lurien",
//   "Monomon",
// ]

// const majorItems = [
//   "Collector's Map",
//   "Abyss Shriek",
//   "Awoken Dream Nail",
//   "Crystal Heart",
//   "Descending Dark",
//   "Desolate Dive",
//   "Dream Gate",
//   "Dream Nail",
//   "Howling Wraiths",
//   "Isma's Tear",
//   "Lumafly Lantern",
//   "Mantis Claw",
//   "Monarch Wings",
//   "Mothwing Cloak",
//   "Shade Cloak",
//   "Shade Soul",
//   "Vengeful Spirit",
// ]

// const minorItems = ["City Crest", "Collector's Map", "Cyclone Slash", "Dash Slash", "Elegant Key", "Great Slash", "Grimmchild", "King Fragment", "King's Brand", "Love Key", "Pale Ore-Basin", "Pale Ore-Colosseum", "Pale Ore-Crystal Peak", "Pale Ore-Grubs", "Pale Ore-Nosk", "Pale Ore-Seer", "Queen Fragment", "Shopkeeper's Key", "Simple Key-Basin", "Simple Key-City", "Simple Key-Lurker", "Simple Key-Sly", "Tram Pass", "Void Heart",]