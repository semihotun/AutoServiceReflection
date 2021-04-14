 using System; 
 using System.Collections.Generic; 
 using System.Text; 
 using X.PagedList; 
 using Reflection.DAL.Abstract; 
 using Reflection.Domain; 
 using Reflection.Manager.Abstract; 
 namespace Reflection.Manager.Concrete { 
 public class ProductManager : IProductManager 
 { 
 private readonly IProductDAL _productDAL; 
 
 public ProductManager(IProductDAL productDAL){ 
 this._productDAL=productDAL; 
 } 
 
 public void AddProduct(Product model){ 
 _productDAL.Add(model); 
 } 
 
 public void DeleteProduct(Product model){ 
 _productDAL.Delete(model); 
 } 
 
 public void UpdateProduct(Product model){ 
 _productDAL.Update(model); 
 } 
 
 public List<Product> GetAllProduct(int Id=0,String Name=null,Boolean ShowHide=false,int pageIndex = 1, int pageSize = int.MaxValue, string orderbytext = null){ 
 var query= _productDAL.Table(); 
 
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
 
 return new PagedList<Product>(query,pageIndex,pageSize); 
 } 
 
 } 
 }
