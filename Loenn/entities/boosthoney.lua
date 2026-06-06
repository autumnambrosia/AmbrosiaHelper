local boosthoney = {}

boosthoney.name = "AmbrosiaHelper/BoostHoney"
boosthoney.depth = 2000
boosthoney.placements = {
    {
        name = "left",
        data = {
            height = 8,
            placementDir = -1,
            fallbackDir = -1,
            useCoreMode = true
        }
    },
    {
        name = "right",
        data = {
            height = 8,
            placementDir = 1,
            fallbackDir = 1,
            useCoreMode = true
        }
    },
    {
        name = "up",
        data = {
            width = 8,
            placementDir = 0,
            fallbackDir = 0,
            useCoreMode = true
        }
    }
}

boosthoney.ignoredFields = {"placementDir"}
boosthoney.fieldInformation = {
    fallbackDir = {
        editable = false,
        options = {
            Up = 0,
            Left = -1,
            Right = 1
        }
    }
}

--[[
1: name of sprite
2: direction when rotated L
3: direction when rotated R
local switch = {
    [-1] = {"left" , false,     0},
    [ 0] = {"up"   ,    -1,     1},
    [ 1] = {"right",     0, false},
}
]]

local drawable_rectangle = require('structs.drawable_rectangle')
local drawable_sprite    = require('structs.drawable_sprite')

boosthoney.sprite = function(room,entity)
    -- weird ass logic
    local dir = 0
    if     not entity.useCoreMode   then dir = entity.fallbackDir
    elseif entity.placementDir ~= 0 then dir = entity.placementDir
    elseif entity.fallbackDir  ~= 0 then dir = entity.fallbackDir
    end

    local incolor  = entity.useCoreMode and (dir == 0 and "#d01c01" or "#0151d0") or "#d19900"
    local outcolor = entity.useCoreMode and (dir == 0 and "#f25e29" or "#4ca2eb") or "#f2d729"
    local xcolor   = entity.useCoreMode and (dir == 0 and "#ff8933" or "#33ffe7") or "#fcff33"

    local sprite_table = {
        drawable_rectangle.fromRectangle("bordered",
            entity.x - (entity.placementDir == -1 and 4 or 0),
            entity.y - (entity.placementDir ==  0 and 4 or 0),
            entity.width or 4,
            entity.height or 4,
            incolor, outcolor
        )
    }

    -- you'd think 1 is an off-by-one error here but it actually ends up working better than 0 for some reason
    for offset = 1, entity.width or entity.height, 8 do
        local tile = drawable_sprite.fromTexture("objects/AmbrosiaHelper/boosthoney/" .. dir, entity)
        tile:setPosition(
            entity.x + (entity.width  and offset + 3 or  2 * dir),
            entity.y + (entity.height and offset + 3 or -2)
        )
        tile:setColor(xcolor)

        table.insert(sprite_table, tile)
    end

    return sprite_table
end

function boosthoney.selection(room, entity)
    return drawable_rectangle.fromRectangle("bordered",
        entity.x - (entity.placementDir == -1 and 8 or 0),
        entity.y - (entity.placementDir ==  0 and 8 or 0),
        entity.width or 8,
        entity.height or 8
    )
end

function boosthoney.rotate(room, entity, dir)
    if entity.placementDir == 0 then
        entity.placementDir = dir
        entity.height = entity.width
        entity.width = nil
        return true
    end
    if entity.placementDir ~= dir then
        entity.placementDir = 0
        entity.width = entity.height
        entity.height = nil
        return true
    end
    return false
end

function boosthoney.flip(room, entity, horiz, vert)
    if entity.placementDir == 0 or vert then return false end
    entity.placementDir *= -1
    return true
end

return boosthoney