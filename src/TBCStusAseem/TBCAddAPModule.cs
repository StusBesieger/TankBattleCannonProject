using System;
using System.Collections;
using System.Xml.Serialization;
using System.ComponentModel;
using UnityEngine;
using Modding;
using Modding.Serialization;
using Modding.Modules;
using skpCustomModule;
using Vector3 = UnityEngine.Vector3;
using Random = UnityEngine.Random;

namespace TBCStusSpace
{
    [XmlRoot("TBCAddAPModule")]
    [Reloadable]
    public class TBCAddAPModule : BlockModule
	{
        [XmlElement("APSpeed")]
        [DefaultValue(0f)]
        [Reloadable]
        public float APSpeed;

        [XmlElement("APtime")]
        [DefaultValue(0f)]
        [Reloadable]
        public float APtime;

        [XmlElement("APcoefficient")]
        [DefaultValue(0f)]
        [Reloadable]
        public int APcoefficient;

        [XmlElement("StandardPenetration")]
        [DefaultValue(0f)]
        [Reloadable]
        public float StandardPenetration;
    }
	public class TBCAddAPBehaviour : BlockModuleBehaviour<TBCAddAPModule>
	{
        private AdShootingBehavour adshootingbehavour;
        private AdProjectileScript adprojectilescript;
        private GameObject projectilepool;
        private Transform projectilmultipool;
        private TBCAPController tbcapcontroller;
        public float aptime;
        public float standardpenetration;
        public int apcoefficient;
        public float APspeed;
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            APspeed = Module.APSpeed;
            aptime = Module.APtime;
            apcoefficient = Module.APcoefficient;
            standardpenetration = Module.StandardPenetration;
            adshootingbehavour = GetComponent<AdShootingBehavour>();

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

                            tbcapcontroller = child.gameObject.GetComponent<TBCAPController>();

                            if (tbcapcontroller == null)
                            {

                                tbcapcontroller = child.gameObject.AddComponent<TBCAPController>();

                            }
                            tbcapcontroller.APSpeed = APspeed;
                            tbcapcontroller.APtime = aptime;
                            tbcapcontroller.APcoefficient = apcoefficient;
                            tbcapcontroller.StandardPenetration = standardpenetration;
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

                            tbcapcontroller = child.gameObject.GetComponent<TBCAPController>();

                            if (tbcapcontroller == null)
                            {

                                tbcapcontroller = child.gameObject.AddComponent<TBCAPController>();
                            }
                            tbcapcontroller.APSpeed = APspeed;
                            tbcapcontroller.APtime = aptime;
                            tbcapcontroller.APcoefficient = apcoefficient;
                            tbcapcontroller.StandardPenetration = standardpenetration;
                        }
                    }
                }
            }

        }

    }
    public class TBCAPController : MonoBehaviour
    {
        private Rigidbody rigidbody;
        private Rigidbody hitrigidbody;
        private AdProjectileScript adProjectileScript;
        private ArmorScript armorScript;
        private NoArmorScript noArmorScript;
        public GameObject componentparent;
        public GameObject componentfind;
        Collider mCollider;
        public bool init;
        public float APtime;
        public float APdis;
        public float APSp;
        public float APSpeed = 0.0f;
        public Vector3 ProjectileSp;
        public float APFixedSp;
        public float Projectilemath;
        public float APStartTime;
        public float hitangle;
        public float ApparentAromrThickness;
        public float PenetrationValue;
        public float StandardPenetration;
        //public float APFuseTimer;
        public int APcoefficient;
        public bool APStop = false;
        private int armornumber;
        public LayerMask layermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        private Vector3 APDirection;
        private RaycastHit hit;
        public AudioSource AudioSource;
        public AudioClip AudioClip1;
        public AudioClip AudioClip2;
        public AudioClip AudioClip3;
        private int rnd;
        

        public void Awake()
        {

            mCollider = this.transform.Find("Gyro").transform.Find("Colliders").GetChild(0).GetComponent<Collider>();
            adProjectileScript = this.gameObject.GetComponent<AdProjectileScript>();
            init = true;
            StartCoroutine(InitStart());
            rigidbody = GetComponent<Rigidbody>();
            AudioSource = gameObject.GetComponent<AudioSource>();
            AudioSource.spatialBlend = 1.0f;
            AudioClip1 = ModResource.GetAudioClip("Ricochet_1");
            AudioClip2 = ModResource.GetAudioClip("Ricochet_2");
            AudioClip3 = ModResource.GetAudioClip("Ricochet_3");
        }
        IEnumerator InitStart()
        {
            yield return new WaitForFixedUpdate();
            init = false;
        }
        public void FixedUpdate()
        {
                if (!init)
                {
                    APDirection = this.rigidbody.velocity.normalized;
                    APFixedSp = this.rigidbody.velocity.magnitude * Time.deltaTime;
                    if (Physics.SphereCast(mCollider.transform.position + APDirection * 0.5f, 0.25f, APDirection, out hit, 2.0f * APFixedSp, layermask))
                    {
                        Physics.SphereCast(mCollider.transform.position + APDirection * 0.5f, 0.25f, APDirection, out hit, 2.0f * APFixedSp, layermask);
                        hitangle = Vector3.Angle(-1 * APDirection, hit.normal);
                        hitrigidbody = hit.collider.gameObject.GetComponent<Rigidbody>();
                        componentfind = hit.collider.gameObject;
                        while (hitrigidbody == null)
                        {
                            componentfind = componentfind.transform.parent.gameObject;
                            hitrigidbody = componentfind.gameObject.GetComponent<Rigidbody>();
                        }
                        componentparent = hit.collider.gameObject;
                        armorScript = componentparent.gameObject.GetComponent<ArmorScript>();
                        noArmorScript = componentparent.gameObject.GetComponent<NoArmorScript>();
                        while (armorScript == null && noArmorScript == null)
                        {
                            componentparent = componentparent.transform.parent.gameObject;
                            armorScript = componentparent.gameObject.GetComponent<ArmorScript>();
                            noArmorScript = componentparent.gameObject.GetComponent<NoArmorScript>();
                        }
                        if (hitrigidbody != null)
                        {
                            if (armorScript)
                            {
                                armornumber = 1;
                            if(60f < hitangle && hitangle < 80f)
                            {
                                hitangle = 60f;
                            }

                                ApparentAromrThickness = armorScript.armorthickness * armorScript.armorhp / ((float)Math.Cos((hitangle * (100f - APcoefficient) / 100f) * Math.PI / 180) * 5f) + armorScript.subarmorthickness * 0.5f;
                                if(armorScript.armorhp > 1)
                                {
                                    if(StandardPenetration > armorScript.armorthickness * 0.5f)
                                    {
                                        armorScript.armorhp--;
                                    }
                                }
                            }
                            if (noArmorScript)
                            {
                                armornumber = 2;
                            }
                            PenetrationValue = StandardPenetration * this.rigidbody.velocity.magnitude / APSpeed;
                            Penetrationjudgment();
                        }
                        init = true;

                    }
                }  
        }
        //貫通判定
        public void Penetrationjudgment()
        {
            APSp = Vector3.Distance(this.rigidbody.velocity, hitrigidbody.velocity);
            if (hitangle < 80)
            {
                if(armornumber == 1 )
                {
                    
                    if (PenetrationValue> ApparentAromrThickness)
                    {
                        StartCoroutine(Penetration());
                        if(ApparentAromrThickness> PenetrationValue*0.1)
                        {
                            APStop = true;
                        }
                    }
                    else
                    {
                        StartCoroutine(NoPenetration());
                        rnd = Random.Range(1,4);
                        if(rnd == 1)
                        {
                            AudioSource.PlayOneShot(AudioClip1);
                        }
                        else if(rnd == 2)
                        {
                            AudioSource.PlayOneShot(AudioClip2);
                        }
                        else if(rnd == 3)
                        {
                            AudioSource.PlayOneShot(AudioClip3);
                        }
                    }
                }
                else if(armornumber == 2 )
                {
                    return;
                }
            }
            else
            {
                StartCoroutine(NoPenetration());
                rnd = Random.Range(1, 4);
                if (rnd == 1)
                {
                    AudioSource.PlayOneShot(AudioClip1);
                }
                else if (rnd == 2)
                {
                    AudioSource.PlayOneShot(AudioClip2);
                }
                else if (rnd == 3)
                {
                    AudioSource.PlayOneShot(AudioClip3);
                }
            }
        }
        //貫通処理
        IEnumerator Penetration()
        {
            yield return new WaitForFixedUpdate();
            this.gameObject.transform.position = hit.point + APDirection * APtime;
            hitrigidbody.AddForce(APSp * APDirection * (float)Math.Log(APtime, 4f) * (float)Math.Pow(APcoefficient + 1, 0.1f), ForceMode.Impulse);

            if (APStop)
            {
                adProjectileScript.alwaysExplodes = true;
                this.rigidbody.velocity = Vector3.zero;
                adProjectileScript.existenceTime = 1f;
                adProjectileScript.Timefuse = Time.deltaTime;
                
            }
            else
            {
                init = false;
                this.rigidbody.velocity *= 0.75f;
            }
        }
        //非貫通処理
        public IEnumerator NoPenetration()
        {
            adProjectileScript.alwaysExplodes = false;
            for (var n = 0; n < 4; n++)
            {
                yield return new WaitForFixedUpdate();
            }


        }
        public void OnDisable()
        {
            init = false;
        }
        public void Update()
        { }
        public void OnEnable()
        { }
        public void OnTriggerEnter()
        {
        }
        public void OnCollisionEnter()
        { }
        public void ValidCollisionOrTrigger()
        { }
    }
}
