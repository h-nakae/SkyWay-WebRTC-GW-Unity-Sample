using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UniRx;
using UnityEngine;
using MiniJSON;
using UnityEngine.UI;
using UnityEngine.Networking;
 

//Unityから直接叩かれるクラス
public class SkyWayRestApi : MonoBehaviour
{
	public Button ConnectButton;
	//public Button CloseButton;
	public InputField TargetIdField;
	public string key;
	public string domain;
	public string peerId;
	public bool turn;

	private System.Net.Sockets.UdpClient _udp = new System.Net.Sockets.UdpClient();

	void Start()
	{
		// initialize
		SkyWayRestApi skyWayRestApi = this.gameObject.GetComponent<SkyWayRestApi>();
		MyConnectionInfo._entryPoint = skyWayRestApi.domain;

		//SkyWay WebRTC Gateway操作用インスタンス生成
		var _restApi = new RestApi(key, domain, peerId, turn);

		//SkyWayサーバと接続されたときに発火させるイベント
		_restApi.OnOpen += () =>
		{
			ConnectButton.onClick.AsObservable().Select(x => TargetIdField.text).Where(x => x != "").Subscribe(x =>
			{
				// 接続相手のPeer IDをCallする。接続完了後はUDPによるテキスト送信が可能
				_restApi.DataCall(x);
			});
		};	
	}

	/// <summary>
	/// サンプル用UDP送信メソッド
	/// </summary>
	public void sendText()
	{
        //UdpClientオブジェクトを作成する
        string sendMsg = "test message 1234,5678";
        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(sendMsg);

        //リモートホストを指定してデータを送信する
        _udp.Send(sendBytes, sendBytes.Length, remoteHostInfo._remotehost, remoteHostInfo._remotePort);

        //UdpClientを閉じる
        //udp.Close();
	}

	public void close() {
		StartCoroutine(closeEnumrator());
	}


	//終了処理
	IEnumerator closeEnumrator()
	{
		var closeDataURL = string.Format("http://{0}/data/connections/{1}", MyConnectionInfo._entryPoint, MyConnectionInfo._data_connection_id);
		var closePeerURL = string.Format("http://{0}/peers/{1}?token={2}", MyConnectionInfo._entryPoint, MyConnectionInfo._peerId, MyConnectionInfo._peerToken);
		
		Debug.Log("close data url: " + closeDataURL);
		Debug.Log("close peer url: " + closePeerURL);

		Debug.Log("berfore delete data");
		UnityWebRequest request = UnityWebRequest.Delete(closeDataURL);
		yield return request.SendWebRequest();
 
        // 通信エラーチェック
        if (request.isNetworkError) {
            Debug.Log(request.error);
        } else {
            if (request.responseCode == 200) {
                // UTF8文字列として取得する
                string text = request.downloadHandler.text;
				Debug.Log("data 200: " + text);
 
                // バイナリデータとして取得する
                byte[] results = request.downloadHandler.data;
            }
        }

		UnityWebRequest peerRequest = UnityWebRequest.Delete(closePeerURL);
		yield return peerRequest.SendWebRequest();
 
        // 通信エラーチェック
        if (peerRequest.isNetworkError) {
            Debug.Log(peerRequest.error);
        } else {
            if (peerRequest.responseCode == 200) {
                // UTF8文字列として取得する
                string text = peerRequest.downloadHandler.text;
				Debug.Log("peer 200: " + text);
 
                // バイナリデータとして取得する
                byte[] results = peerRequest.downloadHandler.data;
            }
        }
	}
}


static public class remoteHostInfo{
	static public int _remotePort;
	static public string _remotehost;
}

static public class MyConnectionInfo
{
	static public string _entryPoint;
	static public string _data_connection_id;
	static public string _peerId;
	static public string _peerToken;
}

//実際にSkyWay WebRTC Gatwayを操作するクラス
class RestApi
{
	//JSON Object定義
	[System.Serializable]
	class PeerOptions
	{
		public string key;
		public string domain;
		public string peer_id;
		public bool turn;
	}


	//JSON Object定義
	[System.Serializable]
	class RedirectParams
	{
		public Redirect video;
		//public Redirect audio;//audioを受信するときはこれも入れる
	}

	[System.Serializable]
	class DataRedirectParams
	{
		public string ip_v4;
		public ushort port;
	}

	//JSON Object定義
	[System.Serializable]
	class Redirect
	{
		public string ip_v4;
		public ushort port;
	}

