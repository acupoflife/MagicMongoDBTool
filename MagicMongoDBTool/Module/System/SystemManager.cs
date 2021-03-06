﻿using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MagicMongoDBTool.Module
{
    public static class SystemManager
    {
        /// <summary>
        /// 测试模式
        /// </summary>
        public static Boolean DEBUG_MODE = false;
        /// <summary>
        /// 是否为MONO
        /// </summary>
        public static Boolean MONO_MODE = false;
        /// <summary>
        /// 版本号
        /// </summary>
        public static String Version = Application.ProductVersion;
        /// <summary>
        /// 配置实例
        /// </summary>
        public static ConfigHelper ConfigHelperInstance = new ConfigHelper();
        /// <summary>
        /// 选择对象标签
        /// </summary>
        public static String SelectObjectTag = String.Empty;
        /// <summary>
        /// 获得当前对象的种类
        /// </summary>
        public static String SelectTagType
        {
            get
            {
                return GetTagType(SelectObjectTag);
            }
        }
        /// <summary>
        /// 驱动版本 MongoDB.Driver.DLL
        /// </summary>
        public static String MongoDBDriverVersion;
        /// <summary>
        /// 驱动版本 MongoDB.Bson.DLL
        /// </summary>
        public static String MongoDBBsonVersion;

        /// <summary>
        /// 获得当前对象的路径
        /// </summary>
        public static String SelectTagData
        {
            get
            {
                return GetTagData(SelectObjectTag);
            }
        }
        /// <summary>
        /// 获得对象的种类
        /// </summary>
        /// <returns></returns>
        public static String GetTagType(String ObjectTag)
        {
            if (ObjectTag == String.Empty)
            {
                return string.Empty;
            }
            else
            {
                return ObjectTag.Split(":".ToCharArray())[0];
            }
        }
        /// <summary>
        /// 获得对象的路径
        /// </summary>
        /// <returns></returns>
        public static String GetTagData(String ObjectTag)
        {
            if (ObjectTag == String.Empty)
            {
                return string.Empty;
            }
            else
            {
                if (ObjectTag.Split(":".ToCharArray()).Length == 2)
                {
                    return ObjectTag.Split(":".ToCharArray())[1];
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        /// <summary>
        /// 文字资源
        /// </summary>
        public static StringResource mStringResource = new MagicMongoDBTool.Module.StringResource();
        /// <summary>
        /// 对话框子窗体的统一管理
        /// </summary>
        /// <param name="mfrm"></param>
        /// <param name="isDispose">有些时候需要使用被打开窗体产生的数据，所以不能Dispose</param>
        /// <param name="isUseAppIcon"></param>
        public static void OpenForm(Form mfrm, Boolean isDispose, Boolean isUseAppIcon)
        {
            mfrm.StartPosition = FormStartPosition.CenterParent;
            mfrm.BackColor = System.Drawing.Color.White;
            mfrm.FormBorderStyle = FormBorderStyle.FixedSingle;
            mfrm.MaximizeBox = false;
            if (isUseAppIcon)
            {
                mfrm.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            mfrm.ShowDialog();
            mfrm.Close();
            if (isDispose) { mfrm.Dispose(); }
        }
        /// <summary>
        /// 获得当前服务器配置
        /// </summary>
        /// <param name="SrvName"></param>
        /// <returns></returns>
        public static ConfigHelper.MongoConnectionConfig GetCurrentServerConfig()
        {
            String ServerName = SelectObjectTag.Split(":".ToCharArray())[1];
            ServerName = ServerName.Split("/".ToCharArray())[(int)MongoDBHelper.PathLv.ConnectionLV];
            ConfigHelper.MongoConnectionConfig rtnMongoConnectionConfig = new ConfigHelper.MongoConnectionConfig();
            if (ConfigHelperInstance.ConnectionList.ContainsKey(ServerName))
            {
                rtnMongoConnectionConfig = ConfigHelperInstance.ConnectionList[ServerName];
            }
            return rtnMongoConnectionConfig;
        }
        /// <summary>
        /// 根据服务器名称获取配置
        /// </summary>
        /// <param name="mongoSvrKey"></param>
        /// <returns></returns>
        public static ConfigHelper.MongoConnectionConfig GetCurrentServerConfig(String mongoSvrKey)
        {
            ConfigHelper.MongoConnectionConfig rtnMongoConnectionConfig = new ConfigHelper.MongoConnectionConfig();
            if (ConfigHelperInstance.ConnectionList.ContainsKey(mongoSvrKey))
            {
                rtnMongoConnectionConfig = ConfigHelperInstance.ConnectionList[mongoSvrKey];
            }
            return rtnMongoConnectionConfig;
        }
        /// <summary>
        /// 获得当前服务器
        /// </summary>
        /// <returns></returns>
        public static MongoServer GetCurrentServer()
        {
            return MongoDBHelper.GetMongoServerBySvrPath(SelectObjectTag);
        }
        /// <summary>
        /// 获得当前数据库
        /// </summary>
        /// <returns></returns>
        public static MongoDatabase GetCurrentDataBase()
        {
            return MongoDBHelper.GetMongoDBBySvrPath(SelectObjectTag);
        }
        /// <summary>
        /// 获得当前数据集
        /// </summary>
        /// <returns></returns>
        public static MongoCollection GetCurrentCollection()
        {
            return MongoDBHelper.GetMongoCollectionBySvrPath(SelectObjectTag);
        }
        /// <summary>
        /// 获得系统JS数据集
        /// </summary>
        /// <returns></returns>
        public static MongoCollection GetCurrentJsCollection()
        {
            MongoDatabase mongoDB = GetCurrentDataBase();
            MongoCollection mongoJsCol = mongoDB.GetCollection(MongoDBHelper.COLLECTION_NAME_JAVASCRIPT);
            return mongoJsCol;
        }
        /// <summary>
        /// Current selected document
        /// </summary>
        public static BsonDocument CurrentDocument;
        /// <summary>
        /// Set Current Document
        /// </summary>
        /// <param name="SelectDocId"></param>
        public static void SetCurrentDocument(TreeNode CurrentNode)
        {
            TreeNode rootNode = FindRootNode(CurrentNode);
            BsonValue SelectDocId = (BsonValue)rootNode.Tag;
            MongoCollection mongoCol = GetCurrentCollection();
            BsonDocument doc = mongoCol.FindOneAs<BsonDocument>(Query.EQ(MongoDBHelper.KEY_ID, SelectDocId));
            CurrentDocument = doc;
        }
        /// <summary>
        /// 获取树形的根
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static TreeNode FindRootNode(TreeNode node)
        {
            if (node.Parent == null)
            {
                return node;
            }
            else
            {
                return FindRootNode(node.Parent);
            }
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="Result"></param>
        public static void SaveResultToJSonFile(BsonDocument Result)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = MongoDBHelper.TxtFilter;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(dialog.FileName, false);
                writer.Write(Result.ToJson(JsonWriterSettings));
                writer.Close();
            }
        }
        /// <summary>
        /// Javascript文件的保存
        /// </summary>
        /// <param name="Result"></param>
        public static void SaveJavascriptFile(String Result)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = MongoDBHelper.JsFilter;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(dialog.FileName, false);
                writer.Write(Result);
                writer.Close();
            }
        }
        public static String LoadFile() {
            OpenFileDialog dialog = new OpenFileDialog();
            String Context = String.Empty;
            if (dialog.ShowDialog() == DialogResult.OK) {
                try
                {
                    StreamReader reader = new StreamReader(dialog.FileName);
                    Context = reader.ReadToEnd();
                    reader.Close();
                }
                catch (Exception ex)
                {
                    SystemManager.ExceptionDeal(ex);    
                }
            }
            return Context;
        }
        /// <summary>
        /// Javascript文件的保存
        /// </summary>
        /// <param name="Result"></param>
        public static void SaveTextFile(String Result,String Filter)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = Filter;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(dialog.FileName, false);
                writer.Write(Result);
                writer.Close();
            }
        }
        /// <summary>
        /// 获得JS名称列表
        /// </summary>
        /// <returns></returns>
        public static List<String> GetJsNameList()
        {
            List<String> jsNamelst = new List<String>();
            foreach (var item in GetCurrentJsCollection().FindAllAs<BsonDocument>())
            {
                jsNamelst.Add(item.GetValue(MongoDBHelper.KEY_ID).ToString());
            }
            return jsNamelst;
        }
        /// <summary>
        /// 是否使用默认语言
        /// </summary>
        /// <returns></returns>
        internal static Boolean IsUseDefaultLanguage
        {
            get
            {
                if (ConfigHelperInstance == null)
                {
                    return true;
                }
                return (ConfigHelperInstance.LanguageFileName == "English" ||
                        String.IsNullOrEmpty(ConfigHelperInstance.LanguageFileName));
            }
        }
        #region"异常处理"
        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="ex">异常</param>
        internal static void ExceptionDeal(Exception ex)
        {
            ExceptionDeal(ex, "Exception", "Exception");
        }
        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="Message">消息</param>
        internal static void ExceptionDeal(Exception ex, String Message)
        {
            ExceptionDeal(ex, "Exception", Message);
        }
        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="Title">标题</param>
        /// <param name="Message">消息</param>
        internal static void ExceptionDeal(Exception ex, String Title, String Message)
        {
            String ExceptionString;
            ExceptionString = "MongoDB.Driver.DLL:" + MongoDBDriverVersion + System.Environment.NewLine;
            ExceptionString += "MongoDB.Bson.DLL:" + MongoDBBsonVersion + System.Environment.NewLine;
            ExceptionString += ex.ToString();
            MyMessageBox.ShowMessage(Title, Message, ExceptionString, true);
            ExceptionLog(ExceptionString);
        }
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="ExceptionString">异常文字</param>
        internal static void ExceptionLog(String ExceptionString)
        {
            StreamWriter exLog = new StreamWriter("Exception.log", true);
            exLog.WriteLine("DateTime:" + DateTime.Now.ToString());
            exLog.WriteLine(ExceptionString);
            exLog.Close();
        }
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="ex">异常对象</param>
        internal static void ExceptionLog(Exception ex)
        {
            String ExceptionString;
            ExceptionString = "MongoDB.Driver.DLL:" + MongoDBDriverVersion + System.Environment.NewLine;
            ExceptionString += "MongoDB.Bson.DLL:" + MongoDBBsonVersion + System.Environment.NewLine;
            ExceptionString += ex.ToString();
            ExceptionLog(ExceptionString);
        }

        #endregion
        /// <summary>
        /// JsonWriterSettings
        /// </summary>
        public static MongoDB.Bson.IO.JsonWriterSettings JsonWriterSettings = new MongoDB.Bson.IO.JsonWriterSettings()
        {
            Indent = true,
            NewLineChars = System.Environment.NewLine,
            OutputMode = MongoDB.Bson.IO.JsonOutputMode.Strict
        };
        /// <summary>
        /// 初始化
        /// </summary>
        internal static void InitLanguage()
        {
            //语言的初始化
            if (!IsUseDefaultLanguage)
            {
                String LanguageFile = "Language" + System.IO.Path.DirectorySeparatorChar + SystemManager.ConfigHelperInstance.LanguageFileName;
                if (File.Exists(LanguageFile))
                {
                    SystemManager.mStringResource.InitLanguage(LanguageFile);
                    MyMessageBox.SwitchLanguage(mStringResource);
                }
            }
        }
    }
}
