# FFME_multiple

In my application that displays streaming video from UDP and playback from file, I use the UnoSquare ffmediaelement replacement for the
Microsoft MediaElement WPF control as the latter does not support streaming video. 

https://github.com/unosquare/ffmediaelement

While I've found the ffmediaelement is fine for streaming video, despite some latency, for video file playback I keep running into
instances where the video frames advance but the rendered video is all black or it gets stuck in an IsSeeking state. I have added some recovery
hooks but I'm trying to make this more seamless for the user. I Googled for ffmediaelement multiple instances and found this result with an answer
from the ffmediaelement author.

https://github.com/unosquare/ffmediaelement/issues/185

I have extended Mario di Vece's sample application that creates a grid of nine instances of the FFMediaElement to have Pause, Play and Seek 
buttons and a slider that also controls a seek position. I also added a LixtBox with status messages. My example videos are 10 to 20 seconds 
long, though my publicly checked in code uses Mario's example filenames so someone using this would need to use their own files or have it browse
to files.

The slider uses the maximum time duration of the set of videos and controls the Seek to the number of seconds relative to this. If the seek time
is more than the time of a particular video, the seek operation seeks to the middle if its video (though seeking to the end might be better).

When I stress the system by clicking on the slider repeatedly I find that some are still IsSeeking. After a couple repeated seeks with this
state, it reopens the video file. Sometimes this fails to recover. Sometimes the program crashes (despite an exception handler around the Seek
call from the slider update property.
