Add UnityBridge.js to folder with index.html

In index.html call 

    window.unityBridge.initialize(unityInstance);
          
When unity instance is created. For example here:

    var script = document.createElement("script");
      script.src = loaderUrl;
      script.onload = () => {
        createUnityInstance(canvas, config, (progress) => {
          progressBarFull.style.width = 100 * progress + "%";
        }).then((unityInstance) => {
          
          window.unityBridge.initialize(unityInstance);
          
          loadingBar.style.display = "none";
        }).catch((message) => {
          alert(message);
        });
      };