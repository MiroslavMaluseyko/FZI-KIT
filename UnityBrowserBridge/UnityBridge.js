class UnityBridge {
    constructor() {
        this.gameAspectRatio = -1;
        this.unityGame = null;
    }

    initialize(unityInstance) {
        this.unityGame = unityInstance;
        console.log("Unity instance initialized:", this.unityGame);

        this.init_visibility_changed();
    }

    // Visibility
    init_visibility_changed() {
        document.addEventListener('visibilitychange', () => {
            if (document.visibilityState === "hidden") {
                this.unityGame.SendMessage("UnityBrowserBridge", "TabVisibilityChanged", 0);
            } else {
                this.unityGame.SendMessage("UnityBrowserBridge", "TabVisibilityChanged", 1);
            }
        });
    }

    // Aspect ratio
    init_aspect_ratio_change() {
        window.addEventListener('resize', () => this.resizeGame());
        window.addEventListener('load', () => this.resizeGame());
    }

    set_aspect_ratio(aspectRatio) {
        if(this.gameAspectRatio == -1)this.init_aspect_ratio_change();
        this.gameAspectRatio = aspectRatio;
        this.resizeGame();
    }

    resizeGame() {
        const width = window.innerWidth;
        const height = window.innerHeight;
        const windowAspectRatio = width / height;
        let newWidth, newHeight;

        if (windowAspectRatio < this.gameAspectRatio) {
            newWidth = width;
            newHeight = width / this.gameAspectRatio;
        } else {
            newHeight = height;
            newWidth = height * this.gameAspectRatio;
        }

        const gameCanvas = document.getElementById('unity-canvas');
        if (gameCanvas) {
            gameCanvas.style.width = newWidth + 'px';
            gameCanvas.style.height = newHeight + 'px';
        } else {
            console.warn("Canvas with ID 'unity-canvas' not found.");
        }
    }
}

// Експорт об'єкта для доступу з інших скриптів
const unityBridge = new UnityBridge();
window.unityBridge = unityBridge;
