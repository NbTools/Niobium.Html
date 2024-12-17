namespace Niobium.Html;

public class SmartException : Exception
{
    public SmartException(string aMessage) : base(aMessage) { }
    public SmartException(Exception ex, string mess) : base(mess, ex) { }

    public static string Exception2String(Exception ex, Action<Exception, StringBuilder>? extraHandler = null)
    {
        StringBuilder bld = new();
        ProcException(ex, bld, 0, extraHandler);
        return bld.ToString();
    }

    private static void ProcException(Exception ex, StringBuilder bld, int margin, Action<Exception, StringBuilder>? extraHandler = null)
    {
        for (int i = margin; i > 0; --i) //Margin without new string allocations
            bld.Append("  ");

        bld.Append(ex.Message);
        bld.Append(" {");
        bld.Append(ex.GetType().Name);
        bld.AppendLine("}");

        extraHandler?.Invoke(ex, bld);

        if (ex is AggregateException aggEx)
        {
            foreach (Exception ex1 in aggEx.InnerExceptions)
                ProcException(ex1, bld, margin + 1);
        }
        else
        {
            if (ex.InnerException != null)
                ProcException(ex.InnerException, bld, margin + 1);
        }
    }

    public override string ToString() => Exception2String(this);
}
