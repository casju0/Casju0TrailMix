local throwBlockTrigger = {}

throwBlockTrigger.name = "Casju0TrailMix/ThrowBlockTrigger"
throwBlockTrigger.placements = {
   name = "throw_block_trigger",
   data = {
      allowDashPickups = true,
      allowClimbPickups = true,
      allowWallJumpPickups = true,
      climbJumpRefundAmount = 0.0,
      graceJumpDuration = 0.1,
      coverRoom = false,
   },
}

return throwBlockTrigger