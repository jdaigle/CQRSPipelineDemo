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

public class AddProductModelHandler : IHandler<AddProductModel>
{
    private IRepository repository;
    private IMyOtherDependency dependency;

    public AddProductModelHandler(IRepository repository, IMyOtherDependency dependency)
    {
        this.repository = repository;
        this.dependency = dependency;
    }

    public int Execute(AddProductModel command)
    {
        var productModel = new ProductModel(name);
        repository.Save(productModel);
        dependency.OtherStuff();
        return productModel.Id;
    }
}

public class SetProductModelNameHandler : IHandler<SetProductModelName>
{
    private IRepository repository;

    public SetProductModelNameHandler(IRepository repository)
    {
        this.repository = repository;
    }

    public bool Execute(SetProductModelName command)
    {
        var productModel = repository.Get(id);
        productModel.SetName(name);
        return productModel.IsUpdated();
    }
}

public class ListProductModelsHandler : IHandler<ListProductModels>  // ** that's pretty good. but _a lot_ of boiler plate. (avg of 5:1). We're also wasting allocations with the class since we're not maintaining state.
{
    private IRepository repository;

    public ListProductModelsHandler(IRepository repository)
    {
        this.repository = repository;
    }

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