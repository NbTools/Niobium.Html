namespace Niobium.Html;

public class CsvTool
{
    private const char DefaulSeparator = ',';
    private const char quote = '"';
    private enum States { SeparatorRead, ReadingField, ReadingQuotedField };

    /// <summary>
    /// DeCsv algorithm runs on the string already read from the file - to be deprecated, run DeCsvLine of the TextReader where possible
    /// </summary>
    /// <param name="str"></param>
    /// <param name="bld">The string builder allocated outside the cycle to reduce the number of allocations</param>
    /// <param name="separator"></param>
    /// <param name="trim"></param>
    /// <returns></returns>
    public static IEnumerable<string> DeCsvLine(string? str, StringBuilder bld, char separator = DefaulSeparator, bool trim = true)
    {
        if (str == null)
            yield break;

        using var rdr = new StringReader(str);
        States state = States.SeparatorRead;

        bld.Clear();
        int curr;
        while ((curr = rdr.Read()) != -1)
        {
            char ch = (char)curr;

            switch (state)
            {
                case States.SeparatorRead:
                    if (ch.Equals(quote))
                        state = States.ReadingQuotedField;
                    else if (ch.Equals(separator)) //Separators next to each other - empty string
                        yield return String.Empty;
                    else
                    {
                        state = States.ReadingField;
                        bld.Append(ch);
                    }
                    break;
                case States.ReadingField:
                    if (ch.Equals(separator))
                    {
                        yield return trim ? bld.ToString().Trim() : bld.ToString(); //The string is ready
                        bld.Clear();
                        state = States.SeparatorRead;
                    }
                    else
                        bld.Append(ch); //Simply next symbol 
                    break;
                case States.ReadingQuotedField:
                    //Don't check for separator inside quotes
                    if (ch.Equals(quote)) //Could be end of the string or embedded quotes
                    {
                        int peek = rdr.Peek();
                        if (peek == -1) //End of line
                            break; //It was the last field in the line and it ends with separator, stop and return the last line
                        else if (((char)peek).Equals(quote))
                        { //Two quotes together
                            rdr.Read(); //Consume the next separator
                            bld.Append(quote); //Keep only one of them
                        }
                        else if (((char)peek).Equals(separator))
                            state = States.ReadingField; //The quote is closed, treat as normal field
                        else
                        { //Not a double quote and not followed by the separator, give a warning.
                          //TODO: give a warning
                            bld.Append(ch); //Append it just in case 
                        }
                    }
                    else
                        bld.Append(ch);
                    break;
            }
        }
        yield return trim ? bld.ToString().Trim() : bld.ToString(); //return the last string
    }
}
