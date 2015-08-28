public interface ICommand<TResult> { }

public interface IMyAPI
{
    TResult Execute<TCommand, TResult>(TCommand<TResult> command);
}

public static class CommandHandlers
{
    [CommandHandlerAttribute] // attribute makes it easy to find via reflection
    public static int Execute(AddProductModel command, IRepository repository, IMyOtherDependency dependency)  // ** We broke our common interface!!! That's okay. You can do dependency injection to functions just like you can with classes.
    {
        var productModel = new ProductModel(name);
        repository.Save(productModel);
        dependency.OtherStuff();
        return productModel.Id;
    }

    [CommandHandlerAttribute]
    public static bool Execute(SetProductModelName command, IRepository repository)
    {
        var productModel = repository.Get(id);
        productModel.SetName(name);
        return productModel.IsUpdated();
    }

    [CommandHandlerAttribute]
    public static List<ProductModelListItem> Execute(ListProductModels command, IRepository repository)
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