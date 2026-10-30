public interface ISettable
{
    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class;
}
