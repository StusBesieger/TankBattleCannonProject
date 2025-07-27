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
	[XmlRoot("TBCAddProjectile")]
	[Reloadable]
	public class TBCAddProjectile : BlockModule
	{
        [XmlElement("Gravity")]
        [DefaultValue(0f)]
        [Reloadable]
        public float Gravity;

        [XmlElement("AmmoFunction")]
        [DefaultValue(5f)]
        [Reloadable]
        public float AmmoFunction;

        [XmlElement("UseMagazine")]
        [DefaultValue(false)]
        [Reloadable]
        public bool UseMagazine;

        [XmlElement("MagazineCapacity")]
        [DefaultValue(null)]
        [Reloadable]
        public int MagazineCapacity;

    }
	public class TBCAddProjectileBehaviour : BlockModuleBehaviour<TBCAddProjectile>
    {
        private AdShootingBehavour adshootingbehavour;
        private AdProjectileScript adprojectilescript;
        private AdExplosionEffect AdExplosionEffect;
        private TBCProjectileController tbcprojectilecontroller;
        private TBCExplodeJointController TBCExplodeJointController;
        private GameObject projectilepool;
        private Transform projectilmultipool;

        public float TBCGravity;
        public float TBCAmmoFunction = 5;
        public int TBCAmmoStock = 10;
        public bool UseMagazine = false;
        public int MagazineCapacity;
        public override void OnBlockPlaced()
        {
            base.OnBlockPlaced();
        }
        public override void OnSimulateStart()
        {
			base.OnSimulateStart();

            TBCGravity = Module.Gravity;
            TBCAmmoFunction = Module.AmmoFunction;
            UseMagazine = Module.UseMagazine;
            MagazineCapacity = Module.MagazineCapacity;

            adshootingbehavour = this.GetComponent<AdShootingBehavour>();
            //残弾数設定
            if (StatMaster.isHosting || !StatMaster.isMP || StatMaster.isLocalSim)
            {
                //弾にスクリプトを貼り付ける

                //レベルエディタ、マルチ以外
                projectilepool = GameObject.Find("PHYSICS GOAL");

                foreach (Transform child in projectilepool.transform)
                {

                    if (child.name == "AdProjectile(Clone)(Clone)")
                    {

                        adprojectilescript = child.gameObject.GetComponent<AdProjectileScript>();

                        //ブロック名が同じならスクリプトを貼り付ける
                        if (adshootingbehavour.BlockName == adprojectilescript.BlockName)
                        {

                            tbcprojectilecontroller = child.gameObject.GetComponent<TBCProjectileController>();

                            if (tbcprojectilecontroller == null)
                            {

                                tbcprojectilecontroller = child.gameObject.AddComponent<TBCProjectileController>();
                            }
                            tbcprojectilecontroller.addgravity = TBCGravity;
                        }
                    }
                }

                //レベルエディタ、マルチのとき
                projectilmultipool = GameObject.Find("PManager").transform.Find("Projectile Pool");

                foreach (Transform child in projectilmultipool.transform)
                {

                    if (child.name == "AdProjectile(Clone)(Clone)")
                    {

                        adprojectilescript = child.gameObject.GetComponent<AdProjectileScript>();

                        //ブロック名が同じならスクリプトを貼り付ける
                        if (adshootingbehavour.BlockName == adprojectilescript.BlockName)
                        {

                            tbcprojectilecontroller = child.gameObject.GetComponent<TBCProjectileController>();

                            if (tbcprojectilecontroller == null)
                            {

                                tbcprojectilecontroller = child.gameObject.AddComponent<TBCProjectileController>();
                            }
                            tbcprojectilecontroller.addgravity = TBCGravity;
                        }
                    }
                }
                projectilmultipool = GameObject.Find("PManager").transform.Find("EffectPool");

                foreach (Transform child in projectilmultipool)
                {

                    if (child.name == "ExplosionEffect")
                    {

                        AdExplosionEffect = child.gameObject.GetComponent<AdExplosionEffect>();

                        //�u���b�N���������Ȃ�X�N���v�g��\��t����
                        if (adshootingbehavour.BlockName == AdExplosionEffect.BlockName)
                        {

                            TBCExplodeJointController = child.gameObject.GetComponent<TBCExplodeJointController>();

                            if (TBCExplodeJointController == null)
                            {

                                TBCExplodeJointController = child.gameObject.AddComponent<TBCExplodeJointController>();
                            }
                        }
                    }
                }

            }
            StartCoroutine(AmmoChange());
		}
        IEnumerator AmmoChange()
        {
            yield return new WaitForFixedUpdate();
            adshootingbehavour.AmmoLeft = (int)(TBCAmmoStock / TBCAmmoFunction);
            if (UseMagazine)
            {
                if (adshootingbehavour.AmmoLeft >= MagazineCapacity)
                {
                    adshootingbehavour.AmmoLeft = MagazineCapacity;
                    adshootingbehavour.AmmoStock = (int)(TBCAmmoStock / TBCAmmoFunction) - MagazineCapacity;
                }
            }
        }
    }

    public class TBCProjectileController : MonoBehaviour
    {
        private Rigidbody rigidbody;
        private Vector3 gravityforce = new Vector3(0.0f, -1.0f, 0.0f);
        private Vector3 startCenterOfMass = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 changeCenterOfMass = new Vector3(0.0f, -1.0f, 0.0f);
        public float addgravity;
        public int time = 0;
        
        public  void FixedUpdate()
        {
            if (this.gameObject.activeInHierarchy)
            {
                if(rigidbody == null)
                {
                    rigidbody = GetComponent<Rigidbody>();
                }
                if (addgravity != 0.0f)
                {
                    rigidbody.AddForce(addgravity * gravityforce, ForceMode.Force);
                }
                if (this.rigidbody.centerOfMass == startCenterOfMass)
                {
                    this.rigidbody.centerOfMass = changeCenterOfMass;
                }
            }
        }
    }
    public class TBCExplodeJointController : AdExplosionEffect
    {
        private ConfigurableJoint[] ConfigurableJoints;
        private AdExplosionEffect AdExplosionEffect;
        private TBCHEController TBCHEController;
        private Rigidbody rigidbody;
        public float range;
        private bool init;
        public LayerMask layermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        public void OnDisable()
        {
            init = false;
        }
        public new void FixedUpdate()
        {
            if (!init)
            {
                if (!StatMaster.isMP || StatMaster.isHosting || StatMaster.isLocalSim)
                {
                    AdExplosionEffect = this.gameObject.GetComponent<AdExplosionEffect>();
                    range = AdExplosionEffect.radius;
                    TBCHEController = this.gameObject.GetComponent<TBCHEController>();
                    if (TBCHEController)
                    {
                        TBCHEController.Explodejudgment();
                    }

                    Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, range, layermask);
                    for(int i = 0; i < hitColliders.Length; i++)
                    {
                        rigidbody = hitColliders[i].GetComponent<Rigidbody>();
                        if(rigidbody)
                        {
                            ConfigurableJoints = hitColliders[i].GetComponents<ConfigurableJoint>();
                            foreach (ConfigurableJoint configurableJoint in ConfigurableJoints)
                            {
                                configurableJoint.breakForce *= 0.75f;
                                configurableJoint.breakTorque *= 0.75f;
                            }
                        }
                        else
                        {
                            ConfigurableJoints = hitColliders[i].transform.parent.GetComponents<ConfigurableJoint>();
                            foreach (ConfigurableJoint configurableJoint in ConfigurableJoints)
                            {
                                configurableJoint.breakForce *= 0.75f;
                                configurableJoint.breakTorque *= 0.75f;
                            }
                        }
                    }
                }
            }
            init = true;

        }

    }
}
