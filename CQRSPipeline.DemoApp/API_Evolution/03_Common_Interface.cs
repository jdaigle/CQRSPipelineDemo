public interface ICommand<TResult> { }

public interface IMyAPI
{
    TResult Execute<TCommand, TResult>(TCommand<TResult> command);
}

public class AddProductModel : ICommand<int>
{
    public string Name;
}

public class SetProductModelName : ICommand<bool>
{
    public int Id;
    public string Name;
}

public class ListProductModels : IQuery<List<ProductModelListItem>>
{
    public int Skip;
    public int Take;
}

public interface IHandler<TCommand<TResult>> where TCommand : ICommand<TResult>
{
    TResult Execute(TCommand command);
}

public class MyServiceImpl : IHandler<AddProductModel>, IHandler<SetProductModelName>, IHandler<ListProductModels>
{
    private IRepository repository;
    private IMyOtherDependency dependency;

    public MyServiceImpl(IRepository repository, IMyOtherDependency dependency)  // ** what if we need to deal with lots of dependencies? SRP?
    {
        this.repository = repository;
        this.dependency = dependency;
    }

    // commands
    public int Execute(AddProductModel command)
    {
        var productModel = new ProductModel(name);
        repository.Save(productModel);
        dependency.OtherStuff();
        return productModel.Id;
    }

    public bool Execute(SetProductModelName command)
    {
        var productModel = repository.Get(id);
        productModel.SetName(name);
        return productModel.IsUpdated();
    }

    // queries
    public List<ProductModelListItem> Execute(ListProductModels command)
    {
        return repository.List();
    }
}

public class Program
{
    public static void Main()
    {
        IMyAPI api;

        var id = api.Execute(new AddProductModel { Name = "Foo" });

        api.Execute(new SetProductModelName { Id = id, Name = "Bar" });

        var productModels = api.Query(new ListProductModels { Skip = 0, Take = 10 });
    }
}