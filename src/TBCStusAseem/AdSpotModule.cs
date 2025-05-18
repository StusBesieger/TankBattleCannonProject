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
        //エフェクト
        public GameObject EffectPrefab;
        public GameObject EffectObject;
        public ParticleSystem particleSystem;
        public Vector3 Effectposition;

        //メッセージ、GUI
        public BlockBehaviour blockBehaviour;
        private bool isOwnerSame = false;


        //プレイヤー操作
        public MKey SpotStart;
        public bool Spotok = true;
        public bool SpotReloading = true;
        public float Range;
        public LayerMask Blocklayermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        public Vector3 ThisDirection;
        private RaycastHit hit;

        //メッセージ
        public int DictID = 0;
        public bool Spotgetmessage = false;

        private bool Clientbool = false;
        public void Awake()
        {
            blockBehaviour = this.GetComponent<BlockBehaviour>();

            if (blockBehaviour.isBuildBlock)
            {
                while (Mod.RangeboolDict.ContainsKey(DictID))
                {
                    DictID++;
                }
                Mod.SpotEffecboolDict.Add(DictID, value: false);
                Mod.SpotPositionDict.Add(DictID, value: new Vector3(0f,0f,0f));
            }
        }
        public override void OnBlockPlaced()
        {
            base.OnBlockPlaced();

        }
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            EffectPrefab = Mod.modAssetBundle.LoadAsset<GameObject>("SpotEffect");
            EffectObject = (GameObject)Instantiate(EffectPrefab, transform);
            particleSystem = EffectObject.GetComponent<ParticleSystem>();
            particleSystem.Stop();
            EffectObject.transform.position = BlockBehaviour.GetCenter();

            Range = Module.SpotRange;
            SpotStart = GetKey(Module.SpotKey);
            UpdateOwnerFlag();
        }
        public override void OnSimulateStop()
        {
            base.OnSimulateStop();
            isOwnerSame = false;
            Mod.SpotEffecboolDict.Remove(DictID);
            Mod.SpotPositionDict.Remove(DictID);
        }
        public override void SimulateFixedUpdateHost()
        {
            base.SimulateFixedUpdateHost();
            if (Spotok)
            {
                ThisDirection = -transform.up;
                if (Physics.SphereCast(this.transform.position + 3f * ThisDirection, 0.25f, ThisDirection, out hit, Range, Blocklayermask))
                {
                    Debug.Log("ray");
                    if(SpotReloading)
                    {
                        StartCoroutine(Spot());
                    }
                }
                
                StartCoroutine(SpotReload());
            }
        }
        public override void SimulateUpdateAlways()
        {
            base.SimulateUpdateAlways();
            if(SpotStart.IsPressed || SpotStart.EmulationPressed())
            {
                Spotok = true;
            }

        }
        public override void SimulateUpdateClient()
        {
            base.SimulateUpdateClient();
            Clientbool = Mod.SpotEffecboolDict[DictID];
            if(Clientbool)
            {
                if (!EffectObject.activeInHierarchy)
                {
                    EffectObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    EffectObject.SetActive(true);
                    StartCoroutine(SpotReload());
                }
                EffectObject.transform.position = Mod.SpotPositionDict[DictID];
            }
            else
            {
                if(EffectObject.activeInHierarchy)
                {
                    EffectObject.SetActive(false);
                }
            }

            

        }
        public override void SimulateUpdateHost()
        {
            base.SimulateUpdateHost();
            if (Spotgetmessage)
            {
                EffectObject.transform.position = hit.transform.position;
                EffectObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                Mod.SpotPositionmessage = Mod.SpotPositionmessageType.CreateMessage(DictID, EffectObject.transform.position);
                ModNetworking.SendToAll(Mod.SpotPositionmessage);
            }
            Mod.Spotboolmessage = Mod.SpotboolmessageTyope.CreateMessage(DictID, Spotgetmessage);
            ModNetworking.SendToAll(Mod.Spotboolmessage);
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
        IEnumerator Spot()
        {
            yield return new WaitForSeconds(1f);
            Spotgetmessage = true;
            particleSystem.Play();
            yield return new WaitForSeconds(10f);
            particleSystem.Stop();
            Spotgetmessage = false;
        }
        IEnumerator SpotReload()
        {
            yield return new WaitForFixedUpdate();
            Spotok = false;
            SpotReloading = false;
            yield return new WaitForSeconds(5f);
            SpotReloading = true;
        }
    }
}
