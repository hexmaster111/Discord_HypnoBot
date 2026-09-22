namespace HaileysSpreadsheats.Expr;

public class ViewableStringStreamer(string expr)
{
    public string Current { get; private set; } = expr;

    public void Next()
    {
        Skip(1);
    }

    public void Skip(int n)
    {
        Current = Current[n..];
    }

    public string Read(int len)
    {
        string r = "";
        for (int i = 0; i < len; i++)
        {
            r += Current[0];
            Next();
        }

        return r;
    }
}