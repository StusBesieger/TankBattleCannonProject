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

        [XmlElement("HEForce")]
        [DefaultValue(0f)]
        [Reloadable]
        public float heForce;

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
        public float TBCHEForce;
        public float TBCHERadius;
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            TBCHEForce = Module.heForce;
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
                            tbchecontroller.HEForce = TBCHEForce;
                            tbchecontroller.HERadius = TBCHERadius;
                        }
                    }
                }
            }

        }
    }
    public class TBCHEController : AdExplosionEffect
    {
        public float HERadius;
        public LayerMask layermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        public bool init;
        private int hitnumber = 0;
        private Rigidbody rb;
        public float HEForce;
        public float hitposition;
        private ArmorScript ArmoS;
        public void OnDisable()
        {
            init = false;
        }
        public new void FixedUpdate()
        {
            if (!init)
            {
                if (!StatMaster.isMP || StatMaster.isHosting || StatMaster.isLocalSim)   //�}���`�łȂ� or �z�X�g�ł��� or ���[�J���V�~���ł���
                {
                    Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, HERadius, layermask);
                    
                    for (int i = 0; i< hitColliders.Length; i++)
                    {
                        rb = hitColliders[i].GetComponent<Rigidbody>();
                        if (rb)
                        {
                            hitnumber++;
                        }
                    }
                    for (int i = 0; i < hitColliders.Length; i++)
                    {
                        rb = hitColliders[i].GetComponent<Rigidbody>();
                        if (rb)
                        {
                            if(hitnumber == 0)
                            {
                                hitnumber = 1;
                            }
                            hitposition = Vector3.Distance(this.transform.position, hitColliders[i].transform.position) + 0.5f;
                            rb.AddExplosionForce(HEForce / hitposition, this.transform.position, HERadius, 0.0f, ForceMode.Impulse);
                            ArmoS = hitColliders[i].GetComponent<ArmorScript>();
                            if (ArmoS)
                            {
                                if (ArmoS.armorthickness < HEForce * 0.1f)
                                {
                                    rb.AddExplosionForce(HEForce * 2f / hitposition, this.transform.position, HERadius * 0.5f, 0.0f, ForceMode.Impulse);

                                }
                            }
                        }
                    }
                }
                init = true;
            }
            base.FixedUpdate();

        }
    }
}
