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
    [XmlRoot("TBCAddFireExtinguisherModule")]
    [Reloadable]
    public class TBCAddFireExtinguisherModule : BlockModule
    {
        [XmlElement("ExtinguishingKey")]
        [RequireToValidate]
        public MKeyReference ExtinguishingKey;

        [XmlElement("ExtinguishingNum")]
        [DefaultValue(0)]
        [Reloadable]
        public int ExtinguishingNum;

        [XmlArray("Sound")]
        [RequireToValidate]
        [CanBeEmpty]
        [XmlArrayItem("AudioClip", typeof(ResourceReference))]
        public object[] Sounds;
    }
    public class TBCAddFireExtinguisherBehaviour : BlockModuleBehaviour<TBCAddFireExtinguisherModule>
    {
        public int blockID;
        private int Spotnum = 0;
        public string SpotEffectName;
        public MKey ExtinguishingKey;
        public int ExtinguishingNum;
        public BlockBehaviour bb;
        public LayerMask Blocklayermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        private bool isOwnerSame;
        private Transform SParentObject;
        private FireTag[] firetags;
        private int windowId;
        private Rect windowRect = new Rect(375, 725, 175, 50);
        private string limitnum;
        private string reloadtime;
        public AudioSource audiosource;
        public AudioClip sound;

        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            ExtinguishingNum = Module.ExtinguishingNum;
            UpdateOwnerFlag();
            limitnum = ExtinguishingNum.ToString();
            SParentObject = this.transform;
            //リスポ時に呼び起されている可能性あるためnullかどうか確認
            if(SParentObject.gameObject.name != "Simulation Machine")
            {
                while (SParentObject.gameObject.name != "Simulation Machine")
                {
                    SParentObject = SParentObject.transform.parent;
                }
            }
            firetags = SParentObject.gameObject.GetComponentsInChildren<FireTag>();
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
        public override void SafeAwake()
        {
            base.SafeAwake();
            windowId = ModUtility.GetWindowId();
            blockID = BlockId;
            try
            {
                ExtinguishingKey = GetKey(Module.ExtinguishingKey);
            }
            catch
            {
                Mod.Error("BlockID" + blockID + "error");
            }
            audiosource = gameObject.AddComponent<AudioSource>();
            audiosource.spatialBlend = 1.0f;
            sound = ModResource.GetAudioClip("FireEx");

        }
        public void Awake()
        {
        }
        public override void SimulateFixedUpdateAlways()
        {
            base.SimulateFixedUpdateAlways();
            if (ExtinguishingKey.IsPressed || ExtinguishingKey.EmulationPressed())
            {
                if (ExtinguishingNum > 0)
                {
                    if (Spotnum == 0)
                    {
                        Spotnum = 1;
                        Extinguishing();

                    }
                }
            }
            if (Spotnum != 0)
            {
                Spotnum++;
                reloadtime = (Spotnum / 5).ToString() + " %";
            }
            if (Spotnum == 500)
            {
                Spotnum = 0;

            }
            if (Spotnum == 0)
            {
                reloadtime = "Use OK";
            }
        }
        //キーを押された時の処理
        public override void SimulateUpdateAlways()
        {
            base.SimulateUpdateAlways();
            
        }
        public void Extinguishing()
        {
            ExtinguishingNum--;
            limitnum = ExtinguishingNum.ToString();
            foreach (FireTag fireTag in firetags)
            {
                fireTag.WaterHit();
                audiosource.PlayOneShot(sound);
            }
        }
        public void OnGUI()
        {
            if (isOwnerSame)
            {
                windowRect = GUILayout.Window(windowId, windowRect, delegate (int windowId)
                {
                    GUILayout.Label(reloadtime);
                    GUILayout.Label("Can use " + limitnum);
                }
                , "Fire Extinguisher");
            }
        }
    }
}
