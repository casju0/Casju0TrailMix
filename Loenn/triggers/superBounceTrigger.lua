local superBounceTrigger = {}

superBounceTrigger.name = "Casju0TrailMix/SuperBounceTrigger"
superBounceTrigger.placements = {
   name = "super_bounce_trigger",
   data = {
      enabled = true,
      dontRecoverDash = true,
      superBounceXBoost = 0,
      superBounceMultiplier = 1.5,
      lowBounceXBoost = 40,
      lowBounceMultiplier = 0.6,
      coverRoom = false,
   },
}

return superBounceTrigger