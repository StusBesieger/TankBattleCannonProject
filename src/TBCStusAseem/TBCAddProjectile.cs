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
        public int AmmoFunction;

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
        private TBCProjectileController tbcprojectilecontroller;
        private GameObject projectilepool;
        private Transform projectilmultipool;

        public float TBCGravity;
        public int TBCAmmoFunction = 5;
        public int TBCAmmoStock = 10;
        public bool start = false;
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
            }

		}
        public void FixedUpdate()
        {
            if(start)
            {
                adshootingbehavour.AmmoLeft = TBCAmmoStock / TBCAmmoFunction;
                if(UseMagazine)
                {
                    if(adshootingbehavour.AmmoLeft >= MagazineCapacity)
                    {
                        adshootingbehavour.AmmoLeft = MagazineCapacity;
                        adshootingbehavour.AmmoStock = TBCAmmoStock / TBCAmmoFunction - MagazineCapacity;
                    }
                }
                start = false;
            }
        }
    }

    public class TBCProjectileController : ProjectileScript
    {
        private Rigidbody rigidbody;
        private Vector3 gravityforce = new Vector3(0.0f, -1.0f, 0.0f);
        private Vector3 look;
        private Quaternion lookrotation;
        public float addgravity;
        public int time = 0;
        public void Awake() 
        {
            base.Awake();
            rigidbody = GetComponent<Rigidbody>();

        }
        public override void FixedUpdate()
        {
            if(addgravity != 0.0f)
            {
                rigidbody.AddForce(addgravity * gravityforce, ForceMode.Force);
            }
            if (this.rigidbody.velocity.magnitude == 0.0f)
            {
                return;
            }
            else
            {
                if(time == 1)
                {
                    look = new Vector3(this.rigidbody.velocity.normalized.x, this.rigidbody.velocity.normalized.y, this.rigidbody.velocity.normalized.z);
                    lookrotation = Quaternion.LookRotation(look, Vector3.up);
                    this.transform.rotation = lookrotation;
                }
                else
                {
                    time++;
                    return;
                }
            }
        }
        public void OnEnable()
        {
        }
        public void OnCollisionEnter()
        { }
        public void ValidCollisionOrTrigger()
        { }
    }
}
