using Microsoft.AspNetCore.Hosting;
using Reflection.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Reflection
{
    class Program
    {
        static void Main(string[] args)
        {
            Services services = new Services();
            services.AddAbstract();

        }

        public class Services
        {
            IFile file = new File();
            public void AddAbstract()
            {
                var classText = "";
                var fileList = file.ReadDomainModel();
                PropertyInfo[] propertyInfos;
                propertyInfos = typeof(Product).GetProperties();
                foreach (var fileName in fileList)
                {
                    var parameters = "";
                    foreach (PropertyInfo propertyInfo in propertyInfos)
                    {
                        var propertyType = propertyInfo.PropertyType.Name;
                        var propertyName = propertyInfo.Name;
                        parameters =parameters + propertyType + " " + propertyName + ",";
                    }

                    var abstractName = "I" + fileName + "Manager";
                    classText = $@"
                        using System;
                        using System.Collections.Generic;
                        using System.Text;
                        using X.PagedList;
                        namespace Business.Abstract {{

                            public interface {abstractName}  
                            {{ 
                                 void Add{fileName}({fileName} model);                             
                                 void Delete{fileName}({fileName} model); 
                                 void Update{fileName}({fileName} model); 
                                 void GetAll{fileName}({parameters.Substring(0,parameters.Length-1)});                      
                            }}

                        }}";


                }


                Console.WriteLine(classText);

            }
        }
        public void AddConcrete()
        {

        }
    }

    public interface IFile
    {
        public void CreateFile(string DirectoryName, string WriteText);
        public List<string> ReadDomainModel();
    }

    public class File : IFile
    {
        public void CreateFile(string DirectoryName, string WriteText)
        {

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

