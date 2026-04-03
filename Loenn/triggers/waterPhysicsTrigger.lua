local waterPhysicsTrigger = {}

waterPhysicsTrigger.name = "Casju0TrailMix/WaterPhysicsTrigger"
waterPhysicsTrigger.placements = { {
   name = "water_physics_trigger_mega_man",
   data = {
      enabled = true,
      swimMode = "MegaMan",
      mmRiseSpeed = -105,
      mmRiseAcceleration = 105,
      smwHorizontalSpeed = 90,
      smwHorizontalAccel = 400,
      smwFallMaxSpeed = 120,
      smwFallAccel = 120,
      smwRiseCancelDecel = 240,
      smwPaddleSpeed = 120,
      smwPaddleMaxSpeed = -120,
      smwSuperPaddleSpeed = 240,
      smwSuperPaddleMaxSpeed = -160,
      coverRoom = false,
   },
}, {
   name = "water_physics_trigger_smw",
   data = {
      enabled = true,
      swimMode = "SMW",
      mmRiseSpeed = -105,
      mmRiseAcceleration = 105,
      smwHorizontalSpeed = 90,
      smwHorizontalAccel = 400,
      smwFallMaxSpeed = 120,
      smwFallAccel = 120,
      smwRiseCancelDecel = 160,
      smwPaddleSpeed = 30,
      smwPaddleMaxSpeed = -120,
      smwSuperPaddleSpeed = 60,
      smwSuperPaddleMaxSpeed = -160,
      coverRoom = false,
   },
} }

waterPhysicsTrigger.fieldInformation = {
   swimMode = {
      options = { "MegaMan", "SMW" },
      editable = false,
   },
}

return waterPhysicsTrigger