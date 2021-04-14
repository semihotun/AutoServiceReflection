 using System; 
 using System.Collections.Generic; 
 using System.Text; 
 using X.PagedList; 
 using Reflection.DAL.Abstract; 
 using Reflection.Domain; 
 using Reflection.Manager.Abstract; 
 namespace Reflection.Manager.Concrete { 
 public class CategoryManager : ICategoryManager 
 { 
 private readonly ICategoryDAL _categoryDAL; 
 
 public CategoryManager(ICategoryDAL categoryDAL){ 
 this._categoryDAL=categoryDAL; 
 } 
 
 public void AddCategory(Category model){ 
 _categoryDAL.Add(model); 
 } 
 
 public void DeleteCategory(Category model){ 
 _categoryDAL.Delete(model); 
 } 
 
 public void UpdateCategory(Category model){ 
 _categoryDAL.Update(model); 
 } 
 
 public List<Category> GetAllCategory(int Id=0,String Name=null,Boolean ShowHide=false,int pageIndex = 1, int pageSize = int.MaxValue, string orderbytext = null){ 
 var query= _categoryDAL.Table(); 
 
 if (Id != 0) 
 { 
 query=query.where(x=>x.Id==Id); 
 } 
 
 if (Name != null) 
 { 
 query=query.where(x=>x.Name==Name); 
 } 
 
 if (ShowHide != false) 
 { 
 query=query.where(x=>x.ShowHide==ShowHide); 
 } 
 
 return new PagedList<Category>(query,pageIndex,pageSize); 
 } 
 
 } 
 }
