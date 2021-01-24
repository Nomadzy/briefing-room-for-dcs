briefingRoom.mission.features.objectiveDestroyAirbase = {}

function briefingRoom.mission.features.objectiveDestroyAirbase:onEvent(event)
  if briefingRoom.mission.status ~= brMissionStatus.IN_PROGRESS then return end -- mission already complete/failed, nothing to do

  if event.id == world.event.S_EVENT_HIT then -- aircraft was hit but not destroyed, check anyway because destroying a parked aircraft in DCS is HARD, and any aircraft with less than 90% hp left is not airworthy
    if event.target == nil then return end -- no target (should never happen)
    if event.target:getCategory() ~= Object.Category.BASE then return end -- target was not an airbase
    local life = event.target:getLife() / event.target:getLife0()
    -- if life > .9 then return end -- not damaged enough
    -- briefingRoom.mission.features.objectiveDestroyAirbase.checkDestroyedAirbase(event.target:getID());
    trigger.action.outText("Airbase "..tostring(event.target:getID()).." hit "..tostring(math.floor(life * 100)).." life left", 2)
  elseif event.id == world.event.S_EVENT_DEAD then -- unit destroyed
    if event.initiator == nil then return end -- no target (should never happen)
    if event.initiator:getCategory() ~= Object.Category.BASE then return end -- target was not an airbase
    -- briefingRoom.mission.features.objectiveDestroyAirbase.checkDestroyedAirbase(event.initiator:getID());
    trigger.action.outText("Airbase "..tostring(event.initiator:getID()).." dead", 2)
  end
end

-- function briefingRoom.mission.features.objectiveDestroyAirbase.checkDestroyedAirbase(unitID)
--   for index,objective in ipairs(briefingRoom.mission.objectives) do
--     if objective.status == brMissionStatus.IN_PROGRESS then
--       if table.contains(briefingRoom.mission.objectives[index].unitsID, unitID) then

--         table.removeValue(briefingRoom.mission.objectives[index].unitsID, unitID)
--         local messageIndex = math.random(1, 2)
--         messages = { "Target destroyed.", "Good hit on target." }
--         briefingRoom.radioManager.play(messages[messageIndex], "RadioHQTargetDestroyedGround"..tostring(messageIndex), math.random(1, 3))

--         if #briefingRoom.mission.objectives[index].unitsID < 1 then
--           briefingRoom.mission.functions.completeObjective(index)
--         end
--       end
--     end
--   end
-- end

world.addEventHandler(briefingRoom.mission.features.objectiveDestroyAirbase)
