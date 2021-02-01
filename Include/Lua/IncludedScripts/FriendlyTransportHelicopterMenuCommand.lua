briefingRoom.mission.features.friendlyTransportHelicopter = { }
briefingRoom.mission.features.friendlyTransportHelicopter.activated = { } -- was the transport helo activated?

-- Activate aircraft and launch bombing run
function briefingRoom.mission.features.friendlyTransportHelicopter.goToCoordinates(index)
  briefingRoom.radioManager.play("XXXXXXXXXXXX", "Radio0")

  local bomberGroup = dcsExtensions.getGroupByID(briefingRoom.mission.featuresUnitGroups.friendlyTransportHelicopter[index])
  if bomberGroup == nil then return end -- no group (destroyed? missing?), no radio answer

  -- helicopter already activated
  -- if briefingRoom.mission.features.friendlyTransportHelicopter.activated[index] then
    -- briefingRoom.radioManager.play("Negative, bombing mission already in progress.", "RadioOtherPilotAlreadyBombing", briefingRoom.radioManager.getAnswerDelay())
    -- return
  -- end

  briefingRoom.radioManager.play("Copy, on my way to the LZ.", "RadioOtherPilotOnMyWayToLZ", briefingRoom.radioManager.getAnswerDelay())

  if not briefingRoom.mission.features.friendlyTransportHelicopter.activated[index]
    bomberGroup:activate()
    briefingRoom.mission.features.friendlyTransportHelicopter.activated[index] = true
  end
end

-- Create F10 menu options
do
  for i,o in ipairs(briefingRoom.mission.objectives) do
    missionCommands.addCommandForCoalition($PLAYERCOALITION$, "Pick up package at target coordinates", briefingRoom.f10Menu.objectives[i], briefingRoom.mission.features.friendlyTransportHelicopter.goToCoordinates, i)
    table.insert(briefingRoom.mission.features.friendlyTransportHelicopter.activated, false) -- set all activated indices to false
  end
end

