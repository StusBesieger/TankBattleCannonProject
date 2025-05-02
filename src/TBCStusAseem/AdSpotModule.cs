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
    [XmlRoot("TBCAddSpotModule")]
    [Reloadable]
    public class TBCAddSpotModule : BlockModule
    {
        [XmlElement("SpotKey")]
        [RequireToValidate]
        public MKeyReference SpotKey;

        [XmlElement("SpotRange")]
        [DefaultValue(0f)]
        [Reloadable]
        public float SpotRange;

    }
    public class TBCAddSpotBehaviour : BlockModuleBehaviour<TBCAddSpotModule>
    {
        public int blockID;
        private int Spotnum = 0;
        public string SpotEffectName;
        private Vector3 ThisDirection;
        public GameObject EffectPrefab;
        public GameObject EffectObject;
        public ParticleSystem particleSystem;
        public MKey SpotStart;
        public Collider mCollider;
        public float Range; 
        public LayerMask Blocklayermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        private RaycastHit hit;
        private bool Spottrue = false;
        private string SpotReload;
        public bool isOwnerSame = false;
        private int windowId;
        private Rect windowRect = new Rect(375, 875, 100,50);
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            EffectPrefab = Mod.modAssetBundle.LoadAsset<GameObject>("SpotEffect");
            EffectObject = (GameObject)Instantiate(EffectPrefab, transform);
            particleSystem = EffectObject.GetComponent<ParticleSystem>();
            particleSystem.Stop();
            EffectObject.transform.position = BlockBehaviour.GetCenter();

            Range = Module.SpotRange;

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
            windowId = ModUtility.GetWindowId();
            blockID = BlockId;
            try
            {
                SpotStart = GetKey(Module.SpotKey);
            }
            catch
            {
                Mod.Error("BlockID" + blockID + "error");
            }
        }
        //キーを押されたとき、前方にスポットエフェクトを呼び出す。
        public override void SimulateUpdateAlways()
        {
            base.SimulateUpdateAlways();
            ThisDirection = - transform.up ;
            if (SpotStart.IsPressed || SpotStart.EmulationPressed())
            {
                if(Spotnum == 0)
                {
                    if (Physics.SphereCast(this.transform.position + 3f * ThisDirection, 0.25f, ThisDirection, out hit, Range, Blocklayermask))
                    {
                        StartCoroutine(PlaySpot());

                    }
                    Spotnum = 1;
                }
            }
        }
        public IEnumerator PlaySpot()
        {
            EffectObject.transform.position = hit.transform.position;
            Spottrue = true;
            yield return new WaitForSeconds(1f);
            particleSystem.Play();
            yield return new WaitForSeconds(10f);
            particleSystem.Stop();
            Spottrue = false;
        }
        public override void SimulateFixedUpdateAlways()
        {
            base.SimulateFixedUpdateAlways();
            if(Spottrue)
            {
                EffectObject.transform.position = hit.transform.position;
                EffectObject.transform.rotation = Quaternion.Euler(90f,0f,0f);

            }
            if (Spotnum != 0)
            {
                Spotnum++;
            }
            if (Spotnum == 500)
            {
                Spotnum = 0;
            }
            if (0 != Spotnum)
            {
                SpotReload = (Spotnum / 5).ToString() + " %";
            }
            if (0 == Spotnum)
            {
                SpotReload = "Spot OK";
            }
        }
        public void OnGUI()
        {
            if(isOwnerSame )
            {
                windowRect = GUILayout.Window(windowId, windowRect, delegate (int windowId)
                {
                    GUILayout.Label(SpotReload);
                }
                , "Spot Reload");
            }
        }
    }
}
