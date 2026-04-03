local superBounceController = {}

superBounceController.name = "Casju0TrailMix/SuperBounceController"
superBounceController.depth = 0
superBounceController.placements = {
   name = "super_bounce_controller",
   data = {
      enabled = true,
      dontRecoverDash = true,
      superBounceXBoost = 0,
      superBounceMultiplier = 1.5,
      lowBounceXBoost = 40,
      lowBounceMultiplier = 0.6,
   },
}
superBounceController.canResize = { false, false }
superBounceController.texture = "objects/Casju0TrailMix/loenn/superBounceController"

return superBounceController