using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Game.Resource;

public class AnimReadData
{
    XmlDocument x;
    private static AnimReadData _instance;

    static object sLock = new object();
    public static AnimReadData Instance      //单例
    {
        get
        {
            //if (null == _instance)
            //{
            //    lock (sLock)
            //    {
                    if (_instance == null)
                    {

                        _instance = new AnimReadData();
                    }
                //}
            //}
            return _instance;
        }
    }

    public void Dispose()
    {
        _instance = null;
    }

    public AnimReadData()
    {
        Init();
    }
    string t;
    void Init()
    {

        string nameTemp = RobotMgr.Instance.rbtnametempt;

        robotid = RobotMgr.Instance.rbt[nameTemp].id;
        x = new XmlDocument();
        List<string> rtid = NormalStringData.DefaultRtID();
        if (rtid.Contains(robotid))
        {
            t = GetXmlByPath().ToString().Trim();
            x.LoadXml(t);
        }
        else
        {
            t=GetResourcesPath("default");
            x.Load(t);
        }
        
        
    }


    public string robotid;
    string path;
    public TextAsset GetXmlByPath()
    {

        path = "Script/Test4/Anim/" + robotid + "/" + robotid;
      
        TextAsset tmp = Resources.Load(path) as TextAsset;

        return tmp;
    }

    public string GetResourcesPath( string opentype)
    {
        string nameTemp = RobotMgr.Instance.rbtnametempt;
        string nameNoType = RobotMgr.NameNoType(nameTemp);
        string pathfile = "";
        if (opentype == "default")
        {
            pathfile = ResourcesEx.GetCommonPathForNoTypeName(nameNoType);
        }
        else
        {
            pathfile = ResourcesEx.GetRobotPathForNoTypeName(nameNoType);
        }
        pathfile = pathfile + "/anim/" + robotid + ".xml";

        return pathfile;
        
    }
	

    public Dictionary<string, string[]> FindAnimData()
    {  
        XmlNodeList nodeList = x.GetElementsByTagName("Anim");
        Dictionary<string, string[]> names = new Dictionary<string, string[]>();
        string[] xl=new string[16];

        int idTemp = 1;
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;   //xe.InnerText:按钮的名称
                    if (xe.GetAttribute("id") !=null)
                    {
                        xl = new string[16];

                        xl[0] = idTemp.ToString();    //labelname

                      
                        xl[1] = xe.GetAttribute("source"); 
                        xl[2] = xe.GetAttribute("start");
                        xl[3] = xe.GetAttribute("end");    //labelname
                        xl[4] = xe.GetAttribute("step");
                        xl[5] = xe.GetAttribute("name"); 
                        string idxe = xe.GetAttribute("id");
                       // Debug.Log("id:"+idxe);
                        if(xe.HasAttribute("parts"))
                        {
                            xl[6] = xe.GetAttribute("parts");
                            //Debug.Log("parts" + xl[6]);
 
                        }
                        else
                        {
                            xl[6] = "";
                        }

                        if (xe.HasAttribute("djid"))
                        {
                            xl[7] = xe.GetAttribute("djid");
                        }
                        else
                        {
                            xl[7] = "id";
                        }

                        if (xe.HasAttribute("shape"))
                        {
                            xl[8] = xe.GetAttribute("shape");
                        }
                        else
                        {
                            xl[8] = "circle";
                        }

                        if (xe.HasAttribute("line"))
                        {
                            xl[9] = xe.GetAttribute("line");
                        }
                        else
                        {
                            xl[9] = "";
                        }

                        if (xe.HasAttribute("type"))
                        {
                            xl[10] = xe.GetAttribute("type");
                        }
                        else
                        {
                            xl[10] = "type";
                        }

                        if (xe.HasAttribute("goname"))
                        {
                            xl[11] = xe.GetAttribute("goname");
                        }
                        else
                        {
                            xl[11] = "null";
                        }

                        if (xe.HasAttribute("pic"))
                        {
                            xl[12] = xe.GetAttribute("pic");
                        }
                        else
                        {
                            xl[12] = "null";
                        }

                        if (xe.HasAttribute("firstPic"))
                        {
                            xl[13] = xe.GetAttribute("firstPic");
                        }
                        else
                        {
                            xl[13] = "false";
                        }

                        if (xe.HasAttribute("lvdainum"))
                        {
                            xl[14] = xe.GetAttribute("lvdainum");
                        }
                        else
                        {
                            xl[14] = null;
                        }

                        if (xe.HasAttribute("sensorID"))
                        {
                            xl[15] = xe.GetAttribute("sensorID");
                            Debug.Log("xl:" + xl[15] + ";" + xl[0]);
                        }
                        else
                        {
                            xl[15] = null;
                        }

                        names.Add(idTemp.ToString(), xl);
                        idTemp++;
                    }
                }

            }
        }
        return names;
    }

    //查找步数
    public string FindStep(string anim)
    {
        XmlNodeList nodeList = x.GetElementsByTagName("Anim");
        Dictionary<string, string[]> names = new Dictionary<string, string[]>();
        string nowstep="0";
        //int i = 1;
        
        if (nodeList != null && nodeList.Count > 0)
        {
            foreach (XmlNode xn in nodeList)
            {
                if (xn != null)
                {
                    XmlElement xe = (XmlElement)xn;   //xe.InnerText:按钮的名称

                
                    if (xe.GetAttribute("id") == anim)
                    {
                       nowstep = xe.GetAttribute("step");     //id,  step
                      // stepNum.Add(i,nowstep);
                    }
                    //i++;
                }

            }
        }
        return nowstep;
    }
}
