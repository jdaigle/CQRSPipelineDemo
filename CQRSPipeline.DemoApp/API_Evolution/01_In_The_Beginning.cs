


















public interface IMyService
{
    // commands
    int AddProductModel(string name); // ** What happens if we need a lot of parameters?
    bool SetProductModelName(int id, string name);

    // queries
    List<ProductModelListItem> ListProductModels();
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
    public int AddProductModel(string name)
    {
        var productModel = new ProductModel(name);
        repository.Save(productModel);
        dependency.OtherStuff();
        return productModel.Id;
    }

    public bool SetProductModelName(int id, string name)
    {
        var productModel = repository.Get(id);
        productModel.SetName(name);
        return productModel.IsUpdated();
    }

    // queries
    public List<ProductModelListItem> ListProductModels()
    {
        return repository.List();
    }
}

public class Program
{
    public static void Main()
    {
        IMyService service;

        var id = service.AddProductModel("Foo");

        service.SetProductModelName(id, "Bar");

        var productModels = service.ListProductModels();
    }
}