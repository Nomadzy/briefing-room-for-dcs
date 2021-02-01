briefingRoom.mission.features.friendlyTransportHelicopter = { }
briefingRoom.mission.features.friendlyTransportHelicopter.activated = false -- was the transport helo activated?
briefingRoom.mission.features.friendlyTransportHelicopter.MARKER_NAME = "helicopter"
briefingRoom.mission.features.friendlyTransportHelicopter.markID = nil -- ID of the mark on the map

-- Activate aircraft and launch bombing run
function briefingRoom.mission.features.friendlyTransportHelicopter.goToCoordinates()
  briefingRoom.radioManager.play("XXXXXXXXXXXX", "Radio0")

  local transportHeloGroup = dcsExtensions.getGroupByID(briefingRoom.mission.featuresUnitGroups.FriendlyTransportHelicopter[1])
  if transportHeloGroup == nil then return end -- no group (destroyed? missing?), no radio answer

  briefingRoom.radioManager.play("Copy, on my way to the LZ.", "RadioOtherPilotOnMyWayToLZ", briefingRoom.radioManager.getAnswerDelay())

  if not briefingRoom.mission.features.friendlyTransportHelicopter.activated then
    transportHeloGroup:activate()
    briefingRoom.mission.features.friendlyTransportHelicopter.activated = true
  end
end

function briefingRoom.mission.features.friendlyTransportHelicopter:onEvent(event)
  if event.id == world.event.S_EVENT_MARK_REMOVED then
    if briefingRoom.mission.features.friendlyTransportHelicopter.markID ~= nil and event.idx == briefingRoom.mission.features.friendlyTransportHelicopter.markID then
      if not briefingRoom.mission.features.friendlyTransportHelicopter.disableCooRemovedRadioMessage then
        briefingRoom.radioManager.play("Affirm, coordinates discarded. Awaiting new coordinates", "RadioCoordinatesDiscardedF")
      end
      briefingRoom.mission.features.friendlyTransportHelicopter.markID = nil
    end
  elseif event.id == world.event.S_EVENT_MARK_ADDED then
    local markText = string.lower(tostring(event.text or ""))
    if markText == briefingRoom.mission.features.friendlyTransportHelicopter.MARKER_NAME then
      if briefingRoom.mission.features.friendlyTransportHelicopter.markID ~= nil then
        briefingRoom.mission.features.friendlyTransportHelicopter.disableCooRemovedRadioMessage = true
        trigger.action.removeMark(briefingRoom.mission.features.friendlyTransportHelicopter.markID)
        briefingRoom.mission.features.friendlyTransportHelicopter.disableCooRemovedRadioMessage = false
      end
      briefingRoom.mission.features.friendlyTransportHelicopter.markID = event.idx
      briefingRoom.radioManager.play("Copy, coordinates updated.", "RadioCoordinatesUpdatedF")
      return
    end
  elseif event.id == world.event.S_EVENT_MARK_CHANGE then
    local markText = string.lower(tostring(event.text or ""))

    if markText == briefingRoom.mission.features.friendlyTransportHelicopter.MARKER_NAME then
      briefingRoom.radioManager.play("Copy, coordinates updated.", "RadioCoordinatesUpdatedF")
      if briefingRoom.mission.features.friendlyTransportHelicopter.markID ~= nil then
        briefingRoom.mission.features.friendlyTransportHelicopter.disableCooRemovedRadioMessage = true
        trigger.action.removeMark(briefingRoom.mission.features.friendlyTransportHelicopter.markID)
        briefingRoom.mission.features.friendlyTransportHelicopter.disableCooRemovedRadioMessage = false
      end
      briefingRoom.mission.features.friendlyTransportHelicopter.markID = event.idx
    elseif briefingRoom.mission.features.friendlyTransportHelicopter.markID ~= nil and event.idx == briefingRoom.mission.features.friendlyTransportHelicopter.markID then
      briefingRoom.radioManager.play("Affirm, coordinates discarded. Awaiting new coordinates", "RadioCoordinatesDiscardedM")
      briefingRoom.mission.features.friendlyTransportHelicopter.markID = nil
    end
  end
end

-- Add event handler
world.addEventHandler(briefingRoom.mission.features.friendlyTransportHelicopter)

-- Create F10 menu options
if briefingRoom.f10Menu.supportMenu == nil then
  briefingRoom.f10Menu.supportMenu = missionCommands.addSubMenuForCoalition($PLAYERCOALITION$, "Support", nil)
end
missionCommands.addCommandForCoalition($PLAYERCOALITION$, "Transport helicopter, land on provided coordinates", briefingRoom.f10Menu.supportMenu, briefingRoom.mission.features.friendlyTransportHelicopter.goToCoordinates, nil)

