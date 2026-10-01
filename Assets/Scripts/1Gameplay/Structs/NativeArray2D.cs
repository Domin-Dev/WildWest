
using Unity.Collections;

public struct NativeArray2D<T> where T : unmanaged
{
    public NativeArray<T> data;
    public int width;
    public int height;

    public NativeArray2D(int width, int height, Allocator allocator)
    {
        this.width = width;
        this.height = height;
        data = new NativeArray<T>(width * height, allocator);
    }

    public T this[int x, int y]
    {
        get => data[y * width + x];
        set => data[y * width + x] = value;
    }

    public void Dispose()
    {
        if (data.IsCreated)
            data.Dispose();
    }
}
