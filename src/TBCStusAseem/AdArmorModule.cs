using System;
using System.Collections;
using System.Collections.Generic; 
using Modding;
using Modding.Blocks;
using Modding.Serialization;
using UnityEngine;

namespace TBCStusSpace
{
	public class AdArmorModule : SingleInstance<AdArmorModule>
	{
        //���b�X�N���v�g�\��t���Ȃ��u���b�N�Q
        public Dictionary<int, Type> BlockDict = new Dictionary<int, Type>
        {
            //�{��
            {23, typeof(NoArmorScript) },
            //�y�􎮃��P�b�g
            {59, typeof(NoArmorScript) },
            //�t�@�C���[�{�[��
            {31, typeof(NoArmorScript) },
            //�ې�
            {36, typeof(NoArmorScript) },
            //�M�C��
            {74, typeof(NoArmorScript) },
            //�J�����u���b�N
            {58, typeof(NoArmorScript) },
            //�s���u���b�N
            {57, typeof(NoArmorScript) },

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
            //�u���b�N�ݒu���ɐ�����C�x���g�֐��쐬
            Events.OnBlockInit += new Action<Block>(AdArmorScript);
        }
        public void AdArmorScript(Block block)
        {
            BlockBehaviour internalObject = block.BuildingBlock.InternalObject;

            //�X�N���v�g�̓\��t��
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
                    if(internalObject.GetComponent<ArmorScript>() == null)
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
        public HingeJoint hingejointchange;
        private BlockBehaviour bb;
        public MSlider ArmorSlider;

        public ConfigurableJoint jointobject;

        public ArmorScript JG;

        //���\�b�h��ݒ肷�邽�߂̕ϐ�
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
        
        //�e���\�b�h�ݒ�

        public void FixedUpdate()
        {
            if(bb.isSimulating)
            {

            }
            else
            {
                
            }
        }
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
            //�d�ʁA�ڑ����x�������邽�߂̏���
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
                    Dmass = rigidbody.mass;
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
            //�\��t����ꂽ�u���b�N�����ꂼ�ꎝ���Ă��邩�����Ă��Ȃ����𔻒�
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
            //���{�ڑ��A�d�ʂ̕ύX
            StartCoroutine(StateChange(armorvalue));
        }
        public override void BuildingUpdate()
        {
           
            if(armorvalue != ArmorSlider.Value)
            {
                armorthickness = ArmorSlider.Value;
                armorvalue = ArmorSlider.Value;
                changevalue = (float)(Math.Log(armorvalue, 25f));
                massdata = Dmass * armorvalue / 25f;
                if (this.transform.Find("TriggerForJoint2"))
                {
                    jointobject.breakForce = jointvalue2 / changevalue;
                    jointobject.breakTorque = jointvalue2 / changevalue;
                }

            }
        }
        public override void SafeAwake()
        {
            base.SafeAwake();
            bb = GetComponent<BlockBehaviour>();
            //���b���̃X���C�_�[�ƒl���擾
            ArmorSlider = bb.AddSlider("Armor thickness", "armorvalue", 25f, 10f, 175f);
            
        }

        public IEnumerator StateChange(float armorvalue)
        {
                yield return new WaitForFixedUpdate();
            if ((float)(Math.Log(armorvalue, 25f)) < 0.5f)
            {
                changevalue = 0.5f;
            }
            if(rigidbody)
            {
                if(armorthickness >150f)
                {
                    rigidbody.mass = Dmass *(armorvalue / 25f * 4f - 18f);
                }
                else
                {
                    rigidbody.mass = Dmass * armorvalue / 25f;
                }
            }
            if (jointchange)
            {
                if (Mathf.Infinity == jointchange.breakForce)
                {
                    jointchange.breakForce = 60000f / changevalue;
                    jointchange.breakTorque = 60000f / changevalue;
                }
                else
                {
                    jointchange.breakForce = jointvalue / changevalue;
                    jointchange.breakTorque = jointvalue / changevalue;
                }
            }
            if (hingejointchange)
            {
                hingejointchange.breakForce = hingejointvalue / changevalue;
                hingejointchange.breakTorque = hingejointvalue / changevalue;
            }
        }

    }

}
