local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local smw1f0 = {}

smw1f0.name = "Casju0TrailMix/Smw1f0"
smw1f0.depth = -9000
smw1f0.canResize = { true, false }
smw1f0.placements = {
   name = "smw1f0",
   data = {
      width = 8,
   },
}

function smw1f0.sprite(room, entity)
   local texture = "objects/jumpthru/smw1f0"

   local x, y = entity.x or 0, entity.y or 0
   local width = entity.width or 8

   local startX, startY = math.floor(x / 8) + 1, math.floor(y / 8) + 1
   local stopX = startX + math.floor(width / 8) - 1
   local len = stopX - startX

   local sprites = {}

   for i = 0, len do
      local quadX = 8
      local quadY = 8

      if i == 0 then
         quadX = 0
         quadY = room.tilesFg.matrix:get(startX - 1, startY, "0") ~= "0" and 0 or 8
      elseif i == len then
         quadY = room.tilesFg.matrix:get(stopX + 1, startY, "0") ~= "0" and 0 or 8
         quadX = 16
      end

      local sprite = drawableSprite.fromTexture(texture, entity)

      sprite:setJustification(0, 0)
      sprite:addPosition(i * 8, 0)
      sprite:useRelativeQuad(quadX, quadY, 8, 8)

      table.insert(sprites, sprite)
   end

   return sprites
end

function smw1f0.selection(room, entity)
   return utils.rectangle(entity.x, entity.y, entity.width, 8)
end

return smw1f0