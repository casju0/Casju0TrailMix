local drawableSprite = require("structs.drawable_sprite")

local onOffSprite = {}

local onOffTexture = "objects/Casju0TrailMix/onOffSprite/onOff"
local offOffTexture = "objects/Casju0TrailMix/onOffSprite/offOff"
local onOnTexture = "objects/Casju0TrailMix/onOffSprite/onOn"
local offOnTexture = "objects/Casju0TrailMix/onOffSprite/offOn"

onOffSprite.name = "Casju0TrailMix/OnOffSprite"
onOffSprite.depth = 2000
onOffSprite.placements = {
   {
      name = "on_off_sprite_on",
      data = {
         flag = "",
         onOffType = true,
         activeImage = onOnTexture,
         inactiveImage = onOffTexture,
         activationSound = "event:/casju0_TrailMix/smw_switch",
      },
   },
   {
      name = "on_off_sprite_off",
      data = {
         flag = "",
         onOffType = false,
         activeImage = offOnTexture,
         inactiveImage = offOffTexture,
         activationSound = "event:/casju0_TrailMix/smw_switch",
      },
   },
}

onOffSprite.canResize = false

function onOffSprite.sprite(room, entity)
   if entity.onOffType then
      return drawableSprite.fromTexture(onOffTexture, entity)
   else
      return drawableSprite.fromTexture(offOffTexture, entity)
   end
end

return onOffSprite