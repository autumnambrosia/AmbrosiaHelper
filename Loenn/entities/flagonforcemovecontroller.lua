local flagforcemove = {}

flagforcemove.name = "AmbrosiaHelper/FlagOnForceMoveController"
flagforcemove.depth = -1000000
flagforcemove.texture = function(room, entity) return "objects/AmbrosiaHelper/Loenn/forcemovecontroller/" .. tostring(entity.direction) end
flagforcemove.placements = {
    name = "main",
    data = {
        direction = 0,
        flag = "playerHasForcemove"
    }
}
flagforcemove.fieldInformation = {
    direction = {
        editable = false,
        options = {
            Left = -1,
            Right = 1,
            None = 0
        }
    }
}

return flagforcemove