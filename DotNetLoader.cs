using System.Reflection;

class DotNetLoader
{
    public object loadAssembly(byte[] bin, object[] commands)
    {
        string r = System.Text.Encoding.UTF8.GetString(bin);
        Assembly ass = Assembly.Load(bin);

        object result = null;
        try
        {
            ass.EntryPoint.Invoke(null, new object[] { commands });
        }
        catch
        {
            MethodInfo method = ass.EntryPoint;
            if (method != null)
            {
                object o = ass.CreateInstance(method.Name);
                result = method.Invoke(o, null);
            }
        }
        return result;
    }
}

