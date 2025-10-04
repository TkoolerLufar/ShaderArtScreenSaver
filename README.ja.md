[Read in English](README.md)

UnityScreenSaver
================

Unity でスクリーンセーバーを作れる？

----

Android用のファイル
-------------------

Android スクリーンセーバー（通称Daydream、なおVRプラットフォームではない）
をビルドするには以下のファイルを使います。

- Assets/
  - Plugins/
    - Android/
      - AndroidManifest.xml
      - UnityPlayerDreamService.kt
      - UnityPlayerDream.kt

この Daydream はC#のマネージドコードで閉じれます。コード例は
**Assets/QuitTimerScript.cs** の **Quit()** をご覧ください。

Windows用のファイル
-------------------

Windowsスクリーンセーバーのランチャーを ExternalPrograms/Windows/UnityScreenSaverLauncher 以下に作りました。

ビルド手順:

1. あらかじめ Visual Studio に ".NET デスクトップ開発" 機能を入れておく
2. このUnityプロジェクトをUnityで開いて *Build/Windows* にビルド
3. *ExternalPrograms/Windows/UnityScreenSaverLauncher/UnityScreenSaverLauncher.sln* を開く
4. そのソリューションを**発行**する。これで *Launcher.exe* が *Build/Windows* に配置されます。
5. 出来上がった **Launcher.exe** の名前を **(自分のUnityプロジェクト名).scr** に変更する
6. 完成！

自分の Unity に導入する手順:

- Unity プロジェクトのルートより3階層下のフォルダーにソリューションをコピー
  (例: /ExternalPrograms/Windows/UnityScreenSaverLauncher/UnityScreenSaverLauncher.sln)
- ソリューションの中に*プロジェクト*が1つある。
  このプロジェクトの名前を Unity プロジェクトの Product Name と同じにする。

License
-------

MIT
