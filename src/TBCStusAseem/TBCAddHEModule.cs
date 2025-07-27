using System;
using System.Collections.Generic;
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
using System.Collections;
using Vector3 = UnityEngine.Vector3;

namespace TBCStusSpace
{
    [XmlRoot("TBCAddHEModule")]
    [Reloadable]
    public class TBCAddHEModule : BlockModule
    {

        [XmlElement("HEPenetration")]
        [DefaultValue(0f)]
        [Reloadable]
        public float hePenetration;

        [XmlElement("HERadius")]
        [DefaultValue(0f)]
        [Reloadable]
        public float heradius;
    }
	public class TBCAddHEBehaviour : BlockModuleBehaviour<TBCAddHEModule>
	{
        private AdShootingBehavour adshootingbehavour;
        private AdExplosionEffect adprojectilescript;
        private Transform projectilmultipool;
        private TBCHEController tbchecontroller;
        public float TBCHEPosition;
        public float TBCHEPenetration;
        public float TBCHERadius;
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            TBCHEPenetration = Module.hePenetration;
            TBCHERadius = Module.heradius;
            adshootingbehavour = GetComponent<AdShootingBehavour>();

            if (StatMaster.isHosting || !StatMaster.isMP || StatMaster.isLocalSim)
            {
                //�e�ɃX�N���v�g��\��t����
                //���x���G�f�B�^�A�}���`�̂Ƃ�
                projectilmultipool = GameObject.Find("PManager").transform.Find("EffectPool");

                foreach (Transform child in projectilmultipool)
                {

                    if (child.name == "ExplosionEffect")
                    {

                        adprojectilescript = child.gameObject.GetComponent<AdExplosionEffect>();

                        //�u���b�N���������Ȃ�X�N���v�g��\��t����
                        if (adshootingbehavour.BlockName == adprojectilescript.BlockName)
                        {

                            tbchecontroller = child.gameObject.GetComponent<TBCHEController>();

                            if (tbchecontroller == null)
                            {

                                tbchecontroller = child.gameObject.AddComponent<TBCHEController>();
                            }
                            tbchecontroller.HEPenetration = TBCHEPenetration;
                            tbchecontroller.HERadius = TBCHERadius;
                        }
                    }
                }
            }

        }
    }
    public class TBCHEController : MonoBehaviour
    {
        private TBCExplodeJointController TBCExplodeJointController;
        private ArmorScript ArmorScript;
        private Rigidbody rigidbody;
        private BlockBehaviour blockBehaviour;
        private FireTag FireTag;
        public float HERadius = 1f;
        public float ExplodeRadius = 1f;
        public LayerMask layermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        public bool init;
        private int hitnumber = 0;
        public float HEPenetration;
        public float hitposition;
        public void OnDisable()
        {
            init = false;
        }
        public void Explodejudgment()
        {
            hitnumber = 0;
            TBCExplodeJointController = this.gameObject.GetComponent<TBCExplodeJointController>();
            Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, 2.5f, layermask);
            foreach(Collider hitcollider in hitColliders)
            {
                ArmorScript = hitcollider.gameObject.GetComponent<ArmorScript>();
                if(!ArmorScript)
                {
                    ArmorScript = hitcollider.gameObject.transform.parent.gameObject.GetComponent<ArmorScript>();
                }
                if(ArmorScript != null)
                {
                    if(HEPenetration >= ArmorScript.armorthickness)
                    {
                        hitnumber++;
                    }
                }
            }
            if(hitColliders.Length == 0)
            {
                TBCExplodeJointController.radius = 1f;

            }
            else
            {
                if((float)hitnumber/hitColliders.Length >0.66f)
                {
                    ExplodeRadius = HERadius;
                    TBCExplodeJointController.range = HERadius;
                }
                else if((float)hitnumber / hitColliders.Length > 0.5f)
                {
                    ExplodeRadius = HERadius / 2f;
                    TBCExplodeJointController.range = HERadius / 2f;
                }
                else
                {
                    ExplodeRadius = HERadius / 4f;
                    TBCExplodeJointController.range = HERadius / 4f;
                }
            }
            HEExplode();

        }
        public void HEExplode()
        {
            Collider[] HEColliders = Physics.OverlapSphere(this.transform.position, ExplodeRadius, layermask);
            foreach(Collider hecollider in HEColliders)
            {
                rigidbody = hecollider.gameObject.GetComponent<Rigidbody>();
                
                if(!rigidbody)
                {
                    rigidbody = hecollider.gameObject.transform.parent.gameObject.GetComponent<Rigidbody>();
                }
                if(rigidbody != null)
                {
                    blockBehaviour = rigidbody.gameObject.GetComponent<BlockBehaviour>();
                    if(blockBehaviour.BlockID == 23 || blockBehaviour.BlockID == 59 || rigidbody.gameObject.GetComponent<AdEnhancedAmmunitionDepot>() || rigidbody.gameObject.GetComponent<AdAmmunitionStorage>())
                    {
                        FireTag = rigidbody.gameObject.GetComponent<FireTag>();
                        if (FireTag != null)
                        {
                            FireTag.Ignite();
                        }
                    }
                    rigidbody.AddExplosionForce(ExplodeRadius * 15f, this.transform.position, ExplodeRadius, 5.0f, ForceMode.Impulse);
                }
            }
        }
    }
}
