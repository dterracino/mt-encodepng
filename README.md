This Unity project demonstrates encoding a PNG from a separate thread.

Simply call `EncodeManager.Instance.EncodeImage` and pass in a texture,
a compression level value (integer, from 0 - 9), and a callback to call
when the encoder has finished.

For example:

EncodeManager.Instance.EncodeImage(texture, 9, OnEncodingComplete);

The callback receives a byte array representing the PNG encoded image.

The PNG encoder was taken from:

https://github.com/moxiecode/moxie/tree/master/src/silverlight/PngEncoder

