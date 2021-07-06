# SkyWay-WebRTC-GW-Unity-Sample

SkyWay WebRTC GWを利用してUnityからbrowserにWEBRTCでテキスト送信するサンプル
Unity用のプロジェクトとブラウザ側のプロジェクト

- WebRTC MediaStreamからのRTP取り出しについては[SkyWay WebRTC GW](https://github.com/skyway/skyway-webrtc-gateway)を利用しています
- RTP Mediaの描画については[mrayGStreamerUnity](https://github.com/mrayy/mrayGStreamerUnity)を利用していますので、そちらのリポジトリの指示に従って、gStreamerの設定をお願いします。

## How to use
### Unity
1. Sample Sceneを起動
2. Canvasに付加されているスクリプトにAPIキー、及び、ドメイン名、Peer_idを入力する
 
![image](https://user-images.githubusercontent.com/56616438/124571411-b775d680-de82-11eb-9744-60d9d7d98b83.png)
![image](https://user-images.githubusercontent.com/56616438/124571548-d8d6c280-de82-11eb-83fd-3e7c0bcf7880.png)

3. SkyWay-WebRTC-GW を動作させる(リモートのマシンを利用する場合は、その管理者に動作していることを確認する）
4. 接続される側のWebページをbrowserで開く。（リモートのサーバーを利用する場合は、その管理者にURLを確認する）
5. Unity Editorで実行する
6. Webページに表示されているMyIDをコピーして、Unityの画面の上側のテキストボックスに入力し、Callボタンを押下する
　 （ここでシグナリングが完了する）
　WEBページ↓
 
  ![image](https://user-images.githubusercontent.com/56616438/124572438-9eb9f080-de83-11eb-9d6f-d0e7bd4877d2.png)

　Unity↓
 
  ![image](https://user-images.githubusercontent.com/56616438/124572627-bf824600-de83-11eb-80dc-71856a4fe019.png)

7. Send Messageボタンを押下すると、サンプルテキストがWebページに届く
  　ブラウザにメッセージが表示される↓
   
   ![image](https://user-images.githubusercontent.com/56616438/124574974-f3f70180-de85-11eb-94ac-00289e94b478.png)


## License
WARRANTIES OR CONDITIONS
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


ご自由に活用いただいて構いませんが、直接的及び間接的な影響について一切の保証は致しません。
