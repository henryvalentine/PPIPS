using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ImportPermitPortal.DataObjects;
using ImportPermitPortal.DataObjects.Helpers;
using ImportPermitPortal.DynamicMapper.DynamicMapper;
using ImportPermitPortal.EF.Model;

namespace ImportPermitPortal.Managers.Managers
{
    public class ProductColourManager
    {
        
        public long AddProductColour(ProductColourObject productColour)
        {
            try
            {
                if (productColour == null)
                {
                    return -2;
                }

                var productColourEntity = ModelMapper.Map<ProductColourObject, ProductColour>(productColour);
                if (productColourEntity == null || string.IsNullOrEmpty(productColourEntity.Name))
                {
                    return -2;
                }
                using (var db = new ImportPermitEntities())
                {
                    var returnStatus = db.ProductColours.Add(productColourEntity);
                    db.SaveChanges();
                    return returnStatus.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }

        public long UpdateProductColour(ProductColourObject productColour)
        {
            try
            {
                if (productColour == null)
                {
                    return -2;
                }

                var productColourEntity = ModelMapper.Map<ProductColourObject, ProductColour>(productColour);
                if (productColourEntity == null || productColourEntity.Id < 1)
                {
                    return -2;
                }

                using (var db = new ImportPermitEntities())
                {
                    db.ProductColours.Attach(productColourEntity);
                    db.Entry(productColourEntity).State = EntityState.Modified;
                    db.SaveChanges();
                    return productColour.Id;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }
        public List<ProductColourObject> GetProductColours()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var productColours = db.ProductColours.ToList();
                    if (!productColours.Any())
                    {
                        return new List<ProductColourObject>();
                    }
                    var objList = new List<ProductColourObject>();
                    productColours.ForEach(app =>
                    {
                        var productColourObject = ModelMapper.Map<ProductColour, ProductColourObject>(app);
                        if (productColourObject != null && productColourObject.Id > 0)
                        {
                            objList.Add(productColourObject);
                        }
                    });

                    return !objList.Any() ? new List<ProductColourObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }

        public List<ProductColourObject> GetProductColours(int? itemsPerPage, int? pageNumber, out int countG)
        {
            try
            {
                if ((itemsPerPage != null && itemsPerPage > 0) && (pageNumber != null && pageNumber >= 0))
                {
                    var tpageNumber = (int)pageNumber;
                    var tsize = (int)itemsPerPage;

                    using (var db = new ImportPermitEntities())
                    {
                        var productColours =
                            db.ProductColours.OrderByDescending(m => m.Id)
                                .Skip((tpageNumber) * tsize)
                                .Take(tsize)
                                .ToList();
                        if (productColours.Any())
                        {
                            var newList = new List<ProductColourObject>();
                            productColours.ForEach(app =>
                            {
                                var productColourObject = ModelMapper.Map<ProductColour, ProductColourObject>(app);
                                if (productColourObject != null && productColourObject.Id > 0)
                                {
                                    newList.Add(productColourObject);
                                }
                            });
                            countG = db.ProductColours.Count();
                            return newList;
                        }
                    }

                }
                countG = 0;
                return new List<ProductColourObject>();
            }
            catch (Exception ex)
            {
                countG = 0;
                return new List<ProductColourObject>();
            }
        }


        public ProductColourObject GetProductColour(long productColourId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var productColours =
                        db.ProductColours.Where(m => m.Id == productColourId)
                            .ToList();
                    if (!productColours.Any())
                    {
                        return new ProductColourObject();
                    }

                    var app = productColours[0];
                    var productColourObject = ModelMapper.Map<ProductColour, ProductColourObject>(app);
                    if (productColourObject == null || productColourObject.Id < 1)
                    {
                        return new ProductColourObject();
                    }

                    return productColourObject;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new ProductColourObject();
            }
        }

        public List<ProductColourObject> Search(string searchCriteria)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var productColours =
                        db.ProductColours.Where(m => m.Name.ToLower().Trim() == searchCriteria.ToLower().Trim())
                        .ToList();

                    if (productColours.Any())
                    {
                        var newList = new List<ProductColourObject>();
                        productColours.ForEach(app =>
                        {
                            var productColourObject = ModelMapper.Map<ProductColour, ProductColourObject>(app);
                            if (productColourObject != null && productColourObject.Id > 0)
                            {
                                newList.Add(productColourObject);
                            }
                        });

                        return newList;
                    }
                }
                return new List<ProductColourObject>();
            }

            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return new List<ProductColourObject>();
            }
        }

        public long DeleteProductColour(long productColourId)
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var myItems =
                        db.ProductColours.Where(m => m.Id == productColourId).ToList();
                    if (!myItems.Any())
                    {
                        return 0;
                    }

                    var item = myItems[0];
                    db.ProductColours.Remove(item);
                    db.SaveChanges();
                    return 5;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return 0;
            }
        }


        public List<ProductColourObject> GerProductColours()
        {
            try
            {
                using (var db = new ImportPermitEntities())
                {
                    var productColours = db.ProductColours.ToList();
                    if (!productColours.Any())
                    {
                        return new List<ProductColourObject>();
                    }
                    var objList = new List<ProductColourObject>();
                    productColours.ForEach(app =>
                    {
                        var productColourObject = ModelMapper.Map<ProductColour, ProductColourObject>(app);
                        if (productColourObject != null && productColourObject.Id > 0)
                        {
                            objList.Add(productColourObject);
                        }
                    });

                    return !objList.Any() ? new List<ProductColourObject>() : objList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LoggError(ex.StackTrace, ex.Source, ex.Message);
                return null;
            }
        }
    }
}
