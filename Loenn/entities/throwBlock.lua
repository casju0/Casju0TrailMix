local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local throwBlock = {}

throwBlock.name = "Casju0TrailMix/ThrowBlock"
throwBlock.depth = 0
throwBlock.fieldInformation = {
   width = {
      editable = false,
   },
   height = {
      editable = false,
   },
}
throwBlock.placements = {
   name = "throw_block",
   data = {
      width = 16,
      height = 16,
      wallSpringVelocityX = 220,
      wallSpringVelocityY = -80,
      floorSpringVelocityX = 0.5,
      floorSpringVelocityY = -330,
      groundFriction = 800,
      gravity = 800,
      maxFallSpeed = 160,
      lifeTime = 10,
      slowFlickerTime = 3,
      hasLight = true,
   },
}
throwBlock.canResize = { false, false }

function throwBlock.sprite(room, entity)
   local sprite = drawableSprite.fromTexture("objects/Casju0TrailMix/throwBlock/idle00", entity)
   sprite.y = sprite.y - 8
   return sprite
end

function throwBlock.selection(room, entity)
   return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

return throwBlock