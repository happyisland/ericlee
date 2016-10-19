using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace Game.Resource
{
    
    public static class ResourcesEx
    {
        static readonly string[] ResFileTypePath = new string[3] { "default", "playerdata", "download"};

        public readonly static string persistentDataPath = Application.persistentDataPath;

        public readonly static string streamingAssetsPath = Application.streamingAssetsPath;
        
        public static ResFileType GetResFileType(string typeStr)
        {
            for (int i = 0, imax = ResFileTypePath.Length; i < imax; ++i)
            {
                if (ResFileTypePath[i].Equals(typeStr))
                {
                    return (ResFileType)(i);
                }
            }
            return ResFileType.Type_playerdata;
        }

        public static string GetFileTypeString(ResFileType type)
        {
            int index = (int)type;
            if (index < 0 || index >= ResFileTypePath.Length)
            {
                return ResFileTypePath[0];
            }
            return ResFileTypePath[index];
        }
        /// <summary>
        /// 获取本地文件根目录
        /// </summary>
        /// <param name="resType"></param>
        /// <returns></returns>
        public static string GetRootPath(ResFileType resType = ResFileType.Type_playerdata)
        {
            return persistentDataPath + "/" + ResFileTypePath[(int)resType];
        }
        /// <summary>
        /// 获得动作路径
        /// </summary>
        /// <returns></returns>
        public static string GetActionsPath(string robotName, ResFileType resType = ResFileType.Type_playerdata)
        {
            string path = GetRootPath(resType) + "/" + robotName + "/actions";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        /// <summary>
        /// 获得动作的完整路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetActionsPath(string robotName, string fileName, ResFileType resType = ResFileType.Type_playerdata)
        {
            string path = GetActionsPath(robotName, resType) + "/" + fileName + ".xml";
            return path;
        }
        /// <summary>
        /// 获取模型文件夹的路径
        /// </summary>
        /// <param name="robotName"></param>
        /// <returns></returns>
        public static string GetRobotPath(string robotName)
        {
            string robotType = RobotMgr.DataType(robotName);
            string name = RobotMgr.NameNoType(robotName);
            return persistentDataPath + "/" + robotType + "/" + name;
        }
        /// <summary>
        /// 获取某个模型的动作列表
        /// </summary>
        /// <param name="robotName">完整的名字</param>
        /// <returns></returns>
        public static List<string> GetRobotActionsPath(string robotName)
        {
            string robotType = RobotMgr.DataType(robotName);
            string name = RobotMgr.NameNoType(robotName);
            string root = persistentDataPath + "/" + robotType + "/" + name + "/actions";
            try
            {
                List<string> list = new List<string>();
                if (Directory.Exists(root))
                {
                    string[] files = Directory.GetFiles(root);
                    if (null != files)
                    {
                        list.AddRange(files);
                    }
                }
                return list;
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
                return null;
            }
        }
        /// <summary>
        /// 获取所有动作列表
        /// </summary>
        /// <param name="resType"></param>
        /// <returns></returns>
        public static List<string> GetAllActionsPath(ResFileType resType = ResFileType.Type_playerdata)
        {
            try
            {
                string root = GetRootPath(resType);
                string[] dirs = Directory.GetDirectories(root);
                if (null != dirs)
                {
                    List<string> list = new List<string>();
                    for (int i = 0, imax = dirs.Length; i < imax; ++i)
                    {
                        string actPath = dirs[i] + "/actions";
                        if (Directory.Exists(actPath))
                        {
                            string[] files = Directory.GetFiles(actPath);
                            if (null != files)
                            {
                                list.AddRange(files);
                            }
                        }
                    }
                    return list;
                }
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
                return null;
            }
            return null;
        }
        
        //获取文件名
        private static string GetFileName(string path)
        {
            int index = path.LastIndexOf(".");
            return index >= 0 ? path.Remove(index) : path;
        }

        public static T Load<T>(string path)
    where T : UnityEngine.Object
        {
            string newPath = GetFileName(path);
            try
            {
                return Resources.Load<T>(newPath);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static UnityEngine.Object Load(string path)
        {
            string newPath = GetFileName(path);
            try
            {
                return Resources.Load(newPath);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public enum XmlNodeType
    {
        head,
        body
    }

    public enum ResFileType
    {
        Type_default = 0,
        Type_playerdata,
        Type_download
    }
}
