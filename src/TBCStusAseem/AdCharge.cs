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
    [XmlRoot("TBCAdCanUseChargeModule")]
    [Reloadable]
    public class AdCanUseChargeModule : BlockModule
    {
        [XmlElement("UseChargeBlock")]
        [DefaultValue(false)]
        [Reloadable]
        public bool useCharge;

        [XmlElement("Chargetype")]
        [DefaultValue(0)]
        [Reloadable]
        public int Chargetype;
    }
    public class AdChargeUsingBehaviour : BlockModuleBehaviour<AdCanUseChargeModule>
    {
        public bool canUseCharge = false;
        public int useChargetype = 0;

        public AdShootingBehavour adShootingBehavour;

        public bool ChargeOK = false;
        public bool ReloadOK = true;
        public int Reloadtime = 0;

        public AudioSource ReloadingAudioSource;
        public AudioSource ChargingAudioSource;
        public AudioClip ReloadSound;
        public AudioClip ChargingSound;
        public int AudioTime = 1;

        public override void SafeAwake()
        {
            base.SafeAwake();
            adShootingBehavour = base.GetComponent<AdShootingBehavour>();
            adShootingBehavour.OnFire += OnFire;
        }
        public override void OnBlockPlaced()
        {
            base.OnBlockPlaced();
            canUseCharge = Module.useCharge;
            useChargetype = Module.Chargetype;
        }
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            ReloadingAudioSource = gameObject.AddComponent<AudioSource>();
            ReloadingAudioSource.spatialBlend = 0.99f;
            ReloadingAudioSource.volume = 2.5f;
            ReloadingAudioSource.maxDistance = 16f;

            ChargingAudioSource = gameObject.AddComponent<AudioSource>();
            ChargingAudioSource.spatialBlend = 0.99f;
            ChargingAudioSource.volume = 2.75f;
            ChargingAudioSource.maxDistance = 16f;

            ReloadSound = ModResource.GetAudioClip("ReloadSound");
            ChargingSound = ModResource.GetAudioClip("ChargingSound");

        }
        public override void SimulateFixedUpdateAlways()
        {
            base.SimulateFixedUpdateAlways();
            if(ChargeOK)
            {
                if (AudioTime == 1)
                {
                    ChargingAudioSource.PlayOneShot(ChargingSound);
                    AudioTime++;
                }
                else if(AudioTime == 64)
                {
                    AudioTime = 0;
                }
                else
                {
                    AudioTime++;
                }
            }
            else
            {
                if(ChargingAudioSource.isPlaying)
                {
                    ChargingAudioSource.Stop();
                }
            }
            ChargingAudioSource.pitch = Time.timeScale;
            ReloadingAudioSource.pitch = Time.timeScale;
        }
        public void OnFire()
        {
            ReloadOK = false;
            if(ChargeOK)
            {
                ChargeOK = false;
                StartCoroutine(ChargeFire());
            }
            StartCoroutine(ReloadJg());
        }
        public IEnumerator ReloadJg()
        {
            Reloadtime = (int)(1f / adShootingBehavour.RateOfFire.Value * 100);
            for (int i = 0; i < Reloadtime; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            ReloadingAudioSource.PlayOneShot(ReloadSound);
            ReloadOK = true;
        }
        public IEnumerator ChargeFire()
        {
            for(int i = 0; i < 2; i++)
            {
                yield return new WaitForSeconds(0.2f);
            }
            adShootingBehavour.PowerSlider.Value -= 150f;
        }
    }
    public class AdChargeBehaviour : BlockScript
    {
        public Transform parentObject;
        public AdShootingBehavour adShootingBehavour;
        public AdChargeUsingBehaviour[] adChargeUsingBehaviour;
        public AdChargeUsingBehaviour[] usingAdChargerUsingBehaivour;

        private int childrensNumber;
        private int childrenUsingNumber;
        private int usingnum = 0;

        public float atCannonDistance;
        private bool thisUsing = false;

        public override void SafeAwake()
        {
            base.SafeAwake();

        }
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            usingnum = 0;
            parentObject = this.transform;
            while (parentObject.gameObject.name != "Simulation Machine")
            {
                parentObject = parentObject.transform.parent;
            }
            adChargeUsingBehaviour = parentObject.GetComponentsInChildren<AdChargeUsingBehaviour>();
            childrensNumber = adChargeUsingBehaviour.Length;
            usingAdChargerUsingBehaivour = new AdChargeUsingBehaviour[0];
            for (int i = 0; i < childrensNumber; i++)
            {
                if(adChargeUsingBehaviour[i].canUseCharge)
                {
                    usingnum++;
                    Array.Resize(ref usingAdChargerUsingBehaivour, usingnum);
                    usingAdChargerUsingBehaivour[usingnum - 1] = adChargeUsingBehaviour[i];
                }
            }
            childrenUsingNumber = usingAdChargerUsingBehaivour.Length;
        }
        public override void SimulateFixedUpdateAlways()
        {
            base.SimulateFixedUpdateAlways();
            if(!thisUsing)
            {
                for (int i = 0; i < childrenUsingNumber; i++)
                {
                    atCannonDistance = Vector3.Distance(this.transform.position + this.transform.forward * 0.5f, usingAdChargerUsingBehaivour[i].transform.position + usingAdChargerUsingBehaivour[i].transform.forward * 0.281f);
                    if (atCannonDistance < 1.5f && usingAdChargerUsingBehaivour[i].ReloadOK && !usingAdChargerUsingBehaivour[i].ChargeOK)
                    {
                        CannonCharge(i);
                    }
                }
            }

        }
        public void CannonCharge(int number)
        {
            usingAdChargerUsingBehaivour[number].ChargeOK = true;
            usingAdChargerUsingBehaivour[number].adShootingBehavour.PowerSlider.Value += 150f;
            thisUsing = true;
            Destroy(this.gameObject);
        }
    }
}