local techlessController = {}

techlessController.name = "Casju0TrailMix/TechlessController"
techlessController.depth = 0
techlessController.placements = {
   name = "techless_controller",
   data = {
      enabled = true,
      enableHypers = true,
      enableSupers = true,
      dashJumpSpeedLimit = -1,
      enableWallBounces = true,
      nerfedWallBoosts = false,
      staminaLimit = -1,
   },
}
techlessController.canResize = { false, false }
techlessController.texture = "objects/Casju0TrailMix/loenn/techlessController"

return techlessController