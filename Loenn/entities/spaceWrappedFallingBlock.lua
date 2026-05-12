local fakeTilesHelper = require("helpers.fake_tiles")

local spaceWrappedFallingBlock = {}

spaceWrappedFallingBlock.name = "Casju0TrailMix/SpaceWrappedFallingBlock"

function spaceWrappedFallingBlock.placements()
    return {
        name = "space_wrapped_falling_block",
        data = {
            tiletype = fakeTilesHelper.getPlacementMaterial(),
            climbFall = true,
            behind = false,
            width = 8,
            height = 8,
        },
    }
end

spaceWrappedFallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
spaceWrappedFallingBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

function spaceWrappedFallingBlock.depth(room, entity)
    return entity.behind and 5000 or 0
end

return spaceWrappedFallingBlock