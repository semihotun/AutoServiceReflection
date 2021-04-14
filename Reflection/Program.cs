using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Reflection.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Reflection
{
    class Program
    {
        static void Main(string[] args)
        {
            Services services = new Services();
            services.AddAbstract();
            services.AddConcrete();

        }

        public class Services
        {
            IFileService file = new FileService();
            public void AddAbstract()
            {
                var fileList = file.ReadDomainModel();
                var classText = "";
                var setting = JsonHelper.Settings;
                foreach (var fileName in fileList)
                {
                    var configureClass = new ConfigureClassSetting().AbstractServiceSetting(fileName);
                    classText = $@" 
                        using System;             
                        using System.Collections.Generic;
                        using System.Text;
                        using X.PagedList;
                        using {setting.MainModelNameSpace};
                        namespace {setting.ManagerAbstractNameSpace} {{

                            public interface {configureClass.abstractName}  
                            {{ 
                                 void Add{fileName}({fileName} model);                             
                                 void Delete{fileName}({fileName} model); 
                                 void Update{fileName}({fileName} model); 
                                 void GetAll{fileName}({configureClass.parameters.Substring(0, configureClass.parameters.Length - 1)});                      
                            }}

                        }}";

                    var removeFromTapText = new RegexHelper().RemoveFromTap(classText);
                    var folderPath = Environment.CurrentDirectory.ToString().Replace("\\bin\\Debug\\netcoreapp3.1", "") + "\\Manager\\Abstract\\" + configureClass.abstractName + ".cs";
                    file.CreateFile(folderPath, removeFromTapText);
                    Console.WriteLine(removeFromTapText);
                }
            }
            public void AddConcrete()
            {
                var classText = "";
                var fileList = file.ReadDomainModel();
                PropertyInfo[] propertyInfos;
                propertyInfos = typeof(Product).GetProperties();
                var setting = JsonHelper.Settings;
                foreach (var fileName in fileList)
                {
                    var configureClass = new ConfigureClassSetting().ConcreteServiceSetting(fileName);

                    classText = $@" 
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

                                 public void Add{fileName}({fileName} model){{
                                  _{configureClass.dalName}.Add(model);
                                 }}    

                                 public void Delete{fileName}({fileName} model){{
                                  _{configureClass.dalName}.Delete(model);
                                 }}

                                 public void Update{fileName}({fileName} model){{
                                   _{configureClass.dalName}.Update(model);
                                 }}

                                 public void GetAll{fileName}({configureClass.parameters.Substring(0, configureClass.parameters.Length - 1)}){{
                                    var query= _{configureClass.dalName}.Table();
                                    {configureClass.parameterIfQuery}
                                    return query.tolist();
                                 }}

                            }}
                        }}";
                    var removeFromTapText = new RegexHelper().RemoveFromTap(classText);
                    var folderPath = Directory.MainDirectory + "\\Manager\\Concrete\\" + configureClass.concretetName + ".cs";
                    file.CreateFile(folderPath, removeFromTapText);
                    Console.WriteLine(removeFromTapText);
                   
                }

            

            }

        }

    }

    public class Directory
    {
        public static string MainDirectory => Environment.CurrentDirectory.ToString().Replace("\\bin\\Debug\\netcoreapp3.1", "");

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
            var settingsStr = File.ReadAllText(Directory.MainDirectory+"\\GenerateService.json");
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
            var returnText = text[0].ToString().ToLower() + text.Substring(1,text.Length-1);
            return returnText;         
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

            configureClass.abstractName = "I" + fileName + "Manager";
            configureClass.concretetName = fileName + "Manager";
            configureClass.dalName = StringHelper.FirstLatterLower(fileName) + "DAL";
            configureClass.dal = "I" + fileName + "DAL";
            return configureClass;
        }
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




}

