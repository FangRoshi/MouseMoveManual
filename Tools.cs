using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace MouseMoveManual
{
    class Tools
    {
        /// <summary>
        /// 将 List<T> 保存到 XML 文件
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">要保存的列表</param>
        /// <param name="filePath">XML 文件路径</param>
        public static void SaveListToXml<T>(List<T> list, string filePath)
        {
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 创建 XML 序列化器
                XmlSerializer serializer = new XmlSerializer(typeof(List<T>));

                // 使用 using 确保正确释放资源
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(fileStream, list);
                }

                Debug.Log("XML 文件保存成功:"+ filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"保存 XML 文件时出错: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从 XML 文件读取并还原为 List<T>
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="filePath">XML 文件路径</param>
        /// <returns>还原后的列表</returns>
        public static List<T> LoadListFromXml<T>(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    //throw new FileNotFoundException($"XML 文件未找到: {filePath}");
                    return null;
                }

                // 创建 XML 反序列化器
                XmlSerializer serializer = new XmlSerializer(typeof(List<T>));

                // 使用 using 确保正确释放资源
                using (StreamReader reader = new StreamReader(filePath))
                {
                    return (List<T>)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                //throw new Exception($"读取 XML 文件时出错: {ex.Message}", ex);
                return null;
            }
        }
    }
}
