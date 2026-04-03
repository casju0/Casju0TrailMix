local waterPhysicsController = {}

waterPhysicsController.name = "Casju0TrailMix/WaterPhysicsController"
waterPhysicsController.depth = 0
waterPhysicsController.placements = { {
   name = "water_physics_controller_mega_man",
   data = {
      enabled = true,
      swimMode = "MegaMan",
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
   },
}, {
   name = "water_physics_controller_smw",
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
   },
} }

waterPhysicsController.fieldInformation = {
   swimMode = {
      options = { "MegaMan", "SMW" },
      editable = false,
   },
}

waterPhysicsController.canResize = { false, false }
waterPhysicsController.texture = "objects/Casju0TrailMix/loenn/waterPhysicsController"

return waterPhysicsController