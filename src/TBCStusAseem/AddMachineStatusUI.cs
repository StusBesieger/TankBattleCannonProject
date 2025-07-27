using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using Modding;
using Modding.Serialization;
using Modding.Modules;
using Modding.Blocks;
using Modding.Common;
using skpCustomModule;
using Vector3 = UnityEngine.Vector3;
using USlider = UnityEngine.UI.Slider;

namespace TBCStusSpace
{
    public class GUIBlockSelector :SingleInstance<GUIBlockSelector>
    {
        public Dictionary<int, Type> BlockDict = new Dictionary<int, Type>
        {
            //スタートブロック
            {0, typeof(HaveMachineStatus) },
        };
        public override string Name
        {
            get
            {
                return "Addmachinestatus";
            }
        }
        public void Awake()
        {
            Events.OnBlockInit += new Action<Block>(AddScript);
        }
        public void AddScript(Block block)
        {
            BlockBehaviour internalObject = block.BuildingBlock.InternalObject;

            if(BlockDict.ContainsKey(internalObject.BlockID))
            {
                Type type = BlockDict[internalObject.BlockID];
                try
                {
                    if(internalObject.GetComponent(type) == null)
                    {
                        internalObject.gameObject.AddComponent(type);
                    }
                }
                catch
                {
                    Mod.Error("Add GUIBlockSelector Error");
                }
                return;
            }
        }
    }
    public class HaveMachineStatus : ModBlockBehaviour
    {
        private BlockBehaviour bb;

        public GameObject playerObject;
        public StorageLocation storageLocation;

        private int TimeRepeat = 50;
        private int TimeCount = 0;
        private GameObject BuildPGameObject;
        private GameObject GUIMachineStatusMaster;
        private Transform GUIMSMasterT;
        private ArmorScript[] ChildObjects;
        private int ChildCount = 0;
        private float totalArmor = 0f;
        private float totaldispersalArmor = 0f;
        public float armorAverage = 25f;
        public float armorDispersal = 0f;
        public float totalDmass = 0f;
        public float Dmass = 0f;
        private int BlockPlayerID =0 ;
        private string MachineName;
        private List<Player> players;
        private string PlayerName;

        public GameObject parentSim;
        public WaterCannonController[] waterCannonControllers;
        private float waterC1 = 1f;
        private float waterC2 = 1f;
        public float waterpowerchange = 1f;
        public void Awake()
        {
            bb = this.GetComponent<BlockBehaviour>();
            if (bb.isBuildBlock)
            {
                //OnBlockPlaced();
                
            }
            else
            {
                OnSimulateStart();
            }
        }
        public void FixedUpdate()
        {
            if (bb.isBuildBlock)
            {
                BuildingInFixedUpdate();
                
            }
            
        }
        public override void OnBlockPlaced()
        {
            BuildPGameObject = this.transform.parent.gameObject;
            GUIMachineStatusMaster = GameObject.Find("TBCGuiController");
            GUIMSMasterT = GUIMachineStatusMaster.transform.Find("TBC_GUI");

            if(playerObject == null)
            {
                playerObject = new GameObject("Player Data");
                playerObject.transform.parent = GUIMSMasterT.transform;
                storageLocation = playerObject.GetComponent<StorageLocation>();
            }
            if(storageLocation == null)
            {
                playerObject.AddComponent<StorageLocation> ();
                storageLocation = playerObject.GetComponent<StorageLocation>();
            }

            if(StatMaster.isMP)
            {
                BlockPlayerID = bb.ParentMachine.PlayerID;
                MachineName = bb.ParentMachine.Name;
            }
            else
            {
                BlockPlayerID = 0;
                MachineName = bb.ParentMachine.Name;
            }
            if (StatMaster.isMP)
            {
                players = Player.GetAllPlayers();
                foreach (Player child in players)
                {
                    if (child.InternalObject.networkId == BlockPlayerID)
                    {
                        PlayerName = child.InternalObject.name;
                    }
                }
            }
            else
            {
                PlayerName = "player";
            }
        }
        public void BuildingInFixedUpdate()
        {
            if(TimeCount >= TimeRepeat)
            {
                totalArmor = 0f;
                totaldispersalArmor = 0f;
                totalDmass = 0f;
                GetBlockStatus();
                TimeCount = 0;
            }
            TimeCount += 1;
        }
        public void GetBlockStatus()
        {
            ChildObjects = BuildPGameObject.gameObject.GetComponentsInChildren<ArmorScript>();
            ChildCount = ChildObjects.Length;
            //平均値
            foreach(ArmorScript ChildObject in ChildObjects)
            {
                totalArmor += ChildObject.armorthickness;
                if (ChildObject.armorthickness > 150f)
                {
                    totalDmass += ChildObject.Dmass * (ChildObject.armorvalue / 5f - 20f);
                }
                else if(ChildObject.armorvalue > 100f)
                {
                    totalDmass += ChildObject.Dmass * (ChildObject.armorvalue * 6f / 50f - 8f);
                }
                else
                {
                    totalDmass += ChildObject.Dmass * ChildObject.armorvalue / 25f;
                }


            }
            armorAverage = (float)Math.Round(totalArmor / ChildCount * 10) / 10;
            //分散
            foreach (ArmorScript ChildObject in ChildObjects)
            {
                totaldispersalArmor += (float)Math.Pow(ChildObject.armorthickness - armorAverage, 2);
            }
            armorDispersal = (float)Math.Round((float)Math.Pow(totaldispersalArmor / ChildCount , 0.5)* 10f) / 10f;
            waterC1 = Mathf.Pow(Mathf.Clamp(armorAverage / 10f - 2f, 0.5f, 8f) * armorDispersal * armorDispersal, 0.5f);
            waterC2 = Mathf.Clamp(25f - 8f * Mathf.Log(waterC1, 5f), 0.5f, 12f);
            waterpowerchange = (float)Math.Round(Mathf.Clamp(waterC2, 0.5f, 8f) * 10f) / 10f;
            if (storageLocation != null)
            {
                if (StatMaster.isMP)
                {
                    //データを転送
                    storageLocation.PlayerID = BlockPlayerID;
                    storageLocation.PlayerName = PlayerName;
                    storageLocation.MachineName = MachineName;
                    storageLocation.armorAverage = armorAverage.ToString();
                    storageLocation.armorDispersal = armorDispersal.ToString();
                    storageLocation.totalmass = totalDmass.ToString();
                    storageLocation.waterpower = waterpowerchange.ToString();
                }
                else
                {
                    storageLocation.PlayerID = BlockPlayerID;
                    storageLocation.PlayerName = PlayerName;
                    storageLocation.MachineName = MachineName;
                    storageLocation.armorAverage = armorAverage.ToString();
                    storageLocation.armorDispersal = armorDispersal.ToString();
                    storageLocation.totalmass = totalDmass.ToString();
                    storageLocation.waterpower = waterpowerchange.ToString();
                }
            }
        }
        public override void OnSimulateStart()
        {
            parentSim = this.gameObject;
            while(parentSim.gameObject.name != "Simulation Machine")
            {
                parentSim = parentSim.transform.parent.gameObject;
            }
            waterCannonControllers = parentSim.GetComponentsInChildren<WaterCannonController>();
            if(waterCannonControllers != null)
            {
                StartCoroutine(WaterPowerChange());
            }
        }
        IEnumerator WaterPowerChange()
        {
            yield return new WaitForFixedUpdate();
            foreach (WaterCannonController waterCannonController in waterCannonControllers)
            {
                waterCannonController.StrengthSlider.Value = waterpowerchange;
                waterCannonController.StrengthSlider.SetValue(waterpowerchange);
            }
        }
        public void OnDisable()
        {
            Destroy(playerObject);
        }
        public void OnEnable()
        {
            if (bb.isBuildBlock)
            {
                OnBlockPlaced();
            }
        }
    }
    public class StorageLocation : MonoBehaviour
    {
        public int PlayerID = 0;
        public string PlayerName = "player";
        public string MachineName   = "machine";
        public string armorAverage = "25";
        public string armorDispersal = "0";
        public string totalmass = "0";
        public string waterpower = "0";
    }
    public class AddMachineStatusUI : SingleInstance<AddMachineStatusUI>
    {
        private int playercount ;
        private Rect windowRect;
        private Rect windowRect2;
        public string[,] MachineStatus;
        private bool windowOK;
        private int windowId;
        private int windowId2;
        private int TimeCount = 0;
        private int TimeRepeat = 100;
        public StorageLocation storageLocation;
        public GameObject FileBrowserView;
        public GameObject ReturnToMenu;
        public GameObject ServerMane;

        public override string Name
        {
            get
            {
                return "TBC_GUI";
            }
        }
        public void FixedUpdate()
        {
            if (TimeCount >= TimeRepeat)
            {
                GetBlockData();
                TimeCount = 0;
            }
            TimeCount += 1;
        }
        public void GetBlockData()
        {
            if (!StatMaster.isMainMenu && !StatMaster._customLevelSimulating)
            {
                playercount = this.transform.childCount;
                MachineStatus = new string[playercount, 6];
                for (int i = 0; i < playercount; i++)
                {
                    storageLocation = this.transform.GetChild(i).GetComponent<StorageLocation>();
                    if (storageLocation != null)
                    {
                        MachineStatus[i, 0] = storageLocation.PlayerName;
                        MachineStatus[i, 1] = storageLocation.MachineName;
                        MachineStatus[i, 2] = storageLocation.armorAverage;
                        MachineStatus[i, 3] = storageLocation.armorDispersal;
                        MachineStatus[i, 4] = storageLocation.totalmass;
                        MachineStatus[i, 5] = storageLocation.waterpower;
                    }
                }
            }
        }
        public void Awake()
        {
            windowId = ModUtility.GetWindowId();
            windowId2 = ModUtility.GetWindowId();
            if (FileBrowserView == null)
            {
                FileBrowserView = GameObject.Find("HUD").transform.Find("FileBrowserView").gameObject;
                ReturnToMenu = GameObject.Find("HUD").transform.Find("RETURN TO MENU").gameObject;
                ServerMane = GameObject.Find("HUD").transform.Find("SERVER MANAGEMENT").gameObject;
            }
        }
        public void OnGUI()
        {
            if (!StatMaster.isMainMenu && !StatMaster._customLevelSimulating && !StatMaster.isLocalSim && !StatMaster.levelSimulating && !FileBrowserView.activeSelf && !ReturnToMenu.activeSelf && !ServerMane.activeSelf)
            {
                windowRect2 = new Rect(250, 800, 150, 50);
                windowRect2 = GUILayout.Window(windowId2, windowRect2, delegate
                 {
                    windowOK = GUILayout.Toggle(windowOK,"Open");
                    GUI.DragWindow();
                 }, "Status Open?");

                windowRect = new Rect(750, 200, 550, 100 + 10 * playercount);
                if(windowOK)
                {
                    windowRect = GUILayout.Window(windowId, windowRect, delegate (int windowId)
                    {
                        GUILayout.BeginHorizontal("box");

                        GUILayout.Label("Player Name  ");
                        GUILayout.Label("Machine Name  ");
                        GUILayout.Label("Armor Average  ");
                        GUILayout.Label("Armor Standard Deviation  ");
                        GUILayout.Label("Machine Mass  ");
                        GUILayout.Label("Machine Water Power");

                        GUILayout.EndHorizontal();

                        for (int i = 0; i < playercount; i++)
                        {
                            GUILayout.BeginHorizontal("box");

                            GUILayout.Label(MachineStatus[i, 0]);
                            GUILayout.Label(MachineStatus[i, 1]);
                            GUILayout.Label(MachineStatus[i, 2] + " mm");
                            GUILayout.Label(MachineStatus[i, 3]);
                            GUILayout.Label(MachineStatus[i, 4]);
                            GUILayout.Label(MachineStatus[i, 5]);

                            GUILayout.EndHorizontal();
                        }
                    }
                    , "Machine Status");
                }

            }
        }


    }
}
