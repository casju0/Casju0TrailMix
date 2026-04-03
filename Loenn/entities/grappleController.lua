local grappleController = {}

grappleController.name = "Casju0TrailMix/GrappleController"
grappleController.depth = 0
grappleController.placements = {
   {
      name = "grapple_controller_lani",
      data = {
         enabled = true,
         inventoryType = "AlwaysEquipped",
         controlType = "ReplaceGrab",
         harpoonMode = false,
         canShootWhenTired = false,
         addMomentumToShot = true,
         playerReleaseMomentum = "RetainGrappleSpeed",
         playerReleaseMomentumSpeedCap = -1,
         playerReleaseSpeedBoost = 40,
         freezeFrameLength = "Long",
         contactPauseLength = "None",
         shockwave = "Small",
         stringColor = "FFFFFF",
         contactColor = "FFB6C1",
         reelColor = "ADD8E6",
         canWallCancel = true,
         canItemCancel = true,
         canWallJumpCancel = true,
         canItemJumpCancel = true,
         jumpCancelStaminaCost = 27.5,
         canWallDashCancel = true,
         canItemDashCancel = true,
         pufferBehavior = "PullPuffer",
         yOffset = -2,
         shootSpeed = 360,
         retractSpeed = 480,
         wallPullSpeed = 240,
         itemPullSpeed = 240,
         minShootDuration = 0.1,
         maxShootDuration = 0.25,
         cooldown = 0,
      },
   },
   {
      name = "grapple_controller_hornet",
      data = {
         enabled = true,
         inventoryType = "AlwaysEquipped",
         controlType = "UserBinding",
         harpoonMode = true,
         canShootWhenTired = true,
         addMomentumToShot = false,
         playerReleaseMomentum = "RetainGrappleSpeed",
         playerReleaseMomentumSpeedCap = 90,
         playerReleaseSpeedBoost = 0,
         freezeFrameLength = "None",
         contactPauseLength = "Short",
         shockwave = "Small",
         stringColor = "FFFFFF",
         contactColor = "FFB6C1",
         reelColor = "ADD8E6",
         canWallCancel = false,
         canItemCancel = false,
         canWallJumpCancel = false,
         canItemJumpCancel = false,
         jumpCancelStaminaCost = 27.5,
         canWallDashCancel = false,
         canItemDashCancel = false,
         pufferBehavior = "PullPlayer",
         yOffset = -2,
         shootSpeed = 600,
         retractSpeed = 600,
         wallPullSpeed = 600,
         itemPullSpeed = 600,
         minShootDuration = 0.1,
         maxShootDuration = 0.1,
         cooldown = 1,
      },
   },
}

grappleController.fieldInformation = {
   inventoryType = {
      options = {
         "RequiresRefill",
         "AlwaysEquipped",
      },
      editable = false,
   },
   controlType = {
      options = {
         "ReplaceGrab",
         "ReplaceDash",
         "UserBinding",
      },
      editable = false,
   },
   playerReleaseMomentum = {
      options = {
         "RetainGrappleSpeed",
         "RevertSpeed",
      },
      editable = false,
   },
   freezeFrameLength = {
      options = {
         "None",
         "Short",
         "Long",
         "VeryLong",
      },
      editable = false,
   },
   contactPauseLength = {
      options = {
         "None",
         "Short",
         "Long",
         "VeryLong",
      },
      editable = false,
   },
   shockwave = {
      options = {
         "None",
         "Small",
         "Large",
      },
      editable = false,
   },
   stringColor = {
      fieldType = "color",
   },
   contactColor = {
      fieldType = "color",
   },
   reelColor = {
      fieldType = "color",
   },
   pufferBehavior = {
      options = {
         "Ignore",
         "PullPuffer",
         "PullPlayer",
      },
      editable = false,
   },
   yOffset = {
      fieldType = "integer",
      minimumValue = -5,
      maximumValue = 5,
   },
}

grappleController.canResize = { false, false }
grappleController.texture = "objects/Casju0TrailMix/loenn/grappleController"

return grappleController