==Controller Priest==

This project has been created as a means to unify multiple controller inputs to a single output port. This is a potentially niche problem but should you have the problem, hopefully this tool can help.

=== The Problem ===

I stream with a friend on our Twitch channel [Rolling Retro](https://twitch.tv/rollingretro) where we play classic video games. Sometimes this can be PS1, PS2 or PC games. Now this is a ton of fun and I highly encourage anybody to check out the stream but due to [recent happenings (at time of writing)](https://en.wikipedia.org/wiki/Coronavirus_disease_2019) we haven't been able to be in the same place at the same time.

This is rather problematic, because a crux of our stream is the ability to pass the controller between each other as we talk, drink and generally muck around. When we play online we can't really play our actual consoles, so we default to using emulators like ePSXe. This would be fine if we could swap controllers with ease, but more often than not we would need to take a break, reset some things and start up again. This isn't ideal since it breaks the flow of the streams when we want to just keep things running.

=== The Solution ===

This tool offers a solution to the problem. It emulates a controller on a port and that a player can take control of and pass off to other players when they want to. A simple thing, but works surprisingly well. It actually surprises me that emulators don't have this built in, or even Parsec for that matter. So here it is! A simple fix for a simple problem.

=== Pre-requisites ===

In order for this to work, you'll need a couple of things.

1. A Windows PC. Sorry Linux and Mac users, this was built with Windows in mind, because the guts of it relies on a windows driver. If you want to use this on those systems, let me know and I can investigate.

2. A screen sharing application that emulates a second controller. You could of course use this locally if you wanted, which is a little odd but not unreasonable, however I envisioned this mostly being used for playing with people when they aren't in the same location. Programs like Parsec or Steams Share Play thing do this and should work with them.

3. You might need to download [ViGEm](https://github.com/ViGEm/ViGEmBus/releases) If you have Parsec installed I believe you should be fine but if the controller has any issues starting up or running, it's probably because you need to download the driver. Most of the work is done by ViGEm or SharpDX in this tool, I just wrote some simple bridging code.

4. Run this on the computer that is running the video game/emulator. No point having it on the other computer.

5. The game/emulator needs to be XInput compatible or be able to resolve XInput controllers without modification. DirectInput won't work as far as I know, though maybe that's worth integrating in the future.

That's all. If you got all that, you should be fine.

=== How To Use The Tool ===

Might be self explanatory, but the wiki has a guide on using the tool.
