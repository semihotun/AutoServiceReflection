 using System; 
 using System.Collections.Generic; 
 using System.Text; 
 using X.PagedList; 
 using Reflection.Domain; 
 namespace Reflection.Manager.Abstract { 
 
 public interface ICategoryManager 
 { 
 void AddCategory(Category model); 
 void DeleteCategory(Category model); 
 void UpdateCategory(Category model); 
 IPagedList<Category>GetAllCategory(int Id=0,String Name=null,Boolean ShowHide=false,int pageIndex = 1, int pageSize = int.MaxValue, string orderbytext = null); 
 } 
 
 }
