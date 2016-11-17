using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Event;

namespace Game.Platform
{
    class DeviceInfo
    {
        private string m_name;
        public string Name { get { return this.m_name; } set { this.m_name = value; } }

        private string m_mac;
        public string Mac { get { return this.m_mac; } }

        public int RSSI = 0;//信号
        public DeviceInfo(string str)
        {
            Parse(str);
        }

        //解析蓝牙设备信息，后17位为mac地址，前面的都是名称
        void Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            string[] ary = str.Split('\n');
            if (ary.Length > 0)
            {
                m_name = ary[0];
            }
            if (ary.Length > 1)
            {
                m_mac = ary[1];
            }
            if (ary.Length > 2)
            {
                RSSI = int.Parse(ary[2]);
            }
            /*if(str.Length>17)
            {
                m_name = str.Substring(0, str.Length - 17);
                m_mac = str.Substring(str.Length - 17, 17);
            }*/
        }
    }

    class BluetoothMgr
    {
        private List<DeviceInfo> m_matchedDevice = new List<DeviceInfo>();                       //已经匹配过的蓝牙设备
        public List<DeviceInfo> MatchedDevice { get { return m_matchedDevice; } }

        private List<DeviceInfo> m_newDevice = new List<DeviceInfo>();                                //新搜寻到的蓝牙设备
        public List<DeviceInfo> NewDevice { get { return m_newDevice; } }

        private bool m_connenctState = false;
        public bool ConnenctState 
        {
            get { return m_connenctState; }
        }

        public BluetoothMgr()
        {
            
        }
        //当发现匹配设备
        public void MatchedFound(string name)
        {
            DeviceInfo device = new DeviceInfo(name);
            if (Check(device))
            {
                m_matchedDevice.Add(device);
                EventMgr.Inst.Fire(EventID.BLUETOOTH_ON_MATCHED_DEVICE_FOUND, new EventArg(device));
            }
        }

        //当发现新设备
        public void NewFound(string name)
        {
            DeviceInfo device = new DeviceInfo(name);
            if (Check(device))
            {
                m_newDevice.Add(device);
                EventMgr.Inst.Fire(EventID.BLUETOOTH_ON_DEVICE_FOUND, new EventArg(device));
            }
        }

        public void OldMatchedFound()
        {
            for (int i = 0, icount = m_matchedDevice.Count; i < icount; ++i)
            {
                EventMgr.Inst.Fire(EventID.BLUETOOTH_ON_MATCHED_DEVICE_FOUND, new EventArg(m_matchedDevice[i]));
            }
        }

        public void OldNewFound()
        {
            for (int i = 0, icount = m_newDevice.Count; i < icount; ++i)
            {
                EventMgr.Inst.Fire(EventID.BLUETOOTH_ON_DEVICE_FOUND, new EventArg(m_newDevice[i]));
            }
        }

        public void MatchResult(bool boo)
        {
            m_connenctState = boo;
            if (!m_connenctState)
            {
                PlatformMgr.Instance.PowerData.isAdapter = false;
                PlatformMgr.Instance.PowerData.isChargingFinished = false;
                EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(boo));
            }
        }

        //剔除掉无效的蓝牙名称
        public bool Check(DeviceInfo device)
        {
            if (device.Name.Equals("null")) return false;
            foreach (DeviceInfo item in m_matchedDevice)
            {
                if (item.Mac == device.Mac) 
                {
                    return false;
                }
            }
            foreach (DeviceInfo item in m_newDevice)
            {
                if (item.Mac == device.Mac)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 通过mac地址获取设备名字
        /// </summary>
        /// <param name="mac"></param>
        /// <returns></returns>
        public string GetNameForMac(string mac)
        {
            for (int i = 0, icount = m_matchedDevice.Count; i < icount; ++i)
            {
                if (m_matchedDevice[i].Mac.Equals(mac))
                {
                    return m_matchedDevice[i].Name;
                }
            }
            for (int i = 0, icount = m_newDevice.Count; i < icount; ++i)
            {
                if (m_newDevice[i].Mac.Equals(mac))
                {
                    return m_newDevice[i].Name;
                }
            }
            return string.Empty;
        }

        public void BlueRename(string name, string mac)
        {
            for (int i = 0, icount = m_matchedDevice.Count; i < icount; ++i)
            {
                if (m_matchedDevice[i].Mac.Equals(mac))
                {
                    m_matchedDevice[i].Name = name;
                }
            }
            for (int i = 0, icount = m_newDevice.Count; i < icount; ++i)
            {
                if (m_newDevice[i].Mac.Equals(mac))
                {
                    m_newDevice[i].Name = name;
                }
            }
        }

        public void ClearDevice()
        {
            m_matchedDevice.Clear();
            m_newDevice.Clear();
        }
        /// <summary>
        /// 断开蓝牙
        /// </summary>
        public void DisConnenctBuletooth()
        {
            ClearDevice();
            m_connenctState = false;
        }
    }
}