	[System.Serializable]
	class DataCallParams
	{
		public string peer_id;
		public string token;
		public Options options;
		public string target_id;

		public Params @params;
		public DataRedirectParams redirect_params;
	}

	[System.Serializable]
	class Options
	{
		public string metadata;
		public string serialization;
	}

	[System.Serializable]
	class Params
	{
		public string data_id;
	}

	//SkyWay WebRTC GWを動かしているIPアドレスとポート番号
	const string entryPoint =  "http://192.168.25.117:8000";//"http://localhost:8000";

	//Peer Object生成タイミングでボタンを表示するためのイベント定義
	public delegate void OnOpenHandler();
	public event OnOpenHandler OnOpen;


	private DataCallParams _CreateDataCallParams(string targetId, string data_id)
	{
		var dataCallParams = new DataCallParams();
		dataCallParams.peer_id = MyConnectionInfo._peerId;
		dataCallParams.token = MyConnectionInfo._peerToken;
		dataCallParams.target_id = targetId;

		var options = new Options();
		options.serialization = "NONE";//"BINARY";

		dataCallParams.options = options;
		var para = new Params();
		para.data_id = data_id;
		dataCallParams.@params = para;
		
		var dataRedirectParams = new DataRedirectParams();
		dataRedirectParams.ip_v4 = "127.0.0.1";
		dataRedirectParams.port = 7000;
		dataCallParams.redirect_params = dataRedirectParams;

		return dataCallParams;
	}

	public void DataCall(string targetId)
	{
		Debug.Log("aaaaa");
		var dummy = new Options();
		string dmyString = JsonUtility.ToJson(dummy);
		byte[] dummyBytes = Encoding.UTF8.GetBytes(dmyString);
		ObservableWWW.Post(entryPoint + "/data", dummyBytes).SelectMany(x =>
		{
			var response = Json.Deserialize(x) as Dictionary<string, object>;
			var data_id = (string) response["data_id"];
			remoteHostInfo._remotehost = (string)response["ip_v4"];
			var portNum = response["port"];
			remoteHostInfo._remotePort = Convert.ToInt32(portNum);

			var callParams = _CreateDataCallParams(targetId, data_id.ToString());
			string callParamsString = JsonUtility.ToJson(callParams);
			byte[] callParamsBytes = Encoding.UTF8.GetBytes(callParamsString);

			//SkyWay WebRTC GWのデータ送信確立用APIを叩く
			Debug.Log(callParams);
			Debug.Log("cccc");
			return ObservableWWW.Post(entryPoint + "/data/connections", callParamsBytes);
		}).SelectMany(y => 
		{
			Debug.Log("dddd");
			var res = Json.Deserialize((string) y) as Dictionary<string, object>;
			var parameters = (IDictionary) res["params"];
			MyConnectionInfo._data_connection_id = (string) parameters["data_connection_id"];			

			//この時点でSkyWay WebRTC GWは接続処理を始めている
			//発信側でやることはもうないが、相手側が応答すると自動で動画が流れ始めるため、
			//STREAMイベントを取って流れ始めたタイミングを確認しておくとボタン表示等を消すのに使える
			string url = string.Format("{0}/data/connections/{1}/events", entryPoint, MyConnectionInfo._data_connection_id);

			return ObservableWWW.Get(url);
		}).Where(z =>
		{
			Debug.Log("eeeee");
			//STREAMイベント以外はいらないのでフィルタ
			var res = Json.Deserialize((string) z) as Dictionary<string, object>;
			return (string) res["event"] == "OPEN";
		}).First().Subscribe(//今回の用途だと最初の一回だけ取れれば良い
			x =>
			{
				//ビデオが正常に流れ始める
				//今回はmrayGStreamerUnityで受けるだけだが、ビデオを送り返したい場合はこのタイミングで
				//SkyWay WebRTC GW宛にRTPパケットの送信を開始するとよい
				//OnStream();
				Debug.Log("text has beed started redirecting");
			}, ex => { Debug.LogError(ex); });
	}

	

	private void _OnDataOpen()
	{
		//UnityのGUI処理をするためにイベントを返してやる
		OnOpen();

		//イベントを監視する
		//今回は着呼イベントしか監視していないが、他にもDataChannel側の着信処理等のイベントも来る
		//これはプログラム起動中はずーっと監視しておくのが正しい。なのでRepeatする。
		var longPollUrl = string.Format("{0}/peers/{1}/events?token={2}", entryPoint, MyConnectionInfo._peerId, MyConnectionInfo._peerToken);
		ObservableWWW.Get(longPollUrl).OnErrorRetry((Exception ex) => { }).Repeat().Where(wx =>
		{
			Debug.Log(wx);
			var res = Json.Deserialize(wx) as Dictionary<string, object>;
			Debug.Log(res.ContainsKey("event"));
			Debug.Log(res["event"]);
			return res.ContainsKey("event") && (string) res["event"] == "CALL";
		}).First().Subscribe(sx =>//今回はCALLイベントしか見る気がないので一回だけ処理できればいいが、複数の相手と接続するときはFirstではまずい
		{
			//相手からCallがあったときに発火。応答処理を始める
			var response = Json.Deserialize(sx) as Dictionary<string, object>;

			// ★TODO DataChannelへの変更対応 一旦は受信処理は実装しない
			// var callParameters = (IDictionary) response["call_params"];
			// _data_connection_id = (string) callParameters["data_connection_id"];
			//応答処理をする
			//_DataAnswer(_data_connection_id);
		}, ex => { Debug.LogError(ex); });
	}

	/// <summary>
	/// Skywway WebRTC Gateway経由でシグナリング
	/// peeridとpeertokenを取得してローカルに保持
	/// </summary>
	/// <param name="key">APIキー</param>
	/// <param name="domain">GWのドメイン</param>
	/// <param name="peerId">自分のpeerID</param>
	/// <param name="turn">turnを有効化するか</param>
	public RestApi(string key, string domain, string peerId, bool turn)
	{
		var peerParams = new PeerOptions();
		peerParams.key = key;
		peerParams.domain = domain;
		peerParams.peer_id = peerId;
		peerParams.turn = turn;
		string peerParamsJson = JsonUtility.ToJson(peerParams);
		byte[] peerParamsBytes = Encoding.UTF8.GetBytes(peerParamsJson);
		//SkyWayサーバとの接続開始するためのAPIを叩く
		ObservableWWW.Post(entryPoint + "/peers", peerParamsBytes).Subscribe(x =>
		{
			//この時点ではSkyWay WebRTC GWが「このPeer IDで処理を開始する」という応答でしかなく、
			//SkyWayサーバで利用できるPeer IDとは限らない(重複で弾かれる等があり得るので)
			var response = Json.Deserialize(x) as Dictionary<string, object>;
			var parameters = (IDictionary) response["params"];
			var peer_id = (string) parameters["peer_id"];
			var token = (string) parameters["token"];
			//SkyWayサーバとSkyWay WebRTC Gatewayが繋がって初めてPeer ID等が正式に決定するので、
			//イベントを監視する
			var url = string.Format("{0}/peers/{1}/events?token={2}", entryPoint, peer_id, token);
			ObservableWWW.Get(url).Repeat().Where(wx =>
			{
				//この時点ではOPENイベント以外はいらないので弾く
				var res = Json.Deserialize(wx) as Dictionary<string, object>;
				return res.ContainsKey("event") && (string) res["event"] == "OPEN";
			}).First().Subscribe(sx => //ここでは最初の一回しか監視しない。着信等のイベントは後で別の場所で取ることにする
			{
				var response_j = Json.Deserialize(sx) as Dictionary<string, object>;
				var parameters_s = (IDictionary) response_j["params"];
				//正式決定したpeer_idとtokenを記録しておく
				MyConnectionInfo._peerId = (string) parameters_s["peer_id"];
				MyConnectionInfo._peerToken = (string) parameters_s["token"];
				
				//SkyWayサーバと繋がったときの処理を始める
				_OnDataOpen();
			}, ex =>
			{
				//ここが発火する場合は多分peer_idやtoken等が間違っている
				//もしくはSkyWay WebRTC GWとSkyWayサーバの間で通信ができてない
				Debug.LogError(ex);
			});

		}, ex =>
		{
			//ここが発火する場合はSkyWay WebRTC GWと通信できてないのでは。
			//そもそも起動してないとか
			//他には、前回ちゃんとClose処理をしなかったため前のセッションが残っている場合が考えられる。
			//その場合はWebRTC GWを再起動するか、別のPeer IDを利用する
			//時間が経てば勝手に開放されるのでそこまで気にしなくてもよい(気にしなくてもいいとは言ってない)
			Debug.LogError("error");
			Debug.LogError(ex);
		});
	}
}

