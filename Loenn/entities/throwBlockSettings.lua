local throwBlockSettings = {}

throwBlockSettings.name = "Casju0TrailMix/ThrowBlockSettings"
throwBlockSettings.depth = 0
throwBlockSettings.placements = {
   name = "throw_block_settings",
   data = {
      allowDashPickups = true,
      allowClimbPickups = true,
      allowWallJumpPickups = true,
      climbJumpRefundAmount = 0.0,
      graceJumpDuration = 0.1,
   },
}
throwBlockSettings.canResize = { false, false }
throwBlockSettings.texture = "objects/Casju0TrailMix/loenn/throwBlockSettings"

return throwBlockSettings