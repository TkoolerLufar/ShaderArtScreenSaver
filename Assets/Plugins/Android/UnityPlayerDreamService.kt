package jp.tkoolerlufar.madewithunity.screensaver

import android.content.Intent
import android.content.res.Configuration
import android.service.dreams.DreamService
import android.view.KeyEvent
import com.unity3d.player.IUnityPlayerLifecycleEvents
import com.unity3d.player.UnityPlayerForActivityOrService

/**
 * A service to provide the made with Unity as a screen saver (also known as a Daydream).
 */
open class UnityPlayerDreamService : DreamService(), KeyEvent.Callback,
    IUnityPlayerLifecycleEvents {
    /**
     * My UnityPlayer instance.
     *
     * It's defined for Unity native code.
     */
    @JvmField
    @Deprecated("Prevent from using directly from Kotlin code")
    protected var mUnityPlayer: UnityPlayerForActivityOrService? = null

    /**
     * My UnityPlayer instance.
     */
    var unityPlayerConnection: UnityPlayerForActivityOrService
        get() {
            @Suppress("DEPRECATION")
            return this.mUnityPlayer!!
        }
        private set(that) {
            @Suppress("DEPRECATION")
            this.mUnityPlayer = that
        }

    /**
     * Override this in your custom UnityPlayerActivity to tweak the command line arguments passed to the Unity Android Player
     * The command line arguments are passed as a string, separated by spaces
     * UnityPlayerActivity calls this from 'onCreate'
     * Supported: -force-gles20, -force-gles30, -force-gles31, -force-gles31aep, -force-gles32, -force-gles, -force-vulkan
     * See https://docs.unity3d.com/Manual/CommandLineArguments.html
     *
     * @param cmdLine the current command line arguments, may be null
     * @return the modified command line string or null
     */
    protected open fun updateUnityCommandLineArguments(cmdLine: String): String {
        return cmdLine
    }

    /**
     * Set up the screen saver.
     */
    override fun onAttachedToWindow() {
        super.onAttachedToWindow()

        // Attach me UnityPlayerDream
        try {
            UnityPlayerDream.currentService = this
        } catch (_: IllegalStateException) {
            this.finish()
            return
        }

        // 全画面で動かす
        // Run on full screen
        this.isFullscreen = true

        // 画面をタッチで閉じるような動作をUnityのマネージドコードで
        // 実装したいときはこれを true にする
        // Make it true when do I want to implement some features
        // such as Touch To Quit by Unity managed code
        this.isInteractive = true

        // コマンドライン引数を整える
        // Format a command line args
        val cmdLine = this.updateUnityCommandLineArguments("")

        // Unity プレイヤーの起動
        // Start up UnityPlayer
        this.unityPlayerConnection = UnityPlayerForActivityOrService(this, this)
            .also { unityPlayer ->
                // コマンドライン引数を渡す
                // Give this the command line args
                unityPlayer.newIntent(Intent(Intent.ACTION_DEFAULT).apply {
                    putExtra("unity", cmdLine)
                })
                this.setContentView(unityPlayer.frameLayout)
                unityPlayer.frameLayout.requestFocus()
            }
    }

    /**
     * Start to animate the screen saver.
     */
    override fun onDreamingStarted() {
        super.onDreamingStarted()
        this.unityPlayerConnection.onResume()
    }

    /**
     * Stop animating the screen saver.
     */
    override fun onDreamingStopped() {
        this.unityPlayerConnection.onPause()
        super.onDreamingStopped()
    }

    /**
     * Tear down the screen saver.
     */
    override fun onDetachedFromWindow() {
        this.unityPlayerConnection.destroy()
        // Detach me UnityPlayerDream
        UnityPlayerDream.currentService = null

        super.onDetachedFromWindow()
    }

    override fun onLowMemory() {
        super.onLowMemory()
        this.unityPlayerConnection.onTrimMemory(UnityPlayerForActivityOrService.MemoryUsage.Critical)
    }

    override fun onTrimMemory(level: Int) {
        super.onTrimMemory(level)
        this.unityPlayerConnection.onTrimMemory(when (level) {
            TRIM_MEMORY_RUNNING_MODERATE -> UnityPlayerForActivityOrService.MemoryUsage.Medium
            TRIM_MEMORY_RUNNING_LOW -> UnityPlayerForActivityOrService.MemoryUsage.High
            TRIM_MEMORY_RUNNING_CRITICAL -> UnityPlayerForActivityOrService.MemoryUsage.Critical
            else -> return
        })
    }

    override fun onConfigurationChanged(newConfig: Configuration) {
        super.onConfigurationChanged(newConfig)
        this.unityPlayerConnection.configurationChanged(newConfig)
    }

    /**
     * Notify Unity of the focus change.
     *
     * @param hasFocus If thw window of the screen saver has focused now.
     */
    override fun onWindowFocusChanged(hasFocus: Boolean) {
        super.onWindowFocusChanged(hasFocus)
        this.unityPlayerConnection.windowFocusChanged(hasFocus)
    }

    // For some reason the multiple keyevent type is not supported by the ndk.
    // Force event injection by overriding dispatchKeyEvent().
    override fun dispatchKeyEvent(event: KeyEvent?): Boolean {
        @Suppress("DEPRECATION")
        return if (event!!.action == KeyEvent.ACTION_MULTIPLE) {
            this.unityPlayerConnection.injectEvent(event)
        } else {
            super.dispatchKeyEvent(event)
        }
    }

    // Pass any events not handled by (unfocused) views straight to UnityPlayer
    override fun onKeyUp(keyCode: Int, event: KeyEvent?): Boolean {
        return this.unityPlayerConnection.frameLayout.onKeyUp(keyCode, event)
    }

    override fun onKeyDown(keyCode: Int, event: KeyEvent?): Boolean {
        return this.unityPlayerConnection.frameLayout.onKeyDown(keyCode, event)
    }

    override fun onKeyLongPress(keyCode: Int, event: KeyEvent?): Boolean {
        return this.unityPlayerConnection.frameLayout.onKeyLongPress(keyCode, event)
    }

    /**
     * For some reason the multiple keyevent type is not supported by the ndk.
     * Force event injection by overriding dispatchKeyEvent().
     */
    override fun onKeyMultiple(p0: Int, p1: Int, event: KeyEvent?): Boolean {
        return this.unityPlayerConnection.injectEvent(event)
    }

    /**
     * When Unity player unloaded finish the screensaver.
     */
    override fun onUnityPlayerUnloaded() {
        this.finish()
    }

    override fun onUnityPlayerQuitted() {
    }
}
