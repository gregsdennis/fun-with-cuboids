# fun-with-cuboids

Matt Parker made an awesome video on how a single cuboid net can actually represent more than one cuboid.

View here: https://youtu.be/jOTTZtVPrgo

While watching the video I had a question about whether _any_ two cuboids that had a common surface area also shared a net.

This project is an attempt to solve that for cuboids up to 10x10x10.  It did not go well.  Well... it did, but there was a snag.

To my surprise, even the cuboids he mentions in the video, the 1x1x5 / 1x2x3 (both surface area 22), take an enormous amount of time to process, mostly because they have an insane number of possible nets!  As of writing this, I've had the console app running for 37 hours, trying to find the unique nets of _just_ the 1x1x5.  It's found 7679 unique nets so far, generating and comparing multiple thousands of nets per second, and it no where near done.

I've made all the optimizations I could find.  Memory isn't a problem, it just take a while.