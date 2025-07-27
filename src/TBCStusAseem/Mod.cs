using System;
using System.Collections.Generic;
using Modding;
using Modding.Blocks;
using UnityEngine;
using UnityEngine.UI;

namespace TBCStusSpace
{
	public class Mod : ModEntryPoint
	{
		public static GameObject TBC_UI;
		public static GameObject TBCController;
		public static GameObject TBCGUI;

		public static ModAssetBundle modAssetBundle;

		public static Dictionary<int, bool> RangeboolDict;
		public static Dictionary<int, string> RangeDistanceDict;
		public static Dictionary<int, bool> SpotEffecboolDict;
		public static Dictionary<int, Vector3> SpotPositionDict;
		public static Dictionary<int, bool> AmmoEffecboolDict;
		public static Dictionary<int, Vector3> AmmoPositionDict;

		public static Message RangeBmessage;
		public static Message RangeDmessage;
		public static MessageType RangeBmessageType;
		public static MessageType RangeDmessageType;

		public static Message Spotboolmessage;
		public static Message SpotPositionmessage;
		public static MessageType SpotboolmessageTyope;
		public static MessageType SpotPositionmessageType;

		public static Message Ammoboolmessage;
		public static Message AmmoPositionmessage;
		public static MessageType AmmoboolmessageTyope;
		public static MessageType AmmoPositionmessageType;
		public static void Log(string msg)
		{
			Debug.Log("TBC Log: " + msg);
		}
		public static void Warning(string msg)
		{
			Debug.LogWarning("TBC Warning: " + msg);
		}
		public static void Error(string msg)
		{
			Debug.LogError("TBC Error: " + msg);
		}

		public override void OnLoad()
		{

			//各ModuleとBehaviourをセットにし、XML上で使えるように
			Modding.Modules.CustomModules.AddBlockModule<TBCAddProjectile, TBCAddProjectileBehaviour>("TBCAddProjectile", true);
			Modding.Modules.CustomModules.AddBlockModule<TBCAddAPModule, TBCAddAPBehaviour>("TBCAddAPModule", true);
			Modding.Modules.CustomModules.AddBlockModule<TBCAddHEModule, TBCAddHEBehaviour>("TBCAddHEModule", true);
			Modding.Modules.CustomModules.AddBlockModule<TBCAddSpotModule, TBCAddSpotBehaviour>("TBCAddSpotModule", true);
			Modding.Modules.CustomModules.AddBlockModule<TBCAmmoUIModule, TBCAmmoUIBehaviour>("TBCAmmoUIModule", true);
			Modding.Modules.CustomModules.AddBlockModule<TBCAddRangeFinderModule, TBCAddRangeFinderBehaviour>("TBCAddRangeFinderModule", true);
			Modding.Modules.CustomModules.AddBlockModule<TBCAddFireExtinguisherModule, TBCAddFireExtinguisherBehaviour>("TBCAddFireExtinguisherModule", true);
			TBCController = new GameObject("TBCController");
			UnityEngine.Object.DontDestroyOnLoad(TBCController);

			SingleInstance<AdArmorModule>.Instance.transform.parent = TBCController.transform;
			SingleInstance<GUIBlockSelector>.Instance.transform.parent = TBCController.transform;
			//SNB_UIを作成、Canvasを追加
			TBC_UI = new GameObject("TBC UI");
			UnityEngine.Object.DontDestroyOnLoad(TBC_UI);
			Canvas val = TBC_UI.AddComponent<Canvas>();
			val.renderMode = 0;
			val.sortingOrder = 0;
			val.gameObject.layer = LayerMask.NameToLayer("HUD");
			TBC_UI.AddComponent<CanvasScaler>().scaleFactor = 1f;   //画面サイズに応じてUIをスケーリングするためのコンポーネントをアタッチする

			TBCGUI = new GameObject("TBCGuiController");
			UnityEngine.Object.DontDestroyOnLoad(TBCGUI);
			SingleInstance<AddMachineStatusUI>.Instance.transform.parent = TBCGUI.transform;
																	// Called when the mod is loaded.
			switch (Application.platform)   //OS毎に変更
			{
				case RuntimePlatform.WindowsPlayer:
					modAssetBundle = ModResource.GetAssetBundle("myasset");
					break;
				case RuntimePlatform.OSXPlayer:
					modAssetBundle = ModResource.GetAssetBundle("myassetmac");
					break;
				case RuntimePlatform.LinuxPlayer:
					modAssetBundle = ModResource.GetAssetBundle("myassetmac");
					break;
				default:
					modAssetBundle = ModResource.GetAssetBundle("myasset");
					break;
			}
			RangeboolDict = new Dictionary<int, bool>();
			RangeDistanceDict = new Dictionary<int, string>();

			SpotEffecboolDict = new Dictionary<int, bool>();
			SpotPositionDict = new Dictionary<int, Vector3>();

			AmmoEffecboolDict = new Dictionary<int, bool>();
			AmmoPositionDict = new Dictionary<int, Vector3>();

			RangeBmessageType = ModNetworking.CreateMessageType(DataType.Integer, DataType.Boolean);
			ModNetworking.Callbacks[RangeBmessageType] += new Action<Message>(ApplyRangeB);
			RangeDmessageType = ModNetworking.CreateMessageType(DataType.Integer, DataType.String);
			ModNetworking.Callbacks[RangeDmessageType] += new Action<Message>(ApplyRangeD);

			SpotboolmessageTyope = ModNetworking.CreateMessageType(DataType.Integer, DataType.Boolean);
			ModNetworking.Callbacks[SpotboolmessageTyope] += new Action<Message>(ApplySpotB);
			SpotPositionmessageType = ModNetworking.CreateMessageType(DataType.Integer, DataType.Vector3);
			ModNetworking.Callbacks[SpotPositionmessageType] += new Action<Message>(ApplySpotP);

			AmmoboolmessageTyope = ModNetworking.CreateMessageType(DataType.Integer, DataType.Boolean);
			ModNetworking.Callbacks[AmmoboolmessageTyope] += new Action<Message>(ApplyAmmoB);
			AmmoPositionmessageType = ModNetworking.CreateMessageType(DataType.Integer, DataType.Vector3);
			ModNetworking.Callbacks[AmmoPositionmessageType] += new Action<Message>(ApplyAmmoP);
		}
		public static void ApplyRangeB(Message message)
        {
			RangeboolDict[(int)message.GetData(0)] = (bool)message.GetData(1);

		}
		public static void ApplyRangeD(Message message)
        {
			RangeDistanceDict[(int)message.GetData(0)] = (string)message.GetData(1);

		}
		public static void ApplySpotB(Message message)
		{
			SpotEffecboolDict[(int)message.GetData(0)] = (bool)message.GetData(1);

		}
		public static void ApplySpotP(Message message)
		{
			SpotPositionDict[(int)message.GetData(0)] = (Vector3)message.GetData(1);

		}
		public static void ApplyAmmoB(Message message)
		{
			AmmoEffecboolDict[(int)message.GetData(0)] = (bool)message.GetData(1);

		}
		public static void ApplyAmmoP(Message message)
		{
			AmmoPositionDict[(int)message.GetData(0)] = (Vector3)message.GetData(1);

		}
	}
}
