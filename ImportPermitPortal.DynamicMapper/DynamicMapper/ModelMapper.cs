using AutoMapper;

namespace ImportPermitPortal.DynamicMapper.DynamicMapper
{
    public static class ModelMapper
    {
        public static TModel Map<TObject, TModel>(TObject objectModel)
        {
            return Mapper.DynamicMap<TObject, TModel>(objectModel);
        }
    }
}
