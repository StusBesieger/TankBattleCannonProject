using System;
using System.Collections;
using System.Collections.Generic; 
using Modding;
using Modding.Blocks;
using Modding.Serialization;
using UnityEngine;
using skpCustomModule;

namespace TBCStusSpace
{
	public class AdArmorModule : SingleInstance<AdArmorModule>
	{
        //装甲スクリプト貼り付けないブロック群
        public Dictionary<int, Type> BlockDict = new Dictionary<int, Type>
        {
            //ボム
            {23, typeof(NoArmorScript) },
            //炸裂式ロケット
            {59, typeof(NoArmorScript) },
            //ファイヤーボール
            {31, typeof(NoArmorScript) },
            //丸石
            {36, typeof(NoArmorScript) },
            //熱気球
            {74, typeof(NoArmorScript) },
            //カメラブロック
            {58, typeof(NoArmorScript) },
            //ピンブロック
            {57, typeof(NoArmorScript) },
            //ブレース
            {7, typeof(NoArmorScript) },
            //ロープウィンチ
            {45, typeof(NoArmorScript) }

        };
        public override string Name
        {
            get
            {
                return "AdArmor";
            }
        }
        public void Awake()
        {
            //ブロック設置時に生じるイベント関数作成
            Events.OnBlockInit += new Action<Block>(AdArmorScript);
        }
        public void AdArmorScript(Block block)
        {
            BlockBehaviour internalObject = block.BuildingBlock.InternalObject;

            //スクリプトの貼り付け
            if(BlockDict.ContainsKey(internalObject.BlockID))
            {
                Type type = BlockDict[internalObject.BlockID];
                try
                {
                    
                    if (internalObject.GetComponent(type) == null)
                    {
                        internalObject.gameObject.AddComponent(type);
                    }
                }
                catch
                {

                }
            }
            else
            {
                try
                {
                    if(internalObject.GetComponent<ArmorScript>() == null && internalObject.GetComponent<Rigidbody>() !=null)
                    {
                        internalObject.gameObject.AddComponent<ArmorScript>();
                    }
                }
                catch
                {

                }
            }
        }
    }
    public class NoArmorScript : MonoBehaviour
    {

    }
    public class ArmorScript : BlockScript
    {
        public Rigidbody rigidbody;
        public ConfigurableJoint jointchange;
        public ConfigurableJoint[] surfacejoint;
        public HingeJoint hingejointchange;
        private BlockBehaviour bb;

        public AdShootingBehavour adShootingBehavour;
        public MSlider ArmorSlider;

        public ConfigurableJoint jointobject;

        public ArmorScript JG;

        //メソッドを設定するための変数
        public bool isSimulatingFirstFrame = true;
        public bool isBuildingFirstFrame = true;
        public bool isBuildingFixedUpdate = true;

        public float armorthickness = 25f;
        public float armorvalue = 25f;
        public float changevalue = 1f;
        public float jointvalue = 1f;
        public float jointvalue2 = 1f;
        public float hingejointvalue = 1.0f;
        public float Dmass = 1.0f;
        public int i = 1;
        public float massdata;

        public bool useTrack = false;
        public MToggle TrackToggle;

        public ArmorScript subarmorscript;
        public float subarmorthickness = 0f;
        public int armorhp = 5;

        public BuildSurface BuildSurface;
        public SurfaceVisualController SurfaceVisualController;

        //各メソッド設定

        public void Update()
        {
            if (bb.isSimulating)
            {
                //if (isSimulatingFirstFrame)
                //{
                //    isSimulatingFirstFrame = false;
                //    OnSimulateStart();
                //}
            }
            else
            {
                //if (isBuildingFirstFrame)
                //{
                //    isBuildingFirstFrame = false;
                //    OnBlockPlaced();
                //    if (!isSimulatingFirstFrame)
                //    {
                //        isSimulatingFirstFrame = true;
                //    }
                //}
                BuildingUpdate();
            }
        }
        public void Awake()
        {
            SafeAwake();
            if (bb.isBuildBlock)
            {
                OnBlockPlaced();
            }
            else
            {
                OnSimulateStart();
            }

        }

