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
		}

	}
}
