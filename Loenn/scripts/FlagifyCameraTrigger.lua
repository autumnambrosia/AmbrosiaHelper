local script = {
    name = "FlagifyCameraTrigger",
    displayName = "Flagify Camera Trigger",
    parameters = {
        id = "0",
        --flagtoggled = true,
    },
    --fieldOrder = {"id", "flagtoggled"},
    tooltip = "Converts a Camera Offset/Target trigger into its corresponding Maddie's Helping Hand Flag-Toggled variant, keeping the same settings.\nIf the trigger is already flag-toggled, this converts it back to a vanilla trigger.",
    tooltips = {
        id = "The entity ID of the trigger to convert.",
        --flagtoggled = "",
    },
}

function script.run(room, args)
    for _, tg in ipairs(room.triggers) do
        -- this is the worst code of all time
        if tg._id ~= tonumber(args.id) then goto w end

        -- tribute to the variable i named the f slur that i didnt even end up using
        
        if tg._name == "cameraTargetTrigger" then
            tg.flag = "camera"..tostring(tg._id)
            tg.inverted = false
            tg._name = "MaxHelpingHand/FlagToggleCameraTargetTrigger"
        elseif tg._name == "MaxHelpingHand/FlagToggleCameraTargetTrigger" then
            tg.flag = nil
            tg.inverted = nil
            tg._name = "cameraTargetTrigger"
        elseif tg._name == "cameraOffsetTrigger" then
            tg.flag = "camera"..tostring(tg._id)
            tg.inverted = false
            tg._name = "MaxHelpingHand/FlagToggleCameraOffsetTrigger"
        elseif tg._name == "MaxHelpingHand/FlagToggleCameraOffsetTrigger" then
            tg.flag = nil
            tg.inverted = nil
            tg._name = "cameraOffsetTrigger"
        end

        ::w:: -- helpful continue spider, courtesy of μ
    end
end

return script