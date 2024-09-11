namespace Niobium.Html;

public static class Helpers
{
    /// <summary>
    /// Matches the given path of the property with the path of the property recorded in the Stack
    /// </summary>
    /// <param name="stack">Stack containing the path of the current property</param>
    /// <param name="names">The path to test agains</param>
    /// <returns>true if path matches</returns>
    public static bool Match(this Stack<string> stack, params string[] names)
    {
        int counter = names.Length - 1; //Child is at the end of array
        foreach (string stackName in stack)
        {
            if (counter < 0)
                break; //All previous matches were ok

            if (names[counter] != stackName)
                return false;

            counter--;
        }
        return counter < 0;
    }
}
