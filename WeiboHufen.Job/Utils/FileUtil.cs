using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeiboHufen.Job.Utils
{
    public static class FileUtil
    {
        public static T GetJson<T>(string fileName)
        {
            var json = File.ReadAllText(fileName);

            JsonConvert.DeserializeObject<T>(json);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string JsonToString(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public static void Save(string fileName, object contents)
        {
            var filePath = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            var jsonStr = JsonConvert.SerializeObject(contents);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.WriteAllText(fileName, jsonStr);
        }
    }
}