        public override void OnBlockPlaced()
        {
            //重量、接続強度をいじるための準備
            if(bb == null)
            {
                bb = GetComponent<BlockBehaviour>();
                if (bb == null)
                {
                    Debug.Log("TBCArmor error || Not Find BlockBehaviour");
                }
            }
            else
            {
            }
            if (StatMaster.isHosting || !StatMaster.isMP || StatMaster.isLocalSim)
            {
                if (rigidbody == null)
                {
                    rigidbody = GetComponent<Rigidbody>();
                    
                    if (rigidbody == null)
                    {
                        Debug.Log("TBCArmor error || Not Find Rigidbody");
                    }
                }
            }

            if(jointchange == null)
            {
                jointchange = GetComponent<ConfigurableJoint>();
            }
            //貼り付けられたブロックがそれぞれ持っているか持っていないかを判定
            if (this.transform.Find("TriggerForJoint2"))
            {
                jointobject = this.transform.Find("TriggerForJoint2").transform.Find("Joint").GetComponent<ConfigurableJoint>();
                jointvalue2 = jointobject.breakForce;
            }
            if (jointchange == null)
            {
                hingejointchange = GetComponent<HingeJoint>();
            }
            if (jointchange)
            {
                jointvalue = jointchange.breakForce;
            }
            if (hingejointchange)
            {
                hingejointvalue = hingejointchange.breakForce;
            }
            
        }
        public override void OnSimulateStart()
        {
            //根本接続、重量の変更
            StartCoroutine(StateChange(armorvalue));
            StartCoroutine(SetSubArmor());
            TrackToggle.Toggled += delegate (bool value)
            {
                useTrack = value;

            };
        }
        public override void BuildingUpdate()
        {
           
            if(armorvalue != ArmorSlider.Value || this.gameObject.GetComponent<SurfaceVisualController>() != null)
            {
                armorthickness = ArmorSlider.Value;
                armorvalue = ArmorSlider.Value;
                changevalue = (float)(Math.Log(armorvalue, 25f));
                Dmass = rigidbody.mass;
                massdata = Dmass * armorvalue / 25f;
                if (this.transform.Find("TriggerForJoint2"))
                {
                    jointobject.breakForce = jointvalue2 / changevalue;
                    jointobject.breakTorque = jointvalue2 / changevalue;
                }
            }
            if (adShootingBehavour != null)
            {
                if (ArmorSlider.Value > 50f)
                {
                    ArmorSlider.SetValue(50f);
                }
            }
        }
        public override void SafeAwake()
        {
            base.SafeAwake();
            bb = GetComponent<BlockBehaviour>();
            BuildSurface = GetComponent<BuildSurface>();
            //装甲厚のスライダーと値を取得
            if (this.gameObject.GetComponent<AdShootingBehavour>() != null)
            {
                adShootingBehavour = this.gameObject.GetComponent<AdShootingBehavour>();
                ArmorSlider = bb.AddSlider("Armor thickness", "armorvalue", 25f, 10f, 50f);
            }
            else if(BuildSurface != null)
            {
                ArmorSlider = bb.AddSlider("Armor thickness", "armorvalue", 25f, 10f, 100f);
            }
            else
            {
                ArmorSlider = bb.AddSlider("Armor thickness", "armorvalue", 25f, 10f, 200f);
            }
            TrackToggle = bb.AddToggle("UseTrack", "tracktoggle", useTrack);

        }

        public IEnumerator StateChange(float armorvalue)
        {
                yield return new WaitForFixedUpdate();
            if ((float)(Math.Log(armorvalue, 25f)) < 0.5f)
            {
                changevalue = 0.5f;
            }
            SurfaceVisualController = this.gameObject.GetComponent<SurfaceVisualController>();
            if (rigidbody)
            {
                Dmass = rigidbody.mass;
                if (armorthickness >150f)
                {
                    rigidbody.mass = Dmass *(armorvalue / 5f - 20f);
                }
                else if(armorthickness > 100f)
                {
                    rigidbody.mass = Dmass * (armorvalue * 6f / 50f - 8f );
                }
                else
                {
                    rigidbody.mass = Dmass * armorvalue / 25f;
                }
            }
            surfacejoint = this.gameObject.GetComponents<ConfigurableJoint>();
            if (jointchange != null || surfacejoint != null)
            {
                if (Mathf.Infinity == jointchange.breakForce)
                {
                    jointchange.breakForce = 60000f / changevalue;
                    jointchange.breakTorque = 60000f / changevalue;
                }
                else
                {
                    if(bb.BlockID == 5)
                    {
                        jointchange.breakForce = jointvalue / changevalue * 1.25f;
                        jointchange.breakTorque = jointvalue / changevalue * 1.25f;
                    }
                    else if(SurfaceVisualController)
                    {
                        for(int i =0; i < surfacejoint.Length; i++)
                        {
                            surfacejoint[i].breakForce = 60000f / changevalue;
                            surfacejoint[i].breakTorque = 60000f / changevalue;
                        }
                    }
                    else
                    {
                        jointchange.breakForce = jointvalue / changevalue;
                        jointchange.breakTorque = jointvalue / changevalue;
                    }
                }
            }
            if (hingejointchange)
            {
                hingejointchange.breakForce = hingejointvalue / changevalue;
                hingejointchange.breakTorque = hingejointvalue / changevalue;
            }
            if (SurfaceVisualController)
            {
                SurfaceVisualController.breakIntoPieces = false;
            }
        }
        IEnumerator SetSubArmor()
        {
            for(int i = 0; i < 4; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            armorhp = 5;
            Joint joint = bb.blockJoint;
            if (joint)
            {
                subarmorscript = joint.connectedBody.GetComponent<ArmorScript>();
                if (subarmorscript)
                {
                    subarmorthickness = subarmorscript.armorvalue;
                }
            }
            else
            {
                subarmorthickness = 0f;
            }
        }
    }

}
