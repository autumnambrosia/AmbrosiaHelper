local bounceBlock = {}

bounceBlock.name = "AmbrosiaHelper/ChillingCoreBlock"
bounceBlock.depth = 8990
bounceBlock.warnBelowSize = {16, 16}
bounceBlock.placements = {
    name = "ice",
    data = {
        width = 16,
        height = 16
    }
}

local drawable_nine_patch = require("structs.drawable_nine_patch")
local drawable_sprite     = require("structs.drawable_sprite")
local ninepatchoptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

function bounceBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local txt_block = "objects/AmbrosiaHelper/chillingcoreblock/ice00"
    local txt_symbol = "objects/AmbrosiaHelper/chillingcoreblock/ice_center08"

    local ninepatch = drawable_nine_patch.fromTexture(txt_block, ninepatchoptions, x, y, width, height)
    local spr_symbol = drawable_sprite.fromTexture(txt_symbol, entity)
    local sprites = ninepatch:getDrawableSprite()
    
    spr_symbol:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, spr_symbol)

    return sprites
end

return bounceBlock