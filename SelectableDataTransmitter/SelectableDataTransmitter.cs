﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Globalization;
using CommNet;


namespace SelectableDataTransmitter
{

    public class SelectableDataTransmitter : ModuleDataTransmitter
    {
        public class DataTransmitterData
        {
            public AntennaType antennaType;
            public float packetInterval;
            public float packetSize;
            public double packetResourceCost;
            public string requiredResource;
            public double antennaPower;
            public bool antennaCombinable = false;
            public double antennaCombinableExponent;
        }


        [KSPField]
        public AntennaType defaultAntennaType = AntennaType.DIRECT;

        [KSPField]
        public float reconfigTime = 60.0f;

        [KSPAction("Set DIRECT")]
        public void setDirectAction(KSPActionParam param)
        { setDirect(); }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiName = "Set DIRECT", guiActiveUnfocused = true, unfocusedRange = 4f, externalToEVAOnly = true)]
        public virtual void setDirect()
        {
            if (!reconfigInProgress)
                SetValues(AntennaType.DIRECT);
        }


        [KSPAction(guiName = "Set RELAY")] //, requireFullControl = false, isPersistent = true, advancedTweakable = false)]
        public void setRelayAction(KSPActionParam param)
        { setRelay(); }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiName = "Set RELAY", guiActiveUnfocused = true, unfocusedRange = 4f, externalToEVAOnly = true)]
        public virtual void setRelay()
        {
            if (!reconfigInProgress)
                SetValues(AntennaType.RELAY);
        }

        [KSPField(isPersistant = true)]
        bool antennaTypeConfigured = false;

        [KSPField(isPersistant = true)]
        AntennaType configuredAntennaType = AntennaType.INTERNAL;

        public List<DataTransmitterData> transData;

        bool reconfigInProgress = false;
        double reconfigStartTime = 0;
        const string modName = "SelectableDataTransmitter";

        void setEvents()
        {
            Debug.Log("setEvents");
            //this.Events["ChangeAntennatype"].guiName = this.antennaType.ToString();
            if (reconfigInProgress)
            {
                this.Events["setDirect"].guiName = "Reconfig in progress: " + (reconfigTime - (Planetarium.GetUniversalTime() - reconfigStartTime)).ToString("0.0");
                this.Events["setRelay"].guiActive = false;
                this.Events["setDirect"].guiActive = true;
            }
            else
            {
                this.Events["setDirect"].guiName = "Set DIRECT";
                this.Events["setRelay"].guiActive = true;
                this.Events["setDirect"].guiActive = true;
                //if (HighLogic.LoadedSceneIsEditor)
                //    return;
                if (this.antennaType == AntennaType.RELAY)
                {
                    this.Events["setRelay"].guiActive = false;
                    this.Events["setRelay"].guiActiveEditor = false;
                    this.Events["setDirect"].guiActiveEditor = true;
                }
                else
                {
                    this.Events["setDirect"].guiActive = false;
                    this.Events["setDirect"].guiActiveEditor = false;
                    this.Events["setRelay"].guiActiveEditor = true;

                }
            }
            this.powerText = KSPUtil.PrintSI(this.antennaPower, string.Empty, 3, false) + ((!this.antennaCombinable) ? string.Empty : " (Combinable)");
        }


        public override void OnAwake()
        {
            if (this.transData == null)
            {
                this.transData = new List<DataTransmitterData>();
            }

            base.OnAwake();
        }


        static string SafeLoad(string value, float oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }
        static string SafeLoad(string value, string oldvalue)
        {
            if (value == null)
                return oldvalue;
            return value;
        }
        static string SafeLoad(string value, double oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }

        static string SafeLoad(string value, bool oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }

        new public void Load(ConfigNode node)
        {
            DataTransmitterData data = new DataTransmitterData();
            data.antennaType = this.antennaType;
            data.packetInterval = this.packetInterval;
            data.packetSize = this.packetSize;
            data.packetResourceCost = this.packetResourceCost;
            data.antennaPower = this.antennaPower;
            data.antennaCombinable = this.antennaCombinable;
            data.antennaCombinableExponent = this.antennaCombinableExponent;

            if (node != null)
            {
                Debug.Log("Loading");
                data.antennaType = (AntennaType)Enum.Parse(typeof(AntennaType), SafeLoad(node.GetValue("antennaType"), data.antennaType.ToString()));
                data.packetInterval = float.Parse(SafeLoad(node.GetValue("packetInterval"), data.packetInterval));
                data.packetSize = float.Parse(SafeLoad(node.GetValue("packetSize"), data.packetSize));
                data.packetResourceCost = double.Parse(SafeLoad(node.GetValue("packetResourceCost"), data.packetResourceCost));
                data.requiredResource = SafeLoad(node.GetValue("requiredResource"), "ElectricCharge");
                data.antennaPower = double.Parse(SafeLoad(node.GetValue("antennaPower"), data.antennaPower));
                data.antennaCombinable = bool.Parse(SafeLoad(node.GetValue("antennaCombinable"), data.antennaCombinable));
                data.antennaCombinableExponent = double.Parse(SafeLoad(node.GetValue("antennaCombinableExponent"), data.antennaCombinableExponent));
                Debug.Log("Loaded: " + data.antennaType.ToString() + "    " + node.GetValue("antennaType"));
                transData.Add(data);
            }
        }

        void LoadConfigs()
        {
            Debug.Log("OnStart");
            ConfigNode node = null;

            if (this.transData == null)
            {
                this.transData = new List<DataTransmitterData>();
            }
            if (part != null && part.partInfo != null)
            {
                ConfigNode entirePartCFG = GameDatabase.Instance.GetConfigNode(part.partInfo.partUrl);
                if (entirePartCFG != null)
                {
                    ConfigNode[] partNodes = entirePartCFG.GetNodes();
                    foreach (var n1 in partNodes)
                    {
                        if (n1.GetValue("name") == modName)
                        {
                            node = n1;
                            break;
                        }
                    }

                    if (node != null)
                    {
                        if (node.HasNode("ANTENNATYPE"))
                        {
                            this.transData.Clear();

                            ConfigNode[] nodes = node.GetNodes("ANTENNATYPE");
                            int i = 0;
                            int num = nodes.Length;
                            while (i < num)
                            {

                                Load(nodes[i]);
                                i++;
                            }
                        }
                    }
                }
                Debug.Log("antennaTypeConfigured: " + antennaTypeConfigured.ToString() + "   configuredAntennaType: " + configuredAntennaType.ToString());
                if (antennaTypeConfigured)
                    SetValues(configuredAntennaType, true);
                else
                    SetValues(defaultAntennaType, true);
            }
            //  setEvents();
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            LoadConfigs();
        }

        public override void OnLoad(ConfigNode node2)
        {
            Debug.Log("OnLoad");
           
            if (this.transData == null)
                this.transData = new List<DataTransmitterData>();

            //base.Load(node);
            base.OnLoad(node2);
            LoadConfigs();
            // setEvents();
            CommNetNetwork.Instance.CommNet.Rebuild();
            CommNetNetwork.Reset();
        }

        double newAntennaPower;

        void SetValues(AntennaType type, bool init = false)
        {
            Debug.Log("SetValues for " + type.ToString());
            foreach (var data in transData)
            {
                if (data.antennaType == type)
                {
                    antennaTypeConfigured = true;
                    configuredAntennaType = type;

                    this.antennaType = type;
                    this.packetInterval = data.packetInterval;
                    this.packetSize = data.packetSize;
                    this.packetResourceCost = data.packetResourceCost;
                    
                    this.antennaPower = data.antennaPower;
                    newAntennaPower = data.antennaPower;
                    this.antennaCombinable = data.antennaCombinable;
                    this.antennaCombinableExponent = data.antennaCombinableExponent;
                    this.powerText = KSPUtil.PrintSI(this.antennaPower, string.Empty, 3, false) + ((!this.antennaCombinable) ? string.Empty : " (Combinable)");
                    
                    // Braces just to keep it together
                    {
                        resHandler.inputResources.Clear();
                        ModuleResource moduleResource = new ModuleResource();
                        moduleResource.name = data.requiredResource;
                        moduleResource.title = KSPUtil.PrintModuleName(data.requiredResource);
                        moduleResource.id = data.requiredResource.GetHashCode();
                        moduleResource.rate = 1.0;
                        this.resHandler.inputResources.Add(moduleResource);
                    }


                    Log.Info("antennaType: " + antennaType);
                    Log.Info("packetInterval: " + packetInterval);
                    Log.Info("packetSize: " + packetSize);
                    Log.Info("packetResourceCost: " + packetResourceCost);
                    Log.Info("requiredResource: " + data.requiredResource);
                    Log.Info("antennaPower: " + antennaPower);
                    Log.Info("anennaCombinable: " + antennaCombinable);
                    Log.Info("antennaCombinableExponent: " + antennaCombinableExponent);
                    Log.Info("powerText: " + powerText);
                    
                    // setEvents();
                    if (!init && HighLogic.LoadedSceneHasPlanetarium &&  !HighLogic.LoadedSceneIsEditor)
                    {
                        Log.Info("reconfig in progress, antennaPower: 0");
                        this.antennaPower = 0;
                        reconfigInProgress = true;
                        reconfigStartTime = Planetarium.GetUniversalTime();
                        setEvents();
                    }
                    else
                    {
                        
                        if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneHasPlanetarium)
                            setEvents();
                    }
                   
                    return;
                }
            }
            Debug.LogError("Missing antenna type: " + type.ToString());
        }


        string GetInfo2(DataTransmitterData data)
        {
            string str = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(data.antennaType.ToString().ToLowerInvariant());
            string text = KSPUtil.PrintSI(data.antennaPower, string.Empty, 3, false);
            string text2 = string.Empty;
            text2 = text2 + "<color=#00ffffff><b>Antenna " + str + "</b> mode</color>\n";
            text2 = text2 + "<b>Antenna Rating: </b>" + this.powerText + "\n";
            text2 = text2 + "vs L1 DSN: " + KSPUtil.PrintSI(CommNetScenario.RangeModel.GetMaximumRange(data.antennaPower, GameVariables.Instance.GetDSNRange(0f)), "m", 3, false);
            text2 = text2 + "\nvs L2 DSN: " + KSPUtil.PrintSI(CommNetScenario.RangeModel.GetMaximumRange(data.antennaPower, GameVariables.Instance.GetDSNRange(0.5f)), "m", 3, false);
            text2 = text2 + "\nvs L3 DSN: " + KSPUtil.PrintSI(CommNetScenario.RangeModel.GetMaximumRange(data.antennaPower, GameVariables.Instance.GetDSNRange(1f)), "m", 3, false);
            text2 += "\n";
            if (data.antennaType != AntennaType.INTERNAL)
            {
                text2 = text2 + "\n<b>Packet size: </b>" + data.packetSize.ToString("0.0") + " Mits\n";
                text2 = text2 + "<b>Bandwidth: </b>" + (data.packetSize / data.packetInterval).ToString("0.0###") + " Mits/sec\n";
                text2 += "\n\nWhen Transmitting:";
                text2 += this.resHandler.PrintModuleResources(data.packetResourceCost / (double)data.packetInterval);
            }
            else
            {
                text2 += "\n<i>Cannot transmit science</i>\n";
            }
            if (!this.moduleIsEnabled)
            {
                text2 += "<b><i>Disabled by default</b></i>\n";
            }
            return text2;
        }

        public override string GetInfo()
        {
            if (transData == null || transData.Count == 0)
                LoadConfigs();

            string s = "";
            foreach (var data in transData)
            {
                s += GetInfo2(data) + "-----------------\n";
            }
            if (s == "")
                s = base.GetInfo();
            return s;
        }

        public void LateUpdate()
        {
            if (HighLogic.LoadedSceneIsEditor)
                return;

            if (reconfigInProgress)
            {
                if (Planetarium.GetUniversalTime() - reconfigStartTime > reconfigTime)
                {
                    reconfigInProgress = false;
                    this.antennaPower = newAntennaPower;
                    if (HighLogic.CurrentGame.Parameters.Difficulty.EnableCommNet)
                    {
                        Log.Info("Network rebuild");
                        CommNetNetwork.Instance.CommNet.Rebuild();
                        CommNetNetwork.Reset();
                    }
                }
                setEvents();
            }

        }

    }
}
