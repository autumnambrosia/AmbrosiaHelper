local forcemovefield = {}

forcemovefield.name = "AmbrosiaHelper/ForceMoveField"
forcemovefield.depth = 2000
forcemovefield.placements = {
    {
        name = "left",
        data = {
            width = 16,
            height = 16,
            direction = -1,
            time = 1,
            renders = true,
            audible = true
        }
    },
    {
        name = "right",
        data = {
            width = 16,
            height = 16,
            direction = 1,
            time = 1,
            renders = true,
            audible = true
        }
    },
    {
        name = "revoke",
        data = {
            width = 16,
            height = 16,
            direction = 0,
            time = 0,
            renders = true,
            audible = true
        }
    },
    {
        name = "revokeonwalljump",
        data = {
            width = 16,
            height = 16,
            direction = -2,
            time = 0,
            renders = true,
            audible = true
        }
    }
}
--[[
colornormal = "eb8b47",
colorstart = "00ff00",
colorend = "ff0000"
]]

forcemovefield.fieldInformation = {
    direction = {
        editable = false,
        options = {
            Left = -1,
            Right = 1,
            Revoke = 0,
            RevokeOnWalljump = -2,
        }
    },
    time = {
        minimumValue = 0
    }
    --[[
    effects = {
        fieldType = "boolean"
    },
    colornormal = {
        fieldType = "color"
    },
    colorstart = {
        fieldType = "color"
    },
    colorend = {
        fieldType = "color"
    }
    ]]
}

local drawable_rectangle = require('structs.drawable_rectangle')
local drawable_sprite    = require('structs.drawable_sprite')
local drawable_text      = require('structs.drawable_text')

--[[
1: color
2: name of sprite
3: direction when flipped H
4: direction when flipped V
]]
local switch = {
    [-1] = {{ 51/255, 187/255, 255/255, 1}, "Left"            ,  1,  0},
    [ 1] = {{ 51/255, 187/255, 255/255, 1}, "Right"           , -1, -2},
    [ 0] = {{231/255, 106/255,  68/255, 1}, "Revoke"          , -2, -1},
    [-2] = {{207/255,  70/255, 235/255, 1}, "RevokeOnWalljump",  0,  1}
}

-- oh my god
-- like half stolen from vivhelper refill walls
forcemovefield.sprite = function(room,entity)
    local outcolor = switch[entity.direction][1]
    local incolor = {outcolor[1]*0.7, outcolor[2]*0.7, outcolor[3]*0.7, 0.7}
    local sprite = switch[entity.direction][2]

    -- (color override for if it doesn't render)
    if not entity.renders then
        outcolor = {1,1,1,1}
        incolor = {1,1,1,0}
    end
    
    local spr = drawable_sprite.fromTexture("objects/AmbrosiaHelper/forcemovefield/" .. sprite, entity)
    spr:setPosition(entity.x + entity.width/2, entity.y + entity.height/2)
    spr:setColor(outcolor)

    local txt = ""
    if math.abs(entity.direction) == 1 then txt = tostring(math.floor(0.5+entity.time*1000)/1000).."s" end

    return {
        drawable_rectangle.fromRectangle("bordered", entity.x, entity.y, entity.width, entity.height, incolor, outcolor),
        spr,
        drawable_text.fromText(txt, entity.x - 8, entity.y + entity.height, entity.width + 16, 6, nil, 1, outcolor)
    }
end

function forcemovefield.rotate(room, entity, dir)
    entity.time += (dir / 10)
    if entity.time <= 0 then
        entity.time = 0.1
        return false
    end
    return true
end

function forcemovefield.flip(room, entity, horiz, vert)
    if horiz == vert then return false end
    entity.direction = switch[entity.direction][(horiz and 3 or 4)]
    return true
end

return forcemovefield