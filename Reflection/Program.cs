using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Reflection.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using static Reflection.JsonHelper;

namespace Reflection
{
    class Program
    {
        static void Main(string[] args)
        {
            Services services = new Services();
            services.Generate();

        }

        public class Services
        {
            IFileService file = new FileService();
            public void Generate()
            {
                var fileList = file.ReadDomainModel();            
                var setting = JsonHelper.Settings;
                PropertyInfo[] propertyInfos;
                propertyInfos = typeof(Product).GetProperties();
                IManagerService managerService = new ManagerService();

                foreach (var fileName in fileList)
                {

                    var configureManagerAbstract = new ConfigureClassSetting().AbstractServiceSetting(fileName);
                    managerService.addManagerAbstract(configureManagerAbstract, setting , fileName);

                    var configureManagerConcrete = new ConfigureClassSetting().ConcreteServiceSetting(fileName);
                    managerService.addManagerConcrete(configureManagerConcrete, setting, fileName);

                }
            }

        }

    }

    public interface IManagerService
    {
        void addManagerAbstract(ConfigureClass configureClass, SettingModel setting, string ModelName);
        void addManagerConcrete(ConfigureClass configureClass, SettingModel setting, string ModelName);
    }
    public class ManagerService : IManagerService
    {
        IFileService file = new FileService();
        public void addManagerAbstract(ConfigureClass configureClass, SettingModel setting, string ModelName)
        {

            var ManagerText = $@" 
                        using System;             
                        using System.Collections.Generic;
                        using System.Text;
                        using X.PagedList;
                        using {setting.MainModelNameSpace};
                        namespace {setting.ManagerAbstractNameSpace} {{

                            public interface {configureClass.abstractName}  
                            {{ 
                                 void Add{ModelName}({ModelName} model);                             
                                 void Delete{ModelName}({ModelName} model); 
                                 void Update{ModelName}({ModelName} model); 
                                 IPagedList<{ModelName}>GetAll{ModelName}({configureClass.parameters});                      
                            }}

                        }}";

            var removeFromTapText = new RegexHelper().RemoveFromTap(ManagerText);
            var folderPath = Directory.MainDirectory + "\\Manager\\Abstract\\" + configureClass.abstractName + ".cs";
            file.CreateFile(folderPath, removeFromTapText);
        }

        public void addManagerConcrete(ConfigureClass configureClass, SettingModel setting, string ModelName)
        {
            var classText = $@" 
                        using System;             
                        using System.Collections.Generic;
                        using System.Text;
                        using X.PagedList;
                        using {setting.DalAbstractNameSpace};
                        using {setting.MainModelNameSpace};
                        using {setting.ManagerAbstractNameSpace};
                        namespace {setting.ManagerConcreteNameSpace} {{
                            public class {configureClass.concretetName} : {configureClass.abstractName} 
                            {{ 
                                 private readonly {configureClass.dal} _{configureClass.dalName};

                                 public  {configureClass.concretetName}({configureClass.dal} {configureClass.dalName}){{
                                    this._{configureClass.dalName}={configureClass.dalName};
                                 }}

                                 public void Add{ModelName}({ModelName} model){{
                                  _{configureClass.dalName}.Add(model);
                                 }}    

                                 public void Delete{ModelName}({ModelName} model){{
                                  _{configureClass.dalName}.Delete(model);
                                 }}

                                 public void Update{ModelName}({ModelName} model){{
                                   _{configureClass.dalName}.Update(model);
                                 }}

                                 public List<{ModelName}> GetAll{ModelName}({configureClass.parameters}){{
                                    var query= _{configureClass.dalName}.Table();
                                    {configureClass.parameterIfQuery}
                                    return new PagedList<{ModelName}>(query,pageIndex,pageSize);
                                 }}

                            }}
                        }}";
            var removeFromTapText = new RegexHelper().RemoveFromTap(classText);
            var folderPath = Directory.MainDirectory + "\\Manager\\Concrete\\" + configureClass.concretetName + ".cs";
            file.CreateFile(folderPath, removeFromTapText);
        }
    }






    public class ConfigureClass
    {

        public int propertyType { get; set; }
        public int propertyName { get; set; }

        public string abstractName { get; set; }
        public string parameters { get; set; }

        public string concretetName { get; set; }
        public string dal { get; set; }
        public string dalName { get; set; }

        public string parameterIfQuery { get; set; }
    }

    public interface IConfigureClassSetting
    {
        ConfigureClass AbstractServiceSetting(string fileName);
        ConfigureClass ConcreteServiceSetting(string fileName);
    }
    public class ConfigureClassSetting : IConfigureClassSetting
    {
        public ConfigureClass AbstractServiceSetting(string fileName)
        {
            var configureClass = new ConfigureClass();
            PropertyInfo[] propertyInfos;
            propertyInfos = typeof(Product).GetProperties();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                var propertyType = propertyInfo.PropertyType.Name;
                var propertyName = propertyInfo.Name;
                if (propertyType == "Int32")
                    propertyType = "int";
                configureClass.parameters = configureClass.parameters + propertyType + " " + propertyName + "=" + new veriableHelper().GetDefaultValue(Type.GetType(propertyInfo.PropertyType.FullName)) + ",";
            }
            configureClass.parameters = configureClass.parameters + "int pageIndex = 1, int pageSize = int.MaxValue, string orderbytext = null";
            configureClass.abstractName = "I" + fileName + "Manager";
            return configureClass;
        }

        public ConfigureClass ConcreteServiceSetting(string fileName)
        {
            var configureClass = new ConfigureClass();
            PropertyInfo[] propertyInfos;
            propertyInfos = typeof(Product).GetProperties();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                var propertyType = propertyInfo.PropertyType.Name;
                var propertyName = propertyInfo.Name;
                if (propertyType == "Int32")
                    propertyType = "int";
                configureClass.parameters = configureClass.parameters + propertyType + " " + propertyName + "=" + new veriableHelper().GetDefaultValue(Type.GetType(propertyInfo.PropertyType.FullName)) + ",";

                configureClass.parameterIfQuery = configureClass.parameterIfQuery+ @$"
                if ({propertyName} != {new veriableHelper().GetDefaultValue(Type.GetType(propertyInfo.PropertyType.FullName))})
                {{
                    query=query.where(x=>x.{propertyName}=={propertyName});
                }}
                ";
            }
            configureClass.parameters = configureClass.parameters + "int pageIndex = 1, int pageSize = int.MaxValue, string orderbytext = null";
            configureClass.abstractName = "I" + fileName + "Manager";
            configureClass.concretetName = fileName + "Manager";
            configureClass.dalName = StringHelper.FirstLatterLower(fileName) + "DAL";
            configureClass.dal = "I" + fileName + "DAL";
            return configureClass;
        }
    }




    public class Directory
    {
        public static string MainDirectory => Environment.CurrentDirectory.ToString().Replace("\\bin\\Debug\\netcoreapp3.1", "");

    }

    public class veriableHelper
    {
        public string GetDefaultValue(Type t)
        {
            var type = "null";
            if (t.IsValueType)
                 type= Activator.CreateInstance(t).ToString().ToLower();

            return type;
            
        }
    }

    public interface IRegexHelper
    {
        string RemoveFromTap(string classText);
    }
    public class RegexHelper : IRegexHelper
    {
        public string RemoveFromTap(string classText)
        {
            const string reduceMultiSpace = @"[ ]{2,}";
            var line = Regex.Replace(classText.Replace("\r", " "), reduceMultiSpace, " ");
            return " " + line.TrimStart();
        }
    }

    public interface IFileService
    {
        public void CreateFile(string DirectoryName, string WriteText);
        public List<string> ReadDomainModel();
    }

    public class FileService : IFileService
    {
        public void CreateFile(string DirectoryName, string WriteText)
        {
            File.WriteAllText(DirectoryName, string.Empty);
            FileStream fileStream = new FileStream(DirectoryName, FileMode.Open, FileAccess.Write);
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                writer.WriteLine(WriteText);
                writer.Close();
            }
            fileStream.Close();
        }
        public List<string> ReadDomainModel()
        {
            var filesName = new List<string>();
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (var item in assembly.ExportedTypes)
            {
                if (item.Namespace == "Reflection.Domain")
                {
                    filesName.Add(item.Name);
                }
            }
            return filesName;
        }
    }

    public class JsonHelper
    {
        public class SettingModel
        {
            public string DALAbstractFolderPath { get; set; }
            public string DALConcreteFolderPath { get; set; }
            public string DalAbstractNameSpace { get; set; }
            public string DalConcreteNameSpace { get; set; }
            public string ManagerAbstractFolderPath { get; set; }
            public string ManagerConcreteFolderPath { get; set; }
            public string ManagerAbstractNameSpace { get; set; }
            public string ManagerConcreteNameSpace { get; set; }
            public string MainModelNameSpace { get; set; }

        }
        public static SettingModel Settings { get; set; }
        static JsonHelper()
        {
            var settingsStr = File.ReadAllText(Directory.MainDirectory + "\\GenerateService.json");
            dynamic settingjson = null;
            try
            {
                settingjson = JsonConvert.DeserializeObject<SettingModel>(settingsStr);
            }
            catch { }

            if (settingjson != null)
            {
                Settings = settingjson;
            }

        }
    }

    public class StringHelper
    {
        public static string FirstLatterLower(string text)
        {
            var returnText = text[0].ToString().ToLower() + text.Substring(1, text.Length - 1);
            return returnText;
        }
    }





}

