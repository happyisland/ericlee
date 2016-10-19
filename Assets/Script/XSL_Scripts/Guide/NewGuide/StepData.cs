//----------------------------------------------
//            积木2: xiongsonglin
// 指引步骤数据结构
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace GuideView
{
    public class StepData
    {
        public int stepID;  //步骤id
        public string targetPath;  //目标按钮的路径
        public int lastStepID;
        public int nextStepID;
        public string stepWaitID = "";
        public int width;
        public int height;
        public int offset_x;  //x的修正
        public int offset_y;  //y的修正
        public TipType tipObjType;
        public bool isText;
        public bool flag1;
        public int flag2;
        public string flag3;

        public StepDataXml TurnToStepdataXml()
        {
            StepDataXml dataxml = new StepDataXml();
            dataxml.stepID = this.stepID.ToString();
            dataxml.targetPath = this.targetPath;
            dataxml.lastStepID = this.lastStepID.ToString() ;
            dataxml.nextStepID = this.nextStepID.ToString();
            dataxml.stepWaitID = this.stepWaitID;
            dataxml.width = this.width.ToString();
            dataxml.height = this.height.ToString();
            dataxml.offset_x = this.offset_x.ToString();
            dataxml.offset_y = this.offset_y.ToString();
            dataxml.isText = this.isText.ToString();
            dataxml.tipObjType = this.tipObjType.ToString();
            dataxml.flag1 = this.flag1.ToString();
            dataxml.flag2 = this.flag2.ToString();
            dataxml.flag3 =  this.flag3;
            return dataxml;
        }

        public string GetTiptext_1()
        {
            if (isText)
            {
                char[] split = new char[1];
                split[0] = '/';
                string[] str = LauguageTool.GetIns().GetText("guideTip_" + stepID).Split(split);
                return str[0];
            }
            else
                return "";
        }
        public string GetTiptext_2()
        {
            if (isText)
            {
                char[] split = new char[1];
                split[0] = '/';
                string[] str = LauguageTool.GetIns().GetText("guideTip_" + stepID).Split(split);
                if (str.Length > 2)
                {
                    return str[1];
                }
                return "";
            }
            else
                return "";
        }

        //public static 
    }

    public class StepDataXml
    {
        [XmlAttribute]
        public string stepID;  //步骤id
        [XmlElement]
        public string targetPath;  //目标按钮的路径
        [XmlElement]
        public string lastStepID;
        [XmlElement]
        public string nextStepID;
        [XmlElement]
        public string stepWaitID = "";
        [XmlElement]
        public string width;
        [XmlElement]
        public string height;
        [XmlElement]
        public string offset_x;  //x的修正
        [XmlElement]
        public string offset_y;  //y的修正
        [XmlElement]
        public string isText;
        [XmlElement]
        public string tipObjType;
        [XmlElement]
        public string flag1;
        [XmlElement]
        public string flag2;
        [XmlElement]
        public string flag3;
        /// <summary>
        /// 转换成stepdata
        /// </summary>
        /// <param name="dataxml"></param>
        /// <returns></returns>
        public StepData TurnToStepdata()
        {
            StepData stepd = new StepData();
            stepd.stepID = int.Parse(this.stepID);
            stepd.targetPath = this.targetPath;
            stepd.lastStepID = int.Parse(this.lastStepID);
            stepd.nextStepID = int.Parse(this.nextStepID);
            stepd.stepWaitID = this.stepWaitID;
            stepd.width = int.Parse(this.width);
            stepd.height = int.Parse(this.height);
            stepd.offset_x = int.Parse(this.offset_x);
            stepd.offset_y = int.Parse(this.offset_y);
            stepd.isText = bool.Parse(this.isText);
            stepd.tipObjType = GetTipType(this.tipObjType);
            stepd.flag1 = bool.Parse(this.flag1);
            stepd.flag2 = int.Parse(this.flag2);
            stepd.flag3 = this.flag3;
            return stepd;
        }

        public TipType GetTipType(string tipType)
        {
            int len = System.Enum.GetNames(typeof(TipType)).Length;
            int type = int.Parse(tipType);
            if (type > len || type < 0)
            {
                type = len;
            }
            return (TipType)type;
        }

    }

    public class TotalSteps
    {
        [XmlElement]
        public List<StepDataXml> totalSteps;

        public List<StepData> TureToStepList()
        {
            List<StepData> steps = new List<StepData>();
            foreach (var tem in totalSteps)
            {
                steps.Add(tem.TurnToStepdata());
            }
            return steps;
        }

        public TotalSteps()
        {
            totalSteps = new List<StepDataXml>();
        }
    }

    public class WriteStep
    {

        public static void WRITE()
        {
            TotalSteps steps = new TotalSteps();
            for (int i = 0; i < 30; i++)
            {
                StepData data = new StepData();
                data.stepID = i;
                data.offset_x = 0;
                data.offset_y = 0;
                data.stepWaitID = "";
                data.targetPath = "a";
                data.lastStepID = 0;
                data.nextStepID = 0;
                data.isText = false;
                data.flag1 = false;
                data.flag2 = 0;
                data.flag3 = "";
                data.height = 0;
                data.width = 0;
             //   data.tipObjType = TipType.None;
                steps.totalSteps.Add(data.TurnToStepdataXml());
            }

            string str = MyMVC.XmlHelper.XmlSerialize(steps, System.Text.Encoding.UTF8);

            FileStream fs = new FileStream("e:/heheee.xml", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            fs.SetLength(0);//首先把文件清空了。
            sw.Write(str);//写你的字符串。
            sw.Close();
        }


    }
}
