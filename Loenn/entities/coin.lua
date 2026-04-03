local coin = {}

coin.fieldInformation = {
   variant = {
      options = {
         "yellow", "blue",
      },
      editable = false,
   },
}

coin.name = "Casju0TrailMix/Coin"
coin.depth = 2000
coin.placements = { {
   name = "coin_yellow",
   data = {
      variant = "yellow",
   },
}, {
   name = "coin_blue",
   data = {
      variant = "blue",
   },
} }

function coin.texture(room, entity)
   return "objects/Casju0TrailMix/coin/" .. entity.variant .. "00"
end

return coin