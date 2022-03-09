using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;

namespace EduMessage.Services
{
    public class JsonFile
    {
        public async static Task<T> Read<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default(T);
            }

            try
            {
                var content = await File.ReadAllTextAsync(path);
                var projectFolder = await StorageFolder.GetFolderFromPathAsync( Environment.CurrentDirectory);
                //var file = projectFolder.
                //var result = JsonConvert.DeserializeObject<T>(content);
                return default(T);
            }
            catch (Exception e)
            {
                return default(T);
            }            
        }

        public async static Task<bool> Write(string path, string content)
        {
            try
            {
                await File.WriteAllTextAsync(path, content);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
