mergeInto(LibraryManager.library, {

  Alert: function (str) {
    window.alert(UTF8ToString(str));
  },
  
  SetAspectRatio: function(width, height)
  {
    window.unityBridge.set_aspect_ratio(width/height);
  },
});