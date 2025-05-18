using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using Modding;
using Modding.Serialization;
using Modding.Modules;
using Modding.Blocks;
using skpCustomModule;
using Vector3 = UnityEngine.Vector3;

namespace TBCStusSpace
{
    [XmlRoot("TBCAddRangeFinderModule")]
    [Reloadable]
    public class TBCAddRangeFinderModule : BlockModule
    {
        [XmlElement("RangeFindKey")]
        [RequireToValidate]
        public MKeyReference FindKey;

        [XmlElement("FindRange")]
        [DefaultValue(0f)]
        [Reloadable]
        public float FindRange;

    }
    public class TBCAddRangeFinderBehaviour : BlockModuleBehaviour<TBCAddRangeFinderModule>
    {
        public int DictID = 0;
        private BlockBehaviour blockBehaviour;

        public MKey findKey;
        private bool findOK = false;
        public float findrange;
        private RaycastHit hit;
        public Vector3 ThisDirection;
        public LayerMask Blocklayermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        private bool isOwnerSame = false;
        public string enemyrange = "NoTarget";
        public bool ReLoading = true;

        private int windowId;
        private Rect windowRect = new Rect(375, 800, 175, 50);
        public override void OnBlockPlaced()
        {
            base.OnBlockPlaced();
        }
        public  void Awake()
        {
            blockBehaviour = this.GetComponent<BlockBehaviour>();

            if(blockBehaviour.isBuildBlock)
            {
                while (Mod.RangeboolDict.ContainsKey(DictID))
                {
                    DictID++;
                }
                Mod.RangeboolDict.Add(DictID, value: false);
                Mod.RangeDistanceDict.Add(DictID, value:"NoTarget");
            }
        }
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            findKey = GetKey(Module.FindKey);
            findrange = Module.FindRange;
            UpdateOwnerFlag();
            windowId = ModUtility.GetWindowId();
        }
        public override void OnSimulateStop()
        {
            base.OnSimulateStop();
            isOwnerSame = false;
            Mod.RangeboolDict.Remove(DictID);
            Mod.RangeDistanceDict.Remove(DictID);
        }
        public override void SimulateFixedUpdateHost()
        {
            base.SimulateFixedUpdateHost();
            if(findOK)
            {
                ThisDirection = -transform.up;
                if(Physics.SphereCast(this.transform.position + 3f * ThisDirection, 0.25f, ThisDirection, out hit, findrange, Blocklayermask))
                {
                    StartCoroutine(RangeFinding());
                }
                findOK = false;
                ReLoading = false;
                Mod.RangeBmessage = Mod.RangeBmessageType.CreateMessage(DictID, ReLoading);
                ModNetworking.SendToAll(Mod.RangeBmessage);
                StartCoroutine(RangeReLoding());
            }
        }
        public override void SimulateUpdateAlways()
        {
            base.SimulateUpdateAlways();
            RangeFinde();
        }
        public override void SimulateUpdateClient()
        {
            base.SimulateUpdateClient();
            enemyrange = Mod.RangeDistanceDict[DictID];
            ReLoading = Mod.RangeboolDict[DictID];
        }
        public override void SimulateUpdateHost()
        {
            base.SimulateUpdateHost();

        }
        private void RangeFinde()
        {
            if (findKey.IsPressed || findKey.EmulationPressed())
            {
                if(ReLoading)
                {
                    findOK = true;
                }
            }
        }
        private void UpdateOwnerFlag()  //プレイヤーとブロックの親が一緒か確認する関数
        {
            if (StatMaster.isMP)
            {
                ushort num;
                try
                {
                    num = base.BlockBehaviour.ParentMachine.PlayerID;
                }
                catch
                {
                    num = 50;
                }
                ushort num2;
                try
                {
                    num2 = PlayerMachine.GetLocal().Player.NetworkId;
                }
                catch
                {
                    num2 = 100;
                }
                isOwnerSame = num == num2;
            }
            else
            {
                isOwnerSame = true;
            }
        }
        IEnumerator RangeFinding()
        {
            int num = 0;
            for(int i = 0; i < 100; i++)
            {
                if(Physics.SphereCast(this.transform.position + 3f * ThisDirection, 0.25f, ThisDirection, out hit, findrange, Blocklayermask))
                {
                    num++;
                    enemyrange = num.ToString() + " %";
                    Mod.RangeDmessage = Mod.RangeDmessageType.CreateMessage(DictID, enemyrange);
                    ModNetworking.SendToAll(Mod.RangeDmessage);
                }
                yield return new WaitForFixedUpdate();
            }
            if(num > 75)
            {
                enemyrange = Math.Round(Vector3.Distance(this.transform.position, hit.point), 1).ToString();
                Mod.RangeDmessage = Mod.RangeDmessageType.CreateMessage(DictID, enemyrange);
                ModNetworking.SendToAll(Mod.RangeDmessage);
            }
            else
            {
                enemyrange = "miss!";
                Mod.RangeDmessage = Mod.RangeDmessageType.CreateMessage(DictID, enemyrange);
                ModNetworking.SendToAll(Mod.RangeDmessage);
            }
        }

        IEnumerator RangeReLoding()
        {
            yield return new WaitForSeconds(7);
            ReLoading = true;
            Mod.RangeBmessage = Mod.RangeBmessageType.CreateMessage(DictID, ReLoading);
            ModNetworking.SendToAll(Mod.RangeBmessage);
        }
        public void OnGUI()
        {
            if(isOwnerSame)
            {
                windowRect = GUILayout.Window(windowId, windowRect, delegate (int windowId)
                {
                    if (ReLoading)
                    {
                        GUILayout.Label("FindOK");
                    }
                    else
                    {
                        GUILayout.Label("ReLoading");
                    }
                    GUILayout.Label(enemyrange);
                }
                , "RangeFinder");
            }
        }
    }
}
