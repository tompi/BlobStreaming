Motivation:
Azure Media Services is deprecated June 2024.

Key take aways:
* Video tag needs range requests to be effective(to be able to seek before finished downloading).
* There does not seem to be any way to alter(add bearer token header) the requests done by the video tag.  (unless we use HLS, in this case requests are via xhr, and we can intercept them and add bearer token "beforeRequest", but this is a bit hacky and apperently is not supported by safari)
* We dont want any parts of the video stored on the client after playing is finished.

Proposed solution:
* Setup ASP.NET endpoint and serve video using "Results.Stream" which supports range requests.
* Add cache-header "no-store" on this endpoint to avoid storing the video on disk.
* Use custom authentication for this endpoint since we do not want to store users "regular" jwt in cookie.
* Issue custom, short lived JWT that is only valid for one video(scope the cookie to video url).
* Sign the JWT using Azure Keyvault(which provides builtin support for key generation and auto-rotation).

Notes:
You will not be able to test range requests locally with small mp4 files.
Tested with 7mb file, and the browser will finish downloading the file
before you can seek in the player...

Large test film(64mb): https://filesamples.com/samples/video/mp4/sample_1280x720_surfing_with_audio.mp4
Download this and install azurite(blob storage emulator) and make a container called "vidtest".
Put the file in the vidtest container as "testfilm_large.mp4.

Posted to reddit to attempt to get some feedback:
https://www.reddit.com/r/dotnet/comments/191gwgo/simple_alternative_to_azure_media_services/
