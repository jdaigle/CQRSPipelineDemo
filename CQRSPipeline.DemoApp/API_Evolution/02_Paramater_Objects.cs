public class AddProductModel
{
    public string Name;
}

public class SetProductModelName
{
    public int Id;
    public string Name;
}

public class ListProductModels
{
    public int Skip;
    public int Take;
}



public interface IMyService
{
    // commands
    int AddProductModel(AddProductModel parameters); // ** those method signatures look a little bit redundant!
    bool SetProductModelName(SetProductModelName parameters);

    // queries
    List<ProductModelListItem> ListProductModels(ListProductModels criteria);
}

public class MyServiceImpl : IMyService
{
    private IRepository repository;
    private IMyOtherDependency dependency;

    public MyServiceImpl(IRepository repository, IMyOtherDependency dependency)
    {
        this.repository = repository;
        this.dependency = dependency;
    }

    // commands
    public int AddProductModel(AddProductModel parameters)
    {
        var productModel = new ProductModel(name);
        repository.Save(productModel);
        dependency.OtherStuff();
        return productModel.Id;
    }

    public bool SetProductModelName(SetProductModelName parameters)
    {
        var productModel = repository.Get(id);
        productModel.SetName(name);
        return productModel.IsUpdated();
    }

    // queries
    public SList<ProductModelListItem> ListProductModels(ListProductModels criteria)
    {
        return repository.List();
    }
}

public class Program
{
    public static void Main()
    {
        IMyService service;

        var id = service.AddProductModel(new AddProductModel { Name = "Foo" });

        service.SetProductModelName(new SetProductModelName { Id = id, Name = "Bar" });

        var productModels = service.ListProductModels(new ListProductModels { Skip = 0, Take = 10 });
    }
}