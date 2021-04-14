using System;
using System.Collections.Generic;
using System.Text;
using X.PagedList;
using Reflection.DAL.Abstract;
using Reflection.Domain;
using Business.Abstract;
namespace Business.Concrete
{
    public class ProductManager : IProductManager
    {
        private readonly IProductDAL _productDAL;

        public ProductManager(IProductDAL productDAL)
        {
            this._productDAL = productDAL;
        }

        public void AddProduct(Product model)
        {
            _productDAL.Add(model);
        }

        public void DeleteProduct(Product model)
        {
            _productDAL.Delete(model);
        }

        public void UpdateProduct(Product model)
        {
            _productDAL.Update(model);
        }

        public void GetAllProduct(int Id = 0, String Name = null, Boolean ShowHide = false)
        {
            var query = _productDAL.Table();

            if (Id != 0)
            {
                query = query.where(x => x.Id == Id);
            }

            if (Name != null)
            {
                query = query.where(x => x.Name == Name);
            }

            if (ShowHide != false)
            {
                query = query.where(x => x.ShowHide == ShowHide);
            }

            return query.tolist();
        }

    }
}
