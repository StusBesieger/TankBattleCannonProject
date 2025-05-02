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
        public MKeyReference SpotKey;

        [XmlElement("FindRange")]
        [DefaultValue(0f)]
        [Reloadable]
        public float SpotRange;

    }
    public class TBCAddRangeFinderBehaviour : BlockModuleBehaviour<TBCAddRangeFinderModule>
    {
        public BlockBehaviour bb;
        public int blockID;
        public int Relaodnum = 0;
        private Vector3 ThisDirection;
        public MKey SpotStart;
        public Collider mCollider;
        public float Range; 
        public LayerMask Blocklayermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        private RaycastHit hit;
        private bool Spottrue = false;
        public string SpotReload ;
        public bool isOwnerSame = false;
        private int windowId;
        private Rect windowRect = new Rect(375, 800, 175,50);
        private bool RangeOK = false;
        public int RangeTime = 0;
        private string rangeS = "No Find";
        public bool press = false;
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();

            Range = Module.SpotRange;
            blockID = BlockId;
            windowId = ModUtility.GetWindowId();
            try
            {
                SpotStart = GetKey(Module.SpotKey);
            }
            catch
            {
                Mod.Error("BlockID" + blockID + "error");
            }
            UpdateOwnerFlag();
        }
        private void UpdateOwnerFlag()  //プレイヤーとブロックの親が一緒か確認する関数
        {
            if (StatMaster.isMP)
            {
                ushort BlockPlayerID = BlockBehaviour.ParentMachine.PlayerID;
                ushort LocalPlayerID = PlayerMachine.GetLocal().Player.NetworkId;
                isOwnerSame = BlockPlayerID == LocalPlayerID;
            }
            else
            {
                isOwnerSame = true;
            }
        }
        public override void OnSimulateStop()
        {
            base.OnSimulateStop();
            isOwnerSame = false;
        }
        public override void SafeAwake()
        {
            base.SafeAwake();
        }
        //キーを押された時の処理。
        public override void SimulateUpdateAlways()
        {
            base.SimulateUpdateAlways();
            if(SpotStart.IsPressed || SpotStart.EmulationPressed())
            {
                Debug.Log("presse");
                press = true;
            }
        }
        public override void SimulateUpdateHost()
        {
            base.SimulateUpdateHost();
            ThisDirection = - transform.up ;
            if (press)
            {
                Debug.Log("presse updateHost");
                if(Relaodnum == 0)
                {
                    if (Physics.SphereCast(this.transform.position + 3f * ThisDirection, 0.25f, ThisDirection, out hit, Range, Blocklayermask))
                    {
                        RangeOK = true;

                    }
                    Relaodnum = 1;
                }
                press = false;
            }
        }
        public override void SimulateFixedUpdateHost()
        {
            base.SimulateFixedUpdateHost();
            if (Relaodnum != 0)
            {
                Relaodnum++;
            }
            if (Relaodnum == 1000)
            {
                Relaodnum = 0;
            }
            if (0 != Relaodnum)
            {
                SpotReload = (Relaodnum / 10).ToString() + " %";
            }
            if (0 == Relaodnum)
            {
                SpotReload = "RangeFinder OK";
            }
            if(RangeOK)
            {
                if (Physics.SphereCast(this.transform.position + 3f * ThisDirection, 0.5f, ThisDirection, out hit, Range, Blocklayermask))
                {
                    if(Relaodnum < 150)
                    {
                        RangeTime++;
                    }
                }
                if(Relaodnum > 150)
                {
                    RangeTime = 0;
                    RangeOK = false;
                    rangeS = "miss!";
                }

            }
        }
        public override void SimulateFixedUpdateAlways()
        {
            base.SimulateFixedUpdateAlways();
            if (Relaodnum == 550)
            {
                rangeS = "No Find";
            }
            if (RangeTime == 100)
            {
                rangeS = Math.Round(Vector3.Distance(this.transform.position, hit.point), 1).ToString();
                Debug.Log(rangeS);
                RangeTime = 0;
                RangeOK = false;
            }
        }
        public void OnGUI()
        {
            if(isOwnerSame )
            {
                windowRect = GUILayout.Window(windowId, windowRect, delegate (int windowId)
                {
                GUILayout.Label(SpotReload);
                if (RangeTime < 100 && 1 < RangeTime )
                {
                    GUILayout.Label(RangeTime.ToString() + " % Time limit " + ((int)(Relaodnum / 1.5)).ToString() + " %");
                }else
                    {
                        GUILayout.Label(rangeS);
                    }
                }
                , "Spot Reload");
            }
        }
    }
}
