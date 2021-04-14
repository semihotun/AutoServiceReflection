 using System; 
 using System.Collections.Generic; 
 using System.Text; 
 using X.PagedList; 
 using Reflection.Domain; 
 namespace Reflection.Manager.Abstract { 
 
 public interface IProductManager 
 { 
 void AddProduct(Product model); 
 void DeleteProduct(Product model); 
 void UpdateProduct(Product model); 
 void GetAllProduct(int Id=0,String Name=null,Boolean ShowHide=false); 
 } 
 
 }
