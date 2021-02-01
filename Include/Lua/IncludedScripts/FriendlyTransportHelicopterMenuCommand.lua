briefingRoom.mission.features.friendlyTransportHelicopter = { }
briefingRoom.mission.features.friendlyTransportHelicopter.MARKER_NAME = "helicopter"
briefingRoom.mission.features.friendlyTransportHelicopter.activated = false -- was the transport helo activated?
briefingRoom.mission.features.friendlyTransportHelicopter.markID = nil -- ID of the mark on the map
briefingRoom.mission.features.friendlyTransportHelicopter.disableCooRemovedRadioMessage = false

-- Activate aircraft and launch bombing run
function briefingRoom.mission.features.friendlyTransportHelicopter.goToCoordinates()
  briefingRoom.radioManager.play("XXXXXXXXXXXX", "Radio0")

  local transportHeloGroup = dcsExtensions.getGroupByID(briefingRoom.mission.featuresUnitGroups.FriendlyTransportHelicopter[1])
  if transportHeloGroup == nil then return end -- no group (destroyed? missing?), no radio answer

  local destination = nil
  local marks = world.getMarkPanels()
  for _,m in ipairs(marks) do
    if briefingRoom.mission.features.friendlyTransportHelicopter.markID ~= nil and m.idx == briefingRoom.mission.features.friendlyTransportHelicopter.markID then
      destination = m.pos
    end
  end

  if destination == nil then return end -- no coordinates, nothing to do

  briefingRoom.radioManager.play("Copy, on my way to the LZ.", "RadioOtherPilotOnMyWayToLZ", briefingRoom.radioManager.getAnswerDelay())

  if not briefingRoom.mission.features.friendlyTransportHelicopter.activated then
    transportHeloGroup:activate()
    briefingRoom.mission.features.friendlyTransportHelicopter.activated = true
  end

  local ctrl = transportHeloGroup:getController()
  local landTask = { id= 'Land', params = { point = destination, durationFlag = false, duration = 3600 } }
  ctrl:setTask(landTask)
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
      briefingRoom.mission.features.friendlyTransportHelicopter.goToCoordinates()
      return
    end
  elseif event.id == world.event.S_EVENT_MARK_CHANGE then
    local markText = string.lower(tostring(event.text or ""))

    if markText == briefingRoom.mission.features.friendlyTransportHelicopter.MARKER_NAME then
      --briefingRoom.radioManager.play("Copy, coordinates updated.", "RadioCoordinatesUpdatedF")
      if briefingRoom.mission.features.friendlyTransportHelicopter.markID ~= nil then
        briefingRoom.mission.features.friendlyTransportHelicopter.disableCooRemovedRadioMessage = true
        trigger.action.removeMark(briefingRoom.mission.features.friendlyTransportHelicopter.markID)
        briefingRoom.mission.features.friendlyTransportHelicopter.disableCooRemovedRadioMessage = false
      end
      briefingRoom.mission.features.friendlyTransportHelicopter.markID = event.idx
      briefingRoom.mission.features.friendlyTransportHelicopter.goToCoordinates()
    elseif briefingRoom.mission.features.friendlyTransportHelicopter.markID ~= nil and event.idx == briefingRoom.mission.features.friendlyTransportHelicopter.markID then
      briefingRoom.radioManager.play("Affirm, coordinates discarded. Awaiting new coordinates", "RadioCoordinatesDiscardedF")
      briefingRoom.mission.features.friendlyTransportHelicopter.markID = nil
    end
  end
end

-- Add event handler
world.addEventHandler(briefingRoom.mission.features.friendlyTransportHelicopter)
