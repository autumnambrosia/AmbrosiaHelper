-- wow, finally? a NORMAL autumn lonn plugin?

local metalbumper = {}

metalbumper.name = "AmbrosiaHelper/MetalBumper"
metalbumper.depth = 0
metalbumper.nodeLineRenderType = "line"
metalbumper.texture = function(room, entity)
    -- i have two sides
    local path = entity.coremode == 1 and "evil" or "idle"
    return "objects/AmbrosiaHelper/metalbumper/" .. path .. "00"
end
metalbumper.nodeLimits = {0, 1}
metalbumper.placements = {
    name = "normal",
    data = {
        static = true,
        coremode = 0
        --custom = false
    }
    --[[
    {
        name = "custom",
        data = {
            static = true,
            coremode = 0,
            sfxname = "event:/game/09_core/pinballbumper_hit",
            respawnsfxname = "event:/game/06_reflection/pinballbumper_reset",
            spritename = "AmbrosiaHelper_metalbumper",
            speed = 300,
            coldparticles = "47b5cc,c4f4ff",
            hotparticles = "ffa808,ffa808",
            respawntime = 0.5,
            anglezones = 8,
            custom = true -- ok i guess
        }
    }
    ]]
}

--[[
metalbumper.fieldOrder = {"x", "y", "coremode", "anglezones", "respawntime", "speed", "sfxname", "respawnsfxname", "coldparticles", "hotparticles", "spritename"}
-- i need ignoredFields because the fuckass particle colors color list thing makes it so it shows even when its nil so i kinda just hide it manuall
-- i also have no idea why tf it takes entity instead of room,entity that was fun to figure out
metalbumper.ignoredFields = function(entity)
    local tbl = {"_name", "_id", "originX", "originY", "custom"}
    if not entity.custom then
        table.insert(tbl, "coldparticles")
        table.insert(tbl, "hotparticles")
    end
    return tbl
end
]]

metalbumper.fieldInformation = {
    coremode = {
        editable = false,
        options = {
            ["Core Mode"] = 0,
            ["Only Hot"] = 1,
            ["Only Cold"] = 2
        }
    }
    --[[
    sfxname = {
        editable = true,
        options = {
            "event:/game/06_reflection/pinballbumper_hit",
            "event:/game/09_core/pinballbumper_hit"
        }
    },
    spritename = {
        editable = true,
        options = {
            "AmbrosiaHelper_metalbumper",
            "bumper"
        }
    },
    coldparticles = {
        fieldType = "list",
        minimumElements = 2,
        maximumElements = 2,
        elementDefault = "ffffff",
        elementOptions = {
            fieldType = "color",
            allowXNAColors = false
        }
    },
    hotparticles = {
        fieldType = "list",
        minimumElements = 2,
        maximumElements = 2,
        elementDefault = "ffffff",
        elementOptions = {
            fieldType = "color",
            allowXNAColors = false
        }
    }
    ]]
}

return metalbumper